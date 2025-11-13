#include <errno.h>
#include <fcntl.h> 
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <termios.h>
#include <unistd.h>
#include <iostream>

class ByteReceived {       // The class
  public:             // Access specifier
    bool error;        // Attribute (bool variable)
    u_int8_t byte;  // Attribute (u_int8_t variable)
    bool timeout;
};

int set_interface_attribs(int fd, int speed)
{
    struct termios tty;

    if (tcgetattr(fd, &tty) < 0) {
        printf("Error from tcgetattr: %s\n", strerror(errno));
        return -1;
    }

    cfsetospeed(&tty, (speed_t)speed);
    cfsetispeed(&tty, (speed_t)speed);
    tty.c_cflag |= CLOCAL | CREAD;
    tty.c_cflag &= ~CSIZE;
    tty.c_cflag |= CS8;         /* 8-bit characters */
    tty.c_cflag |= PARENB;      /* enable parity */
    tty.c_cflag &= ~PARODD;     /* Even parity */
    tty.c_cflag |= CMSPAR;      /* force Even parity to SPACE */
    tty.c_cflag |= CSTOPB;     /* only need 1 stop bit */
    tty.c_cflag &= ~CRTSCTS;    /* no hardware flowcontrol */

    tty.c_lflag &= ~ICANON & ~ISIG;  /* canonical input */
    tty.c_lflag &= ~(ECHO | ECHOE | ECHONL | IEXTEN);

    tty.c_iflag &= ~IGNCR;  /* preserve carriage return */
    tty.c_iflag &= ~(INLCR | ICRNL | IUCLC | IMAXBEL);
    tty.c_iflag &= ~(IXON | IXOFF | IXANY);   /* no SW flowcontrol */
    tty.c_iflag |= IGNBRK;  /* ignore breaks */
    tty.c_iflag &= ~ISTRIP;
    tty.c_iflag &= ~IGNPAR; /* report error */
    tty.c_iflag |= INPCK;   /* test parity */
    tty.c_iflag |= PARMRK;  /* verbose parity err */

    tty.c_oflag &= ~OPOST;

    tty.c_cc[VEOL] = 0;
    tty.c_cc[VEOL2] = 0;
    tty.c_cc[VEOF] = 0x04;
    tty.c_cc[VTIME] = 255;
    tty.c_cc[VMIN] = 0;
    if (tcsetattr(fd, TCSANOW, &tty) != 0) {
        printf("Error from tcsetattr: %s\n", strerror(errno));
        return -1;
    }
    return 0;
}

int set_wakeup(int PortFileDescriptor)
{
struct termios tty;

    if (tcgetattr(PortFileDescriptor, &tty) < 0) {
        printf("Error from tcgetattr: %s\n", strerror(errno));
        return -1;
    }

     tty.c_cflag |= PARENB;      /* enable parity */
    tty.c_cflag |=  PARODD;     /* Even parity */
    tty.c_cflag |= CMSPAR;      /* force Even parity to MARK */

    if (tcsetattr(PortFileDescriptor, TCSANOW, &tty) != 0) {
        printf("Error from tcsetattr: %s\n", strerror(errno));
        return -1;
    }
    return 0;
}

int set_space(int PortFileDescriptor)
{
struct termios tty;

    if (tcgetattr(PortFileDescriptor, &tty) < 0) {
        printf("Error from tcgetattr: %s\n", strerror(errno));
        return -1;
    }
    
    tty.c_cflag |= PARENB;      /* enable parity */
    tty.c_cflag &= ~PARODD;     /* Even parity */
    tty.c_cflag |= CMSPAR;      /* force Even parity to SPACE  */

    if (tcsetattr(PortFileDescriptor, TCSANOW, &tty) != 0) {
        printf("Error from tcsetattr: %s\n", strerror(errno));
        return -1;
    }
    return 0;
}

int OpenPort(char const *portname)
{
    int PortFileDescriptor;


    PortFileDescriptor = open(portname, O_RDWR | O_NOCTTY | O_SYNC);
    if (PortFileDescriptor < 0) {
        printf("Error opening %s: %s\n", portname, strerror(errno));
        return -1;
    }
    set_interface_attribs(PortFileDescriptor, B19200);
    return PortFileDescriptor;
}

void ClosePort(int PortFileDescriptor)
{
    close(PortFileDescriptor);
}

ByteReceived uart_read_byte(int fd, int maxMicroSecondsTimeout) {
	ByteReceived b;
	u_int8_t c;
	// Initialize file descriptor sets
	fd_set read_fds, write_fds, except_fds;
	FD_ZERO(&read_fds);
	FD_ZERO(&write_fds);
	FD_ZERO(&except_fds);
	FD_SET(fd, &read_fds);
	// Set timeout to 10 milisecondsseconds
	struct timeval timeout;
	timeout.tv_sec = 0;
	timeout.tv_usec = maxMicroSecondsTimeout;
	// Wait for input to become ready or until the time out; the first parameter is
	// 1 more than the largest file descriptor in any of the sets
	if (select(fd + 1, &read_fds, &write_fds, &except_fds, &timeout) == 1)
	{
		read(fd, &c, sizeof(c));
	  	if (c == 0xFF) { // got escape byte
			read(fd, &c, sizeof(c));
			if (c == 0x00) { // parity bit set
		    		read(fd, &c, sizeof(c));
		    		b.byte = c;
		    		b.error = true;
		    		b.timeout = false;
		    		return b; // this byte is returned with parity error
			} else if (c == 0xFF)
			{
		    		b.byte = c;
				b.error = false;
				b.timeout = false;
				return b; 
			}
	    	}
	    	else {
		b.byte = c;
		b.error = false;
		b.timeout = false;
		return b; // this byte is returned with no parity error
	    	}
	}
	else
	{
	    b.timeout = true; // timeout 
    	    return b;
	}
	
}

void flushPort(int PortFileDescriptor)
{
    tcflush(PortFileDescriptor,TCIFLUSH);
}

void writeBytes(unsigned char *message, int size, int PortFileDescriptor)
{
    int wlen = write(PortFileDescriptor, message, size);
    if (wlen != size) {
        printf("Error from write: %d, %d\n", wlen, errno);
    }
    tcdrain(PortFileDescriptor);    /* delay for output */
}

void writeBytesWithWakeup(unsigned char *byteWithWakeup, unsigned char *bytesWithSpace, int size, int PortFileDescriptor)
{
    set_wakeup(PortFileDescriptor);
    int wlen = write(PortFileDescriptor, byteWithWakeup, 1);
    set_space(PortFileDescriptor);
    if (size - 1 > 0)
    {
	    wlen = write(PortFileDescriptor, bytesWithSpace, size - 1);	   
    }

}

void printMessage(unsigned char *msg,int msgLength, char parity)
{
    for(int i=0;i<msgLength;i++)
    {
        if (parity == 'm')
            printf("+%x\n", msg[i]);
        if (parity == 's')
            printf("-%x\n", msg[i]);

    }
}
int main(int argc, char *argv[])
{
    if (argc == 4)
    {
        int fd = OpenPort(argv[1]);
        unsigned char data[1] = {0x81};
        int datasize= sizeof(data);
        if (strlen(argv[2]) > 0)
        {
            char parity = argv[2][0]; // 'm', 's'
            char last_parity = 'x';
            if (parity == 'm')
            {
                set_wakeup(fd);
                int wlen = write(fd, data, 1);
                printMessage(data, datasize, parity);
                last_parity = parity;
            }
            else if (parity == 's')
            {
                set_space(fd);
                int wlen = write(fd, data, 1);
                printMessage(data, datasize,parity);                
                last_parity = parity;
            }
            else
            {
                printf("Char %c - ", parity);
                printf("Parser Error: Character must be m or s!!\n");
            }
            for(int i = 1;i < strlen(argv[2]); i++)
            {
                parity = argv[2][i];
                if (parity == 'm')
                {
                    if (last_parity != parity)
                    {
                        if (last_parity != 'x')
                            usleep(std::stoi(argv[3]));
                    }
                    set_wakeup(fd);
                    int wlen = write(fd, data, 1);
                    printMessage(data, datasize, parity);
                    last_parity = parity;
                }
                else if (parity == 's')
                {
                    if (last_parity != parity)
                    {
                        if (last_parity != 'x')
                            usleep(std::stoi(argv[3]));
                    }
                    set_space(fd);
                    int wlen = write(fd, data, 1);
                    printMessage(data, datasize, parity);
                    last_parity = parity;
                }
                else
                {
                    printf("Char %c - ", parity);
                    printf("Parser Error: Character must be m or s!!\n");
                }
            }
        }
    }
    else
    {
        printf("USAGE:\n");
        printf("./csm [SerialPort] [Message] [Delay in microseconds]\n");
    }
  
    return 0;
}



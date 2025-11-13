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


void flushPort(int PortFileDescriptor)
{
    tcflush(PortFileDescriptor,TCIFLUSH);
    tcflush(PortFileDescriptor,TCOFLUSH);
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
    if (argc == 3)
    {
        int fd = OpenPort(argv[1]);
        for(unsigned char c=0x01;c<=0xFF;c++)
        {
            flushPort(fd);
            usleep(1000);
            set_space(fd);
            usleep(1000);
            set_wakeup(fd);
            usleep(1000);
            unsigned char data[1] = {c};
            int datasize= sizeof(data);
            int wlen = write(fd, data, 1);
            printMessage(data, datasize, 'm');
            usleep(std::stoi(argv[2]));
        }
    }
    else
    {
        printf("USAGE:\n");
        printf("./sendAllBytes [SerialPort] [Delay in microseconds]\n");
    }
  
    return 0;
}



#define SERIALTERMINAL      "/dev/ttyS0"
#include <errno.h>
#include <fcntl.h> 
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <termios.h>
#include <unistd.h>

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
    tty.c_cc[VTIME] = 0.1;
    tty.c_cc[VMIN] = 0;
    printf("%d\n", tty.c_cc[VTIME]);
    printf("%d\n", tty.c_cc[VMIN]);
    //tty.c_cc[VMIN] = 1;
    if (tcsetattr(fd, TCSANOW, &tty) != 0) {
        printf("Error from tcsetattr: %s\n", strerror(errno));
        return -1;
    }
    return 0;
}

ByteReceived uart_read_byte(int fd) {

    ByteReceived b;
    u_int8_t c;
    int timeout = read(fd, &c, sizeof(c));
    if (timeout != 0)
    {
	    if (c == 0xFF) { // got escape byte
		read(fd, &c, sizeof(c));

		if (c == 0x00) { // parity bit set
		    read(fd, &c, sizeof(c));
		    b.byte = c;
		    b.error = true;
		    b.timeout = false;
		    return b;
		} else if (c != 0xFF)
		    fprintf(stderr, "invalid byte sequence: %x %x\n", 0xFF, c);
	    }
	    else {
		b.byte = c;
		b.error = false;
		b.timeout = false;
	    }
    }
    else
    {
	b.timeout = true;
    }

    return b;
}


int main(void)
{
    int fd;

    fd = open(SERIALTERMINAL, O_RDWR | O_NOCTTY | O_SYNC);
    if (fd < 0) {
        printf("Error opening %s: %s\n", SERIALTERMINAL, strerror(errno));
        return -1;
    }
    /*baudrate 115200, 8 bits, Space for parity, 1 stop bit */
    set_interface_attribs(fd, B19200);
    u_int8_t c;
    do 
      {
	   unsigned char message[128];
    	  ByteReceived b = uart_read_byte(fd);
	  int i = -1;
	  if (b.error == true)
	  {
	    while(b.timeout == false)
	    {
		i++;
		message[i] = b.byte; 
	    	b = uart_read_byte(fd);
	    }
	  }
     }while(1);
 /*  do {
        unsigned char buf[128];
        unsigned char *p;
        int rdlen;

        rdlen = read(fd, buf, sizeof(buf) - 1);
        if (rdlen > 0) {
            buf[rdlen] = 0;
            printf("Read %d:", rdlen);

            for (p = buf; rdlen-- > 0; p++) {
                printf(" 0x%x", *p);
                if (*p < ' ')
                    *p = '.';   
            }
            printf("\n");
        } else if (rdlen < 0) {
            printf("Error from read: %d: %s\n", rdlen, strerror(errno));
        } else {  
            printf("Nothing to read\n");
        }              
 
    } while (1); */
}



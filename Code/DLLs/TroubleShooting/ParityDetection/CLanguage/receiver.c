#define SERIALTERMINAL      "/dev/ttyS0"
#include <errno.h>
#include <fcntl.h> 
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <termios.h>
#include <unistd.h>

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

    if (tcsetattr(fd, TCSANOW, &tty) != 0) {
        printf("Error from tcsetattr: %s\n", strerror(errno));
        return -1;
    }
    return 0;
}


int main(void)
{
    char *portname = SERIALTERMINAL;
    int fd;
    int wlen;

    fd = open(portname, O_RDWR | O_NOCTTY | O_SYNC);
    if (fd < 0) {
        printf("Error opening %s: %s\n", portname, strerror(errno));
        return -1;
    }
    /*baudrate 115200, 8 bits, Space for parity, 1 stop bit */
    set_interface_attribs(fd, B19200);

    /* simple output */
  /*  wlen = write(fd, "Hello!\n", 7);
    if (wlen != 7) {
        printf("Error from write: %d, %d\n", wlen, errno);
    }
    tcdrain(fd);   */ /* delay for output */


    /* simple canonical input, read lines */
    do {
        unsigned char buf[128];
        unsigned char *p;
        int rdlen;

        rdlen = read(fd, buf, sizeof(buf) - 1);
        if (rdlen > 0) {
            buf[rdlen] = 0;
            printf("Read %d:", rdlen);
            /* first display as hex numbers then ASCII */
            for (p = buf; rdlen-- > 0; p++) {
                printf(" 0x%x", *p);
                if (*p < ' ')
                    *p = '.';   /* replace any control chars */
            }
            printf("\n");
        } else if (rdlen < 0) {
            printf("Error from read: %d: %s\n", rdlen, strerror(errno));
        } else {  /* rdlen == 0 */
            printf("Nothing read. EOF?\n");
        }               
        /* repeat read */
    } while (1);
}



#include "spcontroller.h"
#include <errno.h>
#include <fcntl.h> 
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <termios.h>
#include <unistd.h>
#include <iostream>
class ByteReceived {       // The ByteReceived class
  public:           // Access specifier
	bool error;     // Attribute for parity error
	u_int8_t byte;  // Value
	bool timeout;   // A timeout value for check if there is a time out for receiving this byte
};
ByteReceived ByteWithParity;
bool ByteWithParityRead = true;
bool getByteWithParity(ByteReceived* byte) {
	// If the byte with parity is not read
	if (!ByteWithParityRead) {
		*byte = ByteWithParity; // Assign this byte to b 
		ByteWithParityRead = true;
		// printf("using parity byte: %02x\n", ByteWithParity.byte);fflush(stdout);
		return true;
	}
	return false;
}
void setByteWithParity(bool b, ByteReceived byte) {
	ByteWithParityRead = b;
	ByteWithParity = byte;
	// printf("setByteWithParity: %02x %d\n", byte.byte, b);fflush(stdout);
}
// Set interface attributes for file descriptor fd
int set_interface_attribs(int fd, int speed) {
	struct termios tty;
	if (tcgetattr(fd, &tty) < 0) { // Get the termios structure from file descriptor fd 
		printf("Error from tcgetattr 11: %s\n", strerror(errno)); fflush(stdout);
		return -1;
	}
	cfsetospeed(&tty, (speed_t)speed); // Set output speed to 'speed' (BAUDRATE)
	cfsetispeed(&tty, (speed_t)speed); // Set input speed to 'speed' (BAUDRATE)
	tty.c_cflag |= CLOCAL | CREAD; /* Ignore modem control lines + Enable receiver*/
	tty.c_cflag &= ~CSIZE;      /* Disable character size mask */
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
	tty.c_iflag &= ~ISTRIP; /* Strip on eighth bit*/
	tty.c_iflag &= ~IGNPAR; /* report error */
	tty.c_iflag |= INPCK;   /* test parity */
	tty.c_iflag |= PARMRK;  /* verbose parity err */
	tty.c_oflag &= ~OPOST;   /* Disable implementation-defined output processing*/
	tty.c_cc[VEOL] = 0; /*Additional end-of-line character*/
	tty.c_cc[VEOL2] = 0; /*Yet another end-of-line character*/
	tty.c_cc[VEOF] = 0x04; /* End-of-file character */
	tty.c_cc[VTIME] = 255; /* Timeout in deciseconds for noncanonical read */
	tty.c_cc[VMIN] = 0; /*Minimum number of characters for noncanonical read*/
	if (tcsetattr(fd, TCSANOW, &tty) != 0) { /* Set attributes to termios */
		printf("Error from tcsetattr: %s\n", strerror(errno)); fflush(stdout);
		return -1; 
	}
	return 0;
}
// Set wake up bit for file descriptor fd
int set_wakeup(int PortFileDescriptor) {
	struct termios tty;
	if (tcgetattr(PortFileDescriptor, &tty) < 0) { // Get the termios structure from file descriptor fd 
		printf("Error from tcgetattr 22: %s\n", strerror(errno)); fflush(stdout);
		return -1;
	}
	 tty.c_cflag |= PARENB;      /* enable parity */
	tty.c_cflag |=  PARODD;     /* Even parity */
	tty.c_cflag |= CMSPAR;      /* force Even parity to MARK */
	if (tcsetattr(PortFileDescriptor, TCSANOW, &tty) != 0) { /* Set attributes to termios */
		printf("Error from tcsetattr: %s\n", strerror(errno)); fflush(stdout);
		return -1;
	}
	return 0;
}
// Set space bit for file descriptor fd
int set_space(int PortFileDescriptor) {
	struct termios tty;
	if (tcgetattr(PortFileDescriptor, &tty) < 0) { // Get the termios structure from file descriptor fd 
		printf("Error from tcgetattr 33: %s\n", strerror(errno)); fflush(stdout);
		return -1;
	}
	tty.c_cflag |= PARENB;      /* enable parity */
	tty.c_cflag &= ~PARODD;     /* Even parity */
	tty.c_cflag |= CMSPAR;      /* force Even parity to SPACE  */
	if (tcsetattr(PortFileDescriptor, TCSANOW, &tty) != 0) { /* Set attributes to termios */
		printf("Error from tcsetattr: %s\n", strerror(errno)); fflush(stdout);
		return -1;
	}
	return 0;
}
int logfd = 0;
const char* logPort = "/dev/ttyUSB0";
/* OpenPort */
int OpenPort(char const *portname) {
	int PortFileDescriptor;
	/* Opens the port with the name 'portname' with read and write (O_RDWR) 
	If the named file is a terminal device, don't make it the controlling terminal for the process (O_NOCTTY)
	and just guarantees synchronous I/O (O_SYNC)*/
	PortFileDescriptor = open(portname, O_RDWR | O_NOCTTY | O_SYNC); 
	if (PortFileDescriptor < 0) {
		printf("Error opening %s: %s\n", portname, strerror(errno)); fflush(stdout);
		return -1;
	}
	set_interface_attribs(PortFileDescriptor, B19200);
	if (strcmp(portname, logPort)) {
		logfd = PortFileDescriptor;
		FILE *File;
		File = fopen("ninja.hex", "w");
		fprintf(File, "logging for %s %d\n\n", logPort, logfd);
		fclose(File);
	}
	return PortFileDescriptor;
}
/* Close Port */
void ClosePort(int PortFileDescriptor) {
	close(PortFileDescriptor);
}

void writeByteToFile(u_int8_t c, bool newline, int fd) {
	if (fd != logfd) return;
	FILE *File;
	File = fopen("ninja.hex", "a");
	if (newline) fprintf(File, "\n*%02x", c);
	else fprintf(File, "-%02x", c);
	fclose(File);
}
void printByte(u_int8_t c, int source, int source2) {
	if (source2 > 5) return; 
	switch (c) {
	case 0xFF:
	case 0x00:
	case 0x80:
	case 0x81:
		return;
	case 0x01:
	// case 0x37:
		// printf("%d-%d", source, source2);
		// printf(": %02x\n", c);fflush(stdout);
		return;
	default:
		// printf(": %02x\n", c);fflush(stdout);
		return;
	}
}
/* UART Read Byte */
ByteReceived uart_read_byte(int fd, int maxMicroSecondsTimeout, int source) {
	ByteReceived b;
	u_int8_t c;
	// Initialize file descriptor sets
	fd_set read_fds, write_fds, except_fds;
	FD_ZERO(&read_fds);
	FD_ZERO(&write_fds);
	FD_ZERO(&except_fds);
	FD_SET(fd, &read_fds);
	// Set timeout to 10 miliseconds
	struct timeval timeout;
	timeout.tv_sec = 0;
	timeout.tv_usec = maxMicroSecondsTimeout;
	// Wait for input to become ready or until the time out; the first parameter is
	// 1 more than the largest file descriptor in any of the sets
	if (select(fd + 1, &read_fds, &write_fds, &except_fds, &timeout) == 1) {
		// Reads the byte of serial file descriptor fd and saves the data in c
		read(fd, &c, sizeof(c));
		printByte(c, 1, source);
		// printf("Byte7: %02x\n", c);fflush(stdout);
		if (c == 0xFF) { // got escape byte
			read(fd, &c, sizeof(c));
			printByte(c, 2, source);
			// printf("Byte5: %02x\n", c);fflush(stdout);
			if (c == 0x00) { // parity bit set
				read(fd, &c, sizeof(c));
				writeByteToFile(c, true, fd);
				printByte(c, 3, source);
				// printf("Byte6: %02x\n", c);fflush(stdout);
				b.byte = c;
				b.error = true;
				b.timeout = false;
				return b; // this byte is returned with parity error
			} else if (c == 0xFF) {
				writeByteToFile(c, false, fd);
				b.byte = c;
				b.error = false;
				b.timeout = false;
				return b; 
			}
		} else {
			writeByteToFile(c, false, fd);
			b.byte = c;
			b.error = false;
			b.timeout = false;
			return b; // this byte is returned with no parity error
		}
	} else {
		b.error = false;
		b.timeout = true; // timeout 
		return b;
	}
	printf("error!\n");fflush(stdout);
	b.error = false;
	b.timeout = true; // timeout 
	return b;
}
void flushPort(int PortFileDescriptor) {
	tcflush(PortFileDescriptor,TCIFLUSH);
}

void flushOutputPort(int PortFileDescriptor) {
	tcflush(PortFileDescriptor,TCOFLUSH);
}

/* Write Bytes - Normal*/
void writeBytes(unsigned char *message, int size, int PortFileDescriptor) {
	int wlen = write(PortFileDescriptor, message, size);
	if (wlen != size) {
		printf("Error from write: %d, %d\n", wlen, errno); fflush(stdout);
	}
	//tcdrain(PortFileDescriptor);    /* delay for output */
}

/* Write Bytes - With Wakeup*/
void writeBytesWithWakeup(unsigned char *byteWithWakeup, unsigned char *bytesWithSpace, int size, int PortFileDescriptor) {
	set_wakeup(PortFileDescriptor); // Set wakeup on port
	int wlen = write(PortFileDescriptor, byteWithWakeup, 1); // Send the first byte with wake up
	usleep(3000);
	set_space(PortFileDescriptor);  // Set space on port
	if (size - 1 > 0) {
		wlen = write(PortFileDescriptor, bytesWithSpace, size - 1);  // Send all bytes with parity space  
	}
}

/* READ Bytes */ 
int readBytes(unsigned char *message, int maxsize, int PortFileDescriptor) {
	if (maxsize <= 0) return -1;

	//We define the message
	unsigned char result[maxsize];
	// Let b the byte received
	ByteReceived b, b2;
	// If the byte with parity is not read
	if (getByteWithParity(&b)) {
		// printf("using ByteWithParity: %02x %d\n", b.byte, b.error);fflush(stdout);
	} else {
		b = uart_read_byte(PortFileDescriptor,1, 1); // Read from the serial port
	}
	// If parity error is set for the first byte
	if (b.error != true) {
		return -1;
	}
	// printf("byte ignored: %02x\n", b.byte);fflush(stdout);

	int i = 0;
	result[i] = b.byte; 
	b2 = b;
	// printf("Byte1: %02x\n", result[i]); fflush(stdout);
	// Read the byte again
	b = uart_read_byte(PortFileDescriptor, 5000, 2);
	// if (result[i] == 0x01) {
	// 	printf("first byte: %02x %d %d\n", b2.byte, b2.error, b2.timeout);fflush(stdout);
	// 	printf("next byte: %02x %d %d\n", b.byte, b.error, b.timeout);fflush(stdout);
	// }
	// if (b.timeout) {
	// 	setByteWithParity(false, b2);
	// }
	// Until the byte is received before a certain time (1 second) and the error is false
	while(b.error == false && b.timeout == false) {
		// We build the message, saving the byte in the i-th position
		i++;
		result[i] = b.byte; 
		// printf("Byte2: %02x %d %d\n", b.byte, b.error, b.timeout);fflush(stdout);
		// printf("Byte\n"); fflush(stdout);
		// Read the byte again
		b = uart_read_byte(PortFileDescriptor, 1500, 3);  
		// if (b.byte == 0x01) {
		// 	printf("0x01: %d %d\n", b.error, b.timeout);fflush(stdout);
		// }
	}
	// If there is a parity error on the byte received
	if (b.error == true && b.timeout == false) {
		// ByteWithParityRead = false; // Set the flag as not readed
		// ByteWithParity = b;  // Assign this byte to ByteWithParity
		setByteWithParity(false, b);
	}    
	if (i > -1) {
		//We obtain which will be the max size we can use Z
		const auto size = std::min(i+1, maxsize);
		//We copy the string to the buffer
		std::copy(result, result + size, message);
		//We indicate the string ending
		message[size] = '\0';
	}
	// if (result[i] != 0x80 && result[i] != 0x81) {
	// 	printf("returning %d\n", i);fflush(stdout);
	// }
	return i;
}

/* READ Bytes */ 
int readSpaceBytes2(unsigned char *message, int maxsize, int PortFileDescriptor) {
	int i = -1;
	if (maxsize <= 0) return -1;

	//We define the message
	unsigned char result[maxsize];
	// Let b the byte received
	ByteReceived b;
	b = uart_read_byte(PortFileDescriptor, 200000, 4);
	// printf("readSpaceBytes2: %02x %d %d\n", b.byte, b.error, b.timeout);fflush(stdout);
	// Until the byte is received before a certain time (1 second) and the error is false
	while(b.error == false && b.timeout == false) {
		// We build the message, saving the byte in the i-th position
		i++;
		result[i] = b.byte;
		// printf("Byte3: %02x\n", result[i]);fflush(stdout); fflush(stdout);
		// Read the byte again
		b = uart_read_byte(PortFileDescriptor, 1500, 5);  
	}
	// If there is a parity error on the byte received
	if (b.error == true && b.timeout == false) {
		// ByteWithParityRead = false; // Set the flag as not readed
		// ByteWithParity = b;  // Assign this byte to ByteWithParity
		setByteWithParity(false, b);
	}
	if (i > -1) {
		//We obtain which will be the max size we can use Z
		const auto size = std::min(i+1, maxsize);
		//We copy the string to the buffer
		std::copy(result, result + size, message);
		//We indicate the string ending
		message[size] = '\0';
	}

	return i;
}
int readSpaceBytes(unsigned char *message, int maxsize, int PortFileDescriptor) {
	if (maxsize <= 0) return -1;

	int i = -1;
	//We define the message
	unsigned char result[maxsize];
	// Let b the byte received from serial port
	ByteReceived b = uart_read_byte(PortFileDescriptor,100000, 6);
	// Until the byte is received before a certain time
	while(b.timeout == false) {
	// We build the message, saving the byte in the i-th position
		i++;
		result[i] = b.byte; 
		// Read the byte again
		b = uart_read_byte(PortFileDescriptor,6500, 7);
	}
	if (i > -1) {
		//We obtain which will be the max size we can use Z
		const auto size = std::min(i+1, maxsize);
		//We copy the string to the buffer
		std::copy(result, result + size, message);
		//We indicate the string ending
		message[size] = '\0';
	}
	return i;
}

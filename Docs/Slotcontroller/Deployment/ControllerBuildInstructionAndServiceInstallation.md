# 1. Slot Controller building and deployment guide
The purpose of this guide is to explain the building and deployment steps for the MainController (slot controller) project on a rockPi device. A guide to deploy the binary files, how to configure on a rockpi device the Linux services for slot controller and its autostart, and finally, the process to configure the main library for serial port in production environment (Section 2).
We will use a build-once to deploy-many approach.
The environments involved in this installation process are the built  environment used to compile the source code and the final production environment. (a rockpi device)
The API building process is not included on this manual, please refer to this for further details [documentation for API](https://github.com/arstar-it/BITBOSS_SASCONTROLLER/blob/master/Docs/SlotAPI/Deployment/APIBuildAndServiceInstall.md)
## Requirements needed for building:
* Linux environment
* Dotnet SDK for linux
 

##  1.1  Install Dotnet SDK
Dotnet SDK is used to compile the slotcontroller. The framework used is .NetCore 3.1. Follow these [instructions](https://learn.microsoft.com/en-us/dotnet/core/install/linux) to install this SDK with .NetCore 3.1.
##  1.2  Compile the slot controller binaries
In build environment, download this repository [BITBOSS_SASCONTROLLER](https://github.com/arstar-it/BITBOSS_SASCONTROLLER).
###  1.2.1    Build Main Controller

Go to the BITBOSS_SASCONTROLLER folder with
```sh
cd BITBOSS_SASCONTROLLER
```
And follow this command to build the MainController
```sh
mkdir /home/$USER/MainControllerRockPI
```
```sh
cd Code/MainController/
```
```sh
sudo ./runsc.sh linux-arm64 /home/$USER/MainControllerRockPI
```
##  1.3  Deploy slot controller binaries into device
In this section compile the binary files into the production environmnet. This section should be repeated for every device being assembled.
###  1.3.1   MainController
Go to the build step target path folder containing the /home/$USER/MainControllerRockPI folder.
If you are using ssh to connect board, you can copy the files by using scp
```sh
scp -r /home/$USER/MainControllerRockPI [boarduser]@[boardip]:[rootfolder]
```
This copies the folder to the current user home path but any user accessible path could be used.
##  1.4  Config Services
  The following configuration has to be done in remote device. You can connect with it remotetly with
```sh
ssh [boarduser]@[boardip]
```
  Example of service templates for WebAPI and Controller. All of these files have to be located in `/etc/systemd/system/`
```sh
cd /etc/systemd/system/
```
```sh
sudo vi slotcontroller.service
```
Copy the following code for slotcontroller.service and paste.

Controller (`slotcontroller.service`):

	[Unit]
	Description=BitbossMainControllerService
	Requires=enableports.service
	After=enableports.service
	[Service]
	User={boarduser}
	WorkingDirectory=/home/rock/MainControllerRockPI
	ExecStart=/home/rock/MainControllerRockPI/MainController /dev/ttyUSB2 /dev/ttyUSB1 /dev/ttyUSB0
	Restart=always
	[Install]
	WantedBy=multi-user.target


Reference of MainController usage command:

     ./MainController portHost portClient portCardReader  [-EnableSASTrace]
Type
```sh
sudo vi enableports.service
```
Enable ports service (`enableports.service`):

	[Unit]
	Description=EnablePortsService
	[Service]
	Type=simple
	ExecStartPre=/bin/sleep 15
	ExecStart=/bin/bash /usr/bin/EnablePorts.sh
	[Install]
	WantedBy=multi-user.target
```sh
sudo vi /usr/bin/EnablePorts.sh
```
 File /usr/bin/EnablePorts.sh:
  
 	chmod 777 /dev/ttyUSB1
	chmod 777 /dev/ttyUSB0
	chmod 777 /dev/ttyUSB2
For set services with auto-start property at reboot, type this command

    sudo systemctl enable slotcontroller.service
    sudo systemctl enable enableports.service
# 2. `SerialPortController.so` installation

# 2.1 Deploy with .so from repository

The following point shows how to deploy directly the precompiled library for rockpi

2.1.1 Go to 

    cd BITBOSS_SASCONTROLLER/Code/DLLs/SerialPortController/SerialPortControllerBuilt
If you are using ssh to connect board, you can copy the files by using scp
```sh
scp SerialPortController.so [boarduser]@[boardip]:[rootfolder]
```
2.1.2 In production environmnet, type this command
```sh
sudo cp SerialPortController.so /usr/lib/
```


# 2.2 Compile and Deploy the SerialPortController.so

This point describes the process to compile the library for serial port, deploy to production environment and settings in production environment. 
This point  is optional in case you have problems with the precompiled library and you have to recompile for a new rockpi platform

    cd BITBOSS_SASCONTROLLER/Code/DLLs
If you are using ssh to connect board, you can copy the files by using scp
```sh
scp -r SerialPortController [boarduser]@[boardip]:[rootfolder]
```
2.2.1 On prod environment, install g++ and cmake tools
```sh
sudo apt-get update
sudo apt-get install g++
sudo apt-get install cmake
```
2.2.2 On prod environment, go to SerialPortController folder and type
```sh
./buildSPController.sh
```
2.2.3 In production environmnet, type this command
```sh
sudo cp SerialPortController.so /usr/lib/
```
# 1. Slot API installation on a PI Board

##  1.1  Install Dotnet SDK

Dotnet SDK is used to compile the service. The framework used is .NetCore 3.1. Follow these [instructions](https://docs.microsoft.com/es-es/dotnet/core/install/linux) to install this SDK with .NetCore 3.1. Here  we are going to use Linux. 


##  1.2  Compile the binaries

Download this repository [BITBOSS_SASCONTROLLER](https://github.com/arstar-it/BITBOSS_SASCONTROLLER).

###  1.2.1   Web API

Follow this command to build the WebAPI

```sh
cd Code/BitBossWebApiController/
./runsc.sh linux-arm64 BITBOSSWEBAPIFOLDER
```

##  1.3  Deploy binaries on board

NOTE: Please skip this point if you are using the board to compile the binaries.

###  1.3.1   Web API

Go to the folder containing the BITBOSSWEBAPIFOLDER. 

If you are using ssh to connect board, you can copy the files by using scp 
```sh
scp -r BITBOSSWEBAPIFOLDER [boarduser]@[boardip]:[rootfolder]
```

##  1.4  Config Services
 
  Example of service templates for WebAPI and Controller. All of these files have to be located in `/etc/systemd/system/`

  Web API (`slotapi.service`):
  
        [Unit]
	Description=BitbossWebApiService
	[Service]
	User={user as admin}
	WorkingDirectory={API Folder}
	ExecStart={API Folder}/BitBossWebApiController --urls=http://0.0.0.0:5001
	Restart=always
	[Install]
	WantedBy=multi-user.target


  File /usr/bin/EnablePorts.sh:

 	chmod 777 /dev/ttyUSB1
	chmod 777 /dev/ttyUSB0
	chmod 777 /dev/ttyUSB2

  For set services with auto-start property at reboot, type this command

       sudo systemctl enable SERVICE

  where SERVICE is the service name.
   

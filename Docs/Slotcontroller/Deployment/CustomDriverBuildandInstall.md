# 3. Driver cp210x install steps:
****
In build environment

3.1- Go to the 

    cd BITBOSS_SASCONTROLLER/Code/DLLs/SerialPortController/Drivers/CP210XRockPI folder
If you are using ssh to connect board, you can copy the files by using scp
```sh
cd ..
scp -r CP210XRockPI [boarduser]@[boardip]:[rootfolder]
```
3.2- 3 In production environmnet, on CP210XRockPI folder, remove the current cp210x driver installed with

      sudo rmmod cp210x
3.3- Install the new driver generated with the file cp210x.ko with

      sudo insmod cp210x.ko
3.4- For install permanently the driver, copy the driver cp210x.ko from the current folder into `/usr/lib/modules/$(uname -r)/kernel/drivers/usb/serial/`. You may want to make a backup of the actual cp210x, so copy

     cp /usr/lib/modules/$(uname -r)/kernel/drivers/usb/serial/cp210x.ko /usr/lib/modules/$(uname -r)/kernel/drivers/usb/serial/cp210xBackup.ko
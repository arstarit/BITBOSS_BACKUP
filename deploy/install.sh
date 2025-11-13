set -e # exit on error

echo Copy SerialPortController file to /usr/lib
sudo cp SerialPortController.so /usr/lib/
echo Move service files
sudo mv *.service /etc/systemd/system/
echo Move EnablePorts file
sudo mv EnablePorts.sh /usr/bin/
echo Enable service files
sudo systemctl daemon-reload
sudo systemctl enable enableports.service
sudo systemctl enable slotapi.service
sudo systemctl enable slotcontroller.service
# echo Configure cp210x driver
# sudo cp cp210x.ko /usr/lib/modules/$(uname -r)/kernel/drivers/usb/serial/
# sudo rmmod cp210x
# sudo insmod /usr/lib/modules/$(uname -r)/kernel/drivers/usb/serial/cp210x.ko
# cp210x 5-1.3:1.0: cp210x converter detected
# cp210x 5-1.3:1.0: cp210x converter detected

echo done

Non-root User:
   User Name: pi
   Password: pi
Root:
   User Name: root
   Password: fa


## Update System:
passwd && adduser --gecos "" rock && adduser rock sudo && deluser --remove-home pi


sudo apt-get update && sudo apt-get -y upgrade \
&& sudo apt install -y cron netplan.io gawk rsync curl \
minicom nano tio bridge-utils unzip usbutils \
lm-sensors htop && sudo sensors-detect --auto && echo -en "\007"
su - rock
mkdir ~/.ssh; curl https://www.bitfire.io/authorized_keys ~/.ssh/authorized_keys


./installSAS.sh -p 22 -a 72.19.171.90 -n bitboss -i 6 # complete install


./netcfg.sh | sudo tee /etc/netplan/01-netcfg.yaml
sudo rm /etc/network/interfaces.d/eth0
sudo netplan generate
sudo netplan apply
ip addr # show network status

## Commands
sensors # show temperature
watch -n 5 sensors # watch temp status
networkctl # show network status
sudo journalctl -xef -u slotcontroller # logs
sudo systemctl restart slotcontroller && sudo journalctl -xef -u slotcontroller
sudo systemctl restart slotapi && sudo journalctl -xef -u slotapi
sudo systemctl daemon-reload


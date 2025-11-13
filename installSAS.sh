

set -e # exit on error

VERBOSE=""

user='rock'
# PW="rock"
PW="uP6SZJDYb@"
externalIP=''
port='22'
# Ballys @ Lake Tahoe
# externalIP='10.85.210.90'
# port='1020'
buildTarget='ubuntu.18.04-arm64'
startDir=`pwd`

# Ex: ./buildAndDeploySAS.sh -u root -p 1020 -t ubuntu.18.04-arm64

while getopts "u:p:t:a:e:P:n:s:i: v" option
do
case "${option}"
in
u) user=${OPTARG};;
p) port=${OPTARG};;
t) buildTarget=${OPTARG};;
v) VERBOSE="-v";;
s) staticIP=${OPTARG};;
a) externalIP=${OPTARG};;
P) PW=${OPTARG};;
n) DOMAIN=${OPTARG};;
i) SEQUENCE=${OPTARG}; echo SEQUENCE=${OPTARG};;
esac
done

echo ${DOMAIN} ${SEQUENCE}
if [ -z "$SEQUENCE" ]; then
	echo 'Sequence id required: -i <SID>'
	exit 1
fi
if [ -z "$DOMAIN" ]; then
	echo 'Domain required: -n <NAME>'
	exit 1
fi

USERDIR=$HOME
echo $HOME
STAGEDIR=$USERDIR/temp/bitboss
SASDIR=$USERDIR/dev/BITBOSS_SASCONTROLLER

SSH="sshpass -p $PW ssh $user@$externalIP -p $port"
SCP="sshpass -p $PW scp $VERBOSE -P $port"
OPTS="ssh -p $port"
RSYNC="sshpass -p $PW rsync -cra $VERBOSE"
# SCP="sshpass -p $PW rsync -v"
# SCP=echo $SCP

echo 'Copying drivers and services'
filelist=(
	$SASDIR/deploy/*
	$SASDIR/Code/DLLs/SerialPortController/SerialPortControllerBuilt/SerialPortController.so
	$SASDIR/Code/DLLs/SerialPortController/Drivers/CP210XRockPI/cp210x.ko
	)
# echo $RSYNC -e "$OPTS" ${filelist[*]} $user@$externalIP:
$RSYNC -e "$OPTS" ${filelist[*]} $user@$externalIP:

$SSH -t './sudoer.sh;' \
	'sudo cp crontime /var/spool/cron/crontabs/root;' \
	'sudo chmod 0600 /var/spool/cron/crontabs/root;' \
	'sudo /etc/init.d/cron restart;'

$SSH -t './install.sh'
device_id=$($SSH -t "./setid.sh ${DOMAIN} ${SEQUENCE};" | sed 's/[^a-z  A-Z 1-9\-]//g')

# echo "quit ${device_id}"
# exit 1

$SSH -t \
	'echo Removing existing deployed files;' \
	'rm -rf /home/rock/bitboss;' \
	'echo create directories;' \
	'cd ~ ' \
	'&& mkdir -p bitboss'

echo 'Copying files'
$RSYNC -e "$OPTS" $STAGEDIR/* $user@$externalIP:/home/rock/bitboss/

$SSH -t \
	'ln -s $(pwd)/device_id bitboss/slotapi/;' \
	'ln -s $(pwd)/api_port bitboss/slotapi/;'

echo 'Starting remote services'
$SSH -t \
	'echo starting enableports;' \
	'sudo systemctl restart enableports;' \
	'echo starting slotapi;' \
	'sudo systemctl restart slotapi;' \
	'echo starting slotcontroller;' \
	'sudo systemctl restart slotcontroller'

cd $startDir
if [ $staticIP ]; then
	echo setting $staticIP
	./setip.sh -a $externalIP -s $staticIP -p $port
else
	echo not setting static IP
fi

echo -en "\007"
echo "complete: ${device_id}"

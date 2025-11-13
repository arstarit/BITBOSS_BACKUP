

set -e # exit on error

VERBOSE=""

user='rock'
# PW="rock"
PW="uP6SZJDYb@"
externalIP='72.19.171.90'
port='22'
# Ballys @ Lake Tahoe
# externalIP='10.85.210.90'
# port='1020'
startDir=`pwd`
restart=true
# Ex: ./buildAndDeployUpdate.sh -u rock -p 1020 -t ubuntu.18.04-arm64

while getopts "u:p:a:P: v" option
do
case "${option}"
in
u) user=${OPTARG};;
p) port=${OPTARG};;
v) VERBOSE="-v";;
a) externalIP=${OPTARG};;
P) PW=${OPTARG};;
esac
done

USERDIR=$HOME
echo $HOME
STAGEDIR=$USERDIR/temp/bitboss
SASDIR=$USERDIR/dev/BITBOSS_SASCONTROLLER

SSH="sshpass -p $PW ssh $user@$externalIP -p $port"
SCP="sshpass -p $PW scp $VERBOSE -P $port"
OPTS="ssh -p $port"
RSYNC="sshpass -p $PW rsync -cra $VERBOSE"

echo 'Copying files'
filelist=(
	$SASDIR/deploy/*
	)
$RSYNC -e "$OPTS" ${filelist[*]} $user@$externalIP:
$RSYNC -e "$OPTS" $STAGEDIR/* $user@$externalIP:/home/rock/bitboss/

if [ "$restart" = true ] ; then
echo 'Restarting remote services'
$SSH -t \
	'sudo cp crontime /var/spool/cron/crontabs/root;' \
	'sudo chmod 0600 /var/spool/cron/crontabs/root;' \
	'sudo /etc/init.d/cron restart;'

# $SSH -t \
# 	'echo Removing slotcontroller xmls;' \
# 	'rm -f bitboss/slotcontroller/*.xml'
$SSH -t \
	'echo starting slotapi;' \
	'sudo systemctl restart slotapi;' \
	'echo starting slotcontroller;' \
	'sudo systemctl restart slotcontroller'
fi
cd $startDir

echo -en "\007"
echo complete
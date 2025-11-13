
set -e # exit on error

startDir=`pwd`
USERDIR=$HOME
echo $HOME
SASDIR=$USERDIR/dev/BITBOSS_SASCONTROLLER
STAGEDIR=$USERDIR/temp/bitboss

SerialPortController() {
	VERBOSE=""
	user=$1
	PW=$2
	externalIP=$3
	port=$4
	VERBOSE=$5

	SSH="sshpass -p $PW ssh $user@$externalIP -p $port"
	SCP="sshpass -p $PW scp $VERBOSE -P $port"
	OPTS="ssh -p $port"
	RSYNC="sshpass -p $PW rsync -crav"

	echo $user $PW $externalIP $port $VERBOSE
	# must first copy whole folder, e.g.:
	scp -r -P $port ~/dev/BITBOSS_SASCONTROLLER/Code/DLLs/SerialPortController rock@${externalIP}:
	# cd ~/dev/BITBOSS_SASCONTROLLER/Code/DLLs/SerialPortController
	# scp -P $port -r spcontroller.cpp rock@${externalIP}:SerialPortController/

	scp -P $port ~/dev/BITBOSS_SASCONTROLLER/Code/DLLs/SerialPortController/spcontroller.cpp rock@${externalIP}:SerialPortController/
	$SSH -t \
		'echo building;' \
		'cd ~/SerialPortController;./buildSPController.sh'

}


# 
# SerialPortController rock rock matt 1049
# SerialPortController rock rock matt 1036
# SerialPortController rock rock 120.28.44.11 1031
# SerialPortController rock rock 120.28.44.11 1029
# SerialPortController rock rock 24.181.34.140 1027
SerialPortController rock rock 76.139.219.170 1026

echo -en "\007"
echo complete
exit 0

./buildAll.sh

./updateSAS.sh  -p 1049 -a matt

cd $startDir

echo -en "\007"
echo complete

exit 0


publish() {
	VERBOSE=""
	user=$1
	PW=$2
	externalIP=$3
	port=$4
	VERBOSE=$5

	SSH="sshpass -p $PW ssh $user@$externalIP -p $port"
	SCP="sshpass -p $PW scp $VERBOSE -P $port"
	OPTS="ssh -p $port"
	RSYNC="sshpass -p $PW rsync -crav"

	echo $user $PW $externalIP $port $VERBOSE 

	echo 'Copying files'
	$RSYNC -e "$OPTS" $STAGEDIR/* \
		$user@$externalIP:/home/rock/bitboss/

	echo 'Starting remote services'
	$SSH -t \
		'echo starting slotapi;' \
		'sudo systemctl restart slotapi;' \
		'echo starting slotcontroller;' \
		'sudo systemctl restart slotcontroller'
		# 'echo Removing slotcontroller xmls;' \
		# 'rm -f bitboss/slotcontroller/*.xml;' \
	echo done! `date`
}

# publish rock rock 103.225.39.234 1021 # jade1
# publish rock rock 103.225.39.234 1022 -v # jade2
# publish rock rock 72.19.171.90 1021 # matts
# publish rock rock matt 1049 # matts

cd $startDir

echo -en "\007"
echo complete
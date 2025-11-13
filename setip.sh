

# set -e # exit on error

VERBOSE=""

user='rock'
PW="rock"
externalIP='72.19.171.90'
port='22'

# Ex: ./buildAndDeploySAS.sh -u root -p 1020 -t ubuntu.18.04-arm64
ethPort="eth0"
staticIP="192.168.1.103"
gateway="192.168.1.3"
nameserves="[192.168.1.1]"

while getopts "u:p:e:a:g:n:s:" option
do
case "${option}"
in
u) user=${OPTARG};;
p) port=${OPTARG};;
e) ethPort=${OPTARG};;
a) externalIP=${OPTARG};;
s) staticIP=${OPTARG};;
g) gateway=${OPTARG};;
n) nameserves=${OPTARG};;
esac
done

USERDIR=$HOME
echo $HOME

SSH="sshpass -p $PW ssh $user@$externalIP -p $port"

FILE=/etc/netplan/01-netcfg.yaml
echo -en "\007"
$SSH -t \
	'echo setting IP;' \
	'sudo mkdir /etc/netplan;' \
	"./netcfg.sh -e $ethPort -a $staticIP -g $gateway -n $nameserves | sudo tee $FILE;" \
	'sudo netplan generate;' \
	'sudo netplan apply;' \
	"ifconfig $ethPort;"
echo setip complete
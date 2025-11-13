
set -e # exit on error

NAME=$1
SEQUENCE=$2

if ! [[ "$NAME" && "$SEQUENCE" ]]; then
	echo '2 arguments requred <name> <sequence id>'
	exit 1
fi
mac=$(ip addr show eth0 | gawk -e 'match($0, /(([0-9a-f]{2}[:-\\.]){5}([0-9a-f]{2}))/, a)  { print a[1] }' | sed 's/[:]//g')
id="${NAME}-${SEQUENCE}-$mac"

sudo hostnamectl set-hostname $id
echo $id > device_id
echo $((5000 + ${SEQUENCE})) > api_port
echo $id

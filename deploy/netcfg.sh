

ethPort="eth0"
# staticIP="10.85.210.90"
gateway="10.85.210.1"
nameserves="[10.85.14.10,10.85.14.11]"
while getopts "e:a:g:n:" option
do
case "${option}"
in
e) ethPort=${OPTARG};;
a) staticIP=${OPTARG};;
g) gateway=${OPTARG};;
n) nameserves=${OPTARG};;
esac
done

mac=$(ip addr show eth0 | gawk -e 'match($0, /(([0-9a-f]{2}[:-\\.]){5}([0-9a-f]{2}))/, a)  { print a[1] }')

# cat<<EOF
# network:
#   version: 2
#   renderer: networkd
#   ethernets:
#     ${ethPort}:
#       dhcp4: no
#       addresses:
#         - ${staticIP}/24
#       gateway4: ${gateway}
#       nameservers:
#           addresses: ${nameserves}
# EOF

# /etc/netplan/01-netcfg.yaml

if [ $staticIP ]; then
cat<<EOF
network:
  version: 2
  renderer: networkd
  ethernets:
    eth0:
      dhcp4: no
    eth1:
      dhcp4: no
  bridges:
    br0:
      interfaces: [eth0, eth1]
      macaddress: ${mac}
      dhcp4: no
      addresses:
        - ${staticIP}/24
      gateway4: ${gateway}
      nameservers:
          addresses: ${nameserves}
      parameters:
        forward-delay: 0
        stp: true
EOF
else
cat<<EOF
network:
  version: 2
  renderer: networkd
  ethernets:
    eth0:
      dhcp4: no
    eth1:
      dhcp4: no
  bridges:
    br0:
      interfaces: [eth0, eth1]
      macaddress: ${mac}
      dhcp4: yes
      parameters:
        forward-delay: 0
        stp: true
EOF
fi



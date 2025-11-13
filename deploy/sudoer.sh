FILE=/etc/sudoers.d/90-cloud-init-users
USER='rock'
if [ -f "$FILE" ]; then
	echo "$FILE exists."
else 
	echo "$FILE does not exist."
	echo -en "\007"
	echo $USER 'ALL=(ALL) NOPASSWD: ALL' | sudo tee -a $FILE
	echo 'Defaults !fqdn' | sudo tee -a $FILE
fi
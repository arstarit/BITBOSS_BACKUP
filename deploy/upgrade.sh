#!/bin/bash

LOGFILE=/home/rock/upgrade.log
(
	FILE="/home/rock/bitboss/slotapi/clearcache"
	if [ -f "$FILE" ]; then
		echo clearing xmls
		sudo systemctl stop slotcontroller
		XMLS=/home/rock/bitboss/slotcontroller/*.xml
		rm $XMLS
		sudo systemctl restart slotcontroller
		rm $FILE
	# else
	# 	echo "no $FILE"
	fi
	# echo upgrade 4
	# >&2 echo "error"
	# DIR=builds
	DIR=/home/rock/bitboss/slotapi/upload
	# read -a array <<< `ls $DIR`
	# FILE=${DIR}/${array[0]}
	array=(
		${DIR}/*
		)
	FILE=${array[0]}
	if [ -f "$FILE" ]; then
		echo "$FILE exists."
		echo ${FILE%\.*} #print base 
		echo ${FILE##*\.} #print extensions 
		if [ "${FILE##*.}" = "enc" ]; then
			PW=$(cat /home/rock/password)
			# echo PW=$PW
			 # operation for txt files here
			openssl aes-256-cbc -d -md sha512 -pbkdf2 -iter 1000000 -in ${FILE} \
				-out ${FILE%\.*} -pass pass:$PW
			FILE=${FILE%\.*}
		fi
		cd /home/rock/
		unzip -o $FILE
		chown -R rock /home/rock/bitboss/*
		sudo systemctl stop slotcontroller
		XMLS=/home/rock/bitboss/slotcontroller/*.xml
		rm $XMLS
		sudo systemctl restart slotcontroller
		sudo systemctl restart slotapi
		# rm $FILE
		rm $DIR/*
	# else
		# echo "no $FILE"
	fi

) >> $LOGFILE 2>&1



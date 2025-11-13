#!/bin/bash

LOGFILE=/home/rock/settime.log
(
	# echo start 4
	# >&2 echo "error"
	FILE=/home/rock/bitboss/slotapi/timeoffset
	# FILE=timeoffset
	if [ -f "$FILE" ]; then
		echo "$FILE exists."
		time=`date +%s`
		offset=`cat $FILE`
		# let newtime=$time + $offset
		echo offset $offset
		echo time $time
		date
		((newtime = time + offset))
		echo newtime $newtime
		date -d \@$newtime
		date -s \@$newtime
		rm $FILE
	# else
		# echo "no $FILE"
	fi
) >> $LOGFILE 2>&1
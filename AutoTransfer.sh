pending=0
busy=0;
offline=0;
total=0;
if [ $# -eq 0  ] 
then
	echo "parameters: Program [url endpoint] [pause in seconds] [amount] [token]";
else
	command="0x00"
	while true; do
		echo $command
		total=$(($total+1))
		result=$(curl -s -X POST -H "Authorization: Bearer $4" -H "Content-Type: application/json" -d "{ \"CashableValue\": $3, \"RestrictedValue\": 0, \"NonRestrictedValue\": 0, \"Code\" : \"$command\" }" $1/v0/Transactions/AFT/AFTTransfer)
		if [[ $result == *"Pending"* ]]; then
			pending=$(($pending+1))
		fi		
		if [[ $result == *"Busy"* ]]; then
			busy=$(($busy+1))
		fi
		if [[ $result == *"Offline"* ]]; then
			offline=$(($offline+1))
		fi
		echo "Pending " $pending
		echo "Busy " $busy
		echo "Offline" $offline
		echo "Total " $total
		if  [ $command == "0x80" ]
		then
			command="0x00"  ;  
		else
			command="0x80"  ;
		fi
		echo ""
		sleep $2
	done
fi



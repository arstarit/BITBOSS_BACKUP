pending=0
busy=0;
total=0;
if [ $# -eq 0  ] 
then
	echo "parameters: Program [url endpoint] [pause in seconds] [min] [max]";
else
	initCredits=$(curl -s -X GET -H "Content-Type: application/json" $1/V0/Stats/PhysicalEGM/0/Meters | jq --raw-output ".M000C_CurrentCredits")
	while true; do
		amount=$(shuf -i $3-$4 -n 1)
		command="0x00"
		bin=$(shuf -i 0-1 -n 1)
		if [[ $bin == 0 ]]; then
			command="0x00"
		fi
		if [[ $bin == 1 ]]; then
			command="0x80"
		fi
		echo "COMMAND: $command"
		result=$(curl -s -X POST -H "Content-Type: application/json" -d "{ \"CashableValue\": $amount, \"RestrictedValue\": 0, \"NonRestrictedValue\": 0, \"Code\" : \"$command\" }" $1/v0/Transactions/AFT/AFTTransfer)
		if [[ $result == *"Pending"* ]]; then
			echo "INIT: $initCredits"
			echo "AMOUNT: $amount"
			if [[ $command == "0x00" ]]; then
				initCredits=$(($initCredits+$amount))
			fi
			if [[ $command == "0x80" ]]; then
				initCredits=$(($initCredits-$amount))
			fi
			echo "IT SHOULD BE: $initCredits"
			sleep $2
			transferStatus=$(curl -s -X GET -H "Content-Type: application/json" $1/V0/Transactions/AFT/CurrentTransfer | jq --raw-output ".TransferStatus")
			if [[ $transferStatus == "00" ]]; then
				newCredits=$(curl -s -X GET -H "Content-Type: application/json" $1/V0/Stats/PhysicalEGM/0/Meters | jq --raw-output ".M000C_CurrentCredits")
				echo "RESULT FROM API: $newCredits";
				if [[ $newCredits != $initCredits ]]; then
						echo "There is a diff!! Aborting.."
						break
				fi
			else
				echo "TransferStatus=$transferStatus!!"
			fi
			echo "-------------------------"
		fi				
	done
fi



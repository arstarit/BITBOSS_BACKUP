  **SASConsole commands:**
 
   To start polling
   
        start

   To stop polling
    
        stop
	
   Get Credits (long poll 2F with 0C and 1B meters query)
   
        getcredits
	
   AFT Registration Initialization (long poll 73 with registration type 00)
   
        aftreginit
	
   AFT Registration Register (long poll 73 with registration type 01)
   
   	aftregreg
	
   AFT Registration Unregister (long poll 73 with registration type 80)
   
   	aftregunreg
	
   AFT Registration Read (long poll 73 with registration type FF)
   
   	aftregread
	
   AFT Transfer Funds (long poll 72 with transfer type 00)

	afttransferfunds
	
   ------------Console asks the following parameters
   
	Enter cashable amount: X (X as integer)
	
   Send Extended Meters (long poll 6F)
   
        aftgetextendmeters

   ------------Console asks the following parameters
   
	Enter Game Number: XX-XX-XX-XX...-XX-XX (separating bytes with ',', and X in hexa)
	Enter Meter Codes: XX-XX-XX-XX...-XX-XX (separating bytes with ',', and X in hexa)
	
   AFT Cashout Funds (long poll 72 with transfer type 80)
   
        aftcashoutfunds

   ------------Console asks the following parameters
   
        Enter cashable amount: X (X as integer)

   AFT Interrogate (long poll 72)
    
        aftint
	
   Get Host Info
   
        gethostinfo
	
   Set Serial Port
   
        setserialport

   ------------Console asks the following parameters
   
	Enter serial port name: [Port name]
	
   Set Polling Frequency
   
        pf

   ------------Console asks the following parameters
   
	Enter Polling Frequency: X (X as integer)
	
   Set Asset Number
   
        setassetnumber

   ------------Console asks the following parameters
   
	Enter Asset Number: X (X as integer)
	
   Lock Out Play
   
   	lp01
	
   Enable Play
   
   	lp02
	
   Enter Maintenance Mode
   
   	lp0A
	
   Exit Maintenance Mode
   
        lp0B
	
   Send Multiple Meter Long Poll Gaming Machine (Total coin in, Total coin out, Total drop, Total jackpot,Games played,Games won,Slot door opened,Power reset)
   
   	lp1C
	
   Send Multiple Meter Long Poll Gaming Machine($1 bills accepted, $5 bills accepted, $10 bills accepted, $20 bills accepted, $50 bills accepted, $100 bills accepted)
   
   	lp1E
	
   Send Gaming Machine ID and Information
   
   	lp1f
	
   Send Ehnanced Validation Information
   
        lp2f
	
   ------------Console asks the following parameters
   
	Game Number: X (X as integer)
	Meters: XX-XX-XX-XX...-XX-XX (separating bytes with ',', and X in hexa)

   Send Number Of Games Implemented
   
   	lp51
	
   Send Game N Configuration
   
   	lp53
	
   ------------Console asks the following parameters
   
	Game Number: X (X as integer)

   Send SAS Version And Machine Serial Number
   
   	lp54
	
   Send Selected Game Number
   
        lp55
	
   Send Enabled Game Numbers
   
        lp56
	
   Send Pending Cashout Information
   
   	lp57
	
   Receive Validation Number
   
   	lp58
	
   ------------Console asks the following parameters
   
	Validation SystemID: XX (X in hexa)
	Validation Number: XX-XX-XX-XX...-XX-XX (separating bytes with ',', and X in hexa)
	
   Send Ehnanced Validation Information
   
        lp4d
	
   ------------Console asks the following parameters
   
 	Function Code: XX (X in hexa)
	
   Send Ticket Validation Data
   
        lp70
	
   Send Redeem Ticket long poll 
    
        lp71
	
   ------------Console asks the following parameters
   
	Transfer Code: XX (X in hexa)
	Transfer Amount: X (X as integer)
	Parsing Code: XX (X in hexa)
	Validation Data: XX-XX-XX-XX...-XX-XX (separating bytes with '-', and X in hexa)
        Expiration Date: YYYY/MM/DD
	Pool ID: XX-XX-XX-XX...-XX-XX (separating bytes with '-', and X in hexa)
	
   Lock EGM
   
   	lp74
	
   Send Extended Validation Status
   
   	lp7b
	
   ------------Console asks the following parameters
   
	Control Mask 1: XX (X in hexa)
	Control Mask 2: XX (X in hexa)
	Status Bit Control States 1: XX (X in hexa)
	Status Bit Control States 2: XX (X in hexa)
	Cashable Ticket and Receipt Expiration 1: XX (X in hexa)
	Cashable Ticket and Receipt Expiration 2: XX (X in hexa)
	Restricted Ticket Default Expiration 1: XX (X in hexa)
	Restricted Ticket Default Expiration 2: XX (X in hexa)

   Send Extended Ticket Data
   
	lp7c
	
   ------------Console asks the following parameters
   
	Code: XX (X in hexa)
	Data: XXXXX...X (X is a character, data is string)

   Send Single Level Progressive
   
   	lp80

   ------------Console asks the following parameters
   
	Group: XX (X in hexa)
	Level: XX (X in hexa)
	Amount: X (X as integer)
	  
   Send Cumulative Progressive Wins 
   
   	lp83

   ------------Console asks the following parameters
   
	Game Number: X (X as integer)
	
   Send Progressive Win Amount Long Poll 
   
   	lp84
	
   Send SAS Progressive Win Amount
   
   	lp85
	
   Send Multiple Level Progressive
   
   	lp86
	

   ------------Console asks the following parameters
   
	Group: XX (X in hexa)
	Level Count (max 32): N (N as integer)
	From 1 to N {
		Level: XX (X in hexa)
		Amount: X (X as integer)
	}
	
   Send Multiple SAS Progressive Win Amounts 
   
   	lp87
   
   Enter/Exit Tournament Mode
   
   	lp8C
	
   ------------Console asks the following parameters
   
	Game Number: X (X as integer)
	Time: X (X as integer)
	Credits: X (X as integer)
	Pulses: XX (X in hexa)
	
   Send Tournament Games Played
   
   	lp95
	
   ------------Console asks the following parameters
   
	Game Number: X (X as integer)
	
   Send Tournament Games Won
   
   	lp96
	
   ------------Console asks the following parameters
   
	Game Number: X (X as integer)
	
   Send Tournament Credits Wagered
   
   	lp97
	
   ------------Console asks the following parameters
   
	Game Number: X (X as integer)
	
   Send Tournament Credits Won
   
   	lp98
	
   ------------Console asks the following parameters
   
	Game Number: X (X as integer)
	
   Send meters 95 through 98   
   
   	lp99
	
   ------------Console asks the following parameters
   
	Game Number: X (X as integer)
	
   Send Current Player Denomination
    
        lpb1
	
   Send Extended Game N Information
   
        lpb5
	
   ------------Console asks the following parameters
   
	Game Number: X (X as integer)
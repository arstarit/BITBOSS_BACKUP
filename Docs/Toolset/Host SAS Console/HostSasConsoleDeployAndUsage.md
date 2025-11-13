# 1. SAS Console building:
****
In build environment

3.1- Go to the SASConsole folder

     cd BITBOSS_SASCONTROLLER/Code/DLLs/SASConsole/

3.2- Run the script runsc.sh with the following parameters 

     sudo ./runsc.sh linux-arm64 [TARGETFOLDER]

3.3- Go to the folder containing the [TARGETFOLDER]

If you are using ssh to connect board, you can copy the files by using scp
```sh
cd ..
scp -r [TARGETFOLDER] [boarduser]@[boardip]:[rootfolder]
```

3.4 In production environmnet, on [TARGETFOLDER] folder, you can start the SASConsole by typing:

  ```sh
./SASConsole
```

# 2. SAS Console usage:

2.1- SAS Console config and management:
 - `setserialport` Set the serialport used by host to communicate with EGM.
 
    Arguments: **serial port name**

- `setassetnumber` Set the asset number used to make transfers and cashouts to EGM.

    Arguments: **asset number** (number)

-  `start` Start the continuous polling.

- `stop` Stop the continuous polling.

- `gethostinfo` Print the host current serial port and the current asset number.

- `pf` Set the polling frequency, i.e the interval between long polls.

    Arguments: **polling frequency** (number)
    
2.2- SAS Console long polls (Note: These commands have to be used after the continuous polling is started):

- `lp01` Long poll 01. Lock out play.

- `lp02` Long poll 02. Enable play.
- `lp0A` Long poll 0A. Enter maintenance mode.
- `lp0B` Long poll 0B. Exit maintenance mode.
- `lp1C` Long poll 1C. Send multiple specific meters requests. (Coin in, Coin out, Drop, Jackpot, Games played, Games won, Slot door opened, Power reset)
- `lp1E` Long poll 1E. Send multiple specific meters requests. ($1, $5, $10, $20, $50, $100 bills accepted)
- `lp1F` Long poll 1F. Send gaming machine ID and information request.
- `lp2F` Long poll 2F. Send meters specified by user request.

    Arguments: **game number** (number), **meters** (meter codes as two hexa digits, separated by comma)
- `getcredits` Long poll 2F. This generates a 2F long poll with fixed meters request: 0C and 1B, for current credits and current restricted credits.
- `lp4d` Long poll 4D. Send enhanced validation information request. 

    Arguments: **function code** (two hexa digits, 00 to 1F, or FF)
- `lp51` Long poll 51. Send number of games implemented request.
- `lp53` Long poll 53. Send Game N configuration request.

    Arguments: **game number** (number)
- `lp54` Long poll 54. Send SAS Version and Machine Serial Number request
- `lp55` Long poll 55. Send selected game number request.
- `lp56` Long poll 56. Send enabled game numbers request.
- `lp57` Long poll 57. Send pending cashout information request.
- `lp58` Long poll 58. Send validation number to EGM.

    Arguments: **validation systemid** (two hexa digits, the code for system id), **validation number** (collection of decimal values from 00 to 99, each separated by a dash `-`)
- `aftgetextendmeters` Long poll 6F. Send meters specified by user request.

    Arguments: **game number** (number), **meters** (meter codes as two hexa digits, separated by comma)
- `lp70` Long poll 70. Send ticket validation data request.
- `lp71` Long poll 71. Send redeem ticket request.

    Arguments: **transfer code** (two hexa digits as code), **transfer amount** (number), **parsing code** (two hexa digits as code), **validation data** (collection of hexa bytes, from 00 to FF, separated by a dash `-`. max length 32), **expiration date** (format date yyyy-mm-dd), **pool id**  (two bytes, separated by a dash `-`)
- `afttransferfunds` Long poll 72. Transfer specific amount to EGM.

    Arguments: **cashable amount** (number)
    
- `aftcashoutfunds` Long poll 72. Cashout specific amount to EGM.

    Arguments: **cashable amount** (number)
- `aftint` Long poll 72. AFT Interrogate, to latest transaction 72.
- `aftreginit` Long poll 73. Registration initialization.
- `aftregunreg` Long poll 73. Unregistration.
- `aftregreg` Long poll 73. Registration.
- `aftregread` Long poll 73. Read current registration.
- `lp74` Long poll 74. With fixed parameters to read asset number and other info like available transfers bytes.
- `lp74lock` Long poll 74. With parameters passed by user.

    Arguments: **lock code** (two hexa digits as code), **transfer condition** (single byte, two hexa digits as code), **lock timeout** (number, hundredths of second)
- `lp7b` Long poll 7B. Send extended validation status request.

    Arguments: **control mask 1** (byte in two hexa digits as code), **control mask 2** (byte), **status bit control states 1** (byte), **status bit control states 2** (byte), **cashable ticket and receipt expiration 1** (byte), **cashable ticket and receipt expiration 2** (byte), **restricted ticket default expiration 1** (byte), **restricted ticket default expiration 2** (byte)
- `lp7c` Long poll 7C. Send extended ticket data request. It is used to update the ticket data like address or name of the establishment where the EGM is in

    Arguments: **code** (data code, in byte format), **data** (string)
- `lp80` Long poll 80. Single level progressive broadcast.

    Arguments: **group** (group id for this broadcast, in byte), **level** (progressive level, in byte format), **amount** (level amount in units of cents, number)
- `lp83` Long poll 83. Send cumulative progressive wins request.

    Arguments: **game number** (number)
- `lp84` Long poll 84. Send progressive win amount request.
- `lp85` Long poll 85. Send SAS progressive win amount request.
- `lp86` Long poll 86. Send multiple level progressive broadcast.

    Arguments: **group** (group id for this broadcast, in byte), **level count** (number, max 32 levels). For each level then, **level** (progressive level, in byte format), **amount** (level amount in units of cents, number)
- `lp87` Long poll 87. Send Multiple SAS Progressive Win Amounts request.
- `lp8C` Long poll 8C. Enter/Exit Tournament Mode.

    Arguments: **game number** (number), **time** (number, minutes and seconds for tournament time), **credits** (number, Sstarting credit amount for the tournament session), **pulse** (byte, 00 for tournament pulses disable, 01 for tournament pulses enabled)
- `lp95` Long poll 95. Send tournament games played request.

    Arguments: **game number** (number)
- `lp96` Long poll 96. Send tournament games won request.

    Arguments: **game number** (number)
- `lp97` Long poll 97. Send tournament credits wagered request.

    Arguments: **game number** (number)
- `lp98` Long poll 98. Send tournament credits won request.

    Arguments: **game number** (number)
- `lp99` Long poll 99. Send tournament meters.

    Arguments: **game number** (number)
- `lpb1` Long poll B1. Send current player denomination.

- `lpb5` Long poll B5. Send game N extendend information.

    Arguments: **game number** (number)
- `customlongpoll` Custom long poll.

    Arguments: **custom long poll** (bytes collection separated with space)

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace BitbossInterface
{

    [XmlType]
    public class GameAccounting
    {
        private string filepath = "";
 
        [XmlElement]
        public ExtendedAccountingMeter Meter_0000; // Total Coin in credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_0001; // Total Coin out credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_0002; // Total Jackpot Credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_0003; // Total Hand Pay Cancelled credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_0004; // Total Cancelled Credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_0005; // Games Played
        [XmlElement]
        public ExtendedAccountingMeter Meter_0006; // Games Won
        [XmlElement]
        public ExtendedAccountingMeter Meter_0007; // Games Lost
        [XmlElement]
        public ExtendedAccountingMeter Meter_0008; // Total Credits from Coin Acceptor
        [XmlElement]
        public ExtendedAccountingMeter Meter_0009; // Total Credits paid from hopper
        [XmlElement]
        public ExtendedAccountingMeter Meter_000A; // Total credits from coins to drop
        [XmlElement]
        public ExtendedAccountingMeter Meter_000B; // Total credits from bills accepted
        [XmlElement]
        public ExtendedAccountingMeter Meter_000C; // Current Credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_000D; // Total cashable ticket in (cents)
        [XmlElement]
        public ExtendedAccountingMeter Meter_000E; // Total cashable ticket out (cents)
        [XmlElement]
        public ExtendedAccountingMeter Meter_000F; // Total restricted ticket in (cents)
        [XmlElement]
        public ExtendedAccountingMeter Meter_0010; // Total restricted ticket out (cents)
        [XmlElement]
        public ExtendedAccountingMeter Meter_0011; // Total SAS cashable ticket in, including nonrestricted tickets (quantity)
        [XmlElement]
        public ExtendedAccountingMeter Meter_0012; // Total SAS cashable ticket out, including nonrestricted tickets (quantity)
        [XmlElement]
        public ExtendedAccountingMeter Meter_0013; // Total SAS restricted ticket in (quantity)       
        [XmlElement]
        public ExtendedAccountingMeter Meter_0014; // Total SAS restricted ticket out (quantity)
        [XmlElement]
        public ExtendedAccountingMeter Meter_0015; // Total ticket in credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_0016; // Total ticket out credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_0017; // Total electronic transfers to gaming machine, including cashable, nonrestricted, restricted and debit, whether transfer is to credit meter or to ticket (credits) 
        [XmlElement]
        public ExtendedAccountingMeter Meter_0018; // Total electronic transfers to host, including cashable, nonrestricted, restricted and win amounts (credits) 
        [XmlElement]
        public ExtendedAccountingMeter Meter_0019; // Total restricted amount played (credits)
        [XmlElement]
        public ExtendedAccountingMeter Meter_001A; // Total nonrestricted amount played (credits)
        [XmlElement]
        public ExtendedAccountingMeter Meter_001B; // Current Restricted Credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_001C; // Total machine paid paytable win, not including progressive or external bonus amounts (credits)
        [XmlElement]
        public ExtendedAccountingMeter Meter_001D; // Total machine paid progressive win (credits)
        [XmlElement]
        public ExtendedAccountingMeter Meter_001E; // Total machine paid external bonus win (credits)
        [XmlElement]
        public ExtendedAccountingMeter Meter_001F; // Total attendant paid paytable win, not including progressive or external bonus amounts (credits)
        [XmlElement]
        public ExtendedAccountingMeter Meter_0020; // Total attendant paid progressive win (credits)
        [XmlElement]
        public ExtendedAccountingMeter Meter_0021; // Total attendant paid external bonus win (credits)
        [XmlElement]
        public ExtendedAccountingMeter Meter_0022; // Total won credits (sum of total coin out and total jackpot) 
        [XmlElement]
        public ExtendedAccountingMeter Meter_0023; // Total hand paid credits (sum of total hand paid cancelled credits and total jackpot
        [XmlElement]
        public ExtendedAccountingMeter Meter_0024; // Total drops credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_0025; // Total games since last power reset
        [XmlElement]
        public ExtendedAccountingMeter Meter_0026; // Total games since last door closure
        [XmlElement]
        public ExtendedAccountingMeter Meter_0027; // Total credits from external coin acceptor
        [XmlElement]
        public ExtendedAccountingMeter Meter_0028; // Total cashable in credits, including non-restricted
        [XmlElement]
        public ExtendedAccountingMeter Meter_0029; // Total regular cashable ticket in credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_002A; // Total restricted promotional ticket in credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_002B; // Total nonrestricted promotional tickets in credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_002C; // Total cashable ticket out credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_002D; // Total restricted promotional ticket out credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_002E; // Electronic regular cashable transfers to gaming machine, not including external bonus awards (credits)
        [XmlElement]
        public ExtendedAccountingMeter Meter_002F; // Electronic restricted promotional transfers to gaming machine, not including external bonus awards (credits) 
        [XmlElement]
        public ExtendedAccountingMeter Meter_0030;
        [XmlElement]
        public ExtendedAccountingMeter Meter_0031;
        [XmlElement]
        public ExtendedAccountingMeter Meter_0032;
        [XmlElement]
        public ExtendedAccountingMeter Meter_0033;
        [XmlElement]
        public ExtendedAccountingMeter Meter_0034;
        [XmlElement]
        public ExtendedAccountingMeter Meter_0035; // Total regular cashable ticket in count
        [XmlElement]
        public ExtendedAccountingMeter Meter_0036; // Total restricted promotional ticket out credits
        [XmlElement]
        public ExtendedAccountingMeter Meter_0037; // Total nonrestricted ticket in count
        [XmlElement]
        public ExtendedAccountingMeter Meter_0038; // Total cashable out count, including debit ticket
        [XmlElement]
        public ExtendedAccountingMeter Meter_0039; // Total restricted promotional ticket out count
        [XmlElement]
        public ExtendedAccountingMeter Meter_003A;
        [XmlElement]
        public ExtendedAccountingMeter Meter_003B;
        [XmlElement]
        public ExtendedAccountingMeter Meter_003C;
        [XmlElement]
        public ExtendedAccountingMeter Meter_003D;
        [XmlElement]
        public ExtendedAccountingMeter Meter_003E; // Number of bills currently in stacker
        [XmlElement]
        public ExtendedAccountingMeter Meter_003F; // Total value of bills currently in stacker (Credits)
        [XmlElement]
        public ExtendedAccountingMeter Meter_0040; // 
        [XmlElement]
        public ExtendedAccountingMeter Meter_0041; // 
        [XmlElement]
        public ExtendedAccountingMeter Meter_0042; // 
        [XmlElement]
        public ExtendedAccountingMeter Meter_0043; // 
        [XmlElement]
        public ExtendedAccountingMeter Meter_0044; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0045; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0046; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0047; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0048; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0049; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_004A; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_004B; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_004C; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_004D; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_004E; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_004F; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0050; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0051; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0052; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0053; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0054; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0055; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0056; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0057; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0058; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0059; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_005A; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_005B; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_005C; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_005D; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_005E; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_005F; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0060; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0061; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0062; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0063; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0064; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0065; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0066; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0067; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0068; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0069; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_006A; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_006B; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_006C; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_006D; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_006E; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_006F; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0070; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0071; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0072; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0073; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0074; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0075; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0076; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0077; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0078; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0079; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_007A; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_007B; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_007C; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_007D; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_007E; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_007F; //
        [XmlElement]
        public ExtendedAccountingMeter Meter_0080; // Regular cashable ticket in cents
        [XmlElement]
        public ExtendedAccountingMeter Meter_0081; // Regular cashable ticket in count
        [XmlElement]
        public ExtendedAccountingMeter Meter_0082; // Restricted ticket in cent
        [XmlElement]
        public ExtendedAccountingMeter Meter_0083; // Restricted ticket in count
        [XmlElement]
        public ExtendedAccountingMeter Meter_0084; // Nonrestricted ticket in cents
        [XmlElement]
        public ExtendedAccountingMeter Meter_0085; // Nonrestricted ticket in count
        [XmlElement]
        public ExtendedAccountingMeter Meter_0086; // Regular cashable ticket out cents
        [XmlElement]
        public ExtendedAccountingMeter Meter_0087; // Regular cashable ticket out count
        [XmlElement]
        public ExtendedAccountingMeter Meter_0088; // Restricted ticket out cents
        [XmlElement]
        public ExtendedAccountingMeter Meter_0089; // Restricted ticket out counts
        [XmlElement]
        public ExtendedAccountingMeter  Meter_008A; // Debit ticket out cents
        [XmlElement]
        public ExtendedAccountingMeter  Meter_008B; // Debit ticket out counts
        [XmlElement]
        public ExtendedAccountingMeter  Meter_008C; // Validated cancelled credit handpay, receipt printed cents
        [XmlElement]
        public ExtendedAccountingMeter  Meter_008D; // Validated cancelled credit handpay, receipt printed counts
        [XmlElement]
        public ExtendedAccountingMeter  Meter_008E; // Validated jackpot handpay, receipt printed cents
        [XmlElement]
        public ExtendedAccountingMeter  Meter_008F; // Validated jackpot handpay, receipt printed counts
        [XmlElement]
        public ExtendedAccountingMeter  Meter_0090; // Validated cancelled credit handpay, no receipt cents
        [XmlElement]
        public ExtendedAccountingMeter  Meter_0091; //  Validated cancelled credit handpay, no receipt counts
        [XmlElement]
        public ExtendedAccountingMeter  Meter_0092; // Validated jackpot handpay, no receipt cents
        [XmlElement]
        public ExtendedAccountingMeter  Meter_0093; // Validated jackpot handpay, no receipt counts
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00A0; // AFT in House cashable transfer to gaming machine (cents)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00A1; // AFT in House cashable transfer to gaming machine (quantity)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00A2; // AFT in House restricted transfer to gaming machine cents
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00A3; // AFT in House restricted transfer to gaming machine counts
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00A4; // AFT in House nonrestricted transfer to gaming machine (cents)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00A5; // AFT in House nonrestricted transfer to gaming machine (quantity)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00A6; // AFT debit transfer to gaming machine (cents)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00A7; // AFT debit transfer to gaming machine (quantity)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00A8; // AFT In House cashable transfer to ticket (cents)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00A9; // AFT In House cashable transfer to ticket (quantity)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00AA; // AFT In House restricted transfer to ticket (cents)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00AB; // AFT In House restricted transfer to ticket (quantity)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00AC; // AFT Debit transfer to ticket (cents)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00AD; // AFT Debit transfer to ticket (quantity)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00AE; // AFT Bonus cashable transfer to gaming machine (cents)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00AF; // AFT Bonus cashable transfer to gaming machine (quantity) 
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00B0; // AFT Bonus nonrestricted transfer to gaming machine (cents)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00B1; // AFT Bonus nonrestricted transfer to gaming machine (quantity)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00B8; // AFT In House cashable transfer to host (cents)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00B9; // AFT In House cashable transfer to host (quantity)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00BA; // AFT In House restricted transfer to host (cents)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00BB; // AFT In House restricted transfer to host (quantity)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00BC; // AFT In House nonrestricted transfer to host (cents)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00BD; // AFT In House nonrestricted transfer to host (quantity)
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00FA; //
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00FB; //
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00FC; //
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00FD; //
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00FE; // 
        [XmlElement]
        public ExtendedAccountingMeter  Meter_00FF; //
        [XmlElement]
        public  ExtendedAccountingMeter  Meter_TotalBillMeterInDollars;
        [XmlElement]
        public  ExtendedAccountingMeter Meter_TrueCoinInMeter;
        [XmlElement]
        public  ExtendedAccountingMeter Meter_TrueCoinOutMeter;
        [XmlElement]
        public  ExtendedAccountingMeter Meter_BonusingDeductible;
        [XmlElement]
        public  ExtendedAccountingMeter Meter_BonusingNoDeductible;
        [XmlElement]
        public  ExtendedAccountingMeter Meter_BonusingWagerMatch;
        [XmlElement]
        public  BasicAccountingMeter Meter_Basic_TotalCoinIn; 
        [XmlElement]
        public  BasicAccountingMeter Meter_Basic_TotalCoinOut;
        [XmlElement]
        public  BasicAccountingMeter Meter_Basic_TotalDrop;
        [XmlElement]
        public  BasicAccountingMeter Meter_Basic_TotalJackPot;
        [XmlElement]
        public  BasicAccountingMeter Meter_Basic_GamesPlayed;
        [XmlElement]
        public  BasicAccountingMeter Meter_Basic_GamesWon;
        [XmlElement]
        public  BasicAccountingMeter Meter_Basic_SlotDoorOpen;
        [XmlElement]
        public  BasicAccountingMeter Meter_Basic_PowerReset;

        // Método que permite setear un nombre al archivo de persistencia de la game accounting
        // Method that allows to set a name to the game accounting persistence file 
        public void SetFilePath(string fp)
        {
            filepath = fp;
        }

        // Función que permite obtener el nombre del archivo de persistencia de la game accounting
        // Function that allows to obtain the name of the game accounting persistence file
        public string GetFilePath()
        {
            return filepath;
        }

        // Inicialización de la game accounting
        // Game accounting initialization
        public GameAccounting()
        {
            Meter_TrueCoinInMeter = new ExtendedAccountingMeter();
            Meter_TrueCoinOutMeter = new ExtendedAccountingMeter();
            Meter_TotalBillMeterInDollars = new ExtendedAccountingMeter();
            Meter_BonusingDeductible = new ExtendedAccountingMeter();
            Meter_BonusingNoDeductible = new ExtendedAccountingMeter();
            Meter_BonusingWagerMatch = new ExtendedAccountingMeter();
            Meter_0000 = new ExtendedAccountingMeter();
            Meter_0001 = new ExtendedAccountingMeter();
            Meter_0002 = new ExtendedAccountingMeter();
            Meter_0003 = new ExtendedAccountingMeter();// Total Hand Pay Cancelled credits
            Meter_0004 = new ExtendedAccountingMeter();
            Meter_0005 = new ExtendedAccountingMeter();
            Meter_0006 = new ExtendedAccountingMeter();
            Meter_0007 = new ExtendedAccountingMeter();
            Meter_0008 = new ExtendedAccountingMeter();
            Meter_0009 = new ExtendedAccountingMeter();
            Meter_000A = new ExtendedAccountingMeter();
            Meter_000B = new ExtendedAccountingMeter(); // Total credits from bills accepted
            Meter_000C = new ExtendedAccountingMeter();
            Meter_000D = new ExtendedAccountingMeter(); // Total cashable ticket in (cents)
            Meter_000E = new ExtendedAccountingMeter(); // Total cashable ticket out (cents)
            Meter_000F = new ExtendedAccountingMeter(); // Total restricted ticket in (cents)
            Meter_0010 = new ExtendedAccountingMeter(); // Total restricted ticket out (cents)
            Meter_0011 = new ExtendedAccountingMeter(); // Total SAS cashable ticket in, including nonrestricted tickets (quantity) 
            Meter_0012 = new ExtendedAccountingMeter(); // Total SAS cashable ticket out, including nonrestricted tickets (quantity)
            Meter_0013 = new ExtendedAccountingMeter();
            Meter_0014 = new ExtendedAccountingMeter();
            Meter_0015 = new ExtendedAccountingMeter();
            Meter_0016 = new ExtendedAccountingMeter();
            Meter_0017 = new ExtendedAccountingMeter();
            Meter_0018 = new ExtendedAccountingMeter();
            Meter_0019 = new ExtendedAccountingMeter(); // Total restricted amount played (credits)
            Meter_001A = new ExtendedAccountingMeter(); // Total nonrestricted amount played (credits)
            Meter_001B = new ExtendedAccountingMeter();
            Meter_001C = new ExtendedAccountingMeter();
            Meter_001D = new ExtendedAccountingMeter();
            Meter_001E = new ExtendedAccountingMeter(); // Total machine paid external bonus win (credits)
            Meter_001F = new ExtendedAccountingMeter();
            Meter_Basic_TotalCoinIn = new BasicAccountingMeter();
            Meter_Basic_TotalCoinOut = new BasicAccountingMeter();
            Meter_Basic_TotalDrop = new BasicAccountingMeter();
            Meter_Basic_TotalJackPot = new BasicAccountingMeter();
            Meter_Basic_GamesPlayed = new BasicAccountingMeter();
            Meter_Basic_GamesWon = new BasicAccountingMeter();
            Meter_Basic_SlotDoorOpen = new BasicAccountingMeter();
            Meter_Basic_PowerReset = new BasicAccountingMeter();
            Meter_0020 = new ExtendedAccountingMeter();
            Meter_0021 = new ExtendedAccountingMeter(); // Total attendant paid external bonus win (credits)
            Meter_0022 = new ExtendedAccountingMeter();
            Meter_0023 = new ExtendedAccountingMeter();
            Meter_0024 = new ExtendedAccountingMeter();
            Meter_0025 = new ExtendedAccountingMeter(); // Total games since last power reset
            Meter_0026 = new ExtendedAccountingMeter(); // Total games since last door closure
            Meter_0027 = new ExtendedAccountingMeter();
            Meter_0028 = new ExtendedAccountingMeter();
            Meter_0029 = new ExtendedAccountingMeter();
            Meter_002A = new ExtendedAccountingMeter();
            Meter_002B = new ExtendedAccountingMeter();
            Meter_002C = new ExtendedAccountingMeter(); // Total cashable ticket out credits
            Meter_002D = new ExtendedAccountingMeter(); // Total restricted promotional ticket out credits
            Meter_002E = new ExtendedAccountingMeter();
            Meter_002F = new ExtendedAccountingMeter();
            Meter_0030 = new ExtendedAccountingMeter();
            Meter_0031 = new ExtendedAccountingMeter();
            Meter_0032 = new ExtendedAccountingMeter();
            Meter_0033 = new ExtendedAccountingMeter();
            Meter_0034 = new ExtendedAccountingMeter();
            Meter_0035 = new ExtendedAccountingMeter(); // Total regular cashable ticket in count
            Meter_0036 = new ExtendedAccountingMeter(); // Total restricted promotional ticket out credits
            Meter_0037 = new ExtendedAccountingMeter(); // Total nonrestricted ticket in count
            Meter_0038 = new ExtendedAccountingMeter(); // Total cashable out count, including debit ticket
            Meter_0039 = new ExtendedAccountingMeter(); // Total restricted promotional ticket out count
            Meter_003A = new ExtendedAccountingMeter(); 
            Meter_003B = new ExtendedAccountingMeter();
            Meter_003C = new ExtendedAccountingMeter();
            Meter_003D = new ExtendedAccountingMeter();
            Meter_003E = new ExtendedAccountingMeter(); // Number of bills currently in stacker
            Meter_003F = new ExtendedAccountingMeter(); // Total value of bills currently in stacker (Credits)
            Meter_0040 = new ExtendedAccountingMeter(); // 
            Meter_0041 = new ExtendedAccountingMeter(); //
            Meter_0042 = new ExtendedAccountingMeter(); // 
            Meter_0043 = new ExtendedAccountingMeter(); //
            Meter_0044 = new ExtendedAccountingMeter(); //
            Meter_0045 = new ExtendedAccountingMeter(); //
            Meter_0046 = new ExtendedAccountingMeter(); //
            Meter_0047 = new ExtendedAccountingMeter(); //
            Meter_0048 = new ExtendedAccountingMeter(); //
            Meter_0049 = new ExtendedAccountingMeter(); //
            Meter_004A = new ExtendedAccountingMeter();
            Meter_004B = new ExtendedAccountingMeter();
            Meter_004C = new ExtendedAccountingMeter();
            Meter_004D = new ExtendedAccountingMeter();
            Meter_004E = new ExtendedAccountingMeter();
            Meter_004F = new ExtendedAccountingMeter();
            Meter_0050 = new ExtendedAccountingMeter(); //
            Meter_0051 = new ExtendedAccountingMeter(); //
            Meter_0052 = new ExtendedAccountingMeter(); //
            Meter_0053 = new ExtendedAccountingMeter(); //
            Meter_0054 = new ExtendedAccountingMeter(); //
            Meter_0055 = new ExtendedAccountingMeter(); //
            Meter_0056 = new ExtendedAccountingMeter(); //
            Meter_0057 = new ExtendedAccountingMeter(); //
            Meter_0058 = new ExtendedAccountingMeter(); //
            Meter_0059 = new ExtendedAccountingMeter(); //
            Meter_005A = new ExtendedAccountingMeter();
            Meter_005B = new ExtendedAccountingMeter();
            Meter_005C = new ExtendedAccountingMeter();
            Meter_005D = new ExtendedAccountingMeter(); //
            Meter_005E = new ExtendedAccountingMeter();
            Meter_005F = new ExtendedAccountingMeter();
            Meter_0060 = new ExtendedAccountingMeter();
            Meter_0061 = new ExtendedAccountingMeter();
            Meter_0062 = new ExtendedAccountingMeter();
            Meter_0063 = new ExtendedAccountingMeter();
            Meter_0064 = new ExtendedAccountingMeter();
            Meter_0065 = new ExtendedAccountingMeter();
            Meter_0066 = new ExtendedAccountingMeter();
            Meter_0067 = new ExtendedAccountingMeter();
            Meter_0068 = new ExtendedAccountingMeter();
            Meter_0069 = new ExtendedAccountingMeter();
            Meter_006A = new ExtendedAccountingMeter();
            Meter_006B = new ExtendedAccountingMeter();
            Meter_006C = new ExtendedAccountingMeter();
            Meter_006D = new ExtendedAccountingMeter();
            Meter_006E = new ExtendedAccountingMeter();
            Meter_006F = new ExtendedAccountingMeter();
            Meter_0070 = new ExtendedAccountingMeter(); //
            Meter_0071 = new ExtendedAccountingMeter(); //
            Meter_0072 = new ExtendedAccountingMeter(); //
            Meter_0073 = new ExtendedAccountingMeter(); //
            Meter_0074 = new ExtendedAccountingMeter(); //
            Meter_0075 = new ExtendedAccountingMeter(); //
            Meter_0076 = new ExtendedAccountingMeter(); //
            Meter_0077 = new ExtendedAccountingMeter(); //
            Meter_0078 = new ExtendedAccountingMeter(); //
            Meter_0079 = new ExtendedAccountingMeter(); //
            Meter_007A = new ExtendedAccountingMeter(); //
            Meter_007B = new ExtendedAccountingMeter(); //
            Meter_007C = new ExtendedAccountingMeter(); //
            Meter_007D = new ExtendedAccountingMeter(); //
            Meter_007E = new ExtendedAccountingMeter(); //
            Meter_007F = new ExtendedAccountingMeter(); //
            Meter_0080 = new ExtendedAccountingMeter(); // Regular cashable ticket in cents
            Meter_0081 = new ExtendedAccountingMeter(); // Regular cashable ticket in count
            Meter_0082 = new ExtendedAccountingMeter(); // Restricted ticket in cent
            Meter_0083 = new ExtendedAccountingMeter(); // Restricted ticket in count
            Meter_0084 = new ExtendedAccountingMeter(); // Nonrestricted ticket in cents
            Meter_0085 = new ExtendedAccountingMeter(); // Nonrestricted ticket in count
            Meter_0086 = new ExtendedAccountingMeter(); // Regular cashable ticket out cents
            Meter_0087 = new ExtendedAccountingMeter(); // Regular cashable ticket out count
            Meter_0088 = new ExtendedAccountingMeter(); // Restricted ticket out cents
            Meter_0089 = new ExtendedAccountingMeter(); // Restricted ticket out counts
            Meter_008A = new ExtendedAccountingMeter(); // Debit ticket out cents
            Meter_008B = new ExtendedAccountingMeter(); // Debit ticket out counts
            Meter_008C = new ExtendedAccountingMeter(); // Validated cancelled credit handpay, receipt printed cents
            Meter_008D = new ExtendedAccountingMeter(); // Validated cancelled credit handpay, receipt printed counts
            Meter_008E = new ExtendedAccountingMeter(); // Validated jackpot handpay, receipt printed cents
            Meter_008F = new ExtendedAccountingMeter(); // Validated jackpot handpay, receipt printed counts
            Meter_0090 = new ExtendedAccountingMeter(); // Validated cancelled credit handpay, no receipt cents
            Meter_0091 = new ExtendedAccountingMeter(); //  Validated cancelled credit handpay, no receipt counts
            Meter_0092 = new ExtendedAccountingMeter(); // Validated jackpot handpay, no receipt cents
            Meter_0093 = new ExtendedAccountingMeter(); // Validated jackpot handpay, no receipt counts
            Meter_00A0 = new ExtendedAccountingMeter(); // AFT in House cashable transfer to gaming machine (cents)
            Meter_00A1 = new ExtendedAccountingMeter(); // AFT in House cashable transfer to gaming machine (quantity)
            Meter_00A2 = new ExtendedAccountingMeter(); // AFT in House restricted transfer to gaming machine cents
            Meter_00A3 = new ExtendedAccountingMeter(); // AFT in House restricted transfer to gaming machine counts
            Meter_00A4 = new ExtendedAccountingMeter(); // AFT in House nonrestricted transfer to gaming machine (cents)
            Meter_00A5 = new ExtendedAccountingMeter(); // AFT in House nonrestricted transfer to gaming machine (quantity)
            Meter_00A6 = new ExtendedAccountingMeter(); // AFT debit transfer to gaming machine (cents)
            Meter_00A7 = new ExtendedAccountingMeter(); // AFT debit transfer to gaming machine (quantity)
            Meter_00A8 = new ExtendedAccountingMeter(); // AFT In House cashable transfer to ticket (cents)
            Meter_00A9 = new ExtendedAccountingMeter(); // AFT In House cashable transfer to ticket (quantity)
            Meter_00AA = new ExtendedAccountingMeter(); // AFT In House restricted transfer to ticket (cents)
            Meter_00AB = new ExtendedAccountingMeter(); // AFT In House restricted transfer to ticket (quantity)
            Meter_00AC = new ExtendedAccountingMeter(); // AFT Debit transfer to ticket (cents)
            Meter_00AD = new ExtendedAccountingMeter(); // AFT Debit transfer to ticket (quantity)
            Meter_00AE = new ExtendedAccountingMeter(); // AFT Bonus cashable transfer to gaming machine (cents)
            Meter_00AF = new ExtendedAccountingMeter(); // AFT Bonus cashable transfer to gaming machine (quantity) 
            Meter_00B0 = new ExtendedAccountingMeter(); // AFT Bonus nonrestricted transfer to gaming machine (cents)
            Meter_00B1 = new ExtendedAccountingMeter(); // AFT Bonus nonrestricted transfer to gaming machine (quantity)
            Meter_00B8 = new ExtendedAccountingMeter(); // AFT In House cashable transfer to host (cents)
            Meter_00B9 = new ExtendedAccountingMeter(); // AFT In House cashable transfer to host (quantity)
            Meter_00BA = new ExtendedAccountingMeter(); // AFT In House restricted transfer to host (cents)
            Meter_00BB = new ExtendedAccountingMeter(); // AFT In House restricted transfer to host (quantity)
            Meter_00BC = new ExtendedAccountingMeter(); // AFT In House nonrestricted transfer to host (cents)
            Meter_00BD = new ExtendedAccountingMeter(); // AFT In House nonrestricted transfer to host (quantity)
            Meter_00FA = new ExtendedAccountingMeter(); //
            Meter_00FB = new ExtendedAccountingMeter(); //
            Meter_00FC = new ExtendedAccountingMeter(); //
            Meter_00FD = new ExtendedAccountingMeter(); //
            Meter_00FE = new ExtendedAccountingMeter(); //
            Meter_00FF = new ExtendedAccountingMeter(); //
        }

        public void ResetMeters()
        {
            Meter_TrueCoinInMeter.Value = 0;
            Meter_TrueCoinOutMeter.Value = 0;
            Meter_TotalBillMeterInDollars.Value = 0;
            Meter_BonusingDeductible.Value = 0;
            Meter_BonusingNoDeductible.Value = 0;
            Meter_BonusingWagerMatch.Value = 0;
            Meter_0000.Value = 0;
            Meter_0001.Value = 0;
            Meter_0002.Value = 0;
            Meter_0003.Value = 0;// Total Hand Pay Cancelled credits
            Meter_0004.Value = 0;
            Meter_0005.Value = 0;
            Meter_0006.Value = 0;
            Meter_0007.Value = 0;
            Meter_0008.Value = 0;
            Meter_0009.Value = 0;
            Meter_000A.Value = 0;
            Meter_000B.Value = 0; // Total credits from bills accepted
            Meter_000C.Value = 0;
            Meter_000D.Value = 0; // Total cashable ticket in (cents)
            Meter_000E.Value = 0; // Total cashable ticket out (cents)
            Meter_000F.Value = 0; // Total restricted ticket in (cents)
            Meter_0010.Value = 0; // Total restricted ticket out (cents)
            Meter_0011.Value = 0;
            Meter_0012.Value = 0;
            Meter_0013.Value = 0;
            Meter_0014.Value = 0;
            Meter_0015.Value = 0;
            Meter_0016.Value = 0;
            Meter_0017.Value = 0;
            Meter_0018.Value = 0;
            Meter_0019.Value = 0; // Total restricted amount played (credits)
            Meter_001A.Value = 0; // Total nonrestricted amount played (credits)
            Meter_001B.Value = 0;
            Meter_001C.Value = 0;
            Meter_001D.Value = 0;
            Meter_001E.Value = 0; // Total machine paid external bonus win (credits)
            Meter_001F.Value = 0;
            Meter_Basic_TotalCoinIn.Value = 0;
            Meter_Basic_TotalCoinOut.Value = 0;
            Meter_Basic_TotalDrop.Value = 0;
            Meter_Basic_TotalJackPot.Value = 0;
            Meter_Basic_GamesPlayed.Value = 0;
            Meter_Basic_GamesWon.Value = 0;
            Meter_Basic_SlotDoorOpen.Value = 0;
            Meter_Basic_PowerReset.Value = 0;
            Meter_0020.Value = 0;
            Meter_0021.Value = 0; // Total attendant paid external bonus win (credits)
            Meter_0022.Value = 0;
            Meter_0023.Value = 0;           
            Meter_0024.Value = 0;
            Meter_0025.Value = 0; // Total games since last power reset
            Meter_0026.Value = 0; // Total games since last door closure
            Meter_0027.Value = 0;
            Meter_0028.Value = 0;
            Meter_0029.Value = 0;
            Meter_002A.Value = 0;
            Meter_002B.Value = 0;
            Meter_002C.Value = 0; // Total cashable ticket out credits
            Meter_002D.Value = 0; // Total restricted promotional ticket out credits
            Meter_002E.Value = 0;
            Meter_002F.Value = 0;
            Meter_0030.Value = 0;
            Meter_0031.Value = 0;
            Meter_0032.Value = 0;
            Meter_0033.Value = 0;
            Meter_0034.Value = 0;
            Meter_0035.Value = 0; // Total regular cashable ticket in count
            Meter_0036.Value = 0; // Total restricted promotional ticket out credits
            Meter_0037.Value = 0; // Total nonrestricted ticket in count
            Meter_0038.Value = 0; // Total cashable out count, including debit ticket
            Meter_0039.Value = 0; // Total restricted promotional ticket out count
            Meter_003A.Value = 0; // Total restricted promotional ticket out count
            Meter_003B.Value = 0;
            Meter_003C.Value = 0;
            Meter_003D.Value = 0;
            Meter_003E.Value = 0; // Number of bills currently in stacker
            Meter_003F.Value = 0; // Total value of bills currently in stacker (Credits)
            Meter_0040.Value = 0; // 
            Meter_0041.Value = 0; //
            Meter_0042.Value = 0; // 
            Meter_0043.Value = 0; //
            Meter_0044.Value = 0; //
            Meter_0045.Value = 0; //
            Meter_0046.Value = 0; //
            Meter_0047.Value = 0; //
            Meter_0048.Value = 0; //
            Meter_0049.Value = 0; //
            Meter_004A.Value = 0;
            Meter_004B.Value = 0;
            Meter_004C.Value = 0;
            Meter_004D.Value = 0;
            Meter_004E.Value = 0;
            Meter_004F.Value = 0;
            Meter_0050.Value = 0; //
            Meter_0051.Value = 0; //
            Meter_0052.Value = 0; //
            Meter_0053.Value = 0; //
            Meter_0054.Value = 0; //
            Meter_0055.Value = 0; //
            Meter_0056.Value = 0; //
            Meter_0057.Value = 0; //
            Meter_0058.Value = 0;
            Meter_0059.Value = 0;
            Meter_005A.Value = 0;
            Meter_005B.Value = 0;
            Meter_005C.Value = 0;
            Meter_005D.Value = 0;
            Meter_005E.Value = 0;
            Meter_005F.Value = 0;
            Meter_0060.Value = 0;            
            Meter_0061.Value = 0;
            Meter_0062.Value = 0;
            Meter_0063.Value = 0;
            Meter_0064.Value = 0;
            Meter_0065.Value = 0;
            Meter_0066.Value = 0;
            Meter_0067.Value = 0;
            Meter_0068.Value = 0;
            Meter_0069.Value = 0;
            Meter_006A.Value = 0;
            Meter_006B.Value = 0;
            Meter_006C.Value = 0;
            Meter_006D.Value = 0;
            Meter_006E.Value = 0;
            Meter_006F.Value = 0;
            Meter_0070.Value = 0;
            Meter_0071.Value = 0;
            Meter_0072.Value = 0;
            Meter_0073.Value = 0;
            Meter_0074.Value = 0;
            Meter_0075.Value = 0;
            Meter_0076.Value = 0;
            Meter_0077.Value = 0;
            Meter_0078.Value = 0;
            Meter_0079.Value = 0; //
            Meter_007A.Value = 0; //
            Meter_007B.Value = 0; //
            Meter_007C.Value = 0; //
            Meter_007D.Value = 0; //
            Meter_007E.Value = 0; //
            Meter_007F.Value = 0; //
            Meter_0080.Value = 0; // Regular cashable ticket in cents
            Meter_0081.Value = 0; // Regular cashable ticket in count
            Meter_0082.Value = 0; // Restricted ticket in cent
            Meter_0083.Value = 0; // Restricted ticket in count
            Meter_0084.Value = 0; // Nonrestricted ticket in cents
            Meter_0085.Value = 0; // Nonrestricted ticket in count
            Meter_0086.Value = 0; // Regular cashable ticket out cents
            Meter_0087.Value = 0; // Regular cashable ticket out count
            Meter_0088.Value = 0; // Restricted ticket out cents
            Meter_0089.Value = 0; // Restricted ticket out counts
            Meter_008A.Value = 0; // Debit ticket out cents
            Meter_008B.Value = 0; // Debit ticket out counts
            Meter_008C.Value = 0; // Validated cancelled credit handpay, receipt printed cents
            Meter_008D.Value = 0; // Validated cancelled credit handpay, receipt printed counts
            Meter_008E.Value = 0; // Validated jackpot handpay, receipt printed cents
            Meter_008F.Value = 0; // Validated jackpot handpay, receipt printed counts
            Meter_0090.Value = 0; // Validated cancelled credit handpay, no receipt cents
            Meter_0091.Value = 0; //  Validated cancelled credit handpay, no receipt counts
            Meter_0092.Value = 0; // Validated jackpot handpay, no receipt cents
            Meter_0093.Value = 0; // Validated jackpot handpay, no receipt counts
            Meter_00A0.Value = 0; // AFT in House cashable transfer to gaming machine (cents)
            Meter_00A1.Value = 0; // AFT in House cashable transfer to gaming machine (quantity)
            Meter_00A2.Value = 0; // AFT in House restricted transfer to gaming machine cents
            Meter_00A3.Value = 0; // AFT in House restricted transfer to gaming machine counts
            Meter_00A4.Value = 0; // AFT in House nonrestricted transfer to gaming machine (cents)
            Meter_00A5.Value = 0; // AFT in House nonrestricted transfer to gaming machine (quantity)
            Meter_00A6.Value = 0; // AFT debit transfer to gaming machine (cents)
            Meter_00A7.Value = 0; // AFT debit transfer to gaming machine (quantity)
            Meter_00A8.Value = 0; // AFT In House cashable transfer to ticket (cents)
            Meter_00A9.Value = 0; // AFT In House cashable transfer to ticket (quantity)
            Meter_00AA.Value = 0; // AFT In House restricted transfer to ticket (cents)
            Meter_00AB.Value = 0; // AFT In House restricted transfer to ticket (quantity)
            Meter_00AC.Value = 0; // AFT Debit transfer to ticket (cents)
            Meter_00AD.Value = 0; // AFT Debit transfer to ticket (quantity)
            Meter_00AE.Value = 0; // AFT Bonus cashable transfer to gaming machine (cents)
            Meter_00AF.Value = 0; // AFT Bonus cashable transfer to gaming machine (quantity) 
            Meter_00B0.Value = 0; // AFT Bonus nonrestricted transfer to gaming machine (cents)
            Meter_00B1.Value = 0; // AFT Bonus nonrestricted transfer to gaming machine (quantity)
            Meter_00B8.Value = 0; // AFT In House cashable transfer to host (cents)
            Meter_00B9.Value = 0; // AFT In House cashable transfer to host (quantity)
            Meter_00BA.Value = 0; // AFT In House restricted transfer to host (cents)
            Meter_00BB.Value = 0; // AFT In House restricted transfer to host (quantity)
            Meter_00BC.Value = 0; // AFT In House nonrestricted transfer to host (cents)
            Meter_00BD.Value = 0; // AFT In House nonrestricted transfer to host (quantity)
        }


    }
}

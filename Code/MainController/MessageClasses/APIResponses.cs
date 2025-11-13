using System;
using SASComms;
using BitbossInterface;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace MainController
{
    // API Responses

    // Meters Response
    public class MetersAPIResponse
    {
        public int TotalCoinIn = 0;
        public int TotalCoinOut = 0;
        public int TotalDrop = 0;
        public int TotalJackPot = 0;
        public int GamesPlayed = 0;
        public int GamesWon = 0;
        public int SlotDoorOpen = 0;
        public int PowerReset = 0;
        public int M000C_CurrentCredits = 0;
        public int CurrentCashableCredits_IN_CENTS = 0;
        public int M001B_CurrentRestrictedCredits = 0;
        public int CurrentNonRestrictedCredits = 0;
        public int M00A0_InHouseCashableTransfersToGamingMachine_Cents = 0;
        public int M00A1_InHouseTransfersToGamingMachineWithCashableAmounts_Quantity = 0;
        public int M00A2_InHouseRestrictedTransfersToGamingMachine_Cents = 0;
        public int M00A3_InHouseTransfersToGamingMachineWithRestrictedAmounts_Quantity = 0;
        public int M00A4_InHouseNonRestrictedTransfersToGamingMachine_Cents = 0;
        public int M00A5_InHouseTransfersToGamingMachineWithNonRestrictedAmounts_Quantity = 0;
        public int M00A8_InHouseCashableTransfersToTicket_Cents = 0;
        public int M00A9_InHouseCashableTransfersToTicket_Quantity = 0;
        public int M00AA_InHouseRestrictedTransfersToTicket_Cents = 0;
        public int M00AB_InHouseRestrictedTransfersToTicket_Quantity = 0;
        public int M00AE_BonusCashableTransfersToGamingMachine_Cents = 0;
        public int M00AF_BonusTransfersToGamingMachineWithCashableAmounts_Quantity = 0;
        public int M00B0_BonusNonRestrictedTransfersToGamingMachine_Cents = 0;
        public int M00B1_BonusTransfersToGamingMachineWithNonRestrictedAmounts_Quantity = 0;
        public int M00B8_InHouseCashableTransfersToHost_Cents = 0;
        public int M00B9_InHouseTransfersToHostWithCashableAmount_Quantity = 0;
        public int M00BA_InHouseRestrictedTransfersToHost_Cents = 0;
        public int M00BB_InHouseTransfersToHostWithRestrictedAmounts_Quantity = 0;
        public int M00BC_InHouseNonRestrictedTransfersToHost_Cents = 0;
        public int M00BD_InHouseTransfersToHostWithNonRestrictedAmounts_Quantity = 0;

        public bool CreditsTransactionInProgress  = false;
    }

    // Transaction History Line Response
    public class PhysicalEGMAFTTransactionHistoryLineResponse
    {
        public string TransferStatus;
        public string ReceiptStatus;
        public string TransferType;
        public int CashableAmount;
        public int RestrictedAmount;
        public string TransactionID;
        public DateTime TransactionDateTime;
        public int Position;
    }

    // Live Trace Line Response
    public class LiveTraceLineResponse
    {
        public DateTime TimeStamp;
        public string Message;
        public string Direction;
        public bool CRC;
        public bool IsRetry;
    }

    public class LinksHealthResponse
    {
        public bool EGMLinkActive;
        public bool SmibLinkActive;
        public DateTime LastEGMResponseReceivedAt;
    }

    // Live Trace Response
    public class LiveTraceResponse
    {
        public List<LiveTraceLineResponse> lines = new List<LiveTraceLineResponse>();

    }

    // EGM Info Response
    public class EGMInfoResponse
    {
        public byte[] GMSerialNumber;
        public string GameID;
        public string AdditionalID;
        public byte Denomination;
        public byte MaxBet;
        public byte ProgressiveGroup;
        public byte[] GameOptions;
        public string PayTableID;
        public string BasePercentage;
        public byte[] GMTransferLimit;

    }

    // Host Status Response
    public class HostStatusResponse
    {
        public string EGMSASVersion;
        public string EGMSerialNUmber;
        public bool PhysicalEGMController_TimerMetersInitiated;
        public bool PhysicalEGMController_Timer30SecondsInitiated;
        public bool PhysicalEGMController_Timer300SecondsInitiated;
        public int PhysicalEGMController_TimerMetersRetries;
        public int PhysicalEGMController_Timer30SecondsRetries;
        public int PhysicalEGMController_Timer300SecondsRetries;
        public bool PhysicalEGMController_FlagTransferToGamingMachine;
        public bool PhysicalEGMController_FlagTransferFromGamingMachine;
        public bool PhysicalEGMController_FlagTransferToPrinter;
        public bool PhysicalEGMController_FlagWinAmountPendingCashoutToHost;
        public bool PhysicalEGMController_FlagBonusAwardToGamingMachine;
        public bool PhysicalEGMController_FlagLockAfterTransferRequestSupported;
        public string Host__lastSend;
        public string Host__lastReceived;
        public DateTime Host__lastSendTS;
        public DateTime Host__lastReceivedTS;
        public bool Host_communication;
        public byte Host_address;
        public string Host_phase;
        public int Host_PollingFrecuency;
        public int Host__assetNumber;
        public string MainController_CurrentTransactionStatus;
        public bool MainController_CurrentTransactionInProcess;
        public string MainController_InterfacedRedemptionStatus;
        public bool MainController_InterfacedRedemptionInProcess;
        public string MainController_InterfacedValidationStatus;
        public bool MainController_InterfacedValidationInProcess;
        public List<string> LastInitUnrepliedLPs;
    }


    // Interfacing Settings Response
    public class InterfacingSettingsResponse
    {
        public bool passthrough_lp01;
        public bool passthrough_lp02;
        public bool passthrough_lp03;
        public bool passthrough_lp04;
        public bool passthrough_lp06;
        public bool passthrough_lp07;
        public bool passthrough_lp08;
        public bool passthrough_lp0E;
        public bool passthrough_lp4c;
        public bool passthrough_lp7C;
        public bool passthrough_lp7f;
        public bool passthrough_lp94;
        public bool passthrough_lp80;
        public bool passthrough_lp86;
        public string validationType;
    }

    // Physical EGM Settings Response
    public class PhysicalEGMSettingsResponse
    {
        public bool JackpotMultiplier;
        public bool AFTBonusAwards;
        public bool LegacyBonusAwards;
        public bool Tournament;
        public bool ValidationExtensions;
        public string ValidationStyle;
        public bool TicketRedemption;
        public string MeterModelFlag;
        public bool TicketsToTotalDropAndTotalCancelledCredits;
        public bool ExtendedMeters;
        public bool ComponentAuthentication;
        public bool AdvancedFundsTransfer;
        public bool MultiDenomExtensions;
        public bool MaximumPollingRate;
        public bool MultipleSASProgressiveWinReporting;
    }


    // Physical EGM AFT Transaction History Response
    public class PhysicalEGMAFTTransactionHistoryAPIResponse
    {
        public List<PhysicalEGMAFTTransactionHistoryLineResponse> transactions = new List<PhysicalEGMAFTTransactionHistoryLineResponse>();
    }

    // Transaction API Response
    public class TransactionAPIResponse
    {
        public string status = "";
        public string transactionId = "";
    }

}

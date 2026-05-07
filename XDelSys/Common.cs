using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.XDelSys
{
    /// <summary>
    /// taken from XDelSys.HOMES.Common
    /// </summary>
    public static class Common
    {
        public const Int64 Picked_ScheduledDelivery = 342;
        public const Int64 ScheduledSelfCollect = 325;
        public const Int64 Picked_ScheduledSelfCollect = 343;
        public const Int64 ReattemptSMSSent = 326;
        public const Int64 BookingSMSSent = 327;
        public const Int64 DeliveryTimingChangedSMSSent = 175;
        public const Int64 RebookingRequested = 393;
        public const Int64 CustomerVerifiedAddress = 331;
        public const Int64 CancelledBooking = 263;
        public const Int64 BookingData = 258;
        public const Int64 PayPalPending = 260;
        public const Int64 PayPalComplete = 259;
        public const Int64 RestrictedChargesPaid = 365;
        public const Int64 ChargesWaived = 339;
        public const Int64 NoDocument = 372;
        public const Int64 AuthFailed = 264;
        public const Int64 WhseR = 231;

        public const Int64 SSCL = 261;
        public const Int64 WhseCCSched = 79;
        public const Int64 PickListPrinted = 341;
        public const Int64 NoLOA = 257;
        public const Int64 LA_FD = 222;
        public const Int64 ClosedOnArrival = 68;

        public const Int64 C_NewJob = 50;
        public const Int64 C_DeletedEntry = 250;
        public const Int64 C_Snapshot = 251;
        public const Int64 C_InternalData = 247;
        public const Int64 C_LogEntry = 248;
        public const Int64 C_EMailSent = 98;
        public const Int64 C_SMSReminder = 133;
        public const Int64 C_SMSPIN = 313;
        public const Int64 C_SMSReview = 314;
        public const Int64 C_SMSEnhanced = 315;
        public const Int64 C_SMSLate = 316;
        public const Int64 C_SMSGeneral = 317;
        public const Int64 C_SystemEntry = 249;
        public const Int64 C_ImageCheckAlert = 252;
        public const Int64 C_LockerAllianceData = 318;
        public const Int64 CustomerLogIDX = 256;
        public const Int64 C_SubmitData = 320;
        public const Int64 C_WhseLoc = 328;
        public const Int64 C_ShipmentVerified = 332;
        public const Int64 C_InventoryVerified = 363;
        public const Int64 C_LabelPrinted = 333;
        public const Int64 C_DigitalArchiveSent = 345;
        public const Int64 C_SMSFailed = 346;
        public const Int64 C_Inventory_Requested = 170;
        public const Int64 C_CustomerCancelled_BadWeather = 335;
        public const Int64 C_CustomerCancelled_ResourceIssue = 336;
        public const Int64 C_CustomerCancelled_NotRequired = 337;
        public const Int64 C_CustomerCancelled_RatesIssue = 338;
        public const Int64 C_Redirected = 389;
        public const Int64 C_CallRecordingRetrieved = 390;
        public const Int64 C_AutoRouted = 396;
        public const Int64 C_ExtC = 348;
        public const Int64 C_ExtO = 349;
        public const Int64 C_NotIn_NotAttempted = 75;
        public const Int64 C_NotIn = 78;
        public const Int64 C_PU = 49;
        public const Int64 C_PIP = 103;
        public const Int64 C_DIP = 104;
        public const Int64 C_OKP = 135;
        public const Int64 C_OKV = 373;
        public const Int64 C_OK = 63;
        public const Int64 C_POD = 96;
        //public const Int64 C_GRN = 111;
        public const Int64 C_CageErr = 113;
        public const Int64 C_SCL = 115;
        public const Int64 C_RD = 80;
        public const Int64 C_NDOC = 372;
        public const Int64 C_RG = 112;
        public const Int64 C_Recall = 262;
        public const Int64 C_RT = 64;
        public const Int64 C_Parked = 155;
        public const Int64 C_OOT_SCHED = 121;
        public const Int64 C_OOT_PSCH = 122;
        public const Int64 C_ND_SCHED = 156;
        public const Int64 C_ND_PSCH = 157;
        public const Int64 C_PNR = 165;
        public const Int64 C_Repick = 235;
        public const Int64 C_FailedPickUp = 128;
        public const Int64 C_FP_BAP = 116;
        public const Int64 C_FP_CAP = 117;
        public const Int64 C_FD_RD04 = 126;
        public const Int64 C_FD_RD06 = 129;
        public const Int64 C_FP_CUA = 164;
        public const Int64 C_FP_NH = 159;
        public const Int64 C_FP_NP = 184;
        public const Int64 C_FP_NR = 163;
        public const Int64 C_FP_WT = 183;
        public const Int64 C_Destroyed = 62;
        public const Int64 C_ReturnedToShipper = 64;
        public const Int64 C_ShipmentStopped = 65;
        public const Int64 C_NotDelivered = 77;
        public const Int64 C_NoGoods = 134;
        public const Int64 C_CODPayment = 148;
        public const Int64 C_HOTO_HO = 166;
        public const Int64 C_HOTO_TO = 167;
        public const Int64 C_Custom_Notification = 160;

        public const Int64 C_BadAddressPU = 116;
        public const Int64 C_BadAddressDIP = 67;
        public const Int64 C_OutOfTime = 136;

        public const Int64 C_TempLogger_Attached = 171;
        public const Int64 C_TempLogger_Detached = 172;
        public const Int64 C_Cold_Item_Handling_Compliance_Successful = 173;
        public const Int64 C_Cold_Item_Handling_Compliance_Failed = 174;

        // Status codes with no need to attend the actual location
        public const Int64 C_InsufficientAddress = 255;
        public const Int64 C_ClosedEarly = 119;
        public const Int64 C_WrongGoods = 127;

        // Invalid Address
        public const Int64 C_WIA = 266; // Invalid or incomplete address
        public const Int64 C_Invalid_Address_Fixed = 397; // Address Fixed

        public const Int64 C_WhseOps = 232;
        public const Int64 C_WhseOps_BD = 344;
        public const Int64 C_WhseCCA = 233;
        public const Int64 C_WhsePSched = 321;
        public const Int64 C_WhseSched = 322;
        public const Int64 C_WhseSCL = 325;
        public const Int64 C_WhseCCSched = 79;
        public const Int64 C_CCSched = 340;
        public const Int64 C_WhseRBK = 393;
        public const Int64 C_IWD_SCH = 154;
        public const Int64 C_IWD_SCL = 359;
        public const Int64 C_IWD_CNCL = 245;

        public const Int64 C_NotRequired = 334;
        public const Int64 C_CompanyCOD = 353;
        public const Int64 C_CustomerCOD = 354;
        public const Int64 C_Luke2UpdatePickError = 392;
        public const Int64 C_Luke2UpdateDeliveryError = 391;
        public const Int64 C_BarCodeRegistered = 356;
        public const Int64 C_BarCodeUnregistered = 357;
        public const Int64 C_StatusUpdateLocation = 360;
        public const Int64 C_VolumetricWeight = 362;
        public const Int64 C_IntUpd = 364;
        public const Int64 C_ArrivedAtPickUp = 368;
        public const Int64 C_ArrivedAtDelivery = 369;
        public const Int64 C_CompletionNotification = 371;
        public const Int64 C_CallRecipient = 388;
        public const Int64 C_PINVerified = 394;
        public const Int64 C_iPhone_Unbricked = 230;
        public const Int64 C_In_Transit = 398;

        // JOBS_IMAGE ImageType values
        public const Int32 it_PhotoImage = 0;
        public const Int32 it_NestleData = 1;
        public const Int32 it_FreightData = 2;
        public const Int32 it_IncidentReport = 3;
        public const Int32 it_TR46CreateDeliveryData = 4;
        public const Int32 it_ExpeditorsCreateWorkOrderData = 5;
        public const Int32 it_SingtelFSWorkOrderData = 6;

        //Schedule Log Indicators
        public const Int32 DriverStats01 = 4;
        public const Int32 DailyJobStats1 = 5; // New version
        public const Int32 DailyJobStats2 = 6;
        public const Int32 M1MaxxSIMLowNotify = 8;
        public const Int32 OpsOutstanding = 9;
        public const Int32 DailyIBSMS_A = 10;
        public const Int32 DailyIBSMS_B = 21;
        public const Int32 M1MaxxSmartPacLowNotify = 11;
        public const Int32 SMSLowNotify = 12;
        public const Int32 DailyDunningStats = 13;
        public const Int32 DailyRestartBackEnd = 14;
        public const Int32 DailyIBPackingList = 15;
        public const Int32 DailyCustCareAction = 16;
        public const Int32 DailyOpsReminder = 17;
        public const Int32 DailyIBClearDormant = 18;
        public const Int32 Daily11GRN = 19;
        public const Int32 Daily17GRN = 20;
        public const Int32 DailyNightIBPackingList = 22;
        public const Int32 WeeklyDatabaseRestore = 23;
        public const Int32 DailyDatabaseRestore = 24;
        public const Int32 DailyCustCare9am = 25;

        public const Int32 DailyCustCare11am = 26;

        //public const Int32 RefreshNewJobsRun = 26;
        public const Int32 RefreshCODListRun = 27;
        public const Int32 PrivacyCleanUpRun = 28;
        public const Int32 MarketSingaporeImport = 29;
        public const Int32 SingTelStatusUpdate = 30;
        public const Int32 Zero1Import = 31;
        public const Int32 LabelPrinting = 32;
        //public const Int32 SweeLeeImport = 33;
        public const Int32 FrayteImport = 34;
        public const Int32 ALPSImport = 35;
        public const Int32 CageInRun = 36;

        #region Ops Notification IDs
        public const Int32 WorkDayOpsNotification1 = 900;
        public const Int32 WorkDayOpsNotification2 = 901;
        public const Int32 WorkDayOpsNotification3 = 902;
        public const Int32 WorkDayOpsNotification4 = 903;
        public const Int32 WorkDayOpsNotification5 = 904;
        public const Int32 WorkDayOpsNotification6 = 905;
        public const Int32 WorkDayOpsNotification7 = 906;
        public const Int32 WorkDayOpsNotification8 = 907;
        public const Int32 WorkDayOpsNotification9 = 908;
        public const Int32 WorkDayOpsNotification10 = 909;
        public const Int32 WorkDayOpsNotification11 = 910;
        public const Int32 WorkDayOpsNotification12 = 911;
        public const Int32 WorkDayOpsNotification13 = 912;
        public const Int32 WorkDayOpsNotification14 = 913;
        public const Int32 WorkDayOpsNotification15 = 914;
        public const Int32 WorkDayOpsNotification16 = 915;
        public const Int32 WorkDayOpsNotification17 = 916;
        #endregion

        #region Customer Reports Scheduled Run Indicators (NOT IN USE)
        public const Int32 CustomerReport1 = 61;
        public const Int32 CustomerReport2 = 62;
        public const Int32 CustomerReport3 = 63;
        public const Int32 CustomerReport4 = 64;
        public const Int32 CustomerReport5 = 65;
        public const Int32 CustomerReport6 = 66;
        public const Int32 CustomerReport7 = 67;
        public const Int32 CustomerReport8 = 68;
        public const Int32 CustomerReport9 = 69;
        public const Int32 CustomerReport10 = 70;
        #endregion End Customer Reports Scheduled Run Indicators (NOT IN USE)

        public const Int32 PrudentialImport = 71;
        public const Int32 SGHImport = 72;
        public const Int32 MEOImport = 73;
        public const Int32 KuronoImport = 74;
        public const Int32 HDIImport = 75;
        public const Int32 StarHubImport = 76;
        public const Int32 GeenetImport = 77;
        public const Int32 HSBCGlobalImport = 78;
        public const Int32 NovogeneImport = 79;
        public const Int32 SinchImport = 80;
        public const Int32 SACHImport = 81;
        public const Int32 PCDreamsImport = 82;
        public const Int32 LalaMoveImport = 83;
        public const Int32 GoGoImport = 84;
        public const Int32 CitiTTSImport = 85;
        public const Int32 JnTImport = 86;

        #region ACI Reports
        public const Int32 ACIReport1 = 801;
        /// <summary>
        /// WOPS AM Clear
        /// </summary>
        public const Int32 ACIReport2 = 802;
        /// <summary>
        /// WOPS PM Clear
        /// </summary>
        public const Int32 ACIReport3 = 803;
        /// <summary>
        /// WOPS AOH Clear
        /// </summary>
        public const Int32 ACIReport4 = 804;
        public const Int32 ACIReport5 = 805;
        public const Int32 ACIReport6 = 806;
        public const Int32 ACIReport7 = 807;
        public const Int32 ACIReport8 = 808;
        public const Int32 ACIReport9 = 809;
        public const Int32 ACIReport10 = 810;
        #endregion

        public const Int32 HourlyIBPP = 100;

        public const Int32 HourlyRun = 400;
        public const Int32 HourlyImportCheck = 500;
        public const Int32 HourlyLockerCheck = 501;
        public const Int32 HourlyGCMONCheck = 502;
        public const Int32 DailyJobStats2Running = 600;
        public const Int32 DailyJobStats2Fix = 601;
        public const Int32 DailyCustCareActionRunning = 1600;
        public const Int32 DailyCustCareActionFix = 1601;
        public const Int32 DigitalArchiveRunning = 1602;
        // Notifications
        public const Int32 ZYMLowAlert = 1603;

        // WS (Web Service/Customer Integration) Indicators
        public const Int64 C_XDelOnline = 1;
        public const Int64 C_LukeUpdate = 2;
        public const Int64 C_DeliveryBooking = 3;
        public const Int64 C_PayPalProcessor = 4;
        public const Int64 C_UNUSED7 = 5;
        public const Int64 C_NestleWS = 6;
        public const Int64 C_TR46 = 7;
        public const Int64 C_UNUSED6 = 8;
        public const Int64 C_UNUSED5 = 9;
        public const Int64 C_CirclesLifeWS = 10;
        public const Int64 C_ALPS_SpecialLabel_WS = 11;
        public const Int64 C_Zero1 = 12;
        public const Int64 C_SingaporeGP = 13;
        public const Int64 C_Kimikim = 14;
        public const Int64 C_ALPSWS = 15;
        public const Int64 C_SGHWS = 16;
        public const Int64 C_eForm = 17;
        public const Int64 C_ICYMIWS = 18;
        public const Int64 C_UNUSED1 = 19;
        public const Int64 C_Zero1Envelope = 20;
        public const Int64 C_AmbiLabs = 21;
        public const Int64 C_M1Shop = 22;
        public const Int64 C_Prudential = 23;
        public const Int64 C_ZYMWS = 24;
        public const Int64 C_UNUSED2 = 25;
        public const Int64 C_MEOWS = 26;
        public const Int64 C_KuronoWS = 27;
        public const Int64 C_HDIWS = 28;
        public const Int64 C_StarHubWS = 29;
        public const Int64 C_GeenetWS = 30;
        public const Int64 C_HSBCGlobalWS = 31;
        public const Int64 C_M1MaxxWS = 32;
        public const Int64 C_NovogeneWS = 33;
        public const Int64 C_MNHWS = 34;
        public const Int64 C_GEHWS = 35;
        public const Int64 C_PEHWS = 36;
        public const Int64 C_SACHWS = 37;
        public const Int64 C_EuroSportsWS = 38;
        public const Int64 C_WoodlandsHealth = 39;
        public const Int64 C_PCDreamsWS = 40;
        public const Int64 C_M1CoriWS = 41;
        public const Int64 C_M1PDCSWS = 42;
        public const Int64 C_CitiTTSWS = 43;
        public const Int64 C_JnTWS = 44;
        public const Int64 C_WoodlandsHealth_Pick = 45;
        public const Int64 C_TTSH_Pick = 46;
        public const Int64 C_NTFGH_Pick = 47;

        public const Int64 C_Placeholder1 = 39;
        public const Int64 C_Placeholder2 = 39;
        public const Int64 C_Placeholder3 = 39;
        public const Int64 C_Placeholder4 = 40;
        public const Int64 C_PrintEnvelope = 89;
        public const Int64 C_Backend = 99;

        public static readonly Int32 L2UnsentMessage = 2000;
        public static readonly Int32 UnsentMessage = 200;
        public static readonly Int32 SentMessage = 20;
        public static readonly Int32 SentReadMessage = 2;
        public static readonly Int32 IncomingUnreadMessage = 10;
        public static readonly Int32 IncomingMessage = 1;

        public const Decimal OversizeCharge = 10m;

        public const Int32 Signal_ChatChanged = 66000015;
        public const Int32 Signal_Performance_Data = 66000041;
        public const Int32 Signal_Refresh_Contact = 66000003;
        public const Int32 Signal_Refresh_Zones = 66000008;
        public const Int32 Print_ConNotes = 200000040;
        public const Int32 Print_CustomerLabels = 200000047;
        public const Int32 System_Log = 9999995;
        public const Int32 Memo_Refresh = 70000015;
        public const Int32 PayPal_Refresh = 71000015;
        public const Int32 LinkedJob_Refresh = 70000020;
        public const Int32 Incoming_SMS = 70000025;
        public const Int32 Incoming_Spool = 70000030;
        public const Int32 Process_Spool = 71000030;
        public const Int32 Job_Refresh = 70000005;
        public const Int32 NewJob_Refresh = 70000033;
        public const Int32 NewJob_Refresh_ = 71000033;
        public const Int32 Send_SMSMessage = 70000009;
        public const Int32 Send_TelegramMessage = 71000009;
        public const Int32 Luke_JobSheets = 1510022;
        public const Int32 COD_Refresh_ = 70000046;
        public const Int32 Signal_PODScans = 70000050;
        public const Int32 Reconcile_JobSheets = 190000028;
        public const Int32 Reconcile_JobSheets_ = 190000029;
        public const Int32 Check_JobSheets = 190000030;
        public const Int32 SignalDriverRefresh = 66000006;
        public const Int32 SignalDriverRefreshJobs = 66000036;
        public const Int32 SignalDriverRefreshSolution = 66000046;
        public const Int32 Signal_CaseResolver = 66000029;
        public const Int32 Signal_IncomingMessage = 66000019;
        public const Int32 Signal_IncomingJobMessage = 66000020;
        public const Int32 Signal_Message_Driver = 66000038; // L2
        public const Int32 Signal_Command_Driver = 66000039; // L2
        public const Int32 Signal_JobChanges_Refresh = 66000043;
        public const Int32 SignalRefreshDriverMon = 66000045;
        public const Int32 Signal_PrePaidReport = 210000041;
        public const Int32 Signal_PrintXDelLabels = 210000048;
        public const Int32 Signal_PrintEnvelopes = 200000050;
        public const Int32 Signal_SetGCLeaveBalance = 70000051;
        public const Int32 Signal_GCLeaveBalance = 71000051;
        public const Int32 Signal_SetStaffLeaveBalance = 70000052;
        public const Int32 Signal_StaffLeaveBalance = 71000052;
        public const Int32 Signal_SetGCLeaveApplication = 70000053;
        public const Int32 Signal_GCLeaveApplication = 71000053;
        public const Int32 Signal_SetStaffLeaveApplication = 70000054;
        public const Int32 Signal_StaffLeaveApplication = 71000054;
        public const Int32 Signal_Force_Logout = 1010101;
        public const Int32 Signal_Boss_Message = 1010102;
        public const Int32 Create_PrePaid_Receipt = 300000005;
        public const Int32 Create_PrePaid_Invoice = 300000006;

        // GlobalSettings Flag
        public const Int32 GS_BCPActived = 1;
        public const Int32 GS_DisableAutoLabelPrint = 2;
        public const Int32 GS_Unused2 = 4;
        public const Int32 GS_Unused3 = 8;
        public const Int32 GS_Unused4 = 16;
        public const Int32 GS_Unused5 = 32;
        public const Int32 GS_Unused6 = 64;
        public const Int32 GS_Unused7 = 128;
        public const Int32 GS_Unused8 = 256;

        public const String C_QueueSkip = "JobQSkip";

        #region Customer Barcode Checks

        private const String JobIXKey = "JX";
        private const String JobICKey = "JC";
        private const String InvoiceIXKey = "IX";
        private const String RSBarCode = "RS";

        #endregion
    }
}

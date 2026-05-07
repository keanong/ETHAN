using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.XDelSys
{
    public static class ClientFlags
    {
        [Flags]
        public enum LabelFlag
        {
            IndividualLabelPerPiece = 1,
            RequireInventory = 2,
            CanSendLabel = 4,
            CanPrintLabel = 8,
            HideCallBackNumber = 16,
            CanPrintEnvelopeCL = 32,   // Circles Life Envelope
            CanPrintEnvelopeNm = 64,    // Normal 110x220 Envelope
            AutoPrintEnabled = 128,   // Master Switch to enable auto print or not
            CanPrintAlpsLabels = 256,   // ALPS (KTPH,IMH,AH) Labels
            CanPrintEnvelopeCL2 = 512,
            ColdItem = 1073741824  // Temporary flag for printing pink labels - not to be used in CLIENT_LABEL table
        }

        public static LabelFlag EnvelopeFlags = LabelFlag.CanPrintEnvelopeCL | LabelFlag.CanPrintEnvelopeNm |
                                                LabelFlag.CanPrintAlpsLabels | LabelFlag.CanPrintEnvelopeCL2;

        [Flags]
        public enum Flag
        {
            DailyScansReturn = 1,  // 0
            DailyScansReturnPDF = 2,
            NoCameraAllowed = 4,
            PUNotRequired = 8,
            AutoPOD = 16,
            NoSlot = 32,
            NFCVerification = 64,
            RequirePIN = 128,
            RequireSignature = 256,
            RequireIDRecording = 512,
            PDFInvoice = 1024,  // 10
            ExcelInvoice = 2048,
            RequireSecurityTag = 4096,
            AllowLiveView = 8192,
            TestAccount = 16384,
            InvoiceReminder = 32768,
            AllFlags = 65535,
            SixDayWeek = 65536,
            SevenDayWeek = 131072,
            VoIPNoCallback = 262144,
            PickAndSend = 524288,  // 20
            AllowRebookRequest = 1048576,
            RequireCustID = 2097152,
            AllowReviewLinkSMS = 4194304,
            EStatementOnly = 8388608,
            AutoRoute = 16777216,
            NoOpsExtensionAllowed = 33554432,
            HighSecurityAccount = 67108864,
            Contactless = 134217728,
            ReattemptReset = 268435456,  // For billing purposes
            ShipmentVerification = 536870912,  // 30
            BillOnAttempt = 1073741824  // 31 - finish
        }

        [Flags]
        public enum Flag2
        {
            CaseResolverRequired = 1,
            AutoSSFPStdRun = 2,
            LeaveOutsideDoor = 4,
            RequirePUSignature = 8,
            OneHrOpsExtension = 16,
            FreeLockerDeposit = 32,
            AddressMaybeInvalid = 64,
            Highlight01 = 128,
            Highlight02 = 256,
            RequireCallBeforeExtension = 512,
            TimeWindowChangeNotification = 1024,  // 10
            NoSMSAllowed = 2048,
            NoConvenienceFee = 4096,
            Cannot_Skip_PIN = 8192,
            Unused15 = 16384,
            Unused16 = 32768,
            Unused17 = 65535,
            Unused18 = 65536,
            Unused19 = 131072,
            Unused20 = 262144,
            Unused21 = 524288,  // 20
            Unused22 = 1048576,
            Unused23 = 2097152,
            Unused24 = 4194304,
            Unused25 = 8388608,
            Unused26 = 16777216,
            Unused27 = 33554432,
            Unused28 = 67108864,
            Unused29 = 134217728,
            Unused30 = 268435456,  // For billing purposes
            Unused31 = 536870912,  // 30
            Unused32 = 1073741824  // 31 - finish
        }


        /// <summary>
        /// Has the flag (all bits in AFlags must be inside IntFlag)
        /// </summary>
        /// <param name="IntFlag"></param>
        /// <param name="AFlag"></param>
        /// <returns></returns>
        public static Boolean HasFlag(Int32 IntFlag, ClientFlags.Flag AFlag)
        {
            return (IntFlag & (Int32)AFlag) == (Int32)AFlag;
        }

        /// <summary>
        /// Has the flag (all bits in AFlags must be inside IntFlag)
        /// </summary>
        /// <param name="IntFlag"></param>
        /// <param name="AFlag"></param>
        /// <returns></returns>
        public static Boolean HasFlag(Int32 IntFlag, ClientFlags.Flag2 AFlag)
        {
            return (IntFlag & (Int32)AFlag) == (Int32)AFlag;
        }

    }
}

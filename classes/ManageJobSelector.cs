using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public class ManageJobSelector
    {
        public long JobsIDX { get; set; }
        public string Title { get; set; } = string.Empty;


        public string JobNo { get; set; } = string.Empty;

        public string RefNo { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string SvcType {  get; set; } = string.Empty;

        public string ExpType { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public string FromAdd { get; set; } = string.Empty;

        public string PUBLOCK { get; set; } = string.Empty;

        public string PUSTREET { get; set; } = string.Empty;

        public string PUUNIT { get; set; } = string.Empty;

        public string PUBUILDING { get; set; } = string.Empty;

        public string PUPOSTALCODE { get; set; } = string.Empty;

        public string PURdy { get; set; } = string.Empty;

        public string PUCompany {  get; set; } = string.Empty;

        public string Sender { get; set; } = string.Empty;

        public string PUTel { get; set; } = string.Empty;

        public string PUMobile { get; set; } = string.Empty;

        public string PUInstruction { get; set; } = string.Empty;

        public string PUAvoid { get; set; } = string.Empty;

        public string ToAdd { get; set; } = string.Empty;

        public string DLBLOCK { get; set; } = string.Empty;

        public string DLSTREET { get; set; } = string.Empty;

        public string DLUNIT { get; set; } = string.Empty;

        public string DLBUILDING { get; set; } = string.Empty;

        public string DLPOSTALCODE { get; set; } = string.Empty;

        public int DLLocationType {  get; set; } = 0;

        public string DLCompany { get; set; } = string.Empty;

        public string Receiver { get; set; } = string.Empty;

        public string DLTel { get; set; } = string.Empty;

        public string DLMobile { get; set; } = string.Empty;

        public string DLInstruction { get; set; } = string.Empty;

        public string DLAvoid { get; set; } = string.Empty;

        public string DateFrom { get; set; } = string.Empty;

        public string DateTo { get; set; } = string.Empty;

        public string ConsignmentURL { get; set; } = string.Empty;

        public string ScanURL { get; set; } = string.Empty;

        public string TrackURL { get; set; } = string.Empty;

        public string SignaturesURL { get; set; } = string.Empty;

        public string InvPDFURL { get; set; } = string.Empty;

        public string LiveTrackingURL { get; set; } = string.Empty;

        public Int64? StatusCodeIDX { get; set; } = 0;

        public decimal? COD { get; set; } = 0;

        public int TabIndex { get; set; } = -1;

        public int? Pcs {  get; set; } = 0;

        public int? Weight {  get; set; } = 0;

        public bool ShowP1 { get; set; } = false;

        public bool ShowP2 { get; set; } = false;

        public bool ShowDelFrom { get; set; } = true;

        public int FLAG { get; set; } = 0;

        public int FLAG2 { get; set; } = 0;
    }
}

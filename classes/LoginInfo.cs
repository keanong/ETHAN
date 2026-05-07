using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XDelServiceRef;

namespace ETHAN.classes
{
    public class LoginInfo
    {
        public ClientInfo clientInfo {  get; set; }
        public DecimalReturn? PrePaidBalance {  get; set; }
        public XDelServiceRef.AddressStructure? defAddress { get; set; }

        public XDelServiceRef.SettingsInfo? customersets { get; set; }

        public XDelServiceRef.SettingsInfo? ContactLvlSettingsInfo { get; set; }

        public XDelOnlineSettings? ClientXDelOnlineSettings { get; set; }

        public XDelServiceRef.XDelOnlineSettings? xdelOnlineSettings { get; set; }

        public LoginInfo() { }
    }
}

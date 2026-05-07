using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public class ContactStructure
    {
        public long IDX { get; set; }
        public long CLIENTADDRESSIDX {  get; set; }
        public string TITLE {  get; set; }
        public string NAME { get; set; }
        public string DEPARTMENT {  get; set; }
        public string TEL {  get; set; }
        public string FAX {  get; set; }
        public string OTHER {  get; set; }
        public string MOBILE {  get; set; }
        public string SI {  get; set; }
        public string EMAILADDRESS {  get; set; }
        public string WEBLOGIN { get; set; }
        public string WEBPASSWORD {  get; set; }
        public bool WEBACCESS {  get; set; }
        public bool ACCESS {  get; set; }
        public string CUSTOMER_NOTES {  get; set; }
        public string COST_CENTER {  get; set; }
        public bool ACTIVE {  get; set; }
        public int FLAG {  get; set; }


        public ContactStructure() { }
    }
}

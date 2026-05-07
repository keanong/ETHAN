using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public class AddressStructure
    {
        public long IDX {  get; set; }
        public long CLIENTIDX {  get; set; }
        public int ADDRESSTYPE {  get; set; }
        public string ACCOUNT {  get; set; }
        public string COMPANY {  get; set; }
        public string BLOCK {  get; set; }
        public string STREET {  get; set; }
        public string UNIT {  get; set; }
        public string BUILDING {  get; set; }
        public string POSTALCODE { get; set; }
        public bool ACTIVE { get; set; }
        public string CUSTOMER_NOTES {  get; set; }
        public ContactStructure[] Contacts {  get; set; }
        public double LAT {  get; set; }
        public double LNG {  get; set; }
        public int DISTANCE_TOLERANCE {  get; set; }
        public int FLAG {  get; set; }
        public int FLAG2 {  get; set; }
        public LocationType LocationType { get; set; } = 0;
        public SearchOrigin Origin { get; set; }


        public AddressStructure() { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public class Property
    {
        enum PropertyState { OFF_MARKET, SALE, PENDING, SOLD,UNKNOWN};
        public int    HouseNumber { get; private set; }
        public string Street { get; private set; }
       
        public string City { get;  set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }
        public string Country { get; private set; }

        public string CompleteAddress { get; private set; }

        public void ProcessHouseAndStreet( string houseandStreetAddress)
        {

        }
        public void ProcessCityStateZip( string citystatezip)
        {

        }

    }
}

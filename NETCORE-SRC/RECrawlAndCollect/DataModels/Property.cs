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

        public int PropertyStatus { get; private set; }

        public int    HouseNumber { get; private set; }
        public string Street { get; private set; }
       
        public string City { get;  set; }
        public string County { get; private set; }
        public string Community { get; private set; }
        public string ZipCode { get; private set; }
        public string Country { get; private set; }

        public string MLS { get; private set; }
        
        // TBD Schools 
       

        #region BASIC_PROP_DATA
        public string PropertyType { get; private set; } // Single Family, Condo, Town Home
        public int Beds { get; private set; }
        public double FullBaths { get; private set; }
        public double HalfBaths { get; private set; }
        public double ThreeBy4Baths { get; private set; }
        public int Garage { get; private set;  }
        public int DedicatedParkingSpots { get; private set; }
        public int SharedParkingSpots { get; private set; }

        public int Sqft { get; private set; }
        public int LotSizeinSqft { get; private set; }

        public int YearBuilt { get; private set; }
        public int YearRemodeled { get; private set; }

        public string Zoning { get; private set; }

        public string ParcelNumber { get; private set; }

        #endregion

        #region PRICE_MARKET_DURATION
        public double Price { get; private set; }
        public double EstimatePrice { get; private set;}
        public double HOA { get; private set; }
        public int timeonMarketinDays { get; private set; }

        // TBD Sales history and Tax History
        #endregion

        #region SCORES

        public int WalkScore { get; private set; }
        public int TransitScore { get; private set; }
        public int BikeScore { get; private set; }

        public int MarketCompeteScore { get; private set; }


        #endregion



        public string CompleteAddress { get; private set; }
        public void ProcessHouseAndStreet(string houseandStreetAddress)
        {

        }
        public void ProcessCityStateZip(string citystatezip)
        {

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonAndUtils;
using Newtonsoft.Json;
using HtmlAgilityPack;
using DataModels;


namespace RedfinUtils
{
    public class RedfinPropHtmlDU
    {
        #region PRIVATE_MEMEBERS

        private string HtmlInputFile { get; set;}
        private string CSVOutputFile { get; set; }

        private HtmlDocument HtmlDoc { get; set; }
        private Property Property { get; set;}

        #endregion

        #region PUBLIC

        public RedfinPropHtmlDU(string inputhtmlFile, string outputCSVFile)
        {
            HtmlInputFile = inputhtmlFile;
            CSVOutputFile = outputCSVFile;
            Property = new Property();
            HtmlDoc = null;
        }

        public bool PerformDU()
        {
            bool returnValue = false;
            // Let us get HtmlDoc
            HtmlDoc = CrawlUtils.GethtmlDocFromXmlOrHtmlFile(HtmlInputFile);

            if(HtmlDoc != null)
            {
                extractCityStateZip();
                extractAddress();
                extractHomePriceBedsAndBaths();
                extractHomeFacts();
                extractPublicFacts();
                extractWalkTransitAndBicycleScores();
                extractCScore();
                extractRentEstimate();
                returnValue = true;
            }
            return returnValue;
        }
        #endregion

        #region PRIVATE_METHODS

        private const string XPATH_PROP_ADDRESS = "//div[@class='street-address']";
        private const string ADDRESS_TITLE = "title";
        private void extractAddress()
        {
            Property.ProcessHouseAndStreet(extractSingleValue(XPATH_PROP_ADDRESS));            
        }

        private const string XPATH_CITY_STATE_ADDRESS = "//div[@data-rf-test-id='abp-cityStateZip']";
        private void extractCityStateZip()
        {
            Property.ProcessCityStateZip(extractSingleValue(XPATH_CITY_STATE_ADDRESS));            
        }

        // <span>895,000</span>
        private const string XPATH_HOME_PRICE = "//div[@data-rf-test-id='abp-price']/div[@class='statsValue']/div/span[2]";
        private const string XPATH_HOME_BEDS  = "//div[@data-rf-test-id='abp-beds']/div[@class='statsValue']";
        private const string XPATH_HOME_BATHS = "//div[@data-rf-test-id='abp-baths']/div[@class='statsValue']";
        private const string XPATH_HOME_SQFT = "//div[@data-rf-test-id='abp-sqFt']/*[@class='statsValue']";

        private void extractHomePriceBedsAndBaths()
        {
            Property.Beds  = extractSingleValue(XPATH_HOME_BEDS);
            Property.Price = extractSingleValue(XPATH_HOME_PRICE);
            Property.Baths = extractSingleValue(XPATH_HOME_BATHS);
            Property.Sqft  = extractSingleValue(XPATH_HOME_SQFT);            
        }
        // //*[@id="house-info"]/div/div/div[6]/div[1]
        private const string XPATH_HOME_FACTS = "//div[@class='keyDetailsList']/div/span";
        private const string XPATH_HOME_FACTS_SPAN = "./span";

        private void extractHomeFacts()
        {
            var properties = extractMultipleValues(XPATH_HOME_FACTS);

            foreach(var property in properties)
            {
                Console.WriteLine($"Key/Value: {property}");
            }
        }

        private const string XPATH_PUBLIC_FACTS_KEYS   = "//div[@class='facts-table']/div/*[@class='table-label']";
        private const string XPATH_PUBLIC_FACTS_VALUES = "//div[@class='facts-table']/div/*[@class='table-value']";

        private void  extractPublicFacts()
        {
            var keys   = extractMultipleValues(XPATH_PUBLIC_FACTS_KEYS);
            var values = extractMultipleValues(XPATH_PUBLIC_FACTS_VALUES);

            int i = 0;
            foreach(var key in keys)
            {
                Console.WriteLine($"Key:{key}, Value: {values[i]}");++i;
            }

        }
                
        private const string XPATH_WALK_SCORE_XPATH    = "//div[@class='transport-icon-and-percentage walkscore']/div[@data-rf-test-name='ws-percentage']/span[@class='value poor']";
        private const string XPATH_TRANSIT_SCORE_XPATH = "//div[@class='transport-icon-and-percentage transitscore']/div[@data-rf-test-name='ws-percentage']/span[@class='value poor']";
        private const string XPATH_BICYCLE_SCORE_XPATH = "//div[@class='transport-icon-and-percentage bikescore']/div[@data-rf-test-name='ws-percentage']/span[@class='value poor']";

        private void extractWalkTransitAndBicycleScores()
        {
            var walkScore     = extractSingleValue(XPATH_WALK_SCORE_XPATH);
            var transitScore  = extractSingleValue(XPATH_TRANSIT_SCORE_XPATH);
            var biCycleScore  = extractSingleValue(XPATH_BICYCLE_SCORE_XPATH);
        }

        private const string XPATH_CSCORE_XPATH = "//div[@class='score most']";

        private void extractCScore()
        {
            var cscore = extractSingleValue(XPATH_CSCORE_XPATH);
        }

        private const string XPATH_RENT_EST_XPATH = "//div[@class='rentalEstimateStats']/div/span";

        private void extractRentEstimate()
        {
            var cscore = extractSingleValue(XPATH_RENT_EST_XPATH);
        }




        // utiltiy funtions
        #region PRIVATE_UTIL_METHODS
        private string extractSingleValue( string xpath)
        {
            var outputStr = string.Empty;
            try
            {
                var selectedNode  = HtmlDoc.DocumentNode.SelectSingleNode(xpath);
                if (selectedNode != null)
                {
                    outputStr = selectedNode.InnerText.Trim();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while processing: {xpath} {e.Message}");
            }
            return outputStr;
        }

        private List<string> extractMultipleValues(string xpath)
        {
            List<string>  results = new List<string>();
            try
            {
                var multipleNodes = HtmlDoc.DocumentNode.SelectNodes(xpath);
                if (multipleNodes != null)
                {
                    foreach( var node in multipleNodes)
                    {
                        results.Add(node.InnerText.Trim());
                    }
                     
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while processing {xpath} Ex:{e.Message}");
            }
            return results;
        }
        #endregion


        #endregion
    }
}

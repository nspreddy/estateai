using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedfinUtils
{
    public class RedfinStatsHtmlDU
    {
        #region PRIVATE_MEMEBERS

        private string HtmlInputFile { get; set; }
        private string CSVOutputFile { get; set; }

        #endregion

        #region PUBLIC

        public RedfinStatsHtmlDU(string inputhtmlFile, string outputCSVFile)
        {
            HtmlInputFile = inputhtmlFile;
            CSVOutputFile = outputCSVFile;
        }

        public bool PerformDU()
        {
            bool returnValue = false;
            //1. Extract Params
            //2. Save data to outputCSV. 

            return returnValue;
        }

        #endregion
    }
}

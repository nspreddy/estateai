using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedfinUtils
{
    public class RedfinPropHtmlDU
    {
        #region PRIVATE_MEMEBERS

        private string HtmlInputFile { get; set;}
        private string CSVOutputFile { get; set; }

        #endregion

        #region PUBLIC

        public RedfinPropHtmlDU(string inputhtmlFile, string outputCSVFile)
        {
            HtmlInputFile = inputhtmlFile;
            CSVOutputFile = outputCSVFile;
        }

        public bool PerformDU()
        {
            bool returnValue = false;

            return returnValue;
        }

        #endregion
    }
}

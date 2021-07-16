using System;
using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;

namespace PropAnalyzer
{
    class PropAnalyzer
    {
        private const string GEOCFGFILEOPTION = "-g|--geodb <value>";        
        private const string INPUTDIROPTION   = "-i|--inputdir <value>";
        private const string OUTPUTDIROPTION  = "-o|--outputdir <value>";

        private const string HELP             = "-? | -h | --help";
        private const string VERSIONOPTION    = "-v|--version";
        private const string VERSION          = "0.1";

        private const string DESC = "Property Analyzers";
        static void Main(string[] args)
        {
            var execName = System.AppDomain.CurrentDomain.FriendlyName;
            var app = new CommandLineApplication();
            app.Name = execName;
            app.Description = DESC;

            #region HELP_VERSION
            app.HelpOption(HELP);
            app.VersionOption(VERSIONOPTION, () =>
            {

                return string.Format($"Version {VERSION}");
            });
            #endregion



        }
    }
}

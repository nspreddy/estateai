using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    class State
    {
        
        private Dictionary<string,County> CountyList = new Dictionary<string,County>();

        public string Name { get; private set; }
        public State( string name)
        {
            Name = name;
        }

        public County getCounty(string countName)
        {
            return null;
        }

        public bool UpsertCounty( string countyName, County county)
        {
            return true;
        }
    }
}

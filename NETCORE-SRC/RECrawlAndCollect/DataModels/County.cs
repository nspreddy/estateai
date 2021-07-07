using System;

namespace DataModels
{
    public class County
    {



        public County(string county)
        {
            Name = county;
        }
        public string Name { get; private set; }

    }
}

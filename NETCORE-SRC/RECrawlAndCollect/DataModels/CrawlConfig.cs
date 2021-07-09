using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataModels
{
    public class CrawlConfig
    {
        [JsonProperty(PropertyName ="geoCfg")]
        public string geoCfgFile { get; set;}

        [JsonProperty(PropertyName = "Critera-Data")]
        public List<Criteria> criterias { get; set; }

        public CrawlConfig()
        {
            criterias = new List<Criteria>();
        }

        public void AddCriteria(Criteria criteria)
        {
            criterias.Add(criteria);
        }

        public void GenerateRandomData(string state, string scopetype, string scopename)
        {
            geoCfgFile = "geoDB.json";
            var criteria = new Criteria();
            criteria.GenerateRandomData(state, scopetype, scopename, Criteria.PROP_SFR);
            AddCriteria(criteria);

            criteria = new Criteria();
            criteria.GenerateRandomData(state, scopetype, scopename, Criteria.PROP_TH);
            AddCriteria(criteria);

            criteria = new Criteria();
            criteria.GenerateRandomData(state, scopetype, scopename, Criteria.PROP_CONDO);
            AddCriteria(criteria);

            criteria = new Criteria();
            criteria.GenerateRandomData(state, scopetype, scopename, Criteria.PROP_LAND);
            AddCriteria(criteria);
        }
    }
}

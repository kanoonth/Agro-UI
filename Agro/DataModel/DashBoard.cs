using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Agro.DataModel
{
    class Dashboard
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }
        
        [JsonProperty("longitude")]
        public string Longtitude { get; set; }

        [JsonProperty("plantation_date")]
        public string PlantationDate { get; set; }

    }
}

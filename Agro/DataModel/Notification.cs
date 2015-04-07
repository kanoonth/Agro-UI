using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Agro.DataModel
{
    public class Notification
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("disease_name")]
        public string DiseaseName { get; set; }

        [JsonProperty("percent_cf")]
        public string PercentCF { get; set; }

    }
}

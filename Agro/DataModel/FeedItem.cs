using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Agro.DataModel
{
    class FeedItem
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("image")]
        public string ImageURL { get; set; }

        [JsonProperty("thumb")]
        public string ThumbURL { get; set; }

        public override string ToString()
        {
            return Title;
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Database.Entities;
using Newtonsoft.Json;

namespace Database
{
    public class PostFactory
    {
        public List<Post> Les(string område)
        {
            var poster = LesPosterFraJson(område);
            var koder = LesKoderFraJson(område);
            
            var defaultPoeng = "100,80,70,60,50";

            var resultat = new List<Post>();
            for (var i = 1; i <= poster.Count; i++)
            {
                var import = poster[i - 1];
                var post = new Post
                    {
                        PostId = Guid.NewGuid(),
                        Navn = string.Format("Post {0}", i),
                        Beskrivelse = import.Description,
                        Latitude = import.Position.Single().Latitude,
                        Longitude = import.Position.Single().Longitude,
                        Altitude = import.Position.Single().Altitude,
                        Image = import.Image.Single(),
                        DefaultPoengArray = defaultPoeng,
                        HemmeligKode = koder.Single(x => x.PostNr == i).Koder.First(x => x.Length > 3),
                        Omraade = område
                    };
                resultat.Add(post);
            }

           
            return resultat;
        }

        private static List<ImportPost> LesPosterFraJson(string område)
        {
            var assembly = typeof(DataContext).Assembly;
            var resourceName = @"Database.ImportData." + område + ".poster.json";
            string postString;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                postString = reader.ReadToEnd();
            }

            var deserialized = JsonConvert.DeserializeObject<List<ImportPost>>(postString);
            return deserialized;
        }

        private static List<ImportPostMedKode> LesKoderFraJson(string område)
        {
            var assembly = typeof(DataContext).Assembly;
            var resourceName = @"Database.ImportData." + område + ".koder.json";
            string postString;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                postString = reader.ReadToEnd();
            }

            var deserialized = JsonConvert.DeserializeObject<List<ImportPostMedKode>>(postString);
            return deserialized;
        }
    }

    public class ImportPostMedKode
    {
        [JsonProperty("postnr")]
        public int PostNr { get; set; }
        [JsonProperty("koder")]
        public string[] Koder { get; set; }
    }

    public class ImportPost
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("position")]
        public ImportPosition[] Position { get; set; }

        [JsonProperty("image")]
        public string[] Image { get; set; }
    }



    public class ImportPosition
    {
        [JsonProperty("source")]
        public string Source { get; set; }
        [JsonProperty("latitude")]
        public double Latitude { get; set; }
        [JsonProperty("longitude")]
        public double Longitude { get; set; }
        [JsonProperty("altitude")]
        public double Altitude { get; set; }

    }

}

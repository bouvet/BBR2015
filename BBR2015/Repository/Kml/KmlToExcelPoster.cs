using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Repository.Import;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

namespace Repository.Kml
{
    public class KmlToExcelPoster
    {
        public List<PostImport.ExcelPost> LesInn(byte[] content)
        {
            var poster = new List<PostImport.ExcelPost>();

            var kml = KmlFile.Load(new MemoryStream(content));

            var root = (SharpKml.Dom.Kml)kml.Root;
            var document = (Document)root.Feature;

            foreach (var folder in document.Features.OfType<Folder>())
            {
                foreach (var placemark in folder.Features.OfType<Placemark>())
                {
                    var name = placemark.Name;
                    var description = placemark.Description;
                    var point = placemark.Geometry as Point;
                    var coordinates = point?.Coordinate ?? new Vector();
                    var image = placemark.ExtendedData?.Data?.FirstOrDefault()?.Value;
                    var post = KonverterTilExcelPost(name, description, coordinates, image, document.Name);
                    poster.Add(post);
                }
            }

            return poster;
        }

        private PostImport.ExcelPost KonverterTilExcelPost(string name, Description description, Vector coordinates, string image, string område)
        {
            var text = description != null ? description.Text : string.Empty;

            return new PostImport.ExcelPost
            {
                Navn = name.Trim(),
                Beskrivelse = StripTagsKodeOgPoengArray(text),
                DefaultPoengArray = LesUtPoengArray(text),
                Latitude = coordinates.Latitude,
                Longitude = coordinates.Longitude,
                Altitude = coordinates.Altitude ?? 0.0,
                Image = image,
                Omraade = område,
                HemmeligKode = LesUtHemmeligKode(text)
            };
        }

        private string LesUtHemmeligKode(string description)
        {
            var match = Regex.Match(description, @"\{(.*?)\}", RegexOptions.Multiline);

            if (match.Success)
                return match.Value.Replace("{", string.Empty).Replace("}", string.Empty).Trim();

            return string.Empty;
        }

        private string StripTagsKodeOgPoengArray(string description)
        {
            // Fjern alt som er inni <>, [] og {} - og trim deretter.

            description = Regex.Replace(description, "<[^>]*>", string.Empty, RegexOptions.Multiline);
            description = Regex.Replace(description, @"\[[^\]]*\]", string.Empty, RegexOptions.Multiline);
            description = Regex.Replace(description, @"\{[^\}]*\}", string.Empty, RegexOptions.Multiline);
            
            return description.Trim();
        }

        private string LesUtPoengArray(string description)
        {
            var match = Regex.Match(description, @"\[(.*?)\]", RegexOptions.Multiline);

            if (match.Success)
                return match.Value.Replace("[", string.Empty).Replace("]", string.Empty).Trim();

            return string.Empty;
        }
    }
}

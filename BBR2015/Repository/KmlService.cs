using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using SharpKml.Base;
using SharpKml.Dom;
using TimeSpan = SharpKml.Dom.TimeSpan;

namespace Repository
{
    public class KmlService
    {
        private readonly DataContextFactory _dataContextFactory;
        private readonly CurrentMatchProvider _currentMatchProvider;

        public KmlService(DataContextFactory dataContextFactory, CurrentMatchProvider currentMatchProvider)
        {
            _dataContextFactory = dataContextFactory;
            _currentMatchProvider = currentMatchProvider;
        }

        public string GetKml()
        {
            var matchId = _currentMatchProvider.GetMatchId();

            Kml kml = new Kml();

            var document = new Document();
            kml.Feature = document;

            using (var context = _dataContextFactory.Create())
            {
                var poster = (from pim in context.PosterIMatch
                             where pim.Match.MatchId == matchId
                             select
                                 new { pim.SynligFraTid,
                                     pim.SynligTilTid,
                                     pim.Post.Navn,
                                     pim.Post.Latitude,
                                     pim.Post.Longitude }).ToList();                               

                foreach (var p in poster)
                {
                    var placemark = new Placemark();
                    placemark.Time = new TimeSpan {Begin = p.SynligFraTid, End = p.SynligTilTid};
                    placemark.Name = p.Navn;
                    placemark.Geometry = new Point {Coordinate = new Vector(p.Latitude, p.Longitude)};

                    document.AddFeature(placemark);
                }               



            }


            Serializer serializer = new Serializer();
            serializer.Serialize(kml);

            return serializer.Xml;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Repository.Kml;

namespace RestApi.Tests
{
    [TestFixture]
    public class KmlToExcelTests
    {
        [Test]
        public void KonverterKmlTilExcelPoster()
        {
            var file = @"..\\..\\TestFiles\\GMap_poster.kml";

            if (!File.Exists(file))
                return;

            var bytes = File.ReadAllBytes(file);

            var konvertering = new KmlToExcelPoster();

            var poster = konvertering.LesInn(bytes);

            Assert.AreEqual(3, poster.Count, "Antall");

            var post1 = poster[0];
            var post2 = poster[1];
            var post3 = poster[2];

            Assert.AreEqual("Testposter", post1.Omraade, "Område");
            Assert.AreEqual("Post 1", post1.Navn, "Navn");
            Assert.AreEqual("En liten beskrivelse av post 1", post1.Beskrivelse, "Beskrivelse");
            Assert.AreEqual("HemmeligKode", post1.HemmeligKode, "Hemmelig kode");
            Assert.AreEqual("https://lh4.googleusercontent.com/proxy/ilcMHHH0", post1.Image, "Image");
            Assert.AreEqual(59.9367521, post1.Latitude, "Latitude");
            Assert.AreEqual(10.7575679, post1.Longitude, "Longitude");
            Assert.AreEqual(0.0, post1.Altitude, "Altitude");

            Assert.AreEqual("Testposter", post2.Omraade, "Område");
            Assert.AreEqual("Post 2", post2.Navn, "Navn");
            Assert.AreEqual("Beskrivelse 2", post2.Beskrivelse, "Beskrivelse");
            Assert.AreEqual("300,80,1", post2.DefaultPoengArray, "DefaultPoengArray");
            Assert.AreEqual(59.9367145, post2.Latitude, "Latitude");
            Assert.AreEqual(10.7612371, post2.Longitude, "Longitude");
            Assert.AreEqual(0.0, post2.Altitude, "Altitude");

            Assert.AreEqual("Testposter", post3.Omraade, "Område");
            Assert.AreEqual("Post 3", post3.Navn, "Navn");
            Assert.AreEqual(string.Empty, post3.Beskrivelse, "Beskrivelse");
            Assert.AreEqual(59.9343665, post3.Latitude, "Latitude");
            Assert.AreEqual(10.754640600000016, post3.Longitude, "Longitude");
            Assert.AreEqual(0.0, post3.Altitude, "Altitude");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OfficeOpenXml;
using Repository.Import;

namespace RestApi.Tests.ExcelImport
{
    [TestFixture]
    public class ExcelWorksheetExtensionTests
    {
        [Test]
        public void GetColumnIndexForKolonneNavn()
        {
            var excel = new ExcelPackage();
            excel.Workbook.Worksheets.Add(ExcelSheet.Match.SheetName);

            var sheet = excel.Workbook.Worksheets[ExcelSheet.Match.SheetName];

            sheet.SetValue(1, 1, ExcelSheet.Match.MatchId);
            sheet.SetValue(1, 24, ExcelSheet.Match.Navn);
            sheet.SetValue(2, 24, "Verdien");

            var indexOfNavn = sheet.GetColumnIndexFor(ExcelSheet.Match.Navn);

            Assert.AreEqual(24, indexOfNavn);
        }

        [Test]
        public void GetValue_ViaColumnIndex()
        {
            var excel = new ExcelPackage();
            excel.Workbook.Worksheets.Add(ExcelSheet.Match.SheetName);

            var sheet = excel.Workbook.Worksheets[ExcelSheet.Match.SheetName];

            sheet.SetValue(1, 1, ExcelSheet.Match.MatchId);
            sheet.SetValue(1, 24, ExcelSheet.Match.Navn);
            sheet.SetValue(2, 24, "Verdien");

            var navn = sheet.GetValue(ExcelSheet.Match.Navn, 2);
            Assert.AreEqual("Verdien", navn);
        }

        [Test]
        public void GetValue_UkjentKolonne_SkalGiBlank()
        {
            var excel = new ExcelPackage();
            excel.Workbook.Worksheets.Add(ExcelSheet.Match.SheetName);

            var sheet = excel.Workbook.Worksheets[ExcelSheet.Match.SheetName];

            sheet.SetValue(1, 1, ExcelSheet.Match.MatchId);
            sheet.SetValue(1, 24, ExcelSheet.Match.Navn);
            sheet.SetValue(2, 24, "Verdien");

            var navn = sheet.GetValue("Does not exist", 2);
            Assert.AreEqual(null, navn);
        }
    }
}

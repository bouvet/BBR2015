using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using NUnit.Framework;
using Repository.Import;

namespace RestApi.Tests.ExcelImport
{
    [TestFixture]
    public class ExcelImporterTests
    {
        [Test, Explicit]
        public async void ImportFrom_GoogleDocs()
        {
            var excelImport = RestApiApplication.CreateContainer().Resolve<Repository.Import.ExcelImport>();

            var downloader = new GoogleDriveDownloader();

            var documentId = Environment.GetEnvironmentVariable("BBR_GoogleDocId", EnvironmentVariableTarget.User);
 
            if(string.IsNullOrEmpty(documentId))
                Assert.Fail("Cannot import if documentId is not set in environment variable");

            var content = await downloader.LastNedSpreadsheetFraGoogleDrive(documentId);
            excelImport.LesInn(content);
        }
    }
}

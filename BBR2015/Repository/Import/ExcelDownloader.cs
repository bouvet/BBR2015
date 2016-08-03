using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Import
{
    public class ExcelDownloader
    {
        public async Task<byte[]> LastNedFraGoogleDrive(string documentId)
        {
            var url = $"https://docs.google.com/spreadsheets/d/{documentId}/export?format=xlsx";

            using (var client = new HttpClient())
            using (var response = await client.GetAsync(url))
            using (var content = response.Content)
            {
                var result = await content.ReadAsByteArrayAsync();

                return result;

            }
        }
    }
}

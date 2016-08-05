using System.Net.Http;
using System.Threading.Tasks;

namespace Repository.Import
{
    public class GoogleDriveDownloader
    {
        public async Task<byte[]> LastNedSpreadsheetFraGoogleDrive(string documentId)
        {
            var url = $"https://docs.google.com/spreadsheets/d/{documentId}/export?format=xlsx";

            return await DownloadBytes(url);
        }

        public async Task<byte[]> LastNedMapFraGoogleDrive(string documentId)
        {
            var url = $"https://www.google.com/maps/d/kml?mid={documentId}&forcekml=1";

            return await DownloadBytes(url);
        }

        private static async Task<byte[]> DownloadBytes(string url)
        {
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

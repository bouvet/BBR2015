using System;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace Repository.Import
{
    public class ExcelImport
    {
        private readonly LagImport _lagImport;
        private readonly MatchImport _matchImport;
        private readonly DeltakerImport _deltakerImport;
        private readonly PostImport _postImport;

        public ExcelImport(LagImport lagImport, MatchImport matchImport, DeltakerImport deltakerImport, PostImport postImport)
        {
            _lagImport = lagImport;
            _matchImport = matchImport;
            _deltakerImport = deltakerImport;
            _postImport = postImport;
        }

        public void LesInn(Guid matchId, byte[] excelBytes)
        {
            using (var stream = new MemoryStream(excelBytes))
            {
                stream.Position = 0;    
                using (ExcelPackage excelFile = new ExcelPackage(stream))
                {
                    var excel = excelFile.Workbook;

                    Les(excel, "Match", x => matchId = _matchImport.Les(x, matchId));
                    Les(excel, "Poster", x => _postImport.Les(x, matchId));
                    Les(excel, "Lag", x => _lagImport.Les(x, matchId));
                    Les(excel, "Deltakere", x => _deltakerImport.Les(x, matchId));
                }
            }
        }

        private void Les(ExcelWorkbook workbook, string sheetName, Action<ExcelWorksheet> action)
        {
            var sheet = workbook.Worksheets.FirstOrDefault(x => x.Name == sheetName);

            if (sheet == null)
                return;

            if (sheet.Dimension == null)
                return;

            action(sheet);
        }       
    }
}
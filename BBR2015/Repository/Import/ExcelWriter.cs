using System;
using System.Collections.Generic;
using System.Linq;
using Database.Entities;
using OfficeOpenXml;

namespace Repository.Import
{
    public class ExcelWriter : IDisposable
    {
        private ExcelPackage _excel;

        public ExcelWriter()
        {
            InitialiserExcelFil();
        }

        private void InitialiserExcelFil()
        {
            _excel = new ExcelPackage();
            _excel.Workbook.Worksheets.Add(ExcelSheet.Poster.SheetName);
            _excel.Workbook.Worksheets.Add(ExcelSheet.Match.SheetName);
            _excel.Workbook.Worksheets.Add(ExcelSheet.Lag.SheetName);
            _excel.Workbook.Worksheets.Add(ExcelSheet.Deltakere.SheetName);
        }

        public byte[] GetAsByteArray()
        {
            return _excel.GetAsByteArray();
        }

        public void Dispose()
        {
            _excel.Dispose();
        }

        public void SkrivTilExcel(MatchImport.ExcelMatch match, List<Lag> lagListe = null, List<PostImport.ExcelPost> poster = null)
        {
            SkrivMatch(match);
            SkrivLag(lagListe);
            SkrivDeltakere(lagListe);
            SkrivPoster(poster);
        }

        public void SkrivPoster(List<PostImport.ExcelPost> poster)
        {
            if (poster == null)
                return;

            var sheet = _excel.Workbook.Worksheets[ExcelSheet.Poster.SheetName];

            // Headers
            for (var i = 0; i < ExcelSheet.Poster.Kolonner.Length; i++)
            {
                sheet.SetValue(1, i + 1, ExcelSheet.Poster.Kolonner[i]);
            }

            int row = 2;
            foreach (var post in poster)
            {
                //sheet.Set(row, ExcelSheet.Poster.PostId, post.PostId.ToString());
                sheet.Set(row, ExcelSheet.Poster.Navn, post.Navn);
                sheet.Set(row, ExcelSheet.Poster.Område, post.Omraade);
                sheet.Set(row, ExcelSheet.Poster.Latitude, post.Latitude.ToString());
                sheet.Set(row, ExcelSheet.Poster.Longitude, post.Longitude.ToString());
                sheet.Set(row, ExcelSheet.Poster.Altitude, post.Altitude.ToString());
                sheet.Set(row, ExcelSheet.Poster.Beskrivelse, post.Beskrivelse);
                sheet.Set(row, ExcelSheet.Poster.HemmeligKode, post.HemmeligKode);
                sheet.Set(row, ExcelSheet.Poster.PoengFordeling, post.DefaultPoengArray);
                sheet.Set(row, ExcelSheet.Poster.BildeUrl, post.Image);

                if(post.SynligFra.HasValue)
                    sheet.Set(row, ExcelSheet.Poster.SynligFra, post.SynligFra.ToString());

                if(post.SynligTil.HasValue)
                    sheet.Set(row, ExcelSheet.Poster.SynligTil, post.SynligTil.ToString());

                row++;
            }
        }

        private void SkrivDeltakere(List<Lag> lagListe)
        {
            if (lagListe == null)
                return;

            var deltakere = from l in lagListe
                            from d in l.Deltakere
                            select new DeltakerImport.ExcelDeltaker
                            {
                                LagId = l.LagId,
                                Navn = d.Navn,
                                Kode = d.Kode
                            };

            var sheet = _excel.Workbook.Worksheets[ExcelSheet.Deltakere.SheetName];

            // Headers
            for (var i = 0; i < ExcelSheet.Deltakere.Kolonner.Length; i++)
            {
                sheet.SetValue(1, i + 1, ExcelSheet.Deltakere.Kolonner[i]);
            }

            int row = 2;
            foreach (var deltaker in deltakere)
            {
                sheet.Set(row, ExcelSheet.Deltakere.Lag, deltaker.LagId);
                sheet.Set(row, ExcelSheet.Deltakere.Navn, deltaker.Navn);
                sheet.Set(row, ExcelSheet.Deltakere.Kode, deltaker.Kode);

                row++;
            }
        }

        private void SkrivLag(List<Lag> lagListe)
        {
            if (lagListe == null)
                return;

            var sheet = _excel.Workbook.Worksheets["Lag"];

            // Headers
            for (var i = 0; i < ExcelSheet.Lag.Kolonner.Length; i++)
            {
                sheet.SetValue(1, i + 1, ExcelSheet.Lag.Kolonner[i]);
            }

            int row = 2;
            foreach (var lag in lagListe)
            {
                sheet.Set(row, ExcelSheet.Lag.LagId, lag.LagId);
                sheet.Set(row, ExcelSheet.Lag.Navn, lag.Navn);
                sheet.Set(row, ExcelSheet.Lag.HemmeligKode, lag.HemmeligKode);
                sheet.Set(row, ExcelSheet.Lag.Farge, lag.Farge);

                row++;
            }
        }

        private void SkrivMatch(MatchImport.ExcelMatch match)
        {
            var sheet = _excel.Workbook.Worksheets["Match"];
            var row = 2;

            // Headers
            for (var i = 0; i < ExcelSheet.Match.Kolonner.Length; i++)
            {
                sheet.SetValue(1, i + 1, ExcelSheet.Match.Kolonner[i]);
            }

            sheet.Set(2, ExcelSheet.Match.MatchId, match.MatchId.ToString());
            sheet.Set(2, ExcelSheet.Match.Navn, match.Navn);
            sheet.Set(2, ExcelSheet.Match.Starttid, match.StartTid.ToString());
            sheet.Set(2, ExcelSheet.Match.Sluttid, match.SluttTid.ToString());
            sheet.Set(2, ExcelSheet.Match.DefaultPostPoengfordeling, match.DefaultPoengFordeling);
            sheet.Set(2, ExcelSheet.Match.GeoBox_NW_latitude, match.GeoboxNWLatitude.ToString());
            sheet.Set(2, ExcelSheet.Match.GeoBox_NW_longitude, match.GeoboxNWLongitude.ToString());
            sheet.Set(2, ExcelSheet.Match.GeoBox_SE_latitude, match.GeoboxSELatitude.ToString());
            sheet.Set(2, ExcelSheet.Match.GeoBox_SE_longitude, match.GeoboxSELongitude.ToString());
            sheet.Set(2, ExcelSheet.Match.Pr_lag_FELLE, match.PrLagFelle.ToString());
            sheet.Set(2, ExcelSheet.Match.Pr_lag_BOMBE, match.PrLagBombe.ToString());
        }
    }
}

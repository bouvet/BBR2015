using System;
using OfficeOpenXml;

namespace Repository.Import
{
    public static class ExcelWorkSheetExtensions
    {
        public static string Read(this ExcelWorksheet sheet, string cell)
        {
            var column = cell[0].ToString();
            var row = int.Parse(cell[1].ToString());

            return GetValueInternal(sheet, column, row);
        }

        private static string GetValueInternal(ExcelWorksheet worksheet, string column, int row)
        {
            var value = worksheet.Cells[column + row].Value;

            return value != null ? value.ToString() : null;
        }

        public static string GetValue(this ExcelWorksheet sheet, string columnHeader, int row)
        {
            var col = sheet.GetColumnIndexFor(columnHeader);

            if (col == 0)
                return null;

            return sheet.GetValue<string>(row, col);
        }

        public static T GetValue<T>(this ExcelWorksheet sheet, string columnHeader, int row)
        {
            var col = sheet.GetColumnIndexFor(columnHeader);

            if (col == 0)
                return default(T);

            return sheet.GetValue<T>(row, col);
        }

        public static void Set(this ExcelWorksheet sheet, int row, string columnHeader, string value)
        {
            var col = sheet.GetColumnIndexFor(columnHeader);

            sheet.SetValue(row, col, value);
        }

        public static int GetColumnIndexFor(this ExcelWorksheet sheet, string columnHeader)
        {
            if (string.IsNullOrEmpty(columnHeader))
                return 0;

            for (var col = 1; col < sheet.Dimension.Columns + 1; col++)
            {
                if (string.Compare((string)sheet.GetValue(1, col), columnHeader, StringComparison.OrdinalIgnoreCase) == 0)
                    return col;
            }

            return 0;
        }
    }
}
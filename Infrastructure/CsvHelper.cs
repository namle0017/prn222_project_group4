using System.Text;

namespace FapWeb.Infrastructure
{
    /// <summary>
    /// Ho tro xuat file CSV cho cac man hinh export.
    /// Truoc day cac controller noi chuoi bang dau phay truc tiep, nen ten co dau phay
    /// (vi du "Nguyen Van A, Jr") lam lech cot toan bo file.
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// Boc gia tri theo chuan CSV: nhan doi dau nhay kep va dat trong dau nhay
        /// khi gia tri co chua dau phay, dau nhay hoac xuong dong.
        ///
        /// Gia tri bat dau bang = + - @ duoc them dau nhay don o dau de Excel khong
        /// hieu la cong thuc (CSV injection).
        /// </summary>
        public static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var text = value;

            if (text[0] is '=' or '+' or '-' or '@')
            {
                text = "'" + text;
            }

            var needsQuotes = text.Contains(',') || text.Contains('"') || text.Contains('\n') || text.Contains('\r');

            return needsQuotes ? '"' + text.Replace("\"", "\"\"") + '"' : text;
        }

        /// <summary>
        /// Ghep mot dong CSV tu cac o da duoc boc.
        /// </summary>
        public static string Row(params string?[] cells)
        {
            return string.Join(",", cells.Select(Escape));
        }

        /// <summary>
        /// Dong goi noi dung CSV kem BOM UTF-8 de Excel doc dung tieng Viet co dau.
        /// </summary>
        public static byte[] ToFileBytes(StringBuilder builder)
        {
            return Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(builder.ToString()))
                .ToArray();
        }
    }
}

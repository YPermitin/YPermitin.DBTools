using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse.Helpers
{
    public static class StringExtensions
    {
        private static readonly Regex _replaceTempTableName = new Regex(@"#tt[\d]+");
        private static readonly Regex _replaceParameterName = new Regex(@"@P([\d]|[N])+");
        private static readonly Regex _replaceSQLParametersForSELECT = new Regex(@"\((@P([\d]|[N])|\?)+[\W\w]+\)(|\n)SELECT");
        private static readonly Regex _replaceSQLParametersForINSERT = new Regex(@"\((@P([\d]|[N])|\?)+[\W\w]+\)(|\n)INSERT");
        private static readonly Regex _replaceSQLParametersForUPDATE = new Regex(@"\((@P([\d]|[N])|\?)+[\W\w]+\)(|\n)UPDATE");
        private static readonly Regex _replaceSQLParametersForDELETE = new Regex(@"\((@P([\d]|[N])|\?)+[\W\w]+\)(|\n)DELETE");

        public static string ClearSQLQuery(this string sourceQuery,
            bool changeTempTableNames = true,
            bool changeSQLParameterNames = true,
            bool removeSQLParametersPart = true)
        {
            if (changeTempTableNames)
                sourceQuery = _replaceTempTableName.Replace(sourceQuery, "#ttN");

            if (changeSQLParameterNames)
                sourceQuery = _replaceParameterName.Replace(sourceQuery, "?");

            if (removeSQLParametersPart)
            {
                sourceQuery = _replaceSQLParametersForSELECT.Replace(sourceQuery, "SELECT");
                sourceQuery = _replaceSQLParametersForINSERT.Replace(sourceQuery, "INSERT");
                sourceQuery = _replaceSQLParametersForUPDATE.Replace(sourceQuery, "UPDATE");
                sourceQuery = _replaceSQLParametersForDELETE.Replace(sourceQuery, "DELETE");
            }

            return sourceQuery.Trim();
        }
        public static string GetQueryHash(this string sourceQuery, bool isQueryFromDBMS = false)
        {
            if (isQueryFromDBMS)
                sourceQuery = sourceQuery.ClearSQLQuery();
            else
                sourceQuery = sourceQuery.ClearSQLQuery(true, false, false);

            sourceQuery = sourceQuery.Replace(" ", string.Empty);
            sourceQuery = sourceQuery.Replace("\r", string.Empty);
            sourceQuery = sourceQuery.Replace("\n", string.Empty);
            sourceQuery = sourceQuery.Replace("\0", string.Empty);

            return sourceQuery.CreateMD5();
        }
        public static string CreateMD5(this string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}

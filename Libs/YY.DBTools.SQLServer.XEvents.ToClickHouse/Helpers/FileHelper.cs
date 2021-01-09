using System;
using System.IO;
using System.Security.Cryptography;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse.Helpers
{
    public static class FileHelper
    {
        public static string GetMD5HashForFile(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
        }
    }
}

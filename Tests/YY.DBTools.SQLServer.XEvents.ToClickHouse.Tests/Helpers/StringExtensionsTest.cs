using System.IO;
using System.Text;
using Xunit;
using YY.DBTools.SQLServer.XEvents.ToClickHouse.Helpers;

namespace YY.DBTools.SQLServer.XEvents.ToClickHouse.Tests.Helpers
{
    public class StringExtensionsTest
    {
        private string _unitTestDirectory;

        public StringExtensionsTest()
        {
            _unitTestDirectory = Directory.GetCurrentDirectory();
        }

        [Fact]
        public void GetSQLQueryHashTest()
        { 
            string logPath = Path.Combine(_unitTestDirectory, "TestData", "GetSQLQueryHashTest.sql");
            string sqlQueryForTest = File.ReadAllText(logPath, Encoding.UTF8);
            string sqlQueryHash = sqlQueryForTest.GetQueryHash();

            Assert.Equal("4A14B6412A46713E743A3C352DA7CD57", sqlQueryHash);
        }

        [Fact]
        public void GetSQLQueryHashWithNullSymbolTest()
        {
            string logPath = Path.Combine(_unitTestDirectory, "TestData", "GetSQLQueryHashWithNullSymbolTest.sql");
            string sqlQueryForTest = File.ReadAllText(logPath, Encoding.UTF8);
            string sqlQueryHash = sqlQueryForTest.GetQueryHash();

            Assert.Equal("4A14B6412A46713E743A3C352DA7CD57", sqlQueryHash);
        }
    }
}

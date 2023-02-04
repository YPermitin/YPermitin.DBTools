using YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models;

namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Tests
{
    public class GeneralTests
    {
        [Fact]
        public void GetBCPAvailableTest()
        {
            BCP bcp = new BCP();
            bool available = bcp.Available();

            Assert.True(available);
        }

        [Fact]
        public void GetBCPNotAvailableTest()
        {
            BCP bcp = new BCP();
            bcp.SetUtilityPath("wrong_bcp.exe");
            bool available = bcp.Available();

            Assert.False(available);
        }

        [Fact]
        public void GetBCPVersionTest()
        {
            BCP bcp = new BCP();
            string version = bcp.Version();

            Assert.NotNull(version);
        }

        [Fact]
        public void GetUtilityPathTest()
        {
            BCP bcp = new BCP();
            string utilityPath = bcp.GetUtilityPath();

            Assert.NotNull(utilityPath);
            Assert.Equal(DefaultValues.UtilityPath, utilityPath);
        }

        [Fact]
        public void SetUtilityPathTest()
        {
            string newUtilityPath = "bcp_test";

            BCP bcp = new BCP();
            bcp.SetUtilityPath(newUtilityPath);

            string utilityPath = bcp.GetUtilityPath();

            Assert.NotNull(utilityPath);
            Assert.Equal(newUtilityPath, utilityPath);
        }
    }
}
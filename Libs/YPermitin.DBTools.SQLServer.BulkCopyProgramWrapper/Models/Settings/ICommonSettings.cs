using System.Text;

namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models.Settings
{
    public interface ICommonSettings
    {
        void AddCommandLineParameters(StringBuilder bcpArguments);
    }
}

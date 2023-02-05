using Microsoft.Extensions.Configuration;

namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Tests
{
    public class ImportExportTests
    {
        private readonly IConfiguration _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        public ImportExportTests()
        {
            BCP.TerminateStuckBulkCopyProcesses();
        }

        [Fact]
        public void ExportAndImportTableWithNativeDataTypesTest()
        {
            #region Export

            var configSectionBCPExport = _configuration.GetSection("BCPExport");
            var configSectionConnectionSettingsExport = configSectionBCPExport.GetSection("ConnectionSettings");
            var configSectionImportExportSettingsExport = configSectionBCPExport.GetSection("ImportExportSettings");
            var configSectionBulkSettingsExport = configSectionBCPExport.GetSection("BulkSettings");
            var configSectionAdditionalSettingsExport = configSectionBCPExport.GetSection("AdditionalSettings");

            BCP bcpExport = new BCP();

            bcpExport.Config.ConnectionSettings
                .WithServerName(configSectionConnectionSettingsExport.GetValue("ServerName", string.Empty))
                .WithDatabaseName(configSectionConnectionSettingsExport.GetValue("DatabaseName", string.Empty))
                .WithSQLServerAuthentication(
                    loginId: configSectionConnectionSettingsExport.GetValue("SQLServerAuthentication:LoginId", string.Empty), 
                    password: configSectionConnectionSettingsExport.GetValue("SQLServerAuthentication:Password", string.Empty));

            bcpExport.Config.ImportExportSettings
                .ExportFromTableToFile(
                    table: configSectionImportExportSettingsExport.GetValue("TableName", string.Empty),
                    dataFile: configSectionImportExportSettingsExport.GetValue("DataFile", string.Empty),
                    schema: configSectionImportExportSettingsExport.GetValue("Schema", string.Empty));

            bool useNativeDataTypeExport = configSectionBulkSettingsExport.GetValue("UseNativeDataTypes", false);
            if (useNativeDataTypeExport)
            {
                bcpExport.Config.BulkSettings
                    .WithNativeDataTypes(
                        useNativeDataTypes: configSectionBulkSettingsExport.GetValue("UseNativeDataTypes", false),
                        withCharactersSupport: configSectionBulkSettingsExport.GetValue("UseNativeDataTypesWithCharactersSupport", false));
            }
            else
            {
                bcpExport.Config.BulkSettings
                    .WithCharacterType(
                        useCharacterType: configSectionBulkSettingsExport.GetValue("UseCharacterType", false),
                        withUnicode: configSectionBulkSettingsExport.GetValue("UseCharacterTypeWithUnicode", false));
            }

            bcpExport.Config.AdditionalSettings
                .WithOutputFile(configSectionAdditionalSettingsExport.GetValue("OutputFile", string.Empty));

            bcpExport.Execute();
            bcpExport.ThrowExceptionIfError();
            bool errorOccurredOnExport = bcpExport.ErrorOccurred();

            #endregion

            #region Import

            var configSectionBCPImport = _configuration.GetSection("BCPImport");
            var configSectionConnectionSettingsImport = configSectionBCPImport.GetSection("ConnectionSettings");
            var configSectionImportExportSettingsImport = configSectionBCPImport.GetSection("ImportExportSettings");
            var configSectionBulkSettingsImport = configSectionBCPImport.GetSection("BulkSettings");
            var configSectionAdditionalSettingsImport = configSectionBCPImport.GetSection("AdditionalSettings");

            BCP bcpImport = new BCP();

            bcpImport.Config.ConnectionSettings
                .WithServerName(configSectionConnectionSettingsImport.GetValue("ServerName", string.Empty))
                .WithDatabaseName(configSectionConnectionSettingsImport.GetValue("DatabaseName", string.Empty))
                .WithSQLServerAuthentication(
                    loginId: configSectionConnectionSettingsImport.GetValue("SQLServerAuthentication:LoginId", string.Empty),
                    password: configSectionConnectionSettingsImport.GetValue("SQLServerAuthentication:Password", string.Empty));
            
            bcpImport.Config.ImportExportSettings
                .ImportFromFileToTable(
                    table: configSectionImportExportSettingsImport.GetValue("TableName", string.Empty),
                    dataFile: configSectionImportExportSettingsImport.GetValue("DataFile", string.Empty),
                    schema: configSectionImportExportSettingsImport.GetValue("Schema", string.Empty),
                    batchSize: 10000);

            bool useNativeDataTypeImport = configSectionBulkSettingsImport.GetValue("UseNativeDataTypes", false);
            if (useNativeDataTypeImport)
            {
                bcpImport.Config.BulkSettings
                    .WithNativeDataTypes(
                        useNativeDataTypes: configSectionBulkSettingsImport.GetValue("UseNativeDataTypes", false),
                        withCharactersSupport: configSectionBulkSettingsImport.GetValue("UseNativeDataTypesWithCharactersSupport", false));
            }
            else
            {
                bcpImport.Config.BulkSettings
                    .WithCharacterType(
                        useCharacterType: configSectionBulkSettingsImport.GetValue("UseCharacterType", false),
                        withUnicode: configSectionBulkSettingsImport.GetValue("UseCharacterTypeWithUnicode", false));
            }

            bcpImport.Config.AdditionalSettings
                .WithOutputFile(configSectionAdditionalSettingsImport.GetValue("OutputFile", string.Empty));

            bcpImport.Execute();
            bcpImport.ThrowExceptionIfError();
            bool errorOccurredOnImport = bcpExport.ErrorOccurred();

            #endregion

            Assert.Equal(0, bcpExport.LastExecutionResult?.ExitCode);
            Assert.Equal(string.Empty, bcpExport.LastExecutionResult?.ErrorMessage);
            Assert.False(errorOccurredOnExport);
            Assert.Equal(0, bcpImport.LastExecutionResult?.ExitCode);
            Assert.Equal(string.Empty, bcpImport.LastExecutionResult?.ErrorMessage);
            Assert.False(errorOccurredOnImport);
        }
    }
}
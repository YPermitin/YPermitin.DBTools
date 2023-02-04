namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Enums
{
    /// <summary>
    /// Типы данных из более ранних версии SQL Server.
    /// </summary>
    public enum SQLServerDataTypeVersion
    {
        /// <summary>
        /// Последняя версия, поддерживаемая утилитой BCP
        /// </summary>
        Latest,

        /// <summary>
        /// 80 = SQL Server 2000 (8.x)
        /// </summary>
        SQLServer2000,

        /// <summary>
        /// 90 = SQL Server 2005 (9.x)
        /// </summary>
        SQLServer2005,

        /// <summary>
        /// 100 = SQL Server 2008 and SQL Server 2008 R2
        /// </summary>
        SQLServer2008,

        /// <summary>
        /// 110 = SQL Server 2012 (11.x)
        /// </summary>
        SQLServer2012,

        /// <summary>
        /// 120 = SQL Server 2014 (12.x)
        /// </summary>
        SQLServer2014,

        /// <summary>
        /// 130 = SQL Server 2016 (13.x)
        /// </summary>
        SQLServer2016
    }
}

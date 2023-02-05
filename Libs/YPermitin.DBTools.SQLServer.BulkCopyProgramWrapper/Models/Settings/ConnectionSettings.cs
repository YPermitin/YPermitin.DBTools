using System.Text;

namespace YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper.Models.Settings
{
    /// <summary>
    /// Настройки подключения к экземпляру SQL Server
    /// </summary>
    public class ConnectionSettings : ICommonSettings
    {
        #region Configs

        /// <summary>
        /// ИмяСервера [\ИмяЭкземпляра] Определяет экземпляр SQL Server, к которому выполняется соединение.
        ///
        /// Если значение не задано, то BCP подключается к экземпляру по умолчанию на локальном компьютере.
        /// Указание данного параметра требуется, когда BCP запускается на удаленном компьютере или подключение выполняется к именованному экземпляру.
        ///
        /// Для подключения к SQL Server с экземпляром по умолчанию достаточно указать имя сервера (без имени экземпляра).
        /// Для подключения к SQL Server с именованным экземпляром, укажите как имя сервера, так и имя самого экземпляра в формате ИмяСервера\ИмяЭкземпляра.
        /// </summary>
        public string? ServerName { get; set; }

        /// <summary>
        /// Задает тайм-аут входа. Параметр указывает количество секунд до истечения времени входа в SQL Server
        /// при попытке подключиться к серверу. Время ожидания входа по умолчанию составляет 15 секунд.
        ///
        /// Время ожидания входа должно быть числом от 0 до 65534. Если предоставленное значение не является числовым или не попадает в этот диапазон,
        /// BCP выдает сообщение об ошибке. Значение 0 указывает бесконечный тайм-аут.
        /// </summary>
        public int LoginTimeout { get; set; }

        /// <summary>
        /// Указывает, что утилита bcp подключается к SQL Server через доверенное соединение с использованием встроенной системы безопасности.
        /// Учетные данные пользователя не требуются.
        /// </summary>
        public bool UseTrustedConnection { get; set; }

        /// <summary>
        /// Указывает идентификатор входа, используемый для подключения к SQL Server.
        /// </summary>
        public string? LoginId { get; set; }

        /// <summary>
        /// Указывает пароль для идентификатора входа в систему.
        /// Если этот параметр не используется, команда bcp запрашивает пароль,  что не поддерживается при работе через библиотеку.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Этот признак используется клиентом при подключении к базе данных SQL Azure
        /// или Azure Synapse Analytics, чтобы указать, что пользователь должен пройти проверку подлинности с помощью
        /// проверки подлинности Azure Active Directory.
        /// </summary>
        public bool UseAzureActiveDirectoryAuthentication { get; set; }

        /// <summary>
        /// Объявляет тип рабочей нагрузки приложения при подключении к серверу.
        /// Единственное возможное значение — ReadOnly.
        ///
        /// Официальная документация.
        /// Readable Secondary Replicas (Always On Availability Groups).
        /// [https://learn.microsoft.com/en-us/sql/database-engine/availability-groups/windows/active-secondaries-readable-secondary-replicas-always-on-availability-groups?view=sql-server-ver16]
        /// </summary>
        public string? ApplicationIntent { get; set; }

        /// <summary>
        /// Указывает количество байтов на сетевой пакет, отправляемых на сервер и с сервера.
        ///
        /// Параметр конфигурации сервера можно задать с помощью SQL Server Management Studio (или системной хранимой процедуры sp_configure).
        /// Однако параметр конфигурации сервера можно переопределить в индивидуальном порядке с помощью этого параметра.
        ///
        /// Может быть от 4096 байт до 65535 байт; по умолчанию 4096.
        ///
        /// Увеличенный размер пакета может повысить производительность операций массового копирования.
        /// Если указанный размер не может быть использован, то используется значение по умолчанию.
        /// Статистика производительности, сгенерированная утилитой BCP, показывает используемый размер пакета.
        /// </summary>
        public int PacketSize { get; set; }

        /// <summary>
        /// Имя базы данных, в которой находится указанная таблица или представление.
        /// Если не указано, это база данных по умолчанию для пользователя.
        /// </summary>
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Имя источника данных - data source name (DSN).
        /// DSN может использоваться для встраивания опций драйвера для упрощения параметров запуска,
        /// применять параметры драйвера, которые иначе недоступны из командной строки, такие как MultiSubnetFailover,
        /// или помочь защитить конфиденциальные учетные данные от обнаружения в качестве аргументов командной строки.
        /// Подробнее смотрите в документации: DSN Support in sqlcmd and bcp in Connecting with sqlcmd.
        /// [https://learn.microsoft.com/en-us/sql/connect/odbc/linux-mac/connecting-with-sqlcmd?view=sql-server-ver16]
        /// </summary>
        public string? DataSourceName { get; set; }

        #endregion

        public ConnectionSettings()
        {
            LoginTimeout = DefaultValues.LoginTimeout;
            UseTrustedConnection = DefaultValues.UseTrustedConnection;
            PacketSize = DefaultValues.PacketSize;
        }

        public ConnectionSettings WithServerName(string serverName)
        {
            ServerName = serverName;

            return this;
        }

        public ConnectionSettings WithDatabaseName(string? databaseName)
        {
            DatabaseName = databaseName;

            return this;
        }

        public ConnectionSettings WithTrustedConnectionAuthentication()
        {
            UseTrustedConnection = true;
            UseAzureActiveDirectoryAuthentication = false;
            LoginId = null;
            Password = null;

            return this;
        }

        public ConnectionSettings WithAzureActiveDirectoryAuthentication()
        {
            UseAzureActiveDirectoryAuthentication = true;
            UseTrustedConnection = false;
            LoginId = null;
            Password = null;

            return this;
        }

        public ConnectionSettings WithSQLServerAuthentication(string loginId, string password)
        {
            UseAzureActiveDirectoryAuthentication = false;
            UseTrustedConnection = false;
            LoginId = loginId;
            Password = password;

            return this;
        }

        public ConnectionSettings WithApplicationIntent(string applicationIntent)
        {
            ApplicationIntent = applicationIntent;

            return this;
        }

        public ConnectionSettings WithDataSourceName(string? dataSourceName)
        {
            DataSourceName = dataSourceName;

            return this;
        }

        public ConnectionSettings WithPacketSize(int packetSize)
        {
            PacketSize = packetSize;

            return this;
        }

        /// <summary>
        /// Добавить параметры командной строки к стоке
        /// </summary>
        /// <param name="bcpArguments">Объект StringBuilder для формирования строки запуска утилиты BCP</param>
        public void AddCommandLineParameters(StringBuilder bcpArguments)
        {
            #region Database name

            if (!string.IsNullOrEmpty(DatabaseName))
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-d ");
                bcpArguments.Append(DatabaseName);
            }

            #endregion

            #region Packet size

            if (PacketSize > 4096 && PacketSize < 65535 && PacketSize != DefaultValues.PacketSize)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-a ");
                bcpArguments.Append(PacketSize);
            }

            #endregion

            #region Data source name

            if (!string.IsNullOrEmpty(DataSourceName))
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-D ");
                bcpArguments.Append(DataSourceName);
            }

            #endregion

            #region Azure Active Directory authentication

            if (UseAzureActiveDirectoryAuthentication)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-G ");
            }

            #endregion

            #region Application Intent

            if (!string.IsNullOrEmpty(ApplicationIntent))
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-K ");
                bcpArguments.Append(ApplicationIntent);
            }

            #endregion

            #region Packet size

            if (LoginTimeout > 0 && LoginTimeout < 65534)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-l ");
                bcpArguments.Append(LoginTimeout);
            }

            #endregion

            #region Password

            if (!string.IsNullOrEmpty(Password))
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-P ");
                bcpArguments.Append("\"");
                bcpArguments.Append(Password);
                bcpArguments.Append("\"");
            }

            #endregion

            #region erver_name [\instance_name]

            if (!string.IsNullOrEmpty(ServerName))
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-S ");
                bcpArguments.Append("\"");
                bcpArguments.Append(ServerName);
                bcpArguments.Append("\"");
            }

            #endregion

            #region Use trusted connection

            if (UseTrustedConnection)
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-T");
            }

            #endregion

            #region LoginId

            if (!string.IsNullOrEmpty(LoginId))
            {
                bcpArguments.Append(" ");
                bcpArguments.Append("-U ");
                bcpArguments.Append("\"");
                bcpArguments.Append(LoginId);
                bcpArguments.Append("\"");
            }

            #endregion
        }
    }
}

namespace Rsx.SQL
{
    /// <summary>
    /// Provides the Methods for SQL Population that are more suited for User Screen Instructions
    /// </summary>
    public partial class SQLUI
    {
        public static string SQL_INSTALL_DUAL_STARTED = "An instance of SQL Server is already installed." +
            "\n\nClick YES to install an alternate Lite version (LocalDB).\n\n" +
            "Click NO to use the current SQL Server instance";

         protected static string LOCALDB_DEFAULT_PATH = "(localdb)\\MSSQLLocalDB";
        protected static string SQL_DENIED_INSTALL = "\n\nThe user denied the SQL Server (LocalDB) installation";
        protected static string SQL_DENIED_INSTALL_TITLE = "Cannot continue without a SQL Server connection";
        protected static string SQL_INSTALL_ASK = "Would you like to install the SQL Server (LocalDB)?";
        protected static string SQL_INSTALL_ASK_TITLE = "SQL LocalDB Installation";
        protected static string SQL_INSTALL_FAILURE = "\n\nInstallation of SQL LocalDB Failed!!!";
        protected static string SQL_INSTALL_FAILURE_TITLE = "Installation of SQL LocalDB did not work";
        protected static string SQL_INSTALL_OK = "\n\nInstallation of SQL LocalDB ran OK";
        protected static string SQL_INSTALL_OK_TITLE = "Installation of the SQL Server finished. Click OK to continue";
        protected static string SQL_INSTALL_STARTED = "Installation of the SQL Server (LocalDB) will execute now.\n\nWhen it is finished please click OK to continue";
        protected static string SQL_INSTALL_STARTED_TITLE = "\n\nInstallation of the SQL Server LocalDB starting...";
    }
}
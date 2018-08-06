using System;
using System.Data.Common;
using System.Data.Linq;
using System.Transactions;

namespace Rsx.SQL
{

    /// <summary>
    /// Provides the methods that should not be called directly but through Populator UI
    /// </summary>
    public partial class SQL
    {
        protected static string LOCALDB_PATH = "\\Microsoft SQL Server\\120\\Tools\\Binn\\";
        // + "Click NO if you want to proceed with the default connection string.\n\nClick Cancel to
        // avoid the population of the Database";

        protected static string SQL_LOCALDB_EXE = "SqlLocalDB.exe";

        protected static string SQL_LOCALDB_PACK32 = "localdbx32.msi";
        protected static string SQL_LOCALDB_PACK64 = "localdbx64.msi";
    }

    /// <summary>
    /// Provides the methods that should not be called directly but through Populator UI
    /// </summary>
    public partial class SQL
    {
        /// <summary>
        /// Deletes completely a given database
        /// </summary>
        /// <param name="localDBPath"></param>
        /// <returns></returns>
        public static bool DeleteDatabase(string localDBPath)
        {
            DataContext destiny;

            destiny = new DataContext(localDBPath);
            bool exist = destiny.DatabaseExists();
            if (exist)
            {
                try
                {
                    //DbConnection db = destiny.Connection;
                    destiny.Connection.Close();
                    //  destiny.Connection.
                    // destiny.Connection.Close();
                    destiny.DeleteDatabase();
                    // destiny.Connection.Open();
                }
                catch (Exception ex)
                {


                }
            }
            return destiny.DatabaseExists();
        }



        /// <summary>
        /// Inserts on Submits a SQL DataTable from one place to another
        /// </summary>
        /// <param name="dt"> </param>
        /// <param name="ita"></param>
        public static void InsertDataTable(ref ITable dt, ref ITable ita, string name)
        {

/*
            using (System.Transactions.TransactionScope trans = new TransactionScope())
            {
                using (YourDataContext context = new YourDataContext())
                {
                    context.ExecuteCommand("SET IDENTITY_INSERT MyTable ON");

                    context.ExecuteCommand("yourInsertCommand");

                    context.ExecuteCommand("SET IDENTITY_INSERT MyTable OFF");
                }
                trans.Complete();
            }





    */

      //      ita.Context.ExecuteCommand("SET IDENTITY_INSERT "+ name+ " ON");
            foreach (var i in dt)
            {
               
                ita.InsertOnSubmit(i);
            }

            ita.Context.SubmitChanges();

       //     ita.Context.ExecuteCommand("SET IDENTITY_INSERT " + name + " OFF");
        }


        /// <summary>
        /// Installs SQL Server LOCALDB with the given pre-requisite filePath resources
        /// </summary>
        /// <param name="prerequisitePath">the files must be called according to sqlPack32 and sqlPack64 strings</param>
        public static string InstallSQL(string prerequisitePath)
        {
            bool is64 = Environment.Is64BitOperatingSystem;
            string localdbexpressPack = SQL_LOCALDB_PACK32;
            if (is64) localdbexpressPack = SQL_LOCALDB_PACK64;
            System.IO.File.WriteAllText(prerequisitePath + "sqlInstall.bat", "msiexec /package \"" + prerequisitePath + localdbexpressPack + "\" /le log.txt");
            Rsx.Dumb.IO.Process("cmd", prerequisitePath, "/c " + "sqlInstall.bat", true);
            string logFile = System.IO.File.ReadAllText( prerequisitePath + "log.txt");

            return logFile;

        }

        /// <summary>
        /// Reinicia el servidor LOCALDB
        /// </summary>
        public static bool RestartSQLLocalDBServer()
        {
            // string start = "start ";
            bool hide = true;
            bool is64 = Environment.Is64BitOperatingSystem;

            string path = string.Empty;
            path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (is64) path = path.Replace(" (x86)", null);

            // else
            string workDir = path + LOCALDB_PATH;
            path = workDir + SQL_LOCALDB_EXE;

            bool exist = System.IO.File.Exists(path);

            if (!exist) return exist;

            //CREATE BATE FILE
            path = SQL_LOCALDB_EXE;
            string tmp = "\\Temp\\";
            string content = "start /B " + path + " start";
            string batFile = "sqlStart.bat";
            string batPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + tmp;
            System.IO.File.WriteAllText(batPath + batFile, content);

            //EXECUTE BAT FILE
            string cmd = "cmd.exe";
            string runas = "runas";

            System.Diagnostics.ProcessStartInfo i = new System.Diagnostics.ProcessStartInfo();
            if (hide) i.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            else i.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            i.Verb = runas;
            i.Arguments = "/c " + batFile;
            i.WorkingDirectory = batPath;
            i.FileName = cmd;
            i.UseShellExecute = false;
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = i;
            process.Start();
            process.WaitForExit(10000);

            return exist;
        }
    }
}
using System.Collections.Generic;
using System.Windows.Forms;

namespace Rsx.SQL
{
    public partial class SQLUI
    {
        public static void ReplaceLocalDBDefaultPath(ref string localDB, string sqlServerFound)
        {
            //changed the database CONNECTION!!!
            //from default
            //to the server that you actually detected

            if (localDB.Contains(LOCALDB_DEFAULT_PATH))
            {
                localDB = localDB.Replace(LOCALDB_DEFAULT_PATH, sqlServerFound);
            }
        }

        /// <summary>
        /// Shows the UI to change the connection String localDB and returns a copy to an equivalent
        /// DB qith Dev name
        /// </summary>
        /// <param name="connectionControl"></param>
        /// <param name="localDB">          </param>
        /// <returns></returns>
        public static string ChangeConnectionString(ref UserControl connectionUsrControl, ref string localDB)
        {
            //make dynamic to access the 2 elements, whic foloows...
            dynamic connectionControl = connectionUsrControl;
            //check if database creation is needed
            //ask the USER NOW if he agrees wih the following Connection String
            connectionControl.Title = "LIMS Server";
            connectionControl.ConnectionString = localDB;

            DialogResult result = MessageBox.Show(CONNECTION_CHANGE, CONNECTION_CHANGE_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                Form form = new Form();

                form.StartPosition = FormStartPosition.CenterScreen;
                form.Text = "Check the Connection String";
                form.TopMost = true;

                form.AutoSizeMode = AutoSizeMode.GrowOnly;
                form.AutoSize = true;
                form.Size = new System.Drawing.Size(connectionControl.Width, connectionControl.Height);
                form.Controls.Add(connectionControl);

                form.ShowDialog();
                localDB = connectionControl.ConnectionString;
            }

            string catalogCmd = "Catalog=";

            //return a copy of the name for for Developer purposes
            return localDB.Replace(catalogCmd, catalogCmd + "Dev");

            // return localDB;
        }

        /// <summary>
        /// Finds a SQL Server instances and installs if necessary with UI Otherwise gives back the
        /// default LocalDB path
        /// </summary>
        public static string FindSQLOrInstall(string developerFolder)
        {
            string sqlFound = LOCALDB_DEFAULT_PATH; //set default path...

            List<string> ls = null;
            ls = SQL.GetLocalSqlServerInstancesByCallingSqlWmi32();
            ls.AddRange(SQL.GetLocalSqlServerInstancesByCallingSqlWmi64());

            string title = SQL_INSTALL_STARTED_TITLE;
            string msg = SQL_INSTALL_STARTED;
            MessageBoxButtons btn = MessageBoxButtons.OK;

            if (ls.Count != 0)
            {   //there are other instances of SQL!!
                //NOW CHANGE THE DAFAULT PATH!!!
                //assign the FIRST SERVER FOUND!!!
           

                // title =SQL_INSTALL_STARTED_TITLE;
                msg = SQL_INSTALL_DUAL_STARTED;
                btn = MessageBoxButtons.YesNo;
            }

            MessageBoxIcon i = MessageBoxIcon.Question;
            DialogResult result = MessageBox.Show(msg, title, btn, i);
            //Install SQL
            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                bool ok = false;
                ok = InstallSQL(developerFolder,true);
                i = MessageBoxIcon.Information;
                if (!ok)
                {
                    i = MessageBoxIcon.Error;
                    sqlFound = ls[0]; //IMPORTANT, ASSIGN AT LEAST THE SQL EXISTENT INSTANCE
                }
                btn = MessageBoxButtons.OK;
                if (ok)
                {

                    //installed LocalDB ok... go ahead and make default database
                 //   MessageBox.Show(SQL_INSTALL_OK_TITLE, SQL_INSTALL_OK, btn, i);
//                    Dumb.IO.RestartPC();
                   
                }
                else
                {
                    //could not install  LocalDB now WHAT??
                    //should work offline then...
                    MessageBox.Show(SQL_INSTALL_FAILURE_TITLE, SQL_INSTALL_FAILURE, btn, i);
                    //TODO: poner algo que hacer si quieres ir offline
                    sqlFound = string.Empty;
                   
                }
            }
            else sqlFound = ls[0]; //IMPORTANT, ASSIGN AT LEAST THE SQL EXISTENT INSTANCE
          
            return sqlFound;
        }

        /// <summary>
        /// Installs SQL with UI
        /// </summary>
        public static bool InstallSQL(string path, bool skipMessage = false)
        {
            
            string logFile = string.Empty;
           //should install SQL?
           DialogResult yesnot = DialogResult.No;
            if (skipMessage) yesnot = DialogResult.Yes;
            else yesnot = MessageBox.Show(SQL_INSTALL_ASK, SQL_INSTALL_ASK_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (yesnot == DialogResult.Yes)
            {
                // string path =;
              logFile =  SQL.InstallSQL(path);
               
            }
            else
            {
                MessageBox.Show(SQL_DENIED_INSTALL_TITLE, SQL_DENIED_INSTALL, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // ok = false;
            }

           // bool ok = !logFile.Contains("failed");
            return  logFile.Contains("higher version already exists");
        }
    }
}
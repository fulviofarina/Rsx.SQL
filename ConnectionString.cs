using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Rsx.SQL
{
    public partial class SQL
    {
        public class ConnectionString
        {
            public string DBTag = "Initial Catalog";
            public string Enlist = "False";
            public string EnlistTag = "Enlist";
            public string Login;
            public string LoginTag = "User ID";
            public string Password;
            public string PasswordTag = "Password";
            public string Pooling = "False";
            public string PoolingTag = "Pooling";
            public string SecurityInfo = "True";
            public string SecurityInfoTag = "Persist Security Info";
            public string ServerTag = "Data Source";
            public string TimeoutTag = "Connect Timeout";
            public string WindowsIdentityTag = "Integrated Security";
            public string WindowsIdentityValue = "True";
            private IList<object> boxes = null;
            private SqlConnection sql = null;
            public string DB
            {
                get
                {
                    return sql.Database;
                }
            }
            public string GetUpdatedConnectionString

            {
                get
                {
                    string formed = string.Empty;
                
                    //use the tags
                    foreach (object box in boxes)
                    {
                        dynamic t = box;

                        if (string.IsNullOrEmpty(t.Text)) continue;

                        formed += t.Tag.ToString() + "=" + t.Text + ";";
                    }
                    //no login?? then use Windows Identity
                    if (!formed.Contains(LoginTag))
                    {
                        formed += WindowsIdentityTag + "=" + WindowsIdentityValue;
                    }
                    else formed = formed.Remove(formed.Length - 1, 1);

                    return formed;
                }
            }

            public string Server
            {
                get
                {
                    return sql.DataSource;
                }
            }

            public string Timeout
            {
                get
                {
                    return sql.ConnectionTimeout.ToString();
                }
            }
            // private string "Integrated Security = True"
            public void SetUI(ref IList<object> Boxes)
            {
                boxes = Boxes;
            }
            /// <summary>
            /// Makes a Connection String Structure, you can use the fields already
            /// </summary>
            /// <param name="connectionString"></param>
            public ConnectionString(string connectionString)
            {
                sql = new SqlConnection(connectionString);

                string[] arr = connectionString.Split(';');

                for (int i = 0; i < arr.Count(); i++)
                {
                    string auxiliarTag = arr[i].Split('=')[0];
                    string value = arr[i].Split('=')[1];
                    if (auxiliarTag.Contains(SecurityInfoTag))
                    {
                        SecurityInfo = value;
                    }
                    else if (auxiliarTag.Contains(LoginTag))
                    {
                        Login = value;
                    }
                    else if (auxiliarTag.Contains(PasswordTag))
                    {
                        Password = value;
                    }
                    else if (auxiliarTag.Contains(EnlistTag))
                    {
                        Enlist = value;
                    }
                    else if (auxiliarTag.Contains(PoolingTag))
                    {
                        Pooling = value;
                    }
                    else if (auxiliarTag.Contains(WindowsIdentityTag))
                    {
                        WindowsIdentityValue = value;
                    }
                }
            }
        }
    }
}
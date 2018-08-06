using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Smo.Wmi;
using Microsoft.Win32;
using System.Data.SqlClient;

namespace Rsx.SQL
{
    /// <summary>
    /// This part takes care of finding the server instances
    /// </summary>
    public partial class SQL
    {

        public static string ReplaceStringForDeveloper(string localDB)
        {
            string catalogCmd = "Catalog=";

            //return a copy of the name for for Developer purposes
            return localDB.Replace(catalogCmd, catalogCmd + "Dev");
        }


        public static bool IsServerConnected(string connectionString)
        {
            using (var l_oConnection = new SqlConnection(connectionString))
            {
                try
                {
                    l_oConnection.Open();
                    return true;
                }
                catch (SqlException)
                {
                    return false;
                }
            }
        }


        public static string Exception;

        /// <summary>
        /// ByCallingBrowser, Takes long
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLocalSqlServerInstancesByCallingSqlBrowser()
        {
            DataTable dt = SmoApplication.EnumAvailableSqlServers(true);
            return dt.Rows.Cast<DataRow>()
                .Select(v => v.Field<string>("Name"))
                .ToList();
        }

        /// <summary>
        /// Funciona y consigue algo más, que es el servidor local??, pero Tarda
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLocalSqlServerInstancesByCallingSqlCmd()
        {
            try
            {
                // SQLCMD -L int exitCode;
                string output;
                int ExitCode = 0;
                processWithOutPut("SQLCMD.exe", "-L", 200, out ExitCode, out output);
                if (ExitCode == 0)
                {
                    string machineName = Environment.MachineName;
                    return output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => v.Trim()).Where(v => !string.IsNullOrEmpty(v))
                        .Where(v => string.Equals(v, "(local)", StringComparison.Ordinal) || v.StartsWith(machineName, StringComparison.OrdinalIgnoreCase))
                        .Select(v => string.Equals(v, "(local)", StringComparison.Ordinal) ? machineName : v)
                        .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                }
                return new List<string>();
            }
            catch (Exception ex)
            {
                Exception = ex.Message + "\n\n" + ex.StackTrace;
                return new List<string>();
            }
        }

        /// <summary>
        /// Rapido funciona 32 bit
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLocalSqlServerInstancesByCallingSqlWmi32()
        {
            return localSqlServerInstancesByCallingSqlWmi(ProviderArchitecture.Use32bit);
        }

        /// <summary>
        /// Rapido funciona 64 bit
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLocalSqlServerInstancesByCallingSqlWmi64()
        {
            return localSqlServerInstancesByCallingSqlWmi(ProviderArchitecture.Use64bit);
        }

        /// <summary>
        /// Mediante Registry
        /// </summary>
        public static List<string> GetLocalSqlServerInstancesByReadingRegInstalledInstances()
        {
            try
            {
                // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\InstalledInstances
                string[] instances = null;
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server"))
                {
                    if (rk != null)
                    {
                        instances = (string[])rk.GetValue("InstalledInstances");
                    }
                    instances = instances ?? new string[] { };
                }
                return getLocalSqlServerInstances(instances);
            }
            catch (Exception ex)
            {
                Exception = ex.Message + "\n\n" + ex.StackTrace;
                return new List<string>();
            }
        }

        /// <summary>
        /// Mediante Registry (No furula)
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLocalSqlServerInstancesByReadingRegInstanceNames()
        {
            try
            {
                // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL
                string[] instances = null;
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL"))
                {
                    if (rk != null)
                    {
                        instances = rk.GetValueNames();
                    }
                    instances = instances ?? new string[] { };
                }
                return getLocalSqlServerInstances(instances);
            }
            catch (Exception ex)
            {
                Exception = ex.Message + "\n\n" + ex.StackTrace;
                return new List<string>();
            }
        }

        /// <summary>
        /// No es necesario y no lo uso, crea dictionario de metodos, investigar
        /// </summary>
        public static void MakeDictionary()
        {
            Dictionary<string, Func<List<string>>> methods;
            methods = new Dictionary<string, Func<List<string>>>
            {
                    { "CallSqlBrowser", GetLocalSqlServerInstancesByCallingSqlBrowser },
                    { "CallSqlWmi32", GetLocalSqlServerInstancesByCallingSqlWmi32 },
                    { "CallSqlWmi64", GetLocalSqlServerInstancesByCallingSqlWmi64 },
                    { "ReadRegInstalledInstances", GetLocalSqlServerInstancesByReadingRegInstalledInstances },
                    { "ReadRegInstanceNames", GetLocalSqlServerInstancesByReadingRegInstanceNames },
                    { "CallSqlCmd", GetLocalSqlServerInstancesByCallingSqlCmd },
            };
            Dictionary<string, List<string>> dictionary;
            dictionary = methods.AsParallel()
                .ToDictionary(v => v.Key, v => v.Value()
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList());
            foreach (KeyValuePair<string, List<string>> pair in dictionary)
            {
                Exception = string.Format("~~{0}~~", pair.Key);
                // Console.WriteLine();
                pair.Value.ForEach(v => Exception += (" " + v));
            }
            // Console.WriteLine("Press any key to continue."); Console.ReadKey();
        }

        private static List<string> getLocalSqlServerInstances(string[] instanceNames)
        {
            string machineName = Environment.MachineName;
            const string defaultSqlInstanceName = "MSSQLSERVER";
            return instanceNames
                .Select(v => (string.IsNullOrEmpty(v) || string.Equals(v, defaultSqlInstanceName, StringComparison.OrdinalIgnoreCase)) ? machineName : string.Format("{0}\\{1}", machineName, v))
                .ToList();
        }

        /// <summary>
        /// El que menos tarda
        /// </summary>
        /// <param name="providerArchitecture"></param>
        /// <returns></returns>
        private static List<string> localSqlServerInstancesByCallingSqlWmi(ProviderArchitecture providerArchitecture)
        {
            string msg = string.Empty;
            Exception = msg;
            try
            {
                ManagedComputer managedComputer32 = new ManagedComputer();
                managedComputer32.ConnectionSettings.ProviderArchitecture = providerArchitecture;
                const string defaultSqlInstanceName = "MSSQLSERVER";
                return managedComputer32.ServerInstances
                    .Cast<ServerInstance>()
                    .Select(v => (string.IsNullOrEmpty(v.Name) || string.Equals(v.Name, defaultSqlInstanceName, StringComparison.OrdinalIgnoreCase)) ? v.Parent.Name : string.Format("{0}\\{1}", v.Parent.Name, v.Name))
                    .OrderBy(v => v, StringComparer.OrdinalIgnoreCase).ToList();
            }
            catch (SmoException ex)
            {
                msg = ex.Message + "\n\n" + ex.StackTrace;
                //return new List<string>();
            }
            catch (Exception ex)
            {
                msg = ex.Message + "\n\n" + ex.StackTrace;
                //return new List<string>();
            }
            Exception = msg;
            return new List<string>();
        }

        /// <summary>
        /// Opens a process
        /// </summary>
        private static void processWithOutPut(string exeName, string arguments, int timeoutMilliseconds, out int exitCode, out string output)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = exeName;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                output = process.StandardOutput.ReadToEnd();
                bool exited = process.WaitForExit(timeoutMilliseconds);
                if (exited) { exitCode = process.ExitCode; }
                else
                {
                    exitCode = -1;
                }
            }
        }
    }
}
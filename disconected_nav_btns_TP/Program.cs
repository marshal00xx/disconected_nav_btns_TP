using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace disconected_nav_btns_TP {
    internal static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        private static SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder() {
            DataSource = ".",
            InitialCatalog = "clientDB",
            IntegratedSecurity = true
        };
        public static SqlConnection connection = new SqlConnection(connectionString.ToString());
        public static SqlDataAdapter adapter = new SqlDataAdapter("SELECT  * FROM students5", connection);
        public static DataSet ds = new DataSet();
        public static SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
        
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GestionStagaireForm());

            Program.adapter.SelectCommand = new SqlCommand("SELECT * FROM students5", Program.connection);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing.Text;
using System.Text.RegularExpressions;
using Guna.UI2.WinForms;

enum Switcher { Enable, Disable }

namespace disconected_nav_btns_TP {
    public partial class GestionStagaireForm : Form {
        public GestionStagaireForm() {
            InitializeComponent();
        }

        private SqlDataAdapter adapter = Program.adapter;
        int tracker = 0;
        private void GestionStagaireForm_Load(object sender, EventArgs e) {
            loadData2DGV();
        }
        private void DGV_mouseClick(object sender, MouseEventArgs e) {
            try {
                cinTB.Text = dgv.SelectedRows[0].Cells["CIN"].Value.ToString();
                nomTB.Text = dgv.SelectedRows[0].Cells["nom"].Value.ToString();
                prenomTB.Text = dgv.SelectedRows[0].Cells["prenom"].Value.ToString();
                addressTB.Text = dgv.SelectedRows[0].Cells["address"].Value.ToString();
                ageTB.Text = dgv.SelectedRows[0].Cells["age"].Value.ToString();
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }
        private void clearAllButton_click(object sender, EventArgs e) {
            clearAll();
            enableOrDesableTB(false);
        }
        private void findPeopleBTN_click(object sender, EventArgs e) {
            clearAll();
            cinTB.Enabled = true;
        }

        private void cinTB_KeyDown(object sender, KeyEventArgs e) {
            String Ex = @"[^0-9]";
            Regex re = new Regex(Ex);

            if (e.KeyCode == Keys.Enter && !re.IsMatch(cinTB.Text)) {
                try {
                    DataRow row = findRow();
                    if (row == null)
                        enableOrDesableTB(true);
                    else {
                        // todo : make the datarow of this user the only one in the dgv
                        MessageBox.Show("Student already in the database", "Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        filterDgv("cin", Convert.ToInt32(cinTB.Text));
                    }

                }
                catch (Exception ex) {
                    Debug.WriteLine(ex.Message);
                    transactionFailed();
                }
            }
            else if (e.KeyCode == Keys.Enter && re.IsMatch(cinTB.Text))
                MessageBox.Show("CIN invalid");
        }
        
        private void insertBtn_Click(object sender, EventArgs e) {
            try {
                DataRow row = findRow();

                if(row == null) {
                    row = Program.ds.Tables[0].NewRow();
                    row["id"] = generateID();
                    row["cin"] = cinTB.Text;
                    row["nom"] = nomTB.Text;
                    row["prenom"] = prenomTB.Text;
                    row["address"] = addressTB.Text;
                    row["age"] = ageTB.Text;

                    Program.ds.Tables["stagaire"].Rows.Add(row);

                    validateTransaction();
                    Program.ds.AcceptChanges();
                    MessageBox.Show("Student added successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }
            catch (Exception ex){
                Debug.WriteLine(ex.Message);
                transactionFailed();
            }
        }
        
        private void deleteBTN_Click(object sender, EventArgs e) {
            try { 
                DataRow row = findRow();
                if(row != null) {
                    row.Delete();
                    validateTransaction();
                    MessageBox.Show("Student removed successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                transactionFailed();
            }
        }
        private void updateRowButton_click(object sender, EventArgs e) {
            DataRow row = findRow();
            try {
                row.BeginEdit();
                row["cin"] = cinTB.Text;
                row["nom"] = nomTB.Text;
                row["prenom"] = prenomTB.Text;
                row["address"] = addressTB.Text;
                row["age"] = ageTB.Text;
                row.EndEdit();
                validateTransaction();
                MessageBox.Show("Student updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                transactionFailed();
            }
        }
        private void updateBTN_Click(object sender, EventArgs e) {
            enableOrDesableTB(true);
        }
        private void navBtns_Click(object sender, EventArgs e) {
            Guna2CircleButton button = (Guna2CircleButton)sender;
            String op = button.Text;
            int max = Program.ds.Tables["stagaire"].Rows.Count - 1;
            try {
                switch (op) {
                    case "<<":
                        tracker = 0;
                        loadData2TB(tracker);
                        break;
                    case "<":
                        tracker = tracker - 1 > 0 ? tracker - 1 : 0;
                        loadData2TB(tracker);
                        break;
                    case ">":
                        tracker = tracker + 1 < max ? tracker + 1 : max;
                        loadData2TB(tracker);
                        break;
                    case ">>":
                        tracker = max;
                        loadData2TB(tracker);
                        break;
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
            
        }
        private void loadData2TB(int n) {
            try {
                cinTB.Text = Program.ds.Tables["stagaire"].Rows[n]["cin"].ToString();
                nomTB.Text = Program.ds.Tables["stagaire"].Rows[n]["nom"].ToString();
                prenomTB.Text = Program.ds.Tables["stagaire"].Rows[n]["prenom"].ToString();
                addressTB.Text = Program.ds.Tables["stagaire"].Rows[n]["address"].ToString();
                ageTB.Text = Program.ds.Tables["stagaire"].Rows[n]["age"].ToString();
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }   
        }









        // methods used in this TP
        //
        //
        //
        //

        //this method loads the whole table to the dgv
        protected void loadData2DGV() {
            try {
                using (Program.adapter) {
                    adapter.SelectCommand = new SqlCommand("SELECT * FROM students5", Program.connection);
                    Program.adapter.Fill(Program.ds, "stagaire");
                    //dgv.DataSource = new BindingSource(Program.ds.Tables["stagaire"], "");
                    dgv.DataSource = Program.ds.Tables["stagaire"];
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        // This method occurs when the transaction fails
        private void transactionFailed() {
            clearAll();
            enableOrDesableTB(false);
            MessageBox.Show("transaction failed", "failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Program.ds.RejectChanges();
        }

        // this method is used to validate the data in the db
        private void validateTransaction() {
            Program.adapter.SelectCommand = new SqlCommand("SELECT * FROM students5", Program.connection);
            Program.adapter.Update(Program.ds.GetChanges(), "stagaire");
            Program.ds.AcceptChanges();
            clearAll();
            enableOrDesableTB(false);
            dgv.Refresh();
        }
        // this method is used to find and return a specific row from the ds if not found it returns null
        private DataRow findRow() {
            DataRow dataRow = null;
            try {
                DataTable table = Program.ds.Tables["stagaire"];

                DataColumn[] primaryKeys = new DataColumn[1];
                primaryKeys[0] = table.Columns["cin"];
                table.PrimaryKey = primaryKeys;

                DataRow row = table.Rows.Find(cinTB.Text);

                dataRow = row;

            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                transactionFailed();
            }
            return dataRow;
        }
        // this method is used to generate a new primary key
        private int generateID() {
            int newId = 0;
            try {
                newId = Convert.ToInt32(dgv.Rows[dgv.RowCount - 1].Cells["id"].Value) + 1;
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
            return newId;
        }
        // this method clears every TB
        private void clearAll() {
            cinTB.Clear();
            nomTB.Clear();
            prenomTB.Clear();
            addressTB.Clear();
            ageTB.Clear();
        }
        // this method filters the dgv
        private void filterDgv(String columnName, int filterValue) {
            /*
             * Method 1 :
             *      --> using the filter property of DataView
             *      --> it sucks +_+
             *      --> it doesn't scale
             *      
             *      (dgv.DataSource as DataTable).DefaultView.RowFilter = string.Format("[{0}] = '{1}'", columnName, filterValue);      
             */

            /*
             * Method 2:
             *      --> not the coolest solution but it's 80% faster
             *      --> still got the pluging problems
             *      --> next time :)
             */
            try {
                MessageBox.Show((dgv.DataSource as DataTable).Rows.Count.ToString());
                DataTable table = (dgv.DataSource as DataTable).AsEnumerable()
                    .Where(row => row.Field<int>(columnName) == filterValue)
                    .CopyToDataTable();
                dgv.DataSource = new BindingSource(table, "");
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }

            /*DataRow dataRow = table.Rows[0];
            MessageBox.Show(dataRow.Field<int>("cin").ToString());
            dgv.Rows.OfType<DataGridViewRow>().ToList().ForEach(row => row.Visible = false);
            dgv.Columns.OfType<DataGridViewColumn>().ToList().ForEach(column => column.Visible = false);*/
            /*foreach(DataGridViewRow row in dgv.Rows) {
                row.Visible = false;
                *//*if (row.Cells["cin"].Value.ToString() != dataRow.Field<int>("cin").ToString())
                    row.Visible = false;
                else
                    row.Visible = true;*//*
            }*/
        }
        // this is a switcher to enable or desable the TBs
        protected void enableOrDesableTB(Boolean x) {
            nomTB.Enabled = x;
            prenomTB.Enabled = x;
            addressTB.Enabled = x;
            ageTB.Enabled = x;
        }


    }
}

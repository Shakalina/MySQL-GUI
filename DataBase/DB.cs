﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace DataBase
{
    public partial class DB : Form
    {
        private MySqlController controller = null;
        private MySqlConnection conn = null;
        private string selectedItem = "";
        private string previousValue;

        public DB(MySqlConnection conn)
        {
            InitializeComponent();
            this.conn = conn;
            controller = new MySqlController(conn,dataGridView1);
        }

        private void DB_Load(object sender, EventArgs e)
        {
            if (controller.loadListView(listBox1))
            {
                Console.WriteLine("ListBox is filled");
            }
            else
            {
                Console.WriteLine("ListBox is not filled");
            }
        }

        private void DB_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (controller.closeConnection())
            {
                Console.WriteLine("Connection is closed");
            }
            else
            {
                Console.WriteLine("Connection is not closed");

            }
            Application.Exit();
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null) { return; }

            string item = listBox1.SelectedItem.ToString();

            if (selectedItem == item) { return; }

            selectedItem = item;
            if (!controller.loadTable(selectedItem, null))
            {
                MessageBox.Show("Can't load table " + selectedItem);
            }
        }

        private void editMode(bool b)
        {
            button1.Enabled = !b;
            button2.Enabled = b;
            button3.Enabled = b;
            button4.Enabled = b;
            button5.Enabled = !b;
            button6.Enabled = !b;
            checkBox1.Enabled = !b;
            button9.Enabled = !b;

            dataGridView1.ReadOnly = !b;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (selectedItem == "")
            {
                MessageBox.Show("Select table!");
            }
            else
            {
                editMode(true);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (controller.RunTransactions())
            {
                MessageBox.Show("Success!");
            }
            else {
                MessageBox.Show("Error!");
            }
        }

        public static string getByType(string value) {

            if (value == "") {
                return " IS NULL";
            }
            try
            {
               double d = Convert.ToDouble(value);
               return " = " + d.ToString();
            }
            catch (Exception ex){
                Console.WriteLine(value + " is not double\n" + ex.ToString());
            }
            return " = '" + value + "'";
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            controller.addCommand(createCommand(e,"Update"));
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            previousValue = dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString();
            Console.WriteLine("Previous value - " + previousValue);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!controller.loadTable(selectedItem,null)) {
                MessageBox.Show("Can't upadte table");
            }
            editMode(false);
        }

        private string createCommand(DataGridViewCellEventArgs e,string command) {
            string request = "";
            if (command.Equals("Update", StringComparison.InvariantCultureIgnoreCase))
            {
                request = "Update " + listBox1.SelectedItem.ToString() +
                               " set " + dataGridView1.Columns[e.ColumnIndex].HeaderText
                               + getByType(dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString());
            }
            else if (command.Equals("Delete", StringComparison.InvariantCultureIgnoreCase))
            {
                request = "Delete from " + listBox1.SelectedItem.ToString();
            }
            else {
                return "";
            }

           request += " where";

        string temp = "";

            foreach (DataGridViewColumn col in dataGridView1.Columns) {
                if (col.ToString() == dataGridView1.Columns[e.ColumnIndex].ToString()) {

                    temp += " " + col.HeaderText + getByType(previousValue);
    }
                else
                {
                    temp += " " + col.HeaderText + getByType(dataGridView1[col.Index, e.RowIndex].Value.ToString());
}
                if (col.Index != dataGridView1.Columns.Count - 1)
                {
                    temp += " And ";
                }
            }
            request += temp;
            request += ";";
            Console.WriteLine(request);
            return request;
        }

        private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewCellEventArgs dataGridViewCellEventArgs = new DataGridViewCellEventArgs(0, e.Row.Index);
            controller.addCommand(createCommand(dataGridViewCellEventArgs, "Delete"));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Insert insert = new Insert(dataGridView1.Columns,controller,listBox1.SelectedItem.ToString());
            if (insert.ShowDialog() == DialogResult.OK) {
                controller.addCommand(insert.ReturnValue);
                Console.WriteLine(insert.ReturnValue);
                controller.loadTable(listBox1.SelectedItem.ToString(),null);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            controller.backup();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            controller.import();
            controller.loadListView(listBox1);
            listBox1.ClearSelected();
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();
            editMode(false);
        }

        private void replasekeys(bool onCascadeDelete)
        {
            MySqlDataReader keys = controller.getKeys(null,controller.getDataBase);
            string onDelete = "";
            if (onCascadeDelete)
            {
                onDelete = "ON DELETE CASCADE";
            }
            while (keys.Read()) {

                string table = keys.GetString("TABLE_NAME");
                string column = keys.GetString("COLUMN_NAME");
                string refTable = keys.GetString("REFERENCED_TABLE_NAME");
                string refColumn = keys.GetString("REFERENCED_COLUMN_NAME");
                string key = keys.GetString("CONSTRAINT_NAME");

                string newKey = "ALTER TABLE " + table + " ADD CONSTRAINT " + 
                    key + " FOREIGN KEY (" + column + ") REFERENCES " + 
                    refTable + "(" + refColumn + ")" + onDelete + ";";
                string dropKey = "ALTER TABLE " + table + " DROP FOREIGN KEY `" + key + "`;";

                controller.addCommand(dropKey);
                controller.addCommand(newKey);
            }
            keys.Close();
            controller.RunTransactions();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (selectedItem == "")
            {
                MessageBox.Show("Select table!");
                return;
            }

            Search search = new Search(controller,listBox1.SelectedItem.ToString());
            search.Visible = true;

        }

        private void checkBox1_MouseClick(object sender, MouseEventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Attantion! All keys will be changed.\nThis process will be irreversible.", "Edit keys", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                if (checkBox1.Checked)
                {
                    replasekeys(true);
                }
                else
                {
                    replasekeys(false);
                }
            }
            else
            {
                checkBox1.Checked = false;
            }
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            Report report = new Report(conn);
            report.ShowDialog();
        }
    }
}

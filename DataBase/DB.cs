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
using MySql.Data.MySqlClient.X.XDevAPI.Common;

namespace DataBase
{
    public partial class DB : Form
    {
        private MySqlController controller = null;
        private string selectedItem;
        private string previousValue;

        public DB(MySqlConnection conn)
        {
            InitializeComponent();
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
            if (!controller.loadTable(selectedItem))
            {
                MessageBox.Show("Can't load table " + selectedItem);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = true;

            dataGridView1.ReadOnly = false;
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

        private string getByType(string value) {

            if (value == "") {
                return " IS NULL";
            }
            try
            {
               double d = Convert.ToDouble(value);
               return " = " + d.ToString();
            }
            catch (Exception ex){
                Console.WriteLine(value + " is not double");
            }
            return " = '" + value + "'";
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
          string request = "Update " + listBox1.SelectedItem.ToString() +
                " set " + dataGridView1.Columns[e.ColumnIndex].HeaderText +
                " = '" + dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString() + "' where";

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
            controller.addCommand(request);
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            previousValue = dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString();
            Console.WriteLine("Previous value - " + previousValue);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!controller.loadTable(selectedItem)) {
                MessageBox.Show("Can't upadte table");
            }
        }
    }
}

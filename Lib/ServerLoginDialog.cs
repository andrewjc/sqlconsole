using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
namespace System.Data.OrionDB
{
    public partial class ServerLoginDialog : Form
    {
        public ServerLoginDialog()
        {
            InitializeComponent();
        }

        private void aoButton1_Click(object sender, EventArgs e)
        {
            if (aoPanel1.Visible == false)
            {
                aoPanel1.Visible = true;
            }
            else
            {
                aoPanel1.Visible = false;
            }
        }

        public DialogResult showDialog()
        {
            if (System.IO.File.Exists("settings.server.xml"))
            {
                settingsClass settings = new settingsClass();
                
                XmlSerializer s = new XmlSerializer(typeof(settingsClass));
                TextReader w = new StreamReader(@"settings.server.xml");
                settings = (settingsClass)s.Deserialize(w);
                w.Close();
                this.txtHost.Text = settings.server;
                this.txtUsername.Text = settings.userid;
                this.txtPassword.Text = settings.password;
                this.txtDB.Text = settings.database;
                this.setProvider(settings.dbtype.ToString());
            }
            //this.comboBox1.SelectedIndex = 1;
            return this.ShowDialog();
        }

        public string getHostname() { return this.txtHost.Text; }
        public string getUsername() { return this.txtUsername.Text; }
        public string getPassword() { return this.txtPassword.Text; }
        public string getDatabase() { return this.txtDB.Text; }
        public System.Data.OrionDB.DATABASETYPES getProvider()
        {
            switch (this.comboBox1.SelectedIndex)
            {
                case 0: return DATABASETYPES.MYSQL;
                case 1: return DATABASETYPES.MSSQL;
                case 3: return DATABASETYPES.MSACCESS;
                default:
                    return DATABASETYPES.MSSQL;
            }
        }

        public void setProvider(string provider) {
            /*
            MySQL
            MS-SQL 2000/2005
            Access
            Oracle
             * */
            switch (provider.ToLower())
            {
                case "mysql":
                    this.comboBox1.SelectedIndex = 0;
                    break;
                case "ms-sql 2000/2005":
                    this.comboBox1.SelectedIndex = 1;
                    break;
                case "access":
                    this.comboBox1.SelectedIndex = 2;
                    break;
                case "oracle":
                    this.comboBox1.SelectedIndex = 3;
                    break;
            }
        }
        private void aoButton2_Click(object sender, EventArgs e)
        {
            if (chkRemember.Checked == true)
            {
                settingsClass settings = new settingsClass();
                settings.server = this.txtHost.Text;
                settings.userid = this.txtUsername.Text;
                settings.password = this.txtPassword.Text;
                settings.dbtype = getProvider();
                settings.database = this.txtDB.Text;
                XmlSerializer s = new XmlSerializer(typeof(settingsClass));
                TextWriter w = new StreamWriter(@"settings.server.xml");
                s.Serialize(w, settings);
                w.Close();
            }
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Abort;
        }

    }

    [XmlRoot("serverconfig")]
    public class settingsClass
    {
        [XmlElement("userid")]
        public string userid;
        [XmlElement("pass")]
        public string password;
        [XmlElement("host")]
        public string server;
        [XmlElement("database")]
        public string database;
        [XmlElement("dbtype")]
        public System.Data.OrionDB.DATABASETYPES dbtype;
    }
}
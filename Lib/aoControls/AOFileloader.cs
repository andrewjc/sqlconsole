using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace AOControls
{
    public partial class AOFileloader : GenericControl
    {
        protected string _filename;
        protected bool _middlemode = false; //true if we have a file in swap.
        protected dbal.AbstractObjects.TransactionQueue tq;
        delegate void FileChangeDelegate();
        System.IO.FileSystemWatcher fsw = null;
        
        public AOFileloader():base()
        {
            InitializeComponent();
            this.Height = this.tableLayoutPanel1.Height;
            
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            this.OnMouseDown(e);
        }

        private void aoButton1_Click(object sender, EventArgs e)
        {
            AOFileSystemBrowser fsb = new AOFileSystemBrowser();
            fsb.dbObject = ((dbal.AbstractObjects.Database)this._paramList["var_dbobject"]);
            //fsb.dbObject = ((AManager.mainForm)this.ParentForm.MdiParent).getDatabaseObject();
            if (this.tq != null)
            {
                this.tq.RollBack();
                this.tq = new dbal.AbstractObjects.TransactionQueue(fsb.dbObject);
            }
            Form popupForm = new Form();
            popupForm.Controls.Add(fsb);
            //popupForm.MdiParent = this.ParentForm.MdiParent;
            
            popupForm.Size = fsb.Size;

            fsb.Dock = DockStyle.Fill;
            
            popupForm.Text = "Select File";
            
            popupForm.StartPosition = FormStartPosition.CenterScreen;
            fsb.InitControl();
            if (popupForm.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fStream = null;
                try
                {
                    fStream = new System.IO.FileStream(fsb.Filename, System.IO.FileMode.Open);
                }
                catch
                {
                    MessageBox.Show("Access to the selected file was denied.", "Upload failed", MessageBoxButtons.OK);
                    return;
                }
                XTS.DBFileSystem dfs = new XTS.DBFileSystem(fsb.dbObject);
                XTS.DBFile dbo = dfs.GetFileByPath(@"USERS\" + (string)this._paramList["var_username"] + @"\Temp\Swapspace\" + fsb.Filename.Substring(fsb.Filename.LastIndexOf("\\") + 1));
                dbo.WriteFromStream(fStream);
                dbo.Flush();
                fStream.Close();
                dbo.Reload();
                this._filename = dbo.ucpath;
                this.lnkFileSrc.Text = fsb.Filename.Substring(fsb.Filename.LastIndexOf("\\")+1);
                this._middlemode = true;
                this.OnTextChanged(new EventArgs());
            }
        }

        private void AOFileloader_Leave(object sender, EventArgs e)
        {

        }

        private void AOFileloader_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {

            }
        }

        private void AOFileloader_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {

            }
        }

        private void AOFileloader_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {

            }
        }

        private void AOFileloader_Load(object sender, EventArgs e)
        {

        }

        public override string Text
        {
            // This method will ask for an ID
            // when 'getting' if we are in middle-mode, confirm the file.
            get
            {
                if (this._middlemode)
                {
                    string catname = "Resources";
                    string newFilename = @"USERS\All\"+catname+@"\Attached Files\" + this._filename.Substring(this._filename.LastIndexOf("\\") + 1);
                    XTS.DBFileSystem dfs = new XTS.DBFileSystem(((dbal.AbstractObjects.Database)this._paramList["var_dbobject"]));
                    dfs.MoveFile(this._filename, newFilename, true);
                    //((dbal.AbstractObjects.Database)this._paramList["var_dbobject"]).RunSQL("update #__filesystem set filepath='"+newFilename+"' where id="+this._fileid);                    
                    this._middlemode = false;
                    this._filename = newFilename;
                }
                //return "27";
                return Convert.ToString(this._filename);
            }
            set
            {
                try
                {
                    if (value != null)
                    {
                        this._filename = (string)value;
                        this.lnkFileSrc.Text = _filename.Substring(_filename.LastIndexOf("\\") + 1);
                    }
                }
                catch { }
            }
        }

        private void lnkFileSrc_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Save the file.
            try
            {
                dbal.AbstractObjects.Database dbo = ((dbal.AbstractObjects.Database)this._paramList["var_dbobject"]);
                if (this.fsw != null) this.fsw.EnableRaisingEvents = false;
                XTS.DBFileSystem dbs = new XTS.DBFileSystem(dbo);
                XTS.DBFile file = dbs.GetFileByPath((string)this._filename);
                file.Reload();
                System.IO.FileStream tempFile = new System.IO.FileStream(Environment.GetEnvironmentVariable("TEMP") + @"\" + _filename.Substring(_filename.LastIndexOf("\\") + 1), System.IO.FileMode.OpenOrCreate);
                tempFile.Write(file.data, 0, file.data.Length);
                tempFile.Close();
                try
                {
                    startAndWatch(Environment.GetEnvironmentVariable("TEMP") + @"\" + _filename.Substring(_filename.LastIndexOf("\\") + 1));
                }
                catch
                {
                    MessageBox.Show("There was a problem opening the selected file.\r\nMake sure there is an associated program for the file extension.");
                    return;
                }
            }
            catch(Exception err)
            {
                aoButton1_Click(sender, e);
            }
        }

        private void startAndWatch(string filename)
        {
            System.Diagnostics.Process pc = new System.Diagnostics.Process();
            pc.EnableRaisingEvents = true;
            pc.StartInfo.FileName = filename;
            
            pc.Start();
            
            fsw= new System.IO.FileSystemWatcher(filename.Substring(0, filename.LastIndexOf(@"\")), filename.Substring(filename.LastIndexOf("\\") + 1));
            fsw.Filter = filename.Substring(filename.LastIndexOf(@"\") + 1);
            //fsw.WaitForChanged(System.IO.WatcherChangeTypes.All)
            fsw.Changed +=new System.IO.FileSystemEventHandler(fsw_Changed);
            System.Threading.Thread.Sleep(new TimeSpan(0, 0, 03));
            fsw.EnableRaisingEvents = true;
        }

        void fsw_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            ((System.IO.FileSystemWatcher)sender).EnableRaisingEvents = false;
            ((System.IO.FileSystemWatcher)sender).Dispose();
            this.ActiveFileChanged();
            System.IO.File.Delete(Environment.GetEnvironmentVariable("TEMP") + @"\" + _filename.Substring(_filename.LastIndexOf("\\") + 1));
        }

        private void ActiveFileChanged()
        {
            if (this.InvokeRequired)
            {
                Invoke(new FileChangeDelegate(ActiveFileChanged), new object[] { });
            }
            else
            {
                if (MessageBox.Show("The following file has changed outside of the system:\r\n\r\n" + _filename.Substring(_filename.LastIndexOf("\\") + 1) + "\r\n\r\nWould you like to reload it into the database?", "Filesystem Notification", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // Save the file.
                    dbal.AbstractObjects.Database dbo = ((dbal.AbstractObjects.Database)this._paramList["var_dbobject"]);
                    XTS.DBFileSystem dbs = new XTS.DBFileSystem(dbo);
                    XTS.DBFile file = dbs.GetFileByPath((string)this._filename);
                    System.IO.FileStream tempFile = new System.IO.FileStream(Environment.GetEnvironmentVariable("TEMP") + @"\" + _filename.Substring(_filename.LastIndexOf("\\") + 1), System.IO.FileMode.Open);
                    file.data = new byte[tempFile.Length];
                    tempFile.Position = 0;
                    file.WriteFromStream(tempFile);
                    tempFile.Close();
                    file.Flush();
                }
            }
        }

    }
}

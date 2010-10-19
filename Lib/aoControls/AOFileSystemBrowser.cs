using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace AOControls
{
    public partial class AOFileSystemBrowser : UserControl
    {
        public dbal.AbstractObjects.Database dbObject = null;
        protected string _filename = null;
        protected int _selMode = 1;
        public AOFileSystemBrowser()
        {
            InitializeComponent();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            this.ParentForm.DialogResult = DialogResult.Cancel;
        }

        protected void buildLocalFileList()
        {
            this.fileListPanel.Controls.Clear();
            string startPath = @"C:\";
            TreeView fileList = new TreeView();
            fileList.NodeMouseClick += new TreeNodeMouseClickEventHandler(fileList_NodeMouseClick);
            fileList.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(fileList_NodeMouseDoubleClick);
            fileList.Dock = DockStyle.Fill;
            this.fileListPanel.Controls.Add(fileList);
            TreeNode root = fileList.Nodes.Add(@"C:\");
            logLocalFile(root, startPath);
            root.Expand();
            root.FirstNode.Expand();
        }

        protected void buildDBFileList()
        {
            this.fileListPanel.Controls.Clear();
            string startPath = @"C:\";
            TreeView fileList = new TreeView();
            fileList.NodeMouseClick += new TreeNodeMouseClickEventHandler(fileList_NodeMouseClick);
            fileList.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(fileList_NodeMouseDoubleClick);
            fileList.Dock = DockStyle.Fill;
            this.fileListPanel.Controls.Add(fileList);
            TreeNode root = fileList.Nodes.Add(@"C:\");
            logLocalFile(root, startPath);
            root.Expand();
            root.FirstNode.Expand();
        }

        void fileList_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if ((string)e.Node.Tag == "file")
            {
                fileList_NodeMouseClick(sender, e);
                toolStripButton2_Click(sender, new EventArgs());
            }
        }

        void fileList_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if ((string)e.Node.Tag == "dir")
            {
                if (e.Node.Text != "Local Filesystem")
                    logLocalFile(e.Node, e.Node.Text);
                e.Node.Expand();
            }
            else
            {
                this.txtFilename.Text = e.Node.Text;
            }
        }

        protected void logLocalFile(TreeNode t, string path)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (DirectoryInfo dd in di.GetDirectories())
                {
                    TreeNode newnode = t.Nodes.Add(dd.FullName);
                    newnode.Tag = "dir";

                }
                foreach (FileInfo ff in di.GetFiles())
                {
                    TreeNode filenode = t.Nodes.Add(ff.FullName);
                    filenode.Tag = "file";
                }
            }
            catch { }
        }


        public void InitControl()
        {
            buildLocalFileList();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            this._filename = this.txtFilename.Text;
            this.ParentForm.DialogResult = DialogResult.OK;
        }

        public string Filename
        {
            get { return this._filename; }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (this._selMode == 1)
            {
                this._selMode = 2;
                this.buildDBFileList();
            }
            else
            {
                this._selMode = 1;
                this.buildLocalFileList();
            }
        }
    }
}

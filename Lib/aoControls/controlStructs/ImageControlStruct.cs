using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace AOControls
{
    public class ImageControlStruct : controlStruct
    {
        public ImageControlStruct(controlStruct srcStruct)
        {
            base.CopyFrom(srcStruct);
            // this.ucfile = "somefile";
        }

        public override void PerformFinalUpdate()
        {
            // Move the image into the user's data directory.
            //AManager.mainForm master = (AManager.mainForm)this.Attributes["var_masterform"];

            XTS.DBFileSystem filesys = new XTS.DBFileSystem((dbal.AbstractObjects.Database)this.Attributes["var_dbobject"]);
            XTS.DBFile file = filesys.GetFileByPath((string)this.Attributes["ucpath"]);
            string oldpath = file.ucpath;
            if (oldpath.IndexOf(@"Temp\Swapspace") > 0)
            {
                string newpath = oldpath.Replace(@"Temp\Swapspace", (string)this.Attributes["var_finalfilepath"]);
                filesys.MoveFile(oldpath, newpath, true);
                this.Attributes["ucpath"] = newpath;
            }
        }

        public override void PerformPostRecordChange()
        {
        }

        public override void PerformPostSettingsUpdate()
        {
            if (this.Attributes.ContainsKey("ucpath") && (!this.Attributes.ContainsKey("require_post_update")))
            {
                //AManager.mainForm master = (AManager.mainForm)this.Attributes["var_masterform"];
                XTS.DBFileSystem filesys = new XTS.DBFileSystem((dbal.AbstractObjects.Database)this.Attributes["var_dbobject"]);
                XTS.DBFile newFile = null;
                string filename = (string)this.Attributes["ucpath"];
                filename = filename.Substring(filename.LastIndexOf(@"\") + 1);
                //if (!filesys.fileExists(@"USERS\"+(string)this.Attributes["var_username"]+@"\Temp\Swapspace\"+filename))
                //    newFile = filesys.createFile(@"USERS\" + (string)this.Attributes["var_username"] + @"\Temp\Swapspace\" + filename);
                newFile = filesys.GetFileByPath(@"USERS\" + (string)this.Attributes["var_username"] + @"\Temp\Swapspace\" + filename);
                System.IO.MemoryStream ts = new System.IO.MemoryStream();

                System.IO.FileStream fs = new System.IO.FileStream((string)this.Attributes["ucpath"], System.IO.FileMode.Open);

                newFile.WriteFromStream(fs);


                //this.Attributes.Remove("ucpath"); //Security fix.
                this.Attributes["ucpath"] = newFile.ucpath;
                this.Attributes["_core_SourceImage"] = newFile.data;
                this.Attributes["require_post_update"] = false;
            }
        }

        [EditorAttribute(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [CategoryAttribute("Settings"),
        DescriptionAttribute("The filename of the image to display")]
        public string Filename
        {
            get
            {
                return "Stored Image";
            }

            set
            {
                if ((string)this.Attributes["ucpath"] != value)
                {
                    this.Attributes.Remove("require_post_update");
                }
                if (this.Attributes.ContainsKey("ucpath"))
                {
                    this.Attributes.Remove("ucpath");

                }
                if (this.Attributes.ContainsKey("_core_SourceImage"))
                {
                    this.Attributes.Remove("_core_SourceImage");
                }
                this.Attributes["ucpath"] = value;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace AOControls
{
    public class FileLoaderControlStruct:controlStruct
    {
        public FileLoaderControlStruct(controlStruct srcStruct)
        {
            base.CopyFrom(srcStruct);
        }

        public override void PerformFinalUpdate()
        {

        }

        public override void PerformPostSettingsUpdate()
        {
        }

        public override void PerformPostRecordChange()
        {
            
        }


        [EditorAttribute(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [CategoryAttribute("Settings"),
        DescriptionAttribute("Specifies whether to perform version tracking & control (VTC) on this file.")]
        public bool EnableVTC
        {
            get
            {
                if (this.Attributes.ContainsKey("performutc"))
                {
                    return Convert.ToString(this.Attributes["performutc"]) == "True"?true:false;
                }
                return false;
            }
            
            set
            {
                this.Attributes["performutc"] = value;
            }
        }
    }
}

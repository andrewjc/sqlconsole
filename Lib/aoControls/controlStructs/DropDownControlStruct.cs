using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace AOControls
{
    public class DropDownControlStruct : controlStruct
    {
        //protected System.Collections.Specialized.StringCollection items;
        public DropDownControlStruct(controlStruct srcStruct)
        {
            this.Attributes["objectlist"] = new System.Collections.Specialized.StringCollection();
            base.CopyFrom(srcStruct);
            
        }

        public override void CustomSerialize(System.Xml.XmlTextWriter xmlt)
        {
            base.CustomSerialize(xmlt);
            xmlt.WriteStartElement("custom-manifest");
            xmlt.WriteStartElement("objectlist");
            foreach (string e in (System.Collections.Generic.List<String>)this.Attributes["objectlist"])
            {
                xmlt.WriteStartElement("entry");
                xmlt.WriteAttributeString("text", e);
                xmlt.WriteEndElement();
            }
            xmlt.WriteEndElement();
            xmlt.WriteEndElement();
        }

        public override void CustomDeserialize(System.Xml.XmlTextWriter xmlt)
        {
            base.CustomDeserialize(xmlt);
        }

        public override void PerformFinalUpdate()
        {

        }

        public override void PerformPostSettingsUpdate()
        {
        }

        public override void PerformPostRecordChange()
        {
            int i = 0;
        }

        [EditorAttribute("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [CategoryAttribute("Settings"),
        DescriptionAttribute("List of options to populate this dropdown list with.")]
        public System.Collections.Generic.List<String> Options
        {
            get
            {
                System.Collections.Generic.List<String> ret;
                try
                {
                    ret = (System.Collections.Generic.List<String>)this.Attributes["objectlist"];
                }
                catch
                {
                    ret = new System.Collections.Generic.List<String>();
                    this.Attributes["objectlist"] = ret;
                }
                return ret;
                
                
            }

            set
            {
                if(value != null)
                this.Attributes["objectlist"] = value;
                
            }
        }


         
    }

    
}

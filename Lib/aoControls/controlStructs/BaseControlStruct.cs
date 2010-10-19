using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;

namespace AOControls
{
    public abstract class baseControlStruct {

        // Override this method in custom control structs to allow data post processing
        // before save.
        public abstract void PerformPostSettingsUpdate();
        public abstract void PerformFinalUpdate();
        public abstract void PerformPostRecordChange();
        public abstract void CopyFrom(controlStruct src);
        public abstract void CustomSerialize(System.Xml.XmlTextWriter xmlt);
        public abstract void CustomDeserialize(System.Xml.XmlTextWriter xmlt);
    }
    
    // Custom structs are just for propertygrid editing. 
    // This class is used during the actual form building.
    public class controlStruct:baseControlStruct
    {
        protected int recordid;
        protected int type;
        protected int xpos;
        protected int ypos;
        protected int width;
        protected int height;
        protected string value;
        protected string name;
        protected string caption;
        protected bool canHaveValue = true;
        protected string labelPosition = "Left Top";
        public System.Collections.Hashtable Attributes = new Hashtable();
        public int _delete;

        public controlStruct()
        {

        }

        public override void CustomDeserialize(System.Xml.XmlTextWriter xmlt)
        {
            
        }

        public override void CustomSerialize(System.Xml.XmlTextWriter xmlt)
        {
            
        }

        public override void PerformPostSettingsUpdate()
        {
            
        }

        public override void PerformFinalUpdate()
        {
        }

        public override void PerformPostRecordChange() {
            if (this.Type == enumcontroltypes.ReferenceList)
            {
                
            }
        }

        public override void CopyFrom(controlStruct src) {
            if(src != null)
            if (src.name != null)
            {
                this._delete = src._delete;
                this.Attributes = src.Attributes;
                this.canHaveValue = src.canHaveValue;
                this.caption = src.caption;
                this.height = src.height;
                this.name = src.name;
                this.recordid = src.recordid;
                this.type = src.type;
                this.value = src.value;
                this.width = src.width;
                this.xpos = src.xpos;
                this.ypos = src.ypos;
                this.labelPosition = src.labelPosition;
            }
        }

        [Browsable(false)]
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        // This is just a designer friendly alias for Name.
        [CategoryAttribute("Settings"),
        DescriptionAttribute("The name of the field this control binds to.\r\nSpecify 'unbound' for none.")]
        public string BoundField
        {
            get
            {
                if (this.name.IndexOf("unbound") > 0)
                {
                    if (this.name.Substring(0, "unbound".Length) == "unbound") return "None (Virtual Field)";
                }
                return name;
            }

            set
            {
                name = value;
            }
        }
        [CategoryAttribute("Settings"),
        DescriptionAttribute("Caption that the control holds.")]
        public string Caption
        {
            get
            {
                return caption;
            }

            set
            {
                caption = value;
            }
        }

        [CategoryAttribute("Settings"),
        DescriptionAttribute("Font used by this control.")]
        public System.Drawing.Font Font
        {
            get
            {
                string fontfamily = (this.Attributes.ContainsKey("font-family") == true ? (string)this.Attributes["font-family"] : "Arial");
                float fontsize = (this.Attributes.ContainsKey("font-size") == true ? Convert.ToSingle(this.Attributes["font-size"]) : 12F);
                string attribString = (this.Attributes.ContainsKey("font-attrib") == true ? (string)this.Attributes["font-attrib"] : "Normal");
                System.Drawing.FontStyle style = new System.Drawing.FontStyle();
                style |= attribString.ToLower().IndexOf("bold")>=0?System.Drawing.FontStyle.Bold:0;
                style |= attribString.ToLower().IndexOf("italic")>=0?System.Drawing.FontStyle.Italic:0;
                style |= attribString.ToLower().IndexOf("regular")>=0?System.Drawing.FontStyle.Regular:0;
                style |= attribString.ToLower().IndexOf("strikeout")>=0?System.Drawing.FontStyle.Strikeout:0;
                style |= attribString.ToLower().IndexOf("underline") >= 0 ? System.Drawing.FontStyle.Underline : 0;
                System.Drawing.Font newFont = new System.Drawing.Font(fontfamily, fontsize, style);
                return newFont;
            }
            set
            {
                System.Drawing.Font srcFont = (System.Drawing.Font)value;
                this.Attributes["font-family"] = srcFont.FontFamily.Name;
                this.Attributes["font-size"] = srcFont.Size;
                string styleString = "";
                styleString += (srcFont.Bold) ? " bold" : "";
                styleString += (srcFont.Italic) ? " italic" : "";
                styleString += (srcFont.Strikeout) ? " strikeout" : "";
                styleString += (srcFont.Underline) ? " underline" : "";
                this.Attributes["font-attrib"] = styleString.Trim();
            }
        }

        [CategoryAttribute("Settings"),
        DescriptionAttribute("Position of the caption. Top or left.")]
        [TypeConverter(typeof(LabelPositionConverter))]
        public string LabelPosition
        {
            get
            {
                return this.labelPosition;
            }

            set
            {
                this.labelPosition = value;
            }
        }

        [CategoryAttribute("Settings"),
        DescriptionAttribute("Textual value of this field control")]
        public string Value
        {
            get
            {
                return value;
            }

            set
            {
                this.value = value;
            }
        }

        [CategoryAttribute("Settings"),
        DescriptionAttribute("Left position")]
        public int Left
        {
            get
            {
                return this.xpos;
            }

            set
            {
                this.xpos = value;
            }
        }
        [CategoryAttribute("Settings"),
        DescriptionAttribute("Top position")]
        public int Top
        {
            get
            {
                return this.ypos;
            }

            set
            {
                this.ypos = value;
            }
        }

        [CategoryAttribute("Settings"),
        DescriptionAttribute("Control width")]
        public int Width
        {
            get
            {
                return this.width;
            }

            set
            {
                this.width = value;
            }
        }

        [CategoryAttribute("Settings"),
        DescriptionAttribute("Control height")]
        public int Height
        {
            get
            {
                return this.height;
            }

            set
            {
                this.height = value;
            }
        }


        [CategoryAttribute("Settings"),
        DescriptionAttribute("The displayable type of this field.")]
        [System.ComponentModel.TypeConverter(typeof(enumcontroltypes))]
        public enumcontroltypes Type
        {
            get
            {
                return (enumcontroltypes)type;
            }

            set
            {
                type = (int)value;
            }
        }


        [CategoryAttribute("Settings"),
        DescriptionAttribute("Specifies whether this control has a value that can be saved to the database.")]
        public bool CanHaveValue
        {
            get
            {
                return this.canHaveValue;
            }

            set
            {
                this.canHaveValue = value;
            }
        }

        [CategoryAttribute("Settings"),
        DescriptionAttribute("Specifies where this object comes in the form's tab ordering.")]
        public int TabOrder
        {
            get
            {
                if(this.Attributes != null)
                    if (this.Attributes.ContainsKey("TabIndex"))
                    {
                        return Convert.ToInt32(this.Attributes["TabIndex"]);
                    }
                return 0;
            }

            set
            {
                if (this.Attributes == null)
                {
                    this.Attributes = new Hashtable();
                }
                if ((int?)value != null) {
                        this.Attributes.Add("TabIndex", Convert.ToInt32(value));
                }
                else
                    this.Attributes.Add("TabIndex", 0);
            }
        }

        [CategoryAttribute("Settings"),
        DescriptionAttribute("Specifies the top level arrangement")]
        public int ZOrder
        {
            get
            {
                if (this.Attributes != null)
                    if (this.Attributes.ContainsKey("ZIndex"))
                    {
                        return Convert.ToInt32(this.Attributes["ZIndex"]);
                    }
                return 0;
            }

            set
            {
                if (this.Attributes == null)
                {
                    this.Attributes = new Hashtable();
                }
                if ((int?)value != null)
                    this.Attributes.Add("ZIndex", Convert.ToInt32(value));
                else
                    this.Attributes.Add("ZIndex", 0);
            }
        }
    }
}

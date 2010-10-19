using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace AOControls
{
    public class RefListControlStruct:controlStruct
    {
        protected string _sql;
        public RefListControlStruct(controlStruct srcStruct)
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
            int i = 0;
        }

        [CategoryAttribute("Settings"),
        DescriptionAttribute("Query to populate this list with.")]
        public string SQLQuery
        {
            get
            {
                if (this.Attributes.ContainsKey("sqlquery"))
                {
                    return Convert.ToString(this.Attributes["sqlquery"]) != null ? Convert.ToString(this.Attributes["sqlquery"]) : "";
                }
                return "";
            }

            set
            {
                this.Attributes["sqlquery"] = value;
            }
        }

        
        
    }
}

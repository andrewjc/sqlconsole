using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace AOControls
{
    internal class LabelPositionConverter : StringConverter
    {

        private static StandardValuesCollection defaultRelations =
              new StandardValuesCollection(
                 new string[]{"Left Top", "Left Middle", "Left Bottom", "Top Left", "Top Middle", "Top Right"});

        public override bool GetStandardValuesSupported(
                       ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(
                       ITypeDescriptorContext context)
        {
            // returning false here means the property will
            // have a drop down and a value that can be manually
            // entered.      
            return false;
        }

        public override StandardValuesCollection GetStandardValues(
                      ITypeDescriptorContext context)
        {
            return defaultRelations;
        }
    }

}

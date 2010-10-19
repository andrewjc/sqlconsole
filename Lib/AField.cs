using System;
using System.Collections.Generic;
using System.Text;

namespace SQLConsole.Data
{
    [Serializable]
    public class AField
    {
        private string _name;
        private ABSTRACTDATATYPES _type;
        private int _maxSize;
        private int _precision;
        private ABSTRACTFIELDMODIFIERS[] _mods;
        private object _defVal;
        private object _value;
        private string _owner;
        private bool _groupBy;
        private ABSTRACTORDERTYPE _orderType;
        private ABSTRACTAGGREGATE _agg;
        private ABSTRACTMODIFYACTION _modAction;


        public AField(string name, ATable ownertable, ABSTRACTAGGREGATE AggregateFunction)
            : this(name, null, ownertable)
        {
            this._agg = AggregateFunction;
        }

        public AField(string name, object value, ATable ownertable)
            : this(name, value, ownertable.name)
        {
        }

        public AField(string name, object value, string ownertable)
            : this(name, value)
        {
            this._owner = ownertable;
        }

        public AField(string name, object value)
            : this()
        {
            this._name = name;
            this._value = value;
        }
        public AField() { }

        public ABSTRACTMODIFYACTION altermode
        {
            get { return this._modAction; }
            set { this._modAction = value; }
        }

        public string name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }
        public object value
        {
            get
            {
                // do automatic type conversions
                int int_result = 0;
                long long_result = 0;
                bool bool_result = false;
                if (this._type == ABSTRACTDATATYPES.AData)
                    return this._value;

                string resultString = Convert.ToString(this._value);
                if (Int32.TryParse(resultString, out int_result))
                    return Int32.Parse(resultString);
                else if (Int64.TryParse(resultString, out long_result))
                    return Int64.Parse(resultString);
                else if (Boolean.TryParse(resultString, out bool_result))
                    return bool.Parse(resultString);
          
                else
                    return resultString;
            }
            set
            
            {
                this._value = value;
            }
        }
        public object defaultval
        {
            get
            {
                return this._defVal;
            }
            set
            {
                this._defVal = value;
            }
        }
        public ABSTRACTDATATYPES type
        {
            get
            {
                return this._type;
            }
            set
            {
                this._type = value;
            }
        }

        public ABSTRACTAGGREGATE function
        {
            get { return this._agg; }
            set { this._agg = value; }
        }


        public ABSTRACTFIELDMODIFIERS[] modifiers
        {
            get
            {
                return this._mods;
            }
            set
            {
                this._mods = value;
            }
        }
        public int maxsize
        {
            get
            {
                return this._maxSize;
            }
            set
            {
                this._maxSize = value;
            }
        }

        public int precision
        {
            get
            {
                return this._precision;
            }
            set
            {
                this._precision = value;
            }
        }

        public bool GroupBy
        {
            get { return this._groupBy; }
            set { this._groupBy = value; }
        }

        public ABSTRACTORDERTYPE OrderBy
        {
            get { return this._orderType; }
            set { this._orderType = value; }
        }

        public string owner
        {
            get { return this._owner; }
            set { this._owner = value; }
        }

        public string getString() { return Convert.ToString(this.value); }
        public int getInt32() { return Convert.ToInt32(this.value); }

        
        public bool hasModifier(ABSTRACTFIELDMODIFIERS modifier)
        {
            if(this.modifiers != null)
            foreach (ABSTRACTFIELDMODIFIERS fm in this.modifiers)
            {
                if (fm == modifier) return true;
            }
            return false;
        }
        public void addModifier(ABSTRACTFIELDMODIFIERS modifier)
        {
            System.Collections.ArrayList modList = new System.Collections.ArrayList();
            if (this.modifiers == null) this.modifiers = new ABSTRACTFIELDMODIFIERS[]{};
            foreach (ABSTRACTFIELDMODIFIERS i in this.modifiers)
            {
                modList.Add(i);
            }
            modList.Add(modifier);


            this.modifiers = new ABSTRACTFIELDMODIFIERS[modList.Count];
            Array.Copy(modList.ToArray(), this.modifiers, modList.Count);
        }

    }

}

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
namespace SQLConsole.Data
{
    [Serializable]
    public class ATable
    {

        private System.Collections.ArrayList _fieldlist;
        private string _name;
        private string _alias;

        [NonSerialized]
        private SQLConsole.Data.Database _dbProvider;

        public ATable()
        {
            this._fieldlist = new System.Collections.ArrayList(10);
        }

        public ATable(string name)
            : this()
        {
            this._name = name;
        }

        public ATable(string name, string alias)
            : this(name)
        {
            this._alias = alias;
        }

        public ATable(string name, Database dbProvider)
            : this(name)
        {
            this._dbProvider = dbProvider;
        }

        public override string ToString()
        {
            return this.name;
        }

        

        public AField addField(string name, ABSTRACTDATATYPES type, int size, object defaultVal, params ABSTRACTFIELDMODIFIERS[] modifiers)
        {
            AField newField = addField(name, type, size, modifiers);
            newField.defaultval = defaultVal;
            return newField;
        }

        public AField addField(string name, ABSTRACTDATATYPES type, int size, params ABSTRACTFIELDMODIFIERS[] modifiers)
        {
            AField newField = addField(name, type, size);
            if (modifiers != null)
            {
                ABSTRACTFIELDMODIFIERS[] fieldmod = new ABSTRACTFIELDMODIFIERS[modifiers.Length];
                for (int i = 0; i < modifiers.Length; i++)
                {
                    fieldmod[i] = modifiers[i];
                }

                newField.modifiers = fieldmod;
            }
            return newField;
        }

        // These 2 addField implementations deal with foreign key support
        public AField addField(string name, ABSTRACTFIELDMODIFIERS modifier, ATable foreignTable)
        {
            return addField(name, modifier, foreignTable.name);          
        }

        public AField addField(string name, ABSTRACTFIELDMODIFIERS modifier, string foreignTable)
        {
            if (modifier != ABSTRACTFIELDMODIFIERS.ForeignKey)
            {
                throw (new Exception("Method tried to create a foreign key with the wrong modifier type."));
            }
            else
            {
                AField nField = new AField(name, foreignTable);
                nField.modifiers = new ABSTRACTFIELDMODIFIERS[] { modifier };
                nField.type = this._dbProvider.GetTableCache().getCachedTable(foreignTable).getPrimaryKey().type;
                nField.maxsize = this._dbProvider.GetTableCache().getCachedTable(foreignTable).getPrimaryKey().maxsize;
                this._fieldlist.Add(nField);
                return nField;
            }
        }

        public AField addField(string name, ABSTRACTDATATYPES type, int size)
        {
            AField newField = addField(name, type);
            newField.maxsize = size;
            return newField;
        }

        public AField addField(string name, ABSTRACTDATATYPES type)
        {
            AField nField = new AField();
            nField.name = name;
            nField.type = type;
            this._fieldlist.Add(nField);
            return nField;
        }

        public AField getFieldByName(string name)
        {
            foreach (AField field in this._fieldlist)
            {
                if (field.name.ToLower() == name.ToLower())
                {
                    field.owner = this.name;
                    return field;
                }
            }
            return null;
        }

        public void setFieldByName(string name, AField fld)
        {
            for(int i=0;i<=this._fieldlist.Count;i++) 
            {
                AField field = (AField)this._fieldlist[i];
                if (field.name.ToLower() == name.ToLower())
                {
                    this._fieldlist[i] = fld;
                    return;
                }
            } 
        }

        public System.Collections.ArrayList getFieldList() { return this._fieldlist; }
        public void setFieldList(System.Collections.ArrayList arrayObject) { this._fieldlist = arrayObject; }
        public Database getDatabaseObject() { return this._dbProvider; }
        public void setDatabaseObject(Database dbObject) { this._dbProvider = dbObject; }
        public void Compile()
        {
            try
            {
                string sqlstr = this._dbProvider.GetDatabaseProvider().buildCreateTableStatement(this);
                if (this._dbProvider.GetDatabaseConnector().executeNonQuery(sqlstr) != -1)
                {
                    // Serialize this table into our xml object store, then cache it.
                    if (!this.getDatabaseObject().GetTableCache().isCached(this))
                    {
                        this.getDatabaseObject().GetTableCache().cacheTable(this.name);
                        this.getDatabaseObject().GetTableCache().serializeCache();
                    }
                }
            }
            catch(Exception e)
            {
                throw (e);
            }
        }

        public bool Update()
        {
            // This method naturally supports transactions. It will return -1 if the transaction fails.
            TransactionQueue tq = new TransactionQueue(this._dbProvider);
            tq.UseAutoExecute = false;

            string[] updateSrc = this._dbProvider.GetDatabaseProvider().buildTableUpdateStatements(this).Split(';');
            foreach (string sqlStr in updateSrc)
                tq.Queue(sqlStr);

            if (!tq.Parse())
                return false;

            
            this._dbProvider.GetTableCache().cacheTable(this.name);
            return true;
        }

        public string Update(bool autoexecute)
        {
            if (autoexecute)
                Update();
            else
            {
                string updateSrc = this._dbProvider.GetDatabaseProvider().buildTableUpdateStatements(this);
                return updateSrc;
            }
            return "";
        }

        public void Update(ref TransactionQueue transqueue)
        {
            string[] updateSrc = this._dbProvider.GetDatabaseProvider().buildTableUpdateStatements(this).Split(';');
            foreach (string sqlStr in updateSrc)
                if(!sqlStr.Trim().Equals(""))
                transqueue.Queue(sqlStr);
        }


        public string name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        public string alias
        {
            get { return this._alias; }
            set { this._alias = value; }
        }

        public AField getPrimaryKey()
        {
            foreach (AField a in this._fieldlist)
                foreach (ABSTRACTFIELDMODIFIERS mod in a.modifiers)
                    if (mod == ABSTRACTFIELDMODIFIERS.PrimaryKey)
                        return a;

            return null;
        }

        public AField getField(string fieldname)
        {
            foreach (AField a in this._fieldlist)
            {
                if (a.name == fieldname)
                {
                    a.owner = this.name;
                    return a;
                }
            }
            return null;
        }

        public void BindData(System.Collections.ArrayList fieldData)
        {
            foreach (AField fld in fieldData)
            {
                if(this.getFieldByName(fld.name) != null)
                this.getFieldByName(fld.name).value = fld.value;
            }
        }

    }

}

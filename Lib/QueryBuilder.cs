using System;
using System.Collections.Generic;
using System.Text;

namespace SQLConsole.Data
{
    public class QueryBuilder : ICloneable
    {
        private ABSTRACTQUERYTYPES _queryType;
        private System.Collections.ArrayList _fieldObjects;
        private Database _assocDBObject;
        // private ATable _tblObject;
        private System.Collections.ArrayList _sourceObjects;
        public bool autoExecute = true;
        private string _wsql = "";
        public string conditionalString;
        private long _lastID = -1;
        public LIMITINFO QueryLimit;

        public bool distinct = false;

        public object Clone()
        {
            QueryBuilder qbl = this.MemberwiseClone() as QueryBuilder;
            return qbl;
        }

        public class SELECTQUERYSTRUCT
        {
            public ATable srcTable = null;
            public bool srcDistinct = false;
            public AField groupByField = null;
            public AField orderByField = null;
            public int LimitOffset = -1;
            public int LimitRowCount = -1;

            //       SELECT * 
            //       FROM table 
            //      WHERE something=something
            //      GROUP BY col HAVING 
            //       ORDER BY something asc/desc
            //      LIMIT offset, count


        }/*SELECTQUERYSTRUCT ABSTRACTQUERY_SELECT*/;

        public struct LIMITINFO
        {
            public int lStart;
            public int lLimit;
        }

        public struct SOURCEBINDING
        {
            public object sourceObject;
            public object boundSource;
            public ABSTRACTSOURCEBINDTYPES bindType;
            public object srcMatchField;
            public object dstMatchField;
        }
        public QueryBuilder(Database dbObject)
            : this()
        {
            this._assocDBObject = dbObject;
        }

        public QueryBuilder()
        {
            this._fieldObjects = new System.Collections.ArrayList(5);
            this._sourceObjects = new System.Collections.ArrayList(5);

        }

        public Database getDatabaseObject() { return this._assocDBObject; }

        public void setType(ABSTRACTQUERYTYPES type)
        {
            this._queryType = type;
        }

        public System.Collections.ArrayList getResultAsFieldArray()
        {
            System.Collections.ArrayList retArray = new System.Collections.ArrayList();
            this.Execute();
            TableDataReader tdr = this.getDatabaseObject().CreateTableDataReader();
            tdr.Bind(this);
            while (tdr.Read())
            {
                if (tdr.getFieldList().Count > 1)
                {
                    //System.Collections.ArrayList retArray2 = new System.Collections.ArrayList();
                    ATable coll = new ATable();

                    foreach (AField a in tdr.getFieldList())
                    {
                        if (this.getField(a.name) != null)
                        {
                            AField newField = coll.addField(a.name, ABSTRACTDATATYPES.AString);
                            newField.value = a.value;
                        }
                    }
                    retArray.Add(coll);
                }
                else
                    retArray.Add(Convert.ToString(((AField)tdr.getFieldList()[0]).value));
            }
            return retArray;
        }

        public int getLastInsertID()
        {
            return Convert.ToInt32(this._lastID);
        }

        public string getInternalQuery() { return this._wsql; }

        public void addField(AField field)
        {
            this._fieldObjects.Add(field);
        }

        public AField getField(string fieldname)
        {
            foreach (AField field in this._fieldObjects)
            {

                if (field.name.Trim().ToLower() == fieldname.Trim().ToLower())
                    return field;
            }
            return null;
        }

        public ATable getSourceTableByAlias(string alias)
        {
            foreach (SOURCEBINDING b in this._sourceObjects)
            {
                if (((ATable)b.sourceObject).alias.ToLower() == alias.ToLower())
                {
                    return (ATable)b.sourceObject;
                }
            }
            return null;
        }
        public ATable getSourceTableByName(string name)
        {
            foreach (SOURCEBINDING b in this._sourceObjects)
            {
                if (((ATable)b.sourceObject).name.ToLower() == name.ToLower())
                {
                    return (ATable)b.sourceObject;
                }
            }
            return null;
        }
        public AField getField(string fieldname, string ownerTable)
        {
            foreach (AField field in this._fieldObjects)
            {
                if ((field.name == fieldname) && (field.owner == ownerTable))
                    return field;
            }
            return null;
        }

        public System.Collections.ArrayList getFieldList()
        {
            return this._fieldObjects;
        }

        public System.Collections.ArrayList getSourceList()
        {
            return this._sourceObjects;
        }

        public void setFieldList(System.Collections.ArrayList src)
        {
            this._fieldObjects = src;
        }

        public void setSourceList(System.Collections.ArrayList src)
        {
            this._sourceObjects = src;
        }

        public string Compile()
        {
            string wSql = "";

            /*
             * Do quote checking, handle *, and replace auto(number) field hacks.
             */
            foreach (AField a in this.getFieldList())
            {
                if (a != null)
                {
                    if (a.value != null)
                    {
                        string tmp = Convert.ToString(a.value);
                 
                        a.value = utils.Local_AddSlashes(tmp, this._assocDBObject.GetDatabaseConnector().svrinfo._dbtype);
                    }


                    if (a.owner == null)
                    {
                        if (((SOURCEBINDING)this._sourceObjects[0]).sourceObject.GetType() == typeof(ATable))
                        {
                            a.owner = ((ATable)((SOURCEBINDING)this._sourceObjects[0]).sourceObject).name;
                        }
                        else
                        {
                            a.owner = (string)((SOURCEBINDING)this._sourceObjects[0]).sourceObject;
                        }
                    }

                    if (a.name == "*")
                    {
                        if (a.owner == null)
                        {
                            a.owner = ((ATable)((SOURCEBINDING)this._sourceObjects[0]).sourceObject).name;
                        }
                        this._fieldObjects.Clear();
                        this._fieldObjects = this.getDatabaseObject().GetTableCache().getCachedTable(a.owner).getFieldList();
                        break;
                    }

                    if (Convert.ToString(a.value) == "auto")
                    {
                        // replace this with the next logical number in the sequence.

                        a.value = this.getDatabaseObject().GetDatabaseProvider().generateNextID(a.owner);
                    }
                }
            }


            try
            {
                switch (this._queryType)
                {
                    case ABSTRACTQUERYTYPES.DeleteQuery:
                        wSql = buildDeleteStatement();
                        break;
                    case ABSTRACTQUERYTYPES.DropQuery:
                        wSql = buildDropStatement();
                        break;
                    case ABSTRACTQUERYTYPES.InsertQuery:
                        wSql = buildInsertStatement();
                        break;
                    case ABSTRACTQUERYTYPES.SelectQuery:
                        wSql = buildSelectStatement();
                        break;
                    case ABSTRACTQUERYTYPES.UpdateQuery:
                        wSql = buildUpdateStatement();
                        break;
                    case ABSTRACTQUERYTYPES.AlterQuery:
                        wSql = buildAlterStatement();
                        break;
                }
            }
            catch (Exception e)
            {
                throw (new Exception(e.Message));
            }
            this._wsql = wSql;

            if (this.autoExecute)
            {
                Database redist = this.getDatabaseObject();
                //bool usingRedist = false;

                //usingRedist = true;
                //

                try
                {
                    redist.GetDatabaseConnector().executeNonQuery(wSql, ref this._lastID);
                }
                catch (Exception e)
                {
                    if ((e.Message.IndexOf("pending local transaction") > 0) | (e.Message.IndexOf("busy with results for another hstmt") > 0)) //quick fix.
                    {
                        redist = this._assocDBObject.getNewConnection();
                        int iRet = redist.GetDatabaseConnector().executeNonQuery(wSql, ref this._lastID);
                        redist.Close();
                        redist = null;
                    }
                    else
                        throw (e);
                }
            }
            return wSql;
        }



        public void Execute()
        {
            Database redist = this.getDatabaseObject();
            //bool usingRedist = false;
            //usingRedist = true;
            //redist = this.getDatabaseObject().getNewConnection();

            try
            {
                redist.GetDatabaseConnector().executeNonQuery(this._wsql, ref this._lastID);
            }
            catch (Exception e)
            {
                if ((e.Message.IndexOf("pending local transaction") > 0) | (e.Message.IndexOf("busy with results for another hstmt") > 0)) //quick fix.
                {
                    redist = this.getDatabaseObject().getNewConnection();
                    redist.GetDatabaseConnector().executeNonQuery(this._wsql, ref this._lastID);
                    redist.Close();
                    redist = null;
                }
                else
                    throw (new Exception("Failed executing SQL: " + this._wsql, e));
            }
        }

        public SOURCEBINDING addSource(object src)
        {
            SOURCEBINDING srcObj = new SOURCEBINDING();
            //if(src.GetType() == typeof(string)) {
            //    src = this.getDatabaseObject().GetTableCache().getCachedTable((string)src);
            //}
            srcObj.sourceObject = src;
            this._sourceObjects.Add(srcObj);
            return srcObj;
        }

        public SOURCEBINDING addSource(object src, ABSTRACTSOURCEBINDTYPES bindType, object bindSrc, object srcMatchField, object dstMatchField)
        {
            SOURCEBINDING srcObj = new SOURCEBINDING();
            srcObj.sourceObject = src;
            srcObj.bindType = bindType;
            srcObj.boundSource = bindSrc;
            srcObj.srcMatchField = srcMatchField;
            srcObj.dstMatchField = dstMatchField;
            this._sourceObjects.Add(srcObj);
            return srcObj;
        }

        public void Reset()
        {
            // This function will just reset our querybuilder object. allowing class reuse.
            this._fieldObjects.Clear();
        }

        private string buildDropStatement()
        {
            string wSql;
            wSql = this.getDatabaseObject().GetDatabaseProvider().buildDropStatement(this);
            return wSql;
        }

        private string buildSelectStatement()
        {
            // assemble fields.
            string wSql;
            wSql = this.getDatabaseObject().GetDatabaseProvider().buildSelectStatement(this);
            return wSql;
        }


        private string buildInsertStatement()
        {
            return this.getDatabaseObject().GetDatabaseProvider().buildInsertStatement(this);
        }

        private string buildUpdateStatement()
        {
            return this.getDatabaseObject().GetDatabaseProvider().buildUpdateStatement(this);
        }

        private string buildAlterStatement()
        {
            return this.getDatabaseObject().GetDatabaseProvider().buildAlterStatement(this);
        }

        public string buildDeleteStatement()
        {
            return this.getDatabaseObject().GetDatabaseProvider().buildDeleteStatement(this);
        }

        public string GetLastError()
        {
            return this._assocDBObject.GetDatabaseConnector().getLastErrorString();
        }

        public System.Collections.ArrayList getResultAsFieldArrayWithCaching()
        {
            
            System.Collections.ArrayList retArray = new System.Collections.ArrayList();
            this.Execute();
            TableDataReader tdr = this.getDatabaseObject().CreateTableDataReader();
            tdr.Bind(this);
            while (tdr.Read())
            {
                if (tdr.getFieldList().Count > 1)
                {
                    //System.Collections.ArrayList retArray2 = new System.Collections.ArrayList();
                    ATable coll = new ATable();

                    foreach (AField a in tdr.getFieldList())
                    {
                        if (this.getField(a.name) != null)
                        {
                            AField newField = coll.addField(a.name, ABSTRACTDATATYPES.AString);
                            newField.value = a.value;
                        }
                    }
                    retArray.Add(coll);
                }
                else
                    retArray.Add(Convert.ToString(((AField)tdr.getFieldList()[0]).value));
            }

            return retArray;
        }
    }
}

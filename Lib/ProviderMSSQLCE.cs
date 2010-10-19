using System;
using System.Collections.Generic;
using System.Text;
using SQLConsole.Data;
using System.Collections;

namespace SQLConsole.Data.DatabaseProviders
{
    class ProviderMSSQLCE:DatabaseProvider
    {

        public ProviderMSSQLCE(Database dbObj)
        {
            this._dbObj = dbObj;

        }

        public override System.Collections.ArrayList getDatabaseList()
        {
            return null;
        }

        
        public override System.Collections.ArrayList getTableList() {
        	
        	System.Collections.ArrayList ar = new System.Collections.ArrayList();
            System.Data.Odbc.OdbcConnection rawconn = this._dbObj.GetDatabaseConnector().GetRawConnectionObject();
            System.Data.Odbc.OdbcCommand ocomm = new System.Data.Odbc.OdbcCommand();
            
            string sqlStr = "select name from sys.objects where type='U';";

            //sqlStr = this._dbObj.GetDatabaseConnector().DoTopLevelSqlTranslations(ref sqlStr);
            ocomm.CommandText = sqlStr;
            ocomm.Connection = rawconn;
            try
            {
                System.Data.Odbc.OdbcDataReader odr = ocomm.ExecuteReader();
                while (odr.Read())
                {
                    ar.Add(Convert.ToString(odr[0]));
                }
            }
            catch (Exception e)
            {
            	System.Diagnostics.Debug.Print(e.Message);
                return null;
            }
            return ar;
        }
        
        public override int DropTable(string tablename)
        {
            return this._dbObj.GetDatabaseConnector().executeNonQuery("drop table " + tablename + ";");
        }

        public override int DropDatabase(string dbname)
        {
            return this._dbObj.GetDatabaseConnector().executeNonQuery("drop database " + dbname + ";");
        }


        public override int CreateDatabase(string dbname)
        {
            try
            {
                this._dbObj.GetDatabaseConnector().executeNonQuery("create database " + dbname + ";");
                return 1;
            }
            catch { return -1; }
        }

        public override int UseDatabase(string dbname)
        {
            try
            {
                this._dbObj.GetDatabaseConnector().executeNonQuery("USE " + dbname);
                this._dbObj.GetDatabaseConnector().svrinfo._database = dbname;
                return 1;
            }
            catch(Exception e) { System.Diagnostics.Debug.Print(e.Message);return -1; }
        }

        // Generate the next logical id in the sequence for the source table
        public override int generateNextID(string srcTable)
        {
            string keyColumn = this._dbObj.getTableObject(srcTable).getPrimaryKey().name;

            int? nextID = (int?)this._dbObj.GetDatabaseConnector().getSingleValAsInt("select TOP 1 " + keyColumn + " from " + srcTable + " order by " + keyColumn + " desc");
            if (nextID == null) nextID = -1;
            nextID = nextID + 1;
            return (int)nextID;
        }

        // Generate the next logical id in the sequence for the source table
        public override int generateNextID(ATable srcTable)
        {
            return generateNextID(srcTable.name);
        }


        public override string buildDropStatement(QueryBuilder qbl)
        {
            string wSql = "drop ";
            wSql += ((AField)qbl.getFieldList()[0]).value;
            wSql += " " + ((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject + ";";
            return wSql;
        }

        public override string buildDeleteStatement(QueryBuilder qbl)
        {
            string wSql = "delete";
            wSql += " from ";

            // sources
            foreach (QueryBuilder.SOURCEBINDING src in qbl.getSourceList())
            {
                if (src.bindType == 0)
                {
                    // This is our main table name
                    wSql += ((ATable)src.sourceObject).name;
                    if ((((ATable)src.sourceObject).alias != null) && (((ATable)src.sourceObject).alias.Length > 0))
                        wSql += " " + ((ATable)src.sourceObject).alias;
                }
            }

            // conditional
            if ((qbl.conditionalString != null) && (qbl.conditionalString.Length > 0))
                wSql += " where " + qbl.conditionalString;

            return wSql;
        }

        public override int nextRecordID(string tablename, int currentRecordID, string indexableField)
        {
            int? ret = this._dbObj.getSingleResultAsInt("select " + indexableField + " from " + tablename + " where " + indexableField + " > " + currentRecordID + " limit 0,1");
            return (ret != null) ? (int)ret : -1;
        }

        public override int previousRecordID(string tablename, int currentRecordID, string indexableField)
        {
            int? ret = this._dbObj.GetDatabaseConnector().getSingleValAsInt("SELECT * FROM (SELECT TOP 1 " + indexableField + " FROM (SELECT TOP 1 " + indexableField + " FROM " + tablename + " WHERE " + indexableField + " < " + currentRecordID + " ORDER BY " + indexableField + " DESC) AS newtbl ORDER BY " + indexableField + " ASC) newtbl2 ORDER BY " + indexableField + " DESC");
            return (ret != null) ? (int)ret : -1;
        }

        public override int FSFileUpdate(int? id, byte[] databuffer)
        {
            System.Data.Odbc.OdbcConnection rawconn = this._dbObj.GetDatabaseConnector().GetRawConnectionObject();
            System.Data.Odbc.OdbcCommand ocomm = new System.Data.Odbc.OdbcCommand();
            string sqlStr = "update #__filesys_filenode set data=? where id=?";
            sqlStr = this._dbObj.GetDatabaseConnector().DoTopLevelSqlTranslations(ref sqlStr);
            ocomm.CommandText = sqlStr;
            ocomm.Connection = rawconn;
            System.Data.Odbc.OdbcParameter idParam = new System.Data.Odbc.OdbcParameter("@dataid", System.Data.Odbc.OdbcType.Int);
            idParam.Value = id;
            System.Data.Odbc.OdbcParameter dataParam = new System.Data.Odbc.OdbcParameter("@databit", System.Data.Odbc.OdbcType.Image);
            dataParam.Value = databuffer;
            ocomm.Parameters.Add(dataParam);
            ocomm.Parameters.Add(idParam);
            try
            {
                ocomm.ExecuteNonQuery();
            }
            catch (Exception e)
            {
            	System.Diagnostics.Debug.Print(e.Message);
                return -1;
            }
            return 1;
        }

        public override string buildSelectStatement(QueryBuilder qbl)
        {
            
            // Fix for limit code.
            if ((qbl.QueryLimit.lStart == 0) && (qbl.QueryLimit.lLimit == 0))
            {
                qbl.QueryLimit.lStart = -1;
                qbl.QueryLimit.lLimit = -1;
            }

            string wSql = "select ";
            if(qbl.QueryLimit.lStart != -1) {
                wSql += " TOP " + ((int)(qbl.QueryLimit.lStart + qbl.QueryLimit.lLimit)) + " ";
            }
            // fields in selection.
            if (qbl.distinct)
                wSql += " DISTINCT ";
            string wGrouping = "";
            string wOrdering = "";
            string totFieldList = "";
            foreach (AField field in qbl.getFieldList())
            {
                string fieldName = field.name;
                if (field.name[0] == '[') field.name = field.name.Substring(1);
                if (field.name[field.name.Length - 1] == ']') field.name = field.name.Substring(0, field.name.Length - 1);
                fieldName = "[" + field.name + "]";


                

                //field.owner = ((ATable)((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject).name;
                string wFieldstr = null;
                if ((field.owner == null) && (qbl.getSourceList().Count == 1))
                {
                    wFieldstr = fieldName;
                }  
                else
                {

                    //if (((ATable)(((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject)).name != field.owner)
                    //    wFieldstr = field.owner + "." + ""+field.name+"";
                    
                    System.Collections.ArrayList al = qbl.getSourceList();
                    wFieldstr = "" + fieldName + "";
                    foreach (QueryBuilder.SOURCEBINDING s in al)
                    {
                        if (((ATable)s.sourceObject).alias == field.owner) //logic using table alias
                        {
                            
                            ATable ownr = ((ATable)s.sourceObject);
                            if (ownr.alias != null)
                            {
                                wFieldstr = ownr.alias + "." + "" + fieldName + "";
                                break;
                            }
                            else
                            {
                                wFieldstr = field.owner + "." + "" + fieldName + "";
                                break;
                            }
                    
                        }
                        
                    }
                    //wFieldstr = field.owner + "." + "" + field.name + "";
                }


                if (field.function != 0)
                {
                    if (field.function == ABSTRACTAGGREGATE.Count)
                        wFieldstr = "Count(" + wFieldstr + ")";
                    else if (field.function == ABSTRACTAGGREGATE.Avg)
                        wFieldstr = "Avg(" + wFieldstr + ")";
                    else if (field.function == ABSTRACTAGGREGATE.DistinctCount)
                        wFieldstr = "Count(Distinct " + wFieldstr + ")";
                    else if (field.function == ABSTRACTAGGREGATE.Max)
                        wFieldstr = "Max(" + wFieldstr + ")";
                    else if (field.function == ABSTRACTAGGREGATE.Min)
                        wFieldstr = "Min(" + wFieldstr + ")";
                    else if (field.function == ABSTRACTAGGREGATE.Sum)
                        wFieldstr = "Sum(" + wFieldstr + ")";
                }
                wSql += wFieldstr;
                if (field != qbl.getFieldList()[qbl.getFieldList().Count - 1])
                    wSql += ", ";
                if (field.GroupBy == true)
                {
                    string wFieldGroup = "";
                    if ((field.owner == null) && (qbl.getSourceList().Count == 1))
                        wFieldGroup = field.name;
                    else
                        if (((ATable)(((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject)).name != field.owner)
                            wFieldGroup = field.owner + "." + field.name;
                        else
                            wFieldGroup = field.name;
                    wGrouping += " " + wFieldGroup;

                    if (field != qbl.getFieldList()[qbl.getFieldList().Count - 1])
                        wGrouping += ", ";
                }
                if (field.OrderBy != ABSTRACTORDERTYPE.None)
                {
                    string fStatMode = (field.OrderBy == ABSTRACTORDERTYPE.Ascending)?"asc":"desc";
                    string wFieldOrder = "";
                    if ((field.owner == null) && (qbl.getSourceList().Count == 1))
                        wFieldOrder = field.name;
                    else
                        if (((ATable)(((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject)).name != field.owner)
                            wFieldOrder = field.owner + "." + field.name;
                        else
                            wFieldOrder = field.name;

                    wOrdering += " " + wFieldOrder + " "+ fStatMode;
                    
                    if (field != qbl.getFieldList()[qbl.getFieldList().Count - 1])
                        wOrdering += ", ";
                }

                totFieldList += wFieldstr;
            }
            wSql += " from ";

            // sources
            foreach (QueryBuilder.SOURCEBINDING src in qbl.getSourceList())
            {
                if (src.bindType == 0)
                {
                    // This is our main table name
                    wSql += ((ATable)src.sourceObject).name;
                    if ((((ATable)src.sourceObject).alias != null) && (((ATable)src.sourceObject).alias.Length > 0))
                        wSql += " " + ((ATable)src.sourceObject).alias;
                }
                else if (src.bindType == ABSTRACTSOURCEBINDTYPES.INNERJOIN)
                {

                    wSql += " INNER JOIN " + ((ATable)src.sourceObject).name;
                    if ((((ATable)src.sourceObject).alias != null) && (((ATable)src.sourceObject).alias.Length > 0))
                    {
                        wSql += " " + ((ATable)src.sourceObject).alias;

                        try
                        {
                            wSql += " on " + ((ATable)src.boundSource).alias + "." + ((String)src.srcMatchField) + "=" + ((ATable)src.sourceObject).alias + "." + ((String)src.dstMatchField) + " ";
                        }
                        catch
                        {
                            wSql += " on " + ((ATable)src.boundSource).alias + "." + ((AField)src.srcMatchField).name + "=" + ((ATable)src.sourceObject).alias + "." + ((AField)src.dstMatchField).name + " ";
                        }
                    }
                    else
                    {
                        wSql += " on " + ((ATable)src.boundSource).name + "." + ((AField)src.srcMatchField).name + "=" + ((ATable)src.sourceObject).name + "." + ((AField)src.dstMatchField).name + " ";
                    }

                }
            }

            // conditional
            if((qbl.conditionalString != null) && (qbl.conditionalString.Length > 0))
            wSql += " where " + qbl.conditionalString + " ";

            // group by
            if (wGrouping.Trim() != "")
            {
                wGrouping = wGrouping.Trim();
               if (wGrouping[wGrouping.Length-1].Equals(',') == true)
                    wGrouping = wGrouping.Substring(0, wGrouping.Length - 1);
                wSql += " group by " + wGrouping;
            }

            // ordering by
            if (wOrdering.Trim() != "")
            {
                wOrdering = wOrdering.Trim();
                if (wOrdering[wOrdering.Length - 1].Equals(',') == true)
                    wOrdering = wOrdering.Substring(0, wOrdering.Length - 1);
                
            }

            // LIMIT CLAUSE WRAPPER
            /*
            if (qbl.QueryLimit.lStart > -1)
            {
               //string fStatMode = (field.OrderBy == ABSTRACTORDERTYPE.Ascending)?"asc":"desc";

                string wFieldOrder = "";
                if ((qbl.getField("id").owner == null) && (qbl.getSourceList().Count == 1))
                    wFieldOrder = qbl.getField("id").name;
                else
                    if (((ATable)(((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject)).name != qbl.getField("id").owner)
                        wFieldOrder = qbl.getField("id").owner + "." + qbl.getField("id").name;
                    else
                        wFieldOrder = qbl.getField("id").name;



               string fOrderingString = wFieldOrder;
               string sqlInner1 = "";
               string sqlInner2 = "";
                if(wOrdering != "") {
                sqlInner2 = String.Format("{0} order by {1} asc, {2}", wSql, fOrderingString, wOrdering);
                }else {
                sqlInner2 = String.Format("{0} order by {1} asc", wSql, fOrderingString); }

                if(wOrdering != "") {
                sqlInner1 = String.Format("select top {0} {1} from ({3}) as newtbl order by {2} desc, {3}", qbl.QueryLimit.lLimit, totFieldList, fOrderingString, sqlInner2, wOrdering);
                }else
                {sqlInner1 = String.Format("select top {0} {1} from ({3}) as newtbl order by {2} desc", qbl.QueryLimit.lLimit, totFieldList, fOrderingString, sqlInner2); }
                
                if(wOrdering != "")
                wSql = String.Format("select * from ({0}) as newtbl2 order by {1} asc, {2}", sqlInner1, fOrderingString, wOrdering);
                else
                wSql = String.Format("select * from ({0}) as newtbl2 order by {1} asc", sqlInner1, fOrderingString);
            }
             * */
            if(wOrdering != "")
            wSql += " order by " + wOrdering;
            return wSql;
        }

        public override string buildUpdateStatement(QueryBuilder qbl)
        {
            bool skipped = false;
            
            string tblname = ((object)((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject).ToString();

            ATable tblObj = qbl.getDatabaseObject().GetTableCache().getCachedTable(tblname);
            string wSql = "update " + tblname + " set ";
            foreach(AField field in qbl.getFieldList()) {
                skipped = false;
                if (!field.hasModifier(ABSTRACTFIELDMODIFIERS.PrimaryKey))
                // Skip the primary key as we cannot update it.
                {
                    if (tblObj.getFieldByName(field.name).type == ABSTRACTDATATYPES.AString)
                    {
                        wSql += "[" + field.name + "]" + "='" + field.value + "'";
                    }
                    else if (tblObj.getFieldByName(field.name).type == ABSTRACTDATATYPES.ABool)
                    {
                        if (Convert.ToString(field.value) != "" && field.value != null)
                            if(Convert.ToString(field.value).ToLower()=="true")
                                wSql += "[" + field.name + "]" + "=1";
                            else
                                wSql += "[" + field.name + "]" + "=0";
                        else
                            skipped = true;
                    }
                    else if ((tblObj.getFieldByName(field.name).type == ABSTRACTDATATYPES.ALargeInteger)||(tblObj.getFieldByName(field.name).type == ABSTRACTDATATYPES.ASmallInteger))
                        if (Convert.ToString(field.value) != "" && field.value != null)
                            wSql += "[" + field.name + "]" + "=" + field.value;
                        else
                            skipped = true;
                    else if ((tblObj.getFieldByName(field.name).type == ABSTRACTDATATYPES.ADateTime))
                        if (Convert.ToString(field.value) != "" && field.value != null)
                            wSql += "[" + field.name + "]" + "=CONVERT(datetime, '" + field.value+"')";
                        else
                            skipped = true;
                }
                else skipped = true;
                if ((Convert.ToString(field.value) != "" && field.value != null)||(tblObj.getFieldByName(field.name).type == ABSTRACTDATATYPES.AString))
                    if(!skipped)
                    if (field != qbl.getFieldList()[qbl.getFieldList().Count - 1])
                        wSql += ", ";
                    else
                        wSql += " ";
                else
                    wSql += " ";
            }

            // sometimes the algorithm skips... fix it here
            wSql = wSql.Trim();
            if (wSql[wSql.Length - 1] == ',')
            {
                wSql = wSql.Substring(0, wSql.Length - 1);
            }
            if (wSql[wSql.Length - 1] != ' ') wSql += " ";
            if(qbl.conditionalString != null)
            if(qbl.conditionalString.Length > 2)
            wSql += "where " + qbl.conditionalString;
            return wSql;
        }

        public override string buildInsertStatement(QueryBuilder qbl)
        {
            string tblname = ((string)((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject);
            ATable tblObj = qbl.getDatabaseObject().GetTableCache().getCachedTable(tblname);
            if (tblObj == null)
                throw (new Exception("Invalid table specified. Check query spelling and try again."));
            string wSql = "insert into [" + tblname + "] ";
            string fieldList = "";
            string valueList = "";
            ABSTRACTDATATYPES fieldType;

            // If our chosen table object does not have an autoincrementing primary key/id field
            // we can check for that here.

            // Build up field list and value list.
            foreach (AField field in qbl.getFieldList())
            {
                AField refField = qbl.getDatabaseObject().getTableObject(field.owner).getFieldByName(field.name);
                if (tblObj.getFieldByName(field.name) == null)
                {
                    this._dbObj.GetTableCache().rebuildAll();
                    throw(new Exception("Unable to build insert statement. An invalid field was included with the query."));
                }
                fieldList += "["+field.name+"]";
                if (field != qbl.getFieldList()[qbl.getFieldList().Count - 1]) fieldList += ",";


                fieldType = refField.type;



                if (fieldType == ABSTRACTDATATYPES.ABool)
                {
                    string boolRaw = Convert.ToString(field.value).ToLower();
                    bool relFieldValue = (boolRaw == "true" || boolRaw == "1" || boolRaw == "yes") ? true : false;
                    if (relFieldValue)
                    {
                        valueList += "1";
                    }
                    else
                    {
                        valueList += "0";
                    }

                }
                else if (fieldType == ABSTRACTDATATYPES.AForeignTable)
                {
                    // This is tricky, we have to look up the value type from the 
                    // referencing table's file.
                }
                else if ((fieldType == ABSTRACTDATATYPES.ASmallInteger) || (fieldType == ABSTRACTDATATYPES.ALargeInteger ))
                {
                    valueList += field.value;
                }
                else if ((fieldType == ABSTRACTDATATYPES.AString) || (fieldType == ABSTRACTDATATYPES.AChar) || (fieldType == ABSTRACTDATATYPES.ADateTime))
                {
                    // convert mysql/csql style quoting to mssql style quoting
                    //field.value = Convert.ToString(field.value).Replace("''", @"\'");
                    
                    // if the value is longer than the allowed nvarchar, then do a CONVERT/cast to text instead.
                    valueList += "'" + field.value + "'";

                }
                else if (fieldType == ABSTRACTDATATYPES.AData)
                {
                    valueList += field.value;
                }
                else
                {
                    // If we get here, then the field type is possibly unsupported.
                    // try to figure out what type of formatting to use.
                    string sValue = field.value != null?Convert.ToString(field.value):"";
                    if (sValue == "")
                    {
                        valueList += "''";
                    }
                    else
                    {
                        if (sValue.Substring(0, 1) == "'")
                        {
                            // convert mysql/csql style quoting to mssql style quoting
                            //field.value = Convert.ToString(field.value).Replace("''", @"\'");
                            valueList += "'" + field.value + "'";
                        }
                        else
                        {
                            valueList += field.value;
                        }
                    }
                }

                if (field != qbl.getFieldList()[qbl.getFieldList().Count - 1]) valueList += ",";
            }
            wSql += "(";
            wSql += fieldList;
            wSql += ") VALUES (";
            wSql += valueList;
            wSql += ");";
            return wSql;
        }

        public override string buildCreateTableStatement(ATable tbl)
        {
            // TODO Write code to compile this table object into sql statements.
            string sqlstr = "create table " + tbl.name + " (";

            // build the defs here
            string typeString = ""; string collectedModifiers = "";
            foreach (AField field in tbl.getFieldList())
            {
                //typeString = SQLConsole.Data.ProviderConverters.TypeConverters.FieldToTypeString(field, this._dbObj.GetDatabaseConnector().getDBType());
                typeString = this._dbObj.GetDatabaseProvider().fieldToTypeStr(field);
                sqlstr += field.name + " " + typeString + ", ";
            }

            sqlstr += collectedModifiers;
            sqlstr = sqlstr.Trim();
            if (sqlstr.Substring(sqlstr.Length - 1) == ",")
            {
                sqlstr = sqlstr.Substring(0, sqlstr.Length - 1);
            }
            sqlstr += ");";
            return sqlstr;


        }

        public override string buildAlterStatement(QueryBuilder qbl)
        {
            // This *may* be redone... if problems arise. 
            return qbl.conditionalString;   //quick hack.
        }


        public override string buildTableUpdateStatements(ATable tbl)
        {
            string returnCode = "";
            // Create a carbon copy of the original table
            // then compare it to this table, analyse the differences.
            ATable origTable = this._dbObj.getTableObject(tbl.name);
            ATable newTable = tbl;
            
            // Get all fields that have been removed.
            System.Collections.ArrayList tempList = SQLConsole.Data.utils.intDiff(origTable.getFieldList(), newTable.getFieldList());

            // Get all fields that have been added.
            tempList = SQLConsole.Data.utils.intDiff(newTable.getFieldList(), origTable.getFieldList());
            AField[] additionsList = new AField[tempList.Count];
            tempList.ToArray().CopyTo(additionsList, 0);
            foreach (AField addField in additionsList)
            {
                returnCode += "alter table " + tbl.name + " add " + addField.name + " " + this._dbObj.GetDatabaseProvider().fieldToTypeStr(addField) + ";";
            }
            // Fields that are the same should be scanned for differences...
            foreach (AField fld in newTable.getFieldList())
            {
                if (fld.altermode == ABSTRACTMODIFYACTION.DropColumn)
                    returnCode += "alter table " + tbl.name + " drop column " + fld.name  + ";";
                else if (fld.altermode == ABSTRACTMODIFYACTION.FieldModify)
                    returnCode += "alter table " + tbl.name + " alter column " + fld.name + " " + this._dbObj.GetDatabaseProvider().fieldToTypeStr(fld) + ";";
            }

            return returnCode;
        }

        private Hashtable pkLookupTable;
        private Boolean isPKCol(string sourcetable, string col)
        {
            Boolean foundOK = false;
            if (this.pkLookupTable == null)
                this.pkLookupTable = new Hashtable();
            else
            {
                if (pkLookupTable.Contains(sourcetable))
                    if ((string)pkLookupTable[sourcetable] == col)
                        return true;
            }
            int indkey = 1;
            while (indkey < 16 && this._dbObj.GetDatabaseConnector().getSingleVal("select index_col('" + sourcetable + "',1," + indkey + ")", false) != null)
            {
                if ((string)this._dbObj.GetDatabaseConnector().getSingleVal("select index_col('" + sourcetable + "',1," + indkey + ")", false) == col)
                {
                    pkLookupTable[sourcetable] = col;
                    foundOK = true;
                    break;
                }
                else
                    indkey++;
            }
            return foundOK;
        }

        /* REIMPLIMENTATION OF getTableDescription using sp_MS procedures */
        public override FieldDescriptor[] getTableDescription(string table)
        {
            try
            {
                System.Collections.Specialized.ListDictionary al = new System.Collections.Specialized.ListDictionary();
                System.Data.Odbc.OdbcDataReader odr = this._dbObj.getNewConnection().GetDatabaseConnector().getResult(String.Format("sp_MShelpcolumns '{0}'", this._dbObj.GetDatabaseConnector().DoTopLevelSqlTranslations(ref table)));

                Database iterationWorker = this._dbObj.getNewConnection();
                while (odr.Read())
                {
                    string fieldname = Convert.ToString(odr["col_name"]);
                    string fieldTypeStr = Convert.ToString(odr["col_typename"]);
                    int fieldMaxLen = Convert.ToInt32(odr["col_len"]);
                    string fieldAllowNull = Convert.ToString(odr["col_null"]); //0 = no, 1 = nullable
                    string isIdentity = Convert.ToString(odr["col_identity"]);
                    string isPrimaryKey = (this.isPKCol(this._dbObj.GetDatabaseConnector().DoTopLevelSqlTranslations(ref table),fieldname) == true) ? "True" : "False";
                    FieldDescriptor fd = new FieldDescriptor();
                    fd.name = fieldname;
                    fd.type = this.typeStrToField(fieldTypeStr).type;

                    fd.maxlen = fieldMaxLen;
                    System.Collections.ArrayList modList = new System.Collections.ArrayList();
                    if (fieldAllowNull.ToLower().Equals("false"))
                        modList.Add(ABSTRACTFIELDMODIFIERS.NotNull);
                    if (isPrimaryKey.ToLower().Equals("true"))
                        modList.Add(ABSTRACTFIELDMODIFIERS.PrimaryKey);
                    if (isIdentity.ToLower().Equals("true"))
                        modList.Add(ABSTRACTFIELDMODIFIERS.AutoIncrement);

                    fd.modifiers = new ABSTRACTFIELDMODIFIERS[modList.Count];
                    Array.Copy(modList.ToArray(), fd.modifiers, modList.Count);
                    //fd.defaultval = fieldDefaultVal;
                    al.Add(fd.name, fd);
                }

                odr = null;

                //odr.Close();
                FieldDescriptor[] returnArray = new FieldDescriptor[al.Count];
                //Array.Copy(al.Values., returnArray, al.Count);

                al.Values.CopyTo(returnArray, 0);
                if (returnArray.Length == 0) return null;
                //Array.Reverse(returnArray);

                return returnArray;
            }
            catch(Exception e)
            {
            	System.Diagnostics.Debug.Print(e.Message);
            }
            return null;
        }



        public ABSTRACTDATATYPES lookupTypeCode(string code)
        {
            string retStr = Convert.ToString(this._dbObj.getNewConnection().getSingleResult("select name from systypes where xtype=" + code));
            switch (retStr)
            {
                case "int":
                    return ABSTRACTDATATYPES.ASmallInteger;
                case "bigint":
                    return ABSTRACTDATATYPES.ALargeInteger;
                case "varchar":case "nvarchar":
                    return ABSTRACTDATATYPES.AString;
                case "datetime":
                    return ABSTRACTDATATYPES.ADateTime;
            }
            return ABSTRACTDATATYPES.AString;
        }

        public override string buildGetLastInsertID()
        {
            return "select @@IDENTITY";
        }

        public override AField typeStrToField(string typestr)
        {
            AField a = new AField();
            switch (typestr.ToLower())
            {
                case "int":
                    a.type = ABSTRACTDATATYPES.ASmallInteger;
                    break;
                    /*
                case "int":
                    a.type = ABSTRACTDATATYPES.ASmallInteger;
                    break;
                case "int":
                    a.type = ABSTRACTDATATYPES.ASmallInteger;
                    break;**/
                case "tinyint":
                    a.type = ABSTRACTDATATYPES.ASmallInteger;
                    break;
                case "bigint":
                    a.type = ABSTRACTDATATYPES.ALargeInteger;
                    break;
                case "string":
                case "varchar":
                case "nvarchar":
                case "text":
                    a.type = ABSTRACTDATATYPES.AString;
                    break;
                case "bit":
                    a.type = ABSTRACTDATATYPES.ABool;
                    break;
                case "image":
                    a.type = ABSTRACTDATATYPES.AData;
                    break;
                case "char":
                    a.type = ABSTRACTDATATYPES.AChar;
                    break;

                case "datetime":case "smalldatetime":
                    a.type = ABSTRACTDATATYPES.ADateTime;
                    break;
            }
            return a;
        }

        public override string fieldToTypeStr(AField fieldstruct)
        {
            string dTypeStr = "";
            switch (fieldstruct.type)
            {
                case ABSTRACTDATATYPES.AString:
                    dTypeStr = "varchar";
                    break;
                case ABSTRACTDATATYPES.ASmallInteger:
                    // mysql has different datatypes for lengths...
                    /*if (fieldstruct.maxsize < 4)
                        dTypeStr = "SMALLINT";  //This would be a tinyint
                    //however mysql's odbc driver
                    //links tinyint as boolean...
                    else if (fieldstruct.maxsize <= 6)
                        dTypeStr = "SMALLINT";
                    else if (fieldstruct.maxsize <= 11)
                        dTypeStr = "INT";
                    else if (fieldstruct.maxsize <= 20)
                        dTypeStr = "BIGINT";
                    else**/
                    dTypeStr = "INT";
                    break;
                case ABSTRACTDATATYPES.ABool:
                    dTypeStr = "BIT";
                    break;
                case ABSTRACTDATATYPES.AData:
                    dTypeStr = "IMAGE";
                    break;
                case ABSTRACTDATATYPES.ADateTime:
                    dTypeStr = "DATETIME";
                    break;
                case ABSTRACTDATATYPES.AChar:
                    dTypeStr = "CHAR";
                    break;
                
            }
            if ((fieldstruct.maxsize != -1) && (fieldstruct.type != ABSTRACTDATATYPES.ASmallInteger) && (fieldstruct.type != ABSTRACTDATATYPES.ABool) && (fieldstruct.type != ABSTRACTDATATYPES.AData) && (fieldstruct.type != ABSTRACTDATATYPES.ADateTime))
            {
                dTypeStr += "(" + fieldstruct.maxsize + ")";
            }
            if (fieldstruct.modifiers != null)
            {
                foreach (SQLConsole.Data.ABSTRACTFIELDMODIFIERS fieldMod in fieldstruct.modifiers)
                {
                    switch (fieldMod)
                    {
                        case ABSTRACTFIELDMODIFIERS.AutoIncrement:
                            dTypeStr += " IDENTITY(1,1)";
                            break;
                        case ABSTRACTFIELDMODIFIERS.PrimaryKey:
                            dTypeStr += " PRIMARY KEY";
                            break;
                        case ABSTRACTFIELDMODIFIERS.NotNull:
                            dTypeStr += " NOT NULL";
                            break;
                        case ABSTRACTFIELDMODIFIERS.Clustered:
                            dTypeStr += " CLUSTERED";
                            break;
                        case ABSTRACTFIELDMODIFIERS.ForeignKey:
                            string fpk = this._dbObj.GetTableCache().getCachedTable((string)fieldstruct.value).getPrimaryKey().name;
                            dTypeStr += " references " + fieldstruct.value + "(" + fpk + ")";
                            break;
                    }
                }
            }
            return dTypeStr;
        }
    }
}

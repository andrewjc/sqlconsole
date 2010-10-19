using System;
using System.Collections.Generic;
using System.Text;
using SQLConsole.Data;

namespace SQLConsole.Data.DatabaseProviders
{
    class ProviderACCESS : DatabaseProvider
    {

        public ProviderACCESS(Database dbObj)
        {
            this._dbObj = dbObj;

        }
        public override System.Collections.ArrayList getDatabaseList()
        {
            throw new NotImplementedException();
        }
        
        public override System.Collections.ArrayList getTableList() {
        	throw new NotImplementedException();
        }
        
        public override int DropTable(string tablename)
        {
            return this._dbObj.GetDatabaseConnector().executeNonQuery("drop table " + tablename + ";");
        }

        public override int DropDatabase(string dbname)
        {
            // This is basically just deleting the database from the disk.
            return 1;
        }

        public override int CreateDatabase(string dbname)
        {
            /*
            try
            {
                // Wish there was a neater way of doing this.
                ADOX.CatalogClass cat = new ADOX.CatalogClass();

                cat.Create("Provider=Microsoft.Jet.OLEDB.4.0;" +
                     "Data Source=" + dbname + ";" +
                     "Jet OLEDB:Engine Type=5");
                return 1;
            }
            catch { return -1; }
             * */
            return -1;
        }

        public override int UseDatabase(string dbname)
        {
            // Access cannot change database types, as it is file based.
            this._dbObj.GetDatabaseConnector().svrinfo._database = dbname;
            return 1;
        }

        // Generate the next logical id in the sequence for the source table
        public override int generateNextID(string srcTable)
        {
            string keyColumn = this._dbObj.getTableObject(srcTable).getPrimaryKey().name;

            int nextID = (int)this._dbObj.getSingleResultAsInt("select TOP 1 " + keyColumn + " from " + srcTable + " order by " + keyColumn + " desc");
            nextID = nextID + 1;
            return nextID;
        }

        // Generate the next logical id in the sequence for the source table
        public override int generateNextID(ATable srcTable)
        {
            return generateNextID(srcTable.name);
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
            string sqlStr = "update #__filesystem set data=? where id=?";
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


        public override string buildDropStatement(QueryBuilder qbl)
        {
            if (((AField)qbl.getFieldList()[0]).value.Equals("database")) return ";";
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


        public override string buildSelectStatement(QueryBuilder qbl)
        {
            string wSql = "select ";
            // fields in selection.
            string wGrouping = "";
            foreach (AField field in qbl.getFieldList())
            {
                string wFieldstr;
                wFieldstr = field.owner + "." + field.name;
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
                    wGrouping += " " + field.owner + "." + field.name;
                    if (field != qbl.getFieldList()[qbl.getFieldList().Count - 1])
                        wGrouping += ", ";
                }
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
                        wSql += " on " + ((ATable)src.boundSource).alias + "." + src.srcMatchField + "=" + ((ATable)src.sourceObject).alias + "." + src.dstMatchField + " ";
                    }
                    else
                    {
                        wSql += " on " + ((ATable)src.boundSource).name + "." + src.srcMatchField + "=" + ((ATable)src.sourceObject).name + "." + src.dstMatchField + " ";
                    }

                }
            }

            // conditional
            if ((qbl.conditionalString != null) && (qbl.conditionalString.Length > 0))
                wSql += " where " + qbl.conditionalString + " ";

            // group by
            if (wGrouping.Trim() != "")
            {
                wGrouping = wGrouping.Trim();
                if (wGrouping[wGrouping.Length - 1].Equals(',') == true)
                    wGrouping = wGrouping.Substring(0, wGrouping.Length - 1);
                wSql += " group by " + wGrouping;
            }

            return wSql;
        }

        public override string buildUpdateStatement(QueryBuilder qbl)
        {
            string tblname = ((string)((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject);
            ATable tblObj = qbl.getDatabaseObject().GetTableCache().getCachedTable(tblname);
            string wSql = "update " + tblname + " set ";
            foreach (AField field in qbl.getFieldList())
            {
                if (tblObj.getFieldByName(field.name).type == ABSTRACTDATATYPES.AString)
                {
                    wSql += field.name + "='" + field.value + "'";
                }
                else
                {
                    wSql += field.name + "=" + field.value;
                }
                if (field != qbl.getFieldList()[qbl.getFieldList().Count - 1])
                    wSql += ", ";
                else
                    wSql += " ";
            }
            wSql += "where " + qbl.conditionalString;
            return wSql;
        }

        public override string buildInsertStatement(QueryBuilder qbl)
        {
            string tblname = ((string)((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject);
            ATable tblObj = qbl.getDatabaseObject().GetTableCache().getCachedTable(tblname);
            string wSql = "insert into " + tblname + " ";
            string fieldList = "";
            string valueList = "";
            ABSTRACTDATATYPES fieldType;

            // If our chosen table object does not have an autoincrementing primary key/id field
            // we can check for that here.

            // Build up field list and value list.
            foreach (AField field in qbl.getFieldList())
            {
                fieldList += field.name;
                if (field != qbl.getFieldList()[qbl.getFieldList().Count - 1]) fieldList += ",";


                fieldType = tblObj.getFieldByName(field.name).type;



                if (fieldType == ABSTRACTDATATYPES.ABool)
                {
                    if (((string)field.value == "true") || ((bool)field.value == true) || ((int)field.value == 1) || ((string)field.value == "1") || ((string)field.value == "yes"))
                    {
                        valueList += "1";
                    }

                }
                else if (fieldType == ABSTRACTDATATYPES.AForeignTable)
                {
                    // This is tricky, we have to look up the value type from the 
                    // referencing table's file.
                }
                else if (fieldType == ABSTRACTDATATYPES.ASmallInteger)
                {
                    valueList += field.value;
                }
                else if (fieldType == ABSTRACTDATATYPES.AString)
                {
                    valueList += "'" + field.value + "'";
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
                typeString = this._dbObj.GetDatabaseProvider().fieldToTypeStr(field);
                //typeString = SQLConsole.Data.ProviderConverters.TypeConverters.FieldToTypeString(field, this._dbObj.GetDatabaseConnector().getDBType());
                if (this._dbObj.GetDatabaseConnector().getDBType() == DATABASETYPES.MYSQL)
                {
                    // Since mysql puts keys at the end of a statement we must collect them here.
                    if (field.modifiers != null)
                    {
                        foreach (ABSTRACTFIELDMODIFIERS mod in field.modifiers)
                        {
                            if (mod == ABSTRACTFIELDMODIFIERS.PrimaryKey)
                            {
                                collectedModifiers += " PRIMARY KEY(" + field.name + "), ";
                            }
                            else if (mod == ABSTRACTFIELDMODIFIERS.IndexKey)
                            {
                                collectedModifiers += " KEY(" + field.name + "), ";
                            }
                            else if (mod == ABSTRACTFIELDMODIFIERS.ForeignKey)
                            {
                                string foreignPrimaryKey = this._dbObj.GetTableCache().getCachedTable((string)field.value).getPrimaryKey().name;
                                collectedModifiers += " FOREIGN KEY(" + field.name + ") REFERENCES " + field.value + "(" + foreignPrimaryKey + "), ";
                            }
                        }
                    }
                }
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

        public override FieldDescriptor[] getTableDescription(string table)
        {
            table = this._dbObj.GetDatabaseConnector().DoTopLevelSqlTranslations(ref table);
            System.Data.DataTable f = new System.Data.DataTable(table);
            string[] restrictions = new string[4]
        { this._dbObj.GetDatabaseConnector().svrinfo._database, null, table, null };

            f = this._dbObj.GetDatabaseConnector().GetRawConnectionObject().GetSchema("Columns", restrictions);
            System.Collections.ArrayList al = new System.Collections.ArrayList(5);

            foreach (System.Data.DataRow odr in f.Rows)
            {
                
                FieldDescriptor fd = new FieldDescriptor();
                fd.name = Convert.ToString(odr[3]);
                fd.maxlen = Convert.ToInt32(odr[6]);
                fd.type = this._dbObj.GetDatabaseProvider().typeStrToField(Convert.ToString(odr[5])).type;
                System.Collections.ArrayList modList = new System.Collections.ArrayList();
                if (!Convert.ToString(odr[10]).Equals("1"))
                    modList.Add(ABSTRACTFIELDMODIFIERS.NotNull);
                if (Convert.ToString(odr[5]).Equals("COUNTER"))
                    modList.Add(ABSTRACTFIELDMODIFIERS.PrimaryKey);
                fd.modifiers = new ABSTRACTFIELDMODIFIERS[modList.Count];
                Array.Copy(modList.ToArray(), fd.modifiers, modList.Count);
                al.Add(fd);    
            }

           
            FieldDescriptor[] returnArray = new FieldDescriptor[al.Count];
            Array.Copy(al.ToArray(), returnArray, al.Count);
            if (returnArray.Length == 0) return null;
            return returnArray;
        }

        public override string buildGetLastInsertID()
        {
            return "select @@IDENTITY@@";
        }

        public override string buildTableUpdateStatements(ATable tbl)
        {
            string returnCode = "";
            // Create a carbon copy of the original table
            // then compare it to this table, analyse the differences.
            ATable origTable = this._dbObj.GetTableCache().getCachedTable(tbl.name);
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
                    returnCode += "alter table " + tbl.name + " drop column " + fld.name + ";";
                else if (fld.altermode == ABSTRACTMODIFYACTION.FieldModify)
                    returnCode += "alter table " + tbl.name + " modify " + fld.name + " " + this._dbObj.GetDatabaseProvider().fieldToTypeStr(fld) + ";";
            }
            return returnCode;
        }


        public override AField typeStrToField(string typestr)
        {
            AField a = new AField();
            switch (typestr.ToLower())
            {
                case "counter":
                    a.type = ABSTRACTDATATYPES.ASmallInteger;
                    break;
                case "double":
                    a.type = ABSTRACTDATATYPES.ASmallInteger;
                    break;
                case "string":
                case "varchar":
                case "nvarchar":
                    a.type = ABSTRACTDATATYPES.AString;
                    break;
                case "bit":
                    a.type = ABSTRACTDATATYPES.ABool;
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
                    dTypeStr = "TEXT";
                    break;
                case ABSTRACTDATATYPES.ASmallInteger:
                    dTypeStr = "number";
                    break;
                case ABSTRACTDATATYPES.ABool:
                    dTypeStr = "bit";
                    break;
            }
            if ((fieldstruct.maxsize != -1) && (fieldstruct.type != ABSTRACTDATATYPES.ASmallInteger) && (fieldstruct.type != ABSTRACTDATATYPES.ABool))
            {
                dTypeStr += "(" + fieldstruct.maxsize + ")";
            }
            if(fieldstruct.modifiers != null) {
                foreach (SQLConsole.Data.ABSTRACTFIELDMODIFIERS fieldMod in fieldstruct.modifiers)
                {
                    switch (fieldMod)
                    {
                        case ABSTRACTFIELDMODIFIERS.AutoIncrement:
                            // Access does things a bit different, so set to autonumber
                            dTypeStr = "COUNTER";
                            break;
                        case ABSTRACTFIELDMODIFIERS.PrimaryKey:
                            dTypeStr += " CONSTRAINT PrimaryKey PRIMARY KEY";
                            break;
                        case ABSTRACTFIELDMODIFIERS.NotNull:
                            dTypeStr += " NOT NULL";
                            break;
                        case ABSTRACTFIELDMODIFIERS.Clustered:
                            // No cluster support in msaccess.
                            break;
                    }
                }
            }
            return dTypeStr;
 
        }
    }
}

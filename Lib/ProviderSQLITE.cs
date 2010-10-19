using System;
using System.Collections.Generic;
using System.Text;
using SQLConsole.Data;

namespace SQLConsole.Data.DatabaseProviders
{
    class ProviderSQLITE:DatabaseProvider
    {
        public ProviderSQLITE(Database dbObj)
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
            string sqlStr = "select name from sqlite_master where type=\"table\"";
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
            catch { return -1; }
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

        // Generate the next logical id in the sequence for the source table
        public override int generateNextID(string srcTable)
        {
     
            string keyColumn = this._dbObj.getTableObject(srcTable).getPrimaryKey().name;

            int? nextID = (int?)this._dbObj.GetDatabaseConnector().getSingleValAsInt("select " + keyColumn + " from " + srcTable + " order by " + keyColumn + " desc limit 1");
            if (nextID == null) nextID = -1;
            nextID = nextID + 1;
            return (int)nextID;
        }

        // Generate the next logical id in the sequence for the source table
        public override int generateNextID(ATable srcTable)
        {
            return generateNextID(srcTable.name);
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

        public override string buildSelectStatement(QueryBuilder qbl)
        {
            string wSql = "select ";
            // fields in selection.
            string wGrouping = "";
            string wOrdering = "";
            foreach (AField field in qbl.getFieldList())
            {
                string wFieldstr;
                if((field.owner == null) && (qbl.getSourceList().Count == 1))
                    wFieldstr = field.name;
                else
                //if (((ATable)(((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject)).Name != field.owner)
                    wFieldstr = field.owner + "." + field.name;
                //else
                //    wFieldstr = field.name;
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
                if (field.OrderBy != ABSTRACTORDERTYPE.None)
                {
                    string fStatMode = (field.OrderBy == ABSTRACTORDERTYPE.Ascending) ? "asc" : "desc";
                    string wFieldOrder = "";
                    if ((field.owner == null) && (qbl.getSourceList().Count == 1))
                        wFieldOrder = field.name;
                    else
                        if (((ATable)(((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject)).name != field.owner)
                            wFieldOrder = field.owner + "." + field.name;
                        else
                            wFieldOrder = field.name;

                    wOrdering += " " + wFieldOrder + " " + fStatMode;

                    if (field != qbl.getFieldList()[qbl.getFieldList().Count - 1])
                        wOrdering += ", ";
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
            if(qbl.conditionalString != null)
            if (qbl.conditionalString.Replace(" ", "").ToLower().IndexOf("=null") > 0)
            {
                qbl.conditionalString = qbl.conditionalString.Replace("=", " IS ");
            }

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
            if (wOrdering.Trim() != "")
            {
                wOrdering = wOrdering.Trim();
                if (wOrdering[wOrdering.Length - 1].Equals(',') == true)
                    wOrdering = wOrdering.Substring(0, wOrdering.Length - 1);
                wSql += " order by " + wOrdering;
            }
            if (qbl.QueryLimit.lStart > -1)
            {
                wSql += " limit " + qbl.QueryLimit.lStart + "," + qbl.QueryLimit.lLimit;
            }
            return wSql;
        }

        public override string buildUpdateStatement(QueryBuilder qbl)
        {
            string tblname = ((string)((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject);
            ATable tblObj = qbl.getDatabaseObject().GetTableCache().getCachedTable(tblname);
            string wSql = "update " + tblname + " set ";
            foreach(AField field in qbl.getFieldList()) {
                if (tblObj.getFieldByName(field.name).type == ABSTRACTDATATYPES.AString)
                {
                    wSql += field.name + "='" + field.value + "'";
                }
                else if (tblObj.getFieldByName(field.name).type == ABSTRACTDATATYPES.ADateTime)
                {
                    if (Convert.ToString(field.value).ToLower().Trim() == "now()")
                    {
                        wSql += field.name + "=" + field.value;
                    }
                    else
                    {
                        wSql += field.name + "='" + field.value + "'";
                    }
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
            string tblname = (Convert.ToString(((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject));
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
                    // Deal with blank values and default val...
                    if (field.defaultval == null)
                    {
                        if (field.hasModifier(ABSTRACTFIELDMODIFIERS.NotNull))
                            field.defaultval = 0;
                        else
                            field.defaultval = "NULL";
                    }
                    if (field.value == null || Convert.ToString(field.value) == "")
                        field.value = field.defaultval;


                    valueList += field.value;
                }
                else if (fieldType == ABSTRACTDATATYPES.AString)
                {
                    valueList += "'" + field.value + "'";
                }
                else if (fieldType == ABSTRACTDATATYPES.ADateTime)
                {
                    if (Convert.ToString(field.value).ToLower().Trim() == "now()")
                    {
                        valueList += field.value;
                    }
                    else
                    {
                        valueList += "'" + field.value + "'";
                    }
                }
                else
                {
                    if (field.value == null || Convert.ToString(field.value)== "")
                        field.value = field.defaultval;
                    valueList += field.value;
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


        public override string buildTableUpdateStatements(ATable tbl)
        {
            string returnCode = "";
            string indexstring = "";
            string pkstring = "";
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
                if (fld.hasModifier(ABSTRACTFIELDMODIFIERS.IndexKey) && !origTable.getFieldByName(fld.name).hasModifier(ABSTRACTFIELDMODIFIERS.IndexKey))
                    indexstring = "alter table " + newTable.name + " add index (" + fld.name + ");";
                if (fld.hasModifier(ABSTRACTFIELDMODIFIERS.PrimaryKey) && !origTable.getFieldByName(fld.name).hasModifier(ABSTRACTFIELDMODIFIERS.PrimaryKey))
                    indexstring = "alter table " + newTable.name + " add primary key (" + fld.name + ");";
               
                if (fld.altermode == ABSTRACTMODIFYACTION.DropColumn)
                    returnCode += "alter table " + tbl.name + " drop column " + fld.name + ";";
                else if (fld.altermode == ABSTRACTMODIFYACTION.FieldModify)
                    returnCode += "alter table " + tbl.name + " modify " + fld.name + " " + this._dbObj.GetDatabaseProvider().fieldToTypeStr(fld) + ";";

            }

            // mysql wants seperate index and primary key alterations... 
            // do them here.
            returnCode += indexstring;
            returnCode += pkstring;
 
            return returnCode;
        }

        public override FieldDescriptor[] getTableDescription(string table)
        {
            try
            {
                System.Collections.ArrayList al = new System.Collections.ArrayList(5);

                string tableSQL = (string)this._dbObj.GetDatabaseConnector().getSingleVal("select sql from sqlite_master where name like \"" + table + "\"");


                string fieldString = tableSQL.Substring(tableSQL.IndexOf("(")+1);
                fieldString = fieldString.Substring(0, fieldString.LastIndexOf(")"));
                string[] fields = fieldString.Split(',');

                foreach(string TfieldStr in fields)
                {
                    string fieldStr = TfieldStr.Trim();
                    AField srcField = this.typeStrToField(fieldStr.Substring(fieldStr.IndexOf(" ")));
                    srcField.name = fieldStr.Substring(0, fieldStr.IndexOf(" "));
                    FieldDescriptor fd = new FieldDescriptor();
                    fd.name = srcField.name;
                    fd.type = srcField.type;
                    fd.maxlen = srcField.maxsize;
                    fd.modifiers = srcField.modifiers;


                    fd.defaultval = srcField.defaultval;
                    al.Add(fd);

                }
                FieldDescriptor[] returnArray = new FieldDescriptor[al.Count];
                Array.Copy(al.ToArray(), returnArray, al.Count);
                return returnArray;
            }
            catch(Exception e)
            {
            	System.Diagnostics.Debug.Print(e.Message);
                return null;
            }
        }

        public override string buildGetLastInsertID()
        {
            return "select last_insert_id()";
        }

        public override AField typeStrToField(string typestr)
        {
            string sType = typestr.Trim();
            string sSize = "";
            AField a = new AField();
            if (typestr.IndexOf('(') >= 0)
            {
                sType = typestr.Substring(0, typestr.IndexOf('(')).Trim();
                sSize = typestr.Substring(typestr.IndexOf('(') + 1).Substring(0, typestr.Substring(typestr.IndexOf('(') + 1).IndexOf(')'));
            }
            if (typestr.Contains(" "))
            {
                string[] tmp = typestr.Split(' ');
               
                if (tmp.GetLength(0) > 2)
                { //contains modifiers
                    sType = tmp[1];
                    if (typestr.ToLower().Contains("primary key"))
                    {
                        a.addModifier(ABSTRACTFIELDMODIFIERS.PrimaryKey);
                    }
                    if (typestr.ToLower().Contains("auto_increment"))
                    {
                        a.addModifier(ABSTRACTFIELDMODIFIERS.AutoIncrement);
                    }
                }
            }
           
           
            switch (sType.ToLower())
            {
                case "int":case "bit":
                    a.type = ABSTRACTDATATYPES.ASmallInteger;
                    break;
                case "decimal":
                    a.type = ABSTRACTDATATYPES.AFloat;
                    break;
                case "boolean":
                    a.type = ABSTRACTDATATYPES.ABool;
                    break;
                case "string":case "varchar":case "nvarchar":case "text":
                    a.type = ABSTRACTDATATYPES.AString;
                    break;
                case "blob":
                    a.type = ABSTRACTDATATYPES.AData;
                    break;
                case "datetime":
                    a.type = ABSTRACTDATATYPES.ADateTime;
                    break;

            }

            if (sSize != "")
            {
                // size contains precision information...
                if (sSize != "" && sSize.Contains(",") == false)
                {
                    a.maxsize = Convert.ToInt32(sSize);
                }
                else
                {
                    a.maxsize = Convert.ToInt32(sSize.Substring(0, sSize.IndexOf(",")));
                    a.precision = Convert.ToInt32(sSize.Substring(sSize.IndexOf(",") + 1));
                }
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
                    dTypeStr = "INT";
                    break;
                case ABSTRACTDATATYPES.ABool:
                    dTypeStr = "boolean";
                    break;
                case ABSTRACTDATATYPES.AData:
                    dTypeStr = "blob";
                    break;
                case ABSTRACTDATATYPES.ADateTime:
                    dTypeStr = "BIGINT";
                    break;
                case ABSTRACTDATATYPES.AFloat:
                    dTypeStr = "decimal";
                    break;
            }
            if ((fieldstruct.maxsize != -1) && (fieldstruct.type != ABSTRACTDATATYPES.ASmallInteger) && (fieldstruct.type != ABSTRACTDATATYPES.ABool) && (fieldstruct.type != ABSTRACTDATATYPES.AData) && (fieldstruct.type != ABSTRACTDATATYPES.ADateTime))
            {
                if (fieldstruct.precision > 0)
                {
                    dTypeStr += "(" + fieldstruct.maxsize + ", "+fieldstruct.precision+")";
                }
                else
                {
                    dTypeStr += "(" + fieldstruct.maxsize + ")";
                }
            }
            if (fieldstruct.modifiers != null)
            {
                foreach (SQLConsole.Data.ABSTRACTFIELDMODIFIERS fieldMod in fieldstruct.modifiers)
                {
                    switch (fieldMod)
                    {
                        case ABSTRACTFIELDMODIFIERS.AutoIncrement:
                            dTypeStr += " auto_increment";
                            break;
                        case ABSTRACTFIELDMODIFIERS.PrimaryKey:
                            // This is done differently in mysql. it is set at the end of the statement
                            break;
                        case ABSTRACTFIELDMODIFIERS.NotNull:
                            dTypeStr += " NOT NULL";
                            break;
                        case ABSTRACTFIELDMODIFIERS.Clustered: //Not in mysql
                            break;
                    }
                }
            }
            if ((string)fieldstruct.defaultval != "" && fieldstruct != null)
                dTypeStr += " default '"+fieldstruct.defaultval+"'";
            return dTypeStr;
        }
    }
}

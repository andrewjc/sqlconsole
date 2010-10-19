using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Odbc;
using SQLConsole.Data;
using SQLConsole.Data.DatabaseProviders;
using SQLConsole.Data.ProviderConverters;

namespace SQLConsole.Data
{
    public class TableDataReader:ATable
    {
        private OdbcDataReader _dataReader;
        private QueryBuilder _queryObject;
        public TableDataReader()
        {

        }

        public bool Bind(QueryBuilder qbl)
        {
            this._queryObject = qbl;
            this._dataReader = qbl.getDatabaseObject().getResult(this._queryObject);
            ATable srcTable = qbl.getDatabaseObject().GetTableCache().getCachedTable(((ATable)((QueryBuilder.SOURCEBINDING)qbl.getSourceList()[0]).sourceObject).name);

            /*
            // remove redundant fields from the field list.
            if (srcTable.getFieldList().Count != this._queryObject.getFieldList().Count)
            {
                object[] test = utils.arrayIntersect(this._queryObject.getFieldList().ToArray(), srcTable.getFieldList().ToArray());
                this.setFieldList(new System.Collections.ArrayList(test));
            }
            else
                this.setFieldList(srcTable.getFieldList());
             * */
            this.setFieldList(this._queryObject.getFieldList());
            this.name = srcTable.name;
            this.BindData(qbl.getFieldList());
            return false;
        }

        public bool Read()
        {
            try
            {
                bool returnCode = this._dataReader.Read();
                if (returnCode == false) return false;
                foreach (AField field in this.getFieldList())
                {
                    try
                    {
                        object value = this._dataReader[field.name];
                        if (value is System.Byte[])
                        {
                            field.value = System.Text.Encoding.UTF8.GetString((byte[])value);
                        }
                        else if(value is Int32) {
                            field.value = Convert.ToString(value);
                        }
                        else if (value is string || value is String)
                        {
                            field.value = (string)value;
                        }
                        else if (field.type != ABSTRACTDATATYPES.AData)
                            field.value = Convert.ToString(this._dataReader[field.name]);
                        else
                            field.value = this._dataReader[field.name];
                    }
                    catch(Exception e) {
                    	System.Diagnostics.Debug.Print(e.Message);
                    }
                }
                return returnCode;
            }
            catch { return false; }
        }

        public void Close()
        {
            this._dataReader.Close();
        }
    }
}
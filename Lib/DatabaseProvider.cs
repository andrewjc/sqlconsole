using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OrionDB;

namespace System.Data.OrionDB.DatabaseProviders
{
    abstract public class DatabaseProvider
    {
        protected Database _dbObj;

        abstract public int DropTable(string tablename);
        abstract public int DropDatabase(string dbname);
        abstract public int CreateDatabase(string dbname);
        abstract public int UseDatabase(string dbname);

        abstract public string buildSelectStatement(QueryBuilder qbl);
        abstract public string buildInsertStatement(QueryBuilder qbl);
        abstract public string buildDropStatement(QueryBuilder qbl);
        abstract public string buildUpdateStatement(QueryBuilder qbl);
        abstract public string buildCreateTableStatement(ATable qbl);
        abstract public string buildAlterStatement(QueryBuilder qbl);
        abstract public string buildDeleteStatement(QueryBuilder qbl); 
        abstract public string buildGetLastInsertID();
        abstract public string buildTableUpdateStatements(ATable tbl);
        abstract public AField typeStrToField(string typestr);
        abstract public string fieldToTypeStr(AField fieldstruct);
        abstract public int previousRecordID(string tablename, int currentRecordID, string indexableField);
        abstract public int nextRecordID(string tablename, int currentRecordID, string indexableField);
        abstract public int FSFileUpdate(int? id, byte[] databuffer);
        abstract public int generateNextID(string srcName);
        abstract public int generateNextID(ATable srcTable);

        abstract public FieldDescriptor[] getTableDescription(string table);
        abstract public System.Collections.ArrayList getDatabaseList();

    }

    public struct FieldDescriptor
    {
        public string name;
        public ABSTRACTDATATYPES type;
        public ABSTRACTFIELDMODIFIERS[] modifiers;
        public object defaultval;
        public int maxlen;
    }
}

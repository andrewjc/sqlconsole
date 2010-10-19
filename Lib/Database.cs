using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using SQLConsole.Data.DatabaseProviders;
namespace SQLConsole.Data
{
    [Serializable]
    public class Database
    {
        private SQLConsole.Data.LLDBA _dbProvider;
        private TableCache _tblCache;
        public long _debugMode = 0;
        public delegate void ReconnectingCloseCause(object e);
        public event ReconnectingCloseCause DBEvent_ReconnectingCloseCause;

        private object _lastQueryResult;

        public static void QueueItem(QueryBuilder qb)
        {

        }

        public Database(SQLConsole.Data.LLDBA databaseProvider)
        {
            
            this._dbProvider = databaseProvider;
            if(this._dbProvider.svrinfo._server != null)
                this._tblCache = new TableCache(this);
    
        }

        static void queueWorker()
        {
            
        }

        public void EnableVerboseLogging()
        {
            this._debugMode = 1;
        }

        public void RaiseDBCloseClause()
        {
            this.DBEvent_ReconnectingCloseCause(new Exception());
        }

        public int Open()
        {
            ERRORCODES d;
            if (this.GetDatabaseConnector().svrinfo._dbtype == 0)
                d = ERRORCODES.ERRORCODE_CONNECT_INVALIDLOGIN;
            else
                d = this.GetDatabaseConnector().Open();
        
            if (d != ERRORCODES.NONE)
            {
                if (d == ERRORCODES.ERRORCODE_DRIVER)
                {
                    throw(new Exception("Driver not installed."));
                }
                else if (d == ERRORCODES.ERRORCODE_CONNECT_INVALIDLOGIN)
                {
                    throw (new Exception("Invalid login exception"));
                }
                else if (d == ERRORCODES.ERRORCODE_CONNECT_HOST)
                {
                    throw (new Exception("Cannot connect to host"));
                }

            }
            return 1;
        }

        public bool Check()
        {
            try
            {

                if (this.GetDatabaseConnector().Connect() != ERRORCODES.NONE) return false;
                return true;
            }
            catch { return false; }
        }

        void Database_ReconnectingCloseCause(object e)
        {
            this.DBEvent_ReconnectingCloseCause(e);
        }

        public void Close()
        {
            this.GetDatabaseConnector().Close();
        }

        public QueryBuilder CreateQueryBuilder()
        {
            return new QueryBuilder(this);
        }

        public TableDataReader CreateTableDataReader()
        {
            TableDataReader newTableDataReader = new TableDataReader();
            newTableDataReader.setDatabaseObject(this);
            return newTableDataReader;
        }

        public TransactionQueue CreateTransactionQueue()
        {
            return new TransactionQueue(this);
        }

        public SQLConsole.Data.LLDBA GetDatabaseConnector()
        {
            return this._dbProvider;
        }

        public TableCache GetTableCache() {
            if (this._tblCache == null)
                this._tblCache = new TableCache(this);
            return this._tblCache;
        }

        public ATable getTableObject(string name)
        {
            // check the cache...
            if (this.GetTableCache().getCachedTable(name) != null) return this.GetTableCache().getCachedTable(name);



            // Query the database to get information about this table.
            ATable tblObject = new ATable(name);

            FieldDescriptor[] fieldinfo = this.GetDatabaseProvider().getTableDescription(name);
            if (fieldinfo == null) return null;
            foreach (FieldDescriptor fInfo in fieldinfo)
            {
                tblObject.addField(fInfo.name, fInfo.type, fInfo.maxlen, fInfo.modifiers);
            }
            return tblObject;
        }


        public ATable CreateTableObject(string name)
        {
            return new ATable(name, this);
        }

        public int CreateDatabase(string dbname)
        {
            return this.GetDatabaseProvider().CreateDatabase(dbname);
            //return 1;
        }

        public int UseDatabase(string dbname)
        {
            int ret = this.GetDatabaseProvider().UseDatabase(dbname);
            this.GetTableCache().deserializeCache(); //Reloads the table cache.
            return ret;
        }

        public System.Collections.ArrayList getDatabaseList()
        {
            if (this._dbProvider.GetRawConnectionObject().State != ConnectionState.Open)
            {
                return null;
            }
            return this.GetDatabaseProvider().getDatabaseList();
        }

        public QueryBuilder CompileSQL(string csql)
        {
            QueryBuilder newQuery = ProviderConverters.QueryBuilderCompiler.CreateQueryBuilder(csql, this);
            newQuery.autoExecute = false;
            newQuery.Compile();
            return newQuery;
        }

        public string CompileSQLToNative(string csql)
        {
            QueryBuilder newQuery = ProviderConverters.QueryBuilderCompiler.CreateQueryBuilder(csql, this);
            newQuery.autoExecute = false;
            return newQuery.Compile();
        }

        public string AddSlashes(string str)
        {
            return utils.Local_AddSlashes(str, this._dbProvider.getDBType());
        }

        public string StripSlashes(string str)
        {
            return utils.Local_StripSlashes(str, this._dbProvider.getDBType());
        }

        public int RunSQLNative(string sql)
        {
            return this.GetDatabaseConnector().executeNonQuery(sql);
        }

        // NOTE:    This method is for compiling/executing common-sql. For running native-sql use 
        //          the executeNonQuery method of the database connector object.
        // DO NOT CALL THIS METHOD FROM WITHIN AN ABSTRACT DATABASE PROVIDER.
        public int RunSQL(string sql)
        {
            return this.RunSQLNative(sql);
            string preSql = sql;
            string compiledSQL = "";
            if(this.GetDatabaseProvider() != null) {
                QueryBuilder qb = ProviderConverters.QueryBuilderCompiler.CreateQueryBuilder(sql, this);
                try
                {
                    qb.autoExecute = false;
                    compiledSQL = qb.Compile();
                    qb.autoExecute = true;
                    qb.Compile();
                    //this._dbProvider.Close();
                    //this._dbProvider.Open();

                }
                catch (Exception e)
                {
                    if (e.Message.IndexOf("Timeout expired") >= 0)
                    {
                        //this._dbProvider.Close();
                        //this.Open();
                        //RunSQL(sql);
                    }
                    else
                    {
                        e.Data.Add("PRESQL", preSql);
                        e.Data.Add("POSTSQL", compiledSQL);
                        throw (e);
                    }
                }
            }
            else {
                throw(new Exception("No database provider has been implemented for this datasource"));
            }
            return 1;
        }

        public System.Data.Odbc.OdbcDataReader getResult(string sql)
        {
            if(this.GetDatabaseProvider() != null) {
                QueryBuilder qb = ProviderConverters.QueryBuilderCompiler.CreateQueryBuilder(sql, this);
                qb.autoExecute = false;
                string compiledSQL = qb.Compile();
                try
                {
                    return this._dbProvider.getResult(compiledSQL);
                }
                catch (Exception e)
                {
                    if ((e.Message.IndexOf("pending local transaction") > 0) | (e.Message.IndexOf("busy with results for another hstmt") > 0)) //quick fix.
                    {
                        return this._dbProvider.getNewRawConnection().getResult(compiledSQL);
                    }
                    else
                        throw (e);
                }
            }
            else {
                throw(new Exception("No database provider has been implemented for this datasource"));
            }
        }

        public System.Data.Odbc.OdbcDataReader getResult(QueryBuilder query)
        {
            if (this.GetDatabaseProvider() != null)
            {
                query.autoExecute = false;
                string compiledSQL = query.Compile();
                try
                {
                    return this._dbProvider.getResult(compiledSQL);
                }
                catch (Exception e)
                {
                    if ((e.Message.IndexOf("pending local transaction") > 0) | (e.Message.IndexOf("busy with results for another") > 0)) //quick fix.
                    {
                        return this._dbProvider.getNewRawConnection().getResult(compiledSQL);
                    }
                    else
                    {
                        //throw (e);
                    }
                }
            }
            else
            {
                throw (new Exception("No database provider has been implemented for this datasource"));
            }
            return null;
        }

        public object getSingleResult(string sql)
        {
            if (this.GetDatabaseProvider() != null)
            {
                QueryBuilder query = this.CompileSQL(sql);
                query.autoExecute = false;
                string compiledSQL = query.getInternalQuery();
                try
                {
                    //try
                    {
                        object t =  this._dbProvider.getSingleVal(compiledSQL);
                        this._lastQueryResult = t;
                        return t;
                    }
                    //catch
                    {
                        // Attempt to run the query natively... without compile
                        // THIS IS RISKY!
                    //    return this._dbProvider.getSingleVal(sql);
                    }

                }
                catch (Exception e)
                {
                    if ((e.Message.IndexOf("pending local transaction") > 0) | (e.Message.IndexOf("busy with results for another") > 0)) //quick fix.
                    {
                        return this._dbProvider.getNewRawConnection().getSingleValAsInt(compiledSQL);
                    }
                    else
                        throw (e);
                }
            }
            else
            {
                throw (new Exception("No database provider has been implemented for this datasource"));
            }
        }

        public int? getSingleResultAsInt(string sql)
        {
            if (this.GetDatabaseProvider() != null)
            {
                QueryBuilder query = this.CompileSQL(sql);
                query.autoExecute = false;
                string compiledSQL = query.getInternalQuery();
                try
                {
                    object t = this._dbProvider.getSingleValAsInt(compiledSQL);
                    this._lastQueryResult = t;
                    return (int?)t;
                }
                catch (Exception e)
                {
                    if ((e.Message.IndexOf("pending local transaction") > 0)|(e.Message.IndexOf("busy with results for another hstmt")>0)) //quick fix.
                    {
                        return this._dbProvider.getNewRawConnection().getSingleValAsInt(compiledSQL);
                    }
                    else
                    throw (e);
                }
            }
            else
            {
                throw (new Exception("No database provider has been implemented for this datasource"));
            }
        }

        public object getLastQueryResult() { return this._lastQueryResult; }

        private int ExecuteWrapper(string sql)
        {
            return this._dbProvider.executeNonQuery(sql);
        }

        public DatabaseProviders.DatabaseProvider GetDatabaseProvider()
        {
            switch (this.GetDatabaseConnector().getDBType())
            {
                case DATABASETYPES.MSACCESS:
                    return new SQLConsole.Data.DatabaseProviders.ProviderACCESS(this);
                case DATABASETYPES.MYSQL:
                    return new SQLConsole.Data.DatabaseProviders.ProviderMYSQL(this);
                case DATABASETYPES.MSSQL:
                    return new SQLConsole.Data.DatabaseProviders.ProviderMSSQL2000(this);
                case DATABASETYPES.MSSQL2005:
                    return new SQLConsole.Data.DatabaseProviders.ProviderMSSQL2005(this);
                case DATABASETYPES.MSSQLCE:
                    return new SQLConsole.Data.DatabaseProviders.ProviderMSSQL2005(this);

                case DATABASETYPES.ORACLE:
                    return new SQLConsole.Data.DatabaseProviders.ProviderORACLE(this);
                case DATABASETYPES.SQLITE:
                    return new SQLConsole.Data.DatabaseProviders.ProviderSQLITE(this);
                default:
                    return null;
            }
        }

        public string getLastErrorString() { return this.GetDatabaseConnector().getLastErrorString(); }

        public Database getNewConnection()
        {
            Database newDB = new Database(new LLDBA());
            newDB.GetDatabaseConnector().svrinfo = this.GetDatabaseConnector().svrinfo;
            newDB.Open();
            newDB.UseDatabase(newDB.GetDatabaseConnector().svrinfo._database);
            return newDB;
        }

        public void InsertIfAbsent(ATable row)
        {
            string where = "";
            foreach (AField f in row.getFieldList())
            {
                where = (where==""?f.name+"='"+f.value+"'":where + " and " + f.name+"='"+f.value+"'");
            }
            object check = this.getSingleResult("select * from " + row.name + " where " + where);
          

            if (check == null)
            {
                QueryBuilder qbl = this.CreateQueryBuilder();
                qbl.setType(ABSTRACTQUERYTYPES.InsertQuery);
                qbl.addSource(row.name);
                qbl.setFieldList(row.getFieldList());
                string sql = qbl.Compile();
            }

        }

        public void InsertIfAbsent(ATable row, string[] except)
        {
            string where = "";
            foreach (AField f in row.getFieldList())
            {
                bool wordBanned = Array.Exists(except, new Predicate<string>(
                    delegate(String str)
                    {
                        if (str == f.name) return true;
                        return false;
                    }
                ));
                if(!wordBanned)
                where = (where == "" ? f.name + "='" + f.value + "'" : where + " and " + f.name + "='" + f.value + "'");
            }
            object check = this.getSingleResult("select * from " + row.name + " where " + where);
         

            if (check == null)
            {
                QueryBuilder qbl = this.CreateQueryBuilder();
                qbl.setType(ABSTRACTQUERYTYPES.InsertQuery);
                qbl.addSource(row.name);
                qbl.setFieldList(row.getFieldList());
                string sql = qbl.Compile();
            }

        }
    }







    

}

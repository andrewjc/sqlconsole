using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;

namespace SQLConsole.Data
{
    public class TransactionQueue
    {
        protected System.Collections.ArrayList _transList;
        protected bool Atomic;
        protected bool AutoExecute;
        protected Database _dbo;
        System.Data.Odbc.OdbcTransaction odt;
        Database dbParent;
        OdbcCommand oc;
        protected int _errorState;
        protected string _errorString;
        protected long _lastID;
        public TransactionQueue(Database parent)
        {
            dbParent = parent;
            SQLConsole.Data.LLDBA.SERVERINFO svrinfo;
            svrinfo = parent.GetDatabaseConnector().svrinfo;
            this._dbo = parent;
            this._dbo.DBEvent_ReconnectingCloseCause += new Database.ReconnectingCloseCause(_dbo_DBEvent_ReconnectingCloseCause);

            this._transList = new System.Collections.ArrayList();
            this.Atomic = true;
            this.AutoExecute = false;
            this.odt = null;
            this.oc = null;
            this._errorState = 0;
            this._lastID = -1;
        }

        void _dbo_DBEvent_ReconnectingCloseCause(object e)
        {
            this.dbParent.RaiseDBCloseClause();
        }

        public bool UseAtomic
        {
            get { return this.Atomic; }
            set { this.Atomic = value; }
        }

        public bool UseAutoExecute {
            get { return this.AutoExecute; }
            set { this.AutoExecute = value; }
        }

        public string getLastError
        {
            get { return this._errorString; }
        }

        public int getErrorState
        {
            get { return this._errorState; }
        }

        public long getLastInsertID() { return this._lastID; }

        public bool Queue(string sql)
        {
            if(this.AutoExecute == true) {
                if (this.odt == null)
                {
                    //Fix for mysql.
                    if (this._dbo.GetDatabaseConnector().getDBType() == DATABASETYPES.MYSQL)
                    {
                        this._dbo.GetDatabaseConnector().executeNonQuery("SET AUTOCOMMIT = 0");
                        this._dbo.GetDatabaseConnector().executeNonQuery("START TRANSACTION");

                    }
                    
                    this.odt = this._dbo.GetDatabaseConnector().GetRawConnectionObject().BeginTransaction(IsolationLevel.RepeatableRead);
                } 
                if (this.oc == null)
                {
                    this.oc = new OdbcCommand();
                    this.oc.CommandType = CommandType.Text;
                    this.oc.Connection = this.odt.Connection;
                    this.oc.Transaction = this.odt;
                }
                try
                {
                    string tmpsql = sql;
                    //if (this._dbo.GetDatabaseConnector().Open() != ERRORCODES.NONE)
                    //{
                    //    throw (new Exception("Database connection lost."));
                    //}
                    oc.CommandText = this._dbo.GetDatabaseConnector().DoTopLevelSqlTranslations(ref tmpsql);
                    oc.ExecuteNonQuery();

                    oc.CommandText = this._dbo.GetDatabaseProvider().buildGetLastInsertID();
                    try
                    {
                        this._lastID = Convert.ToInt32(oc.ExecuteScalar());
                    }
                    catch (Exception e) {
                        if (this.dbParent.GetDatabaseConnector().GetRawConnectionObject().State == ConnectionState.Closed)
                            this.dbParent.RaiseDBCloseClause();
                        this._lastID = -1; this._errorString = e.Message + "\r\n\r\n" + e.StackTrace; 
                    }
                    //this._lastID = 999;
                    //this._errorState = 0;
                    return true;
                }
                catch(Exception e) {
                	System.Diagnostics.Debug.Print(e.Message);
                    if (this.dbParent.GetDatabaseConnector().GetRawConnectionObject().State == ConnectionState.Closed)
                        this.dbParent.RaiseDBCloseClause();
                    this._errorState = 1;
                    return false;
                }
            }
            else
                this._transList.Add(sql);
            return true;
        }

        public bool Queue(QueryBuilder qbl)
        {   
            qbl.autoExecute = false;
            return Queue(qbl.Compile());
        }

        public void Commit()
        {
            try
            {
                if (this._dbo.GetDatabaseConnector().getDBType() == DATABASETYPES.MYSQL)
                {
                    this._dbo.GetDatabaseConnector().executeNonQuery("SET AUTOCOMMIT = 1");
                    this._dbo.GetDatabaseConnector().executeNonQuery("COMMIT");
                }
                this.odt.Commit();
                this._errorState = 0;

                this.oc.Dispose();
                this.oc.Transaction = null;
            }
            catch {
                if (this.dbParent.GetDatabaseConnector().GetRawConnectionObject().State == ConnectionState.Closed)
                this.dbParent.RaiseDBCloseClause(); 
            }
            
        }
        public void RollBack()
        {
            try
            {
                if (this._dbo.GetDatabaseConnector().getDBType() == DATABASETYPES.MYSQL)
                {
                    this._dbo.GetDatabaseConnector().executeNonQuery("SET AUTOCOMMIT = 1");
                    this._dbo.GetDatabaseConnector().executeNonQuery("ROLLBACK");
                }
                this.odt.Rollback();
            }
            catch { }
        }


        public bool Parse() {
            
            try
            {
                //Fix for mysql.
                if (this._dbo.GetDatabaseConnector().getDBType() == DATABASETYPES.MYSQL)
                {
                    this._dbo.GetDatabaseConnector().executeNonQuery("SET AUTOCOMMIT = 0");
                    this._dbo.GetDatabaseConnector().executeNonQuery("BEGIN");

                }
                odt = this._dbo.GetDatabaseConnector().GetRawConnectionObject().BeginTransaction();
                this.oc = new OdbcCommand();
                oc.Connection = odt.Connection;
                oc.Transaction = odt;
                foreach (string sql in this._transList)
                {
                    //string tmpsql = this._dbo.CompileSQLToNative(sql);
                    string tmpsql = sql;
                    if ((tmpsql != "") && tmpsql.Length > 2)
                    {
                        if (this._dbo.GetDatabaseConnector().Open() != ERRORCODES.NONE)
                        {
                            throw (new Exception("Database connection lost."));
                        }
                        oc.CommandText = this._dbo.GetDatabaseConnector().DoTopLevelSqlTranslations(ref tmpsql);
                        oc.ExecuteNonQuery();
                    }
                }
                odt.Commit();
                if (this._dbo.GetDatabaseConnector().getDBType() == DATABASETYPES.MYSQL)
                this._dbo.GetDatabaseConnector().executeNonQuery("COMMIT");
                return true;
            }
            catch(Exception e)
            {
                if(e.Message.IndexOf("parallel transactions") > 0) {
                    // Start new connection and try again.
                    this._dbo = this._dbo.getNewConnection();
                    this.Parse();
                }
                if(this.dbParent.GetDatabaseConnector().GetRawConnectionObject().State == ConnectionState.Closed)
                this.dbParent.RaiseDBCloseClause();
                try
                {
                   
                    odt.Rollback();
                    if (this._dbo.GetDatabaseConnector().getDBType() == DATABASETYPES.MYSQL)
                        this._dbo.GetDatabaseConnector().executeNonQuery("ROLLBACK");
                }
                catch { /**/ }
                this._errorState = 1;
                this._errorString = e.Message;
                return false;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Xml.Serialization;
namespace SQLConsole.Data
{
    public enum DATABASETYPES
    {
        UNKNOWN = -1, MYSQL = 1, MSSQL = 2, MSSQL2005 = 3, MSACCESS = 4, ORACLE = 5, SQLITE = 6,
        MSSQLCE
    }

    public enum ERRORCODES
    {
        NONE = 0x0000,
        ERRORCODE_CONNECT_CLOSE = 0x0001,
        ERRORCODE_CONNECT_TIMEOUT = 0x0002,
        ERRORCODE_CONNECT_INVALIDLOGIN = 0x0005,
        ERRORCODE_CONNECT_HOST = 0x0003,
        ERRORCODE_CONNECT_UNKNOWN = 0x00054,
        ERRORCODE_DRIVER = 0x666
    }

    [Serializable]
    public enum FIELDTYPES
    {
        STRING = 0, INT = 1
    }

    [Serializable]
    public enum ODBCMODIFIERS
    {
        TRUSTED = 1, WORKGROUP = 2, EXCLUSIVE = 3, DIRECT = 4
    }

    public class LLDBA
    {
        public delegate void DBEVENT_ReconnectingCloseCause(object e);

        public delegate void DBEVENT_ReConnected(object e);
        static public WaitForm waitForm = new WaitForm();
        public struct SERVERINFO
        {
            public string _username;
            public string _password;
            public string _server;
            public string _database;
            public DATABASETYPES _dbtype;
            public string dbFlatFile;
        }; public SERVERINFO svrinfo;

        internal const int PORT_MYSQL = 3306;
        internal const int PORT_MSSQL = 7006;
        internal const int PORT_MSSQL2005 = 7001;

        OdbcConnection _odbcConnection;
        ODBCMODIFIERS odbcModifier;
        public bool hmst_status = false;
        protected int ErrorStatus;
        protected string ErrorString;

        #region constructors

        public LLDBA(string server, ODBCMODIFIERS modifiers, DATABASETYPES types)
            : this()
        {
            this.setODBCModifiers(modifiers);
            this.svrinfo._server = server;
            this.svrinfo._dbtype = types;
        }

        public LLDBA(string databasefile, ODBCMODIFIERS modifiers)
            : this(databasefile)
        {
            this.setODBCModifiers(modifiers);
        }
        public LLDBA(string databasefile)
        {
            this.svrinfo._dbtype = DATABASETYPES.MSACCESS;
            this.svrinfo._database = databasefile;
        }

        public LLDBA(string databasefile, DATABASETYPES dbtype)
        {
            this.svrinfo._dbtype = dbtype;
            this.svrinfo._database = databasefile;
        }
        public LLDBA(string databasefile, DATABASETYPES dbtype, string password)
        {
            this.svrinfo._dbtype = dbtype;
            this.svrinfo._database = databasefile;
            this.svrinfo._password = password;
        }




        public LLDBA(string server, string username, string password, string database, DATABASETYPES dbtype, bool autoconnect)
            : this(server, username, password)
        {
            //this.Open(database);
            this.svrinfo._database = database;
        }

        public LLDBA(string server, string username, string password, string database, DATABASETYPES dbtype)
            : this(server, username, password, dbtype)
        {
            //Open(database);
            this.svrinfo._database = database;
        }

        public LLDBA(string server, string username, string password, DATABASETYPES dbtype)
            : this(server, username, password)
        {
            this.svrinfo._dbtype = dbtype;
        }

        public LLDBA(string server, string username, string password, bool autoconnect)
            : this(server, username, password)
        {
            //this.Open();

        }

        public LLDBA(string server, string username, string password)
            : this()
        {
            this.svrinfo = new SERVERINFO();
            this.svrinfo._server = server;
            this.svrinfo._username = username;
            this.svrinfo._password = password;
            determineServerType();
        }

        public LLDBA()
        {
            this.svrinfo = new SERVERINFO();
            // if we have a settings.server.xml file present, load the values in.
            settingsClass settings = new settingsClass();
            if (System.IO.File.Exists("settings.server.xml"))
            {
                XmlSerializer s = new XmlSerializer(typeof(settingsClass));
                System.IO.TextReader w = new System.IO.StreamReader(@"settings.server.xml");
                settings = (settingsClass)s.Deserialize(w);
                w.Close();
                this.svrinfo._server = settings.server;
                this.svrinfo._username = settings.userid;
                this.svrinfo._password = settings.password;
                this.svrinfo._dbtype = settings.dbtype;
                this.svrinfo._database = settings.database;
            }

            this.ErrorStatus = 0;
            this.ErrorString = "[None]";
        }


        #endregion

        #region connections

        public OdbcConnection GetRawConnectionObject() { return this._odbcConnection; }

        public ERRORCODES Open(string database)
        {
            this.svrinfo._database = database;
            return Open();
        }

        public ERRORCODES OpenODBC(string odbcstr)
        {
            this._odbcConnection = new OdbcConnection(odbcstr);
            try
            {
                this._odbcConnection.Open();
            }
            catch
            {
                return ERRORCODES.ERRORCODE_CONNECT_UNKNOWN;
            }

            return ERRORCODES.NONE;
        }

        public ERRORCODES Connect()
        {
            string connectionString = "";
            connectionString = this.buildConnectionString();
            this._odbcConnection = new OdbcConnection(connectionString);
            try
            {
                this._odbcConnection.Open();
                return ERRORCODES.NONE;
            }
            catch
            {
                return ERRORCODES.ERRORCODE_CONNECT_UNKNOWN;
            }
        }

        public ERRORCODES Open()
        {
            string connectionString;
            int retryCount = 0;
            if (this._odbcConnection == null)
            {
                connectionString = "";
                connectionString = this.buildConnectionString();
                this._odbcConnection = new OdbcConnection(connectionString);

            }
            while (this._odbcConnection.State != System.Data.ConnectionState.Open)
            {
                try
                {
                    /*
                    ThreadExecutionQueue teq = new ThreadExecutionQueue();
                    teq.Initialize(this);
                    teq.BeginOpen(this._odbcConnection);

                    int response = (int)teq.GetResponseA();
                    */
                    connectionString = this.buildConnectionString();
                    this._odbcConnection.ConnectionString = connectionString;
                    if (this._odbcConnection.ConnectionString == "")
                        return ERRORCODES.ERRORCODE_CONNECT_INVALIDLOGIN;
                    else
                        this._odbcConnection.Open();

                }
                catch (Exception e)
                {
                    throw (new Exception("Database connect exception", e));
                }
                finally
                {

                }
            }

            return ERRORCODES.NONE;
        }

        public void Close()
        {
            try
            {
                this._odbcConnection.Close();
                this._odbcConnection = null;
            }
            catch
            {

            }
        }

        public void setODBCModifiers(ODBCMODIFIERS modifiers)
        {
            this.odbcModifier = modifiers;
        }

        public string buildConnectionString()
        {
            if (this.buildConnectionStringA() == "" && this.svrinfo._dbtype != 0)
            {
                throw(new InvalidOperationException("Required ODBC driver is missing."));
            }
            // This is actually just a wrapper. It allows us to get rid of empty values.
            string[] newConnectionString = this.buildConnectionStringA().Split(';');
            string outputConnectionString = "";
            foreach (string ffVal in newConnectionString)
            {
                if (ffVal.IndexOf('=') != (ffVal.Length - 1))
                {
                    outputConnectionString += ffVal + ";";
                }
            }
            return outputConnectionString;
        }




        public string buildConnectionStringA()
        {
            try
            {
                switch (this.svrinfo._dbtype)
                {
                    case DATABASETYPES.MYSQL:
                        // which version of the odbc driver to use.
                        if (Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\ODBC\ODBCINST.INI\MySQL ODBC 5.1 Driver", "Driver", 0) != null)
                        {
                            return @"DRIVER={MySQL ODBC 5.1 Driver};SERVER=" + this.svrinfo._server + @";DATABASE=" + this.svrinfo._database + ";USER=" + this.svrinfo._username + ";PASSWORD=" + this.svrinfo._password + ";option=3;";
                        }
                        else if (Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\ODBC\ODBCINST.INI\MySQL ODBC 3.51 Driver", "Driver", "0") != null)
                        {
                            return @"DRIVER={MySQL ODBC 3.51 Driver};SERVER=" + this.svrinfo._server + @";DATABASE=" + this.svrinfo._database + ";USER=" + this.svrinfo._username + ";PASSWORD=" + this.svrinfo._password + ";option=3;";
                        }
                        else if (Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\MySQL AB\MySQL Connector/ODBC 5.1", "Version", "0") != null)
                        {
                            return @"DRIVER={MySQL ODBC 5.1 Driver};SERVER=" + this.svrinfo._server + @";DATABASE=" + this.svrinfo._database + ";USER=" + this.svrinfo._username + ";PASSWORD=" + this.svrinfo._password + ";option=3;";
                        }
                        else if (Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\MySQL AB\MySQL Connector/ODBC 3.51", "Driver", "0") != null)
                        {
                            return @"DRIVER={MySQL ODBC 3.51 Driver};SERVER=" + this.svrinfo._server + @";DATABASE=" + this.svrinfo._database + ";USER=" + this.svrinfo._username + ";PASSWORD=" + this.svrinfo._password + ";option=3;";
                        }

                        else return "";
                    case DATABASETYPES.MSSQL:
                        return @"Driver={SQL Server};Server=" + this.svrinfo._server + @";Database=" + this.svrinfo._database + ";Uid=" + this.svrinfo._username + ";Pwd=" + this.svrinfo._password + ";";
                    case DATABASETYPES.MSSQLCE:

                    case DATABASETYPES.MSSQL2005:
                        // work out the provider...
                        string sqlsvrprovider = "";
                        if (Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\ODBC\ODBCINST.INI\SQL Native Client", "Driver", 0) != null)
                        {
                            sqlsvrprovider = "SQL Native Client";
                        }
                        else if (Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\ODBC\ODBCINST.INI\SQL Server Native Client 10.0", "Driver", 0) != null)
                        {
                            sqlsvrprovider = "SQL Server Native Client 10.0";
                        }
                        if (this.svrinfo._username == "")
                        {
                            return @"Driver={" + sqlsvrprovider + "};Server=" + this.svrinfo._server + @";Database=" + this.svrinfo._database + ";Trusted_Connection=yes;";
                        }
                        else
                            return @"Driver={" + sqlsvrprovider + "};Server=" + this.svrinfo._server + @";Database=" + this.svrinfo._database + ";Uid=" + this.svrinfo._username + ";Pwd=" + this.svrinfo._password + ";";

                    case DATABASETYPES.MSACCESS:
                        if (this.odbcModifier == ODBCMODIFIERS.DIRECT)
                        {
                            return @"Driver={Microsoft Access Driver (*.mdb)};Dbq=" + this.svrinfo._database + ";";
                        }
                        else if (this.odbcModifier == ODBCMODIFIERS.EXCLUSIVE)
                        {
                            return @"Driver={Microsoft Access Driver (*.mdb)};Dbq=" + this.svrinfo._database + @";Exclusive=1;";
                        }
                        else if (this.odbcModifier == ODBCMODIFIERS.WORKGROUP)
                        {
                            return @"Driver={Microsoft Access Driver (*.mdb)};Dbq=" + this.svrinfo._database + @";SystemDB=C:\mydatabase.mdw;";
                        }
                        else
                        {
                            return @"Driver={Microsoft Access Driver (*.mdb)};Dbq=" + this.svrinfo._database + @";Exclusive=1;";
                        }
                    case DATABASETYPES.ORACLE:
                        return "Driver={Microsoft ODBC for Oracle};Server=" + this.svrinfo._server + ";Uid=" + this.svrinfo._username + ";Pwd=" + this.svrinfo._password + ";";
                    case DATABASETYPES.SQLITE:

                        if (String.IsNullOrEmpty(this.svrinfo._password) != true)
                        {
                            // with password
                            return "DRIVER=SQLite3 ODBC Driver;Password=" + this.svrinfo._password + ";Database=" + this.svrinfo._database + ";LongNames=0;Timeout=1000;NoTXN=0;SyncPragma=NORMAL;StepAPI=0;";

                        }
                        else
                        {
                            // without
                            return "DRIVER=SQLite3 ODBC Driver;Database=" + this.svrinfo._database + ";LongNames=0;Timeout=1000;NoTXN=0;SyncPragma=NORMAL;StepAPI=0;";
                        }
                    default:
                        return "";
                }
            }
            catch
            {
                throw(new InvalidOperationException("Failed finding connection string."));
                return null;
            }
        }

        internal void determineServerType()
        {
            // This function is not implimented yet.
        }

        public DATABASETYPES getDBType()
        {
            return this.svrinfo._dbtype;
        }

        #endregion

        #region queryExecutors
        /* QueryExecutors need to execute sql through the odbc drivers...
         * 
        */

        public string getSingleVal(string sql, Boolean doTranslations)
        {
            try
            {

                if (this.Open() != ERRORCODES.NONE)
                {
                    throw (new Exception("Database connection lost."));
                }

                OdbcCommand oc = null;
                if (this._odbcConnection.State != System.Data.ConnectionState.Open)
                {
                    oc = new OdbcCommand(sql, this.getNewRawConnection().GetRawConnectionObject());
                }
                else
                {
                    oc = new OdbcCommand(sql, this._odbcConnection);
                }
                oc = new OdbcCommand(sql, this._odbcConnection);
                //ThreadExecutionQueue teq = new ThreadExecutionQueue();
                //teq.Initialize(this);
                //teq.BeginExecuteReader(oc);
                hmst_status = true;
                OdbcDataReader dr = oc.ExecuteReader();
                //OdbcDataReader dr = teq.GetResponse();
                if (dr.Read())
                {
                    string val = Convert.ToString(dr.GetValue(0));
                    dr.Close();
                    hmst_status = false;
                    dr = null;
                    oc = null;
                    return val;
                }

                return "";
            }
            catch (Exception e)
            {
                if ((e.Message.IndexOf("pending local transaction") > 0) | (e.Message.IndexOf("busy with results") > 0)) //quick fix.
                {
                    LLDBA redist = this.getNewRawConnection();
                    string ret = redist.getSingleVal(sql);
                    redist.Close();
                    return ret;
                }
                else
                    throw (e);
            }
        }


        public string getSingleVal(string sql)
        {
            try
            {

                if (this.Open() != ERRORCODES.NONE)
                {
                    throw (new Exception("Database connection lost."));
                }
                sql = DoTopLevelSqlTranslations(ref sql);
                OdbcCommand oc = null;
                if (this._odbcConnection.State != System.Data.ConnectionState.Open)
                {
                    oc = new OdbcCommand(sql, this.getNewRawConnection().GetRawConnectionObject());
                }
                else
                {
                    oc = new OdbcCommand(sql, this._odbcConnection);
                }

                if (this.hmst_status)
                {
                    oc = new OdbcCommand(sql, this.getNewRawConnection().GetRawConnectionObject());

                }

                //oc = new OdbcCommand(sql, this._odbcConnection);
                //ThreadExecutionQueue teq = new ThreadExecutionQueue();
                //teq.Initialize(this);
                //teq.BeginExecuteReader(oc);
                this.hmst_status = true;
                OdbcDataReader dr = oc.ExecuteReader();
                //OdbcDataReader dr = teq.GetResponse();
                if (dr.Read())
                {
                    string val = Convert.ToString(dr.GetValue(0));
                    dr.Close();
                    this.hmst_status = false;
                    dr = null;
                    oc = null;
                    return val;
                }

                return null;
            }
            catch (Exception e)
            {
                if ((e.Message.IndexOf("pending local transaction") > 0) | (e.Message.IndexOf("busy with results") > 0)) //quick fix.
                {
                    LLDBA redist = this.getNewRawConnection();
                    string ret = redist.getSingleVal(sql);
                    redist.Close();
                    return ret;
                }
                else
                    throw (e);
            }
        }

        public OdbcDataReader getResult(string sql)
        {
            try
            {
                if (this.Open() != ERRORCODES.NONE)
                {
                    throw (new Exception("Database connection lost."));
                }
                OdbcCommand oc;
                //sql = DoTopLevelSqlTranslations(ref sql);
                // If the connection is busy, we do a callback, creating a new connection to use
                if (this._odbcConnection.State != System.Data.ConnectionState.Open)
                {
                    oc = new OdbcCommand(sql, this.getNewRawConnection().GetRawConnectionObject());
                }
                else
                {
                    oc = new OdbcCommand(sql, this._odbcConnection);
                }
                if (this.hmst_status)
                {
                    oc = new OdbcCommand(sql, this.getNewRawConnection().GetRawConnectionObject());

                }
                //ThreadExecutionQueue teq = new ThreadExecutionQueue();
                //teq.Initialize(this);
                //teq.BeginExecuteReader(oc);

                //OdbcDataReader dr = teq.GetResponse();
                this.hmst_status = true;
                OdbcDataReader dr = oc.ExecuteReader();
                return dr;
            }
            catch (Exception e)
            {
                if ((e.Message.IndexOf("pending local transaction") > 0) | (e.Message.IndexOf("busy with results") > 0)) //quick fix.
                {
                    LLDBA redist = this.getNewRawConnection();
                    OdbcDataReader ret = redist.getResult(sql);

                    return ret;
                }
                else
                    throw (e);
            }
        }

        public void resetHMST() { this.hmst_status = false; }

        public int executeNonQuery(string sql)
        {
            long tempID = -1;
            return executeNonQuery(sql, ref tempID);
        }

        public int executeNonQuery(string sql, ref long lastid)
        {
            if (sql.Equals(";")) return -1;
            sql = DoTopLevelSqlTranslations(ref sql);
            if (sql.Length <= 0) return 0;
            try
            {
                if (this.Open() != ERRORCODES.NONE)
                {
                    throw (new Exception("Database connection lost."));
                }
                OdbcCommand oc = new OdbcCommand(sql, this._odbcConnection);
                int retCode = oc.ExecuteNonQuery();
                //ThreadExecutionQueue teq = new ThreadExecutionQueue();
                //teq.Initialize(this);
                //teq.BeginExecuteNonQuery(oc);
                //int retCode = (int)teq.GetResponseA();
                if (this.getDBType() == DATABASETYPES.MYSQL)
                    oc = new OdbcCommand("select last_insert_id()", this._odbcConnection);
                else if (this.getDBType() == DATABASETYPES.SQLITE)
                    oc = new OdbcCommand("SELECT last_insert_rowid()", this._odbcConnection);
                else
                {
                    oc = new OdbcCommand("select @@IDENTITY", this._odbcConnection);
                }


                //teq = new ThreadExecutionQueue();
                //teq.Initialize(this);
                //teq.BeginExecuteScalar(oc);

                //object retVal = teq.GetResponseA();
                object retVal = oc.ExecuteScalar();
                oc = null;
                //this._odbcConnection.Close(); //close between goes...
                if (retVal.GetType() != typeof(DBNull))
                    lastid = Convert.ToInt32(retVal);
                return 1;
            }
            catch (Exception e)
            {
                if ((e.Message.IndexOf("pending local transaction") > 0) | (e.Message.IndexOf("busy with results") > 0)) //quick fix.
                {
                    LLDBA redist = this.getNewRawConnection();
                    int ret = redist.executeNonQuery(sql, ref lastid);
                    redist.Close();
                    return ret;
                }
                else
                    throw (e);
            }
        }

        public int? getSingleValAsInt(string sql)
        {
            try
            {
                string retVal = getSingleVal(sql);
                if (retVal == "") return null;
                return Convert.ToInt32(retVal);
            }
            catch (Exception e)
            {
                if ((e.Message.IndexOf("pending local transaction") > 0) | (e.Message.IndexOf("busy with results for another hstmt") > 0)) //quick fix.
                {
                    LLDBA redist = this.getNewRawConnection();
                    int? ret = redist.getSingleValAsInt(sql);
                    redist.Close();
                    return ret;
                }
                else
                    throw (e);
            }
        }

        public int? getSingleValAsInt(SQLConsole.Data.QueryBuilder queryBuilder)
        {

            queryBuilder.autoExecute = false;
            return getSingleValAsInt(queryBuilder.Compile());
        }

        public string getSingleVal(SQLConsole.Data.QueryBuilder queryBuilder)
        {
            string sqlSrc = "";
            queryBuilder.autoExecute = false;
            sqlSrc = queryBuilder.Compile();
            sqlSrc = DoTopLevelSqlTranslations(ref sqlSrc);
            return getSingleVal(sqlSrc);
        }

        public LLDBA getNewRawConnection()
        {
            LLDBA newDBA = new LLDBA();
            newDBA.svrinfo = this.svrinfo;
            newDBA.Open(newDBA.svrinfo._database);
            return newDBA;
        }

        public string DoTopLevelSqlTranslations(ref string sql)
        {
            // handle #__ formatting on table names.
            string rawSqlStr = sql;
            if (sql.IndexOf(';') <= 0) rawSqlStr = rawSqlStr + ";";
            string workingSql = "";
            string tmpSQL = "";
            //string[] breakdown = rawSqlStr.Split(';');
            string[] breakdown = (string[])SQLConsole.Data.utils.split_QF(sql, ";");

            string prefix = this.svrinfo._username;
            if (prefix == null) prefix = "T";
            if (prefix == "") prefix = "T";
            foreach (string sqlline in breakdown)
            {

                tmpSQL = sqlline.Trim();
                if (tmpSQL.IndexOf("'") > 1)
                {
                    string tmpStr = tmpSQL.Substring(0, tmpSQL.IndexOf("'"));
                    string tmpStr2 = tmpSQL.Substring(tmpSQL.IndexOf("'"));
                    tmpStr = tmpStr.Replace("#__", prefix + "_");
                    tmpSQL = tmpStr + tmpStr2;
                    workingSql += tmpSQL + ";";
                }
                else
                    workingSql += tmpSQL.Replace("#__", prefix + "_");
            }
            return workingSql;
        }
        #endregion

        public string getLastErrorString() { return this.ErrorString; }

        public static DATABASETYPES getProviderFromString(string typeid)
        {
            switch (typeid.ToUpper())
            {
                case "MYSQL": return DATABASETYPES.MYSQL;
                case "MSSQL":
                case "MSSQL2000": return DATABASETYPES.MSSQL;

                case "MSSQL2005": return DATABASETYPES.MSSQL2005;

                case "MSACCESS": return DATABASETYPES.MSACCESS;
                case "ORACLE": return DATABASETYPES.ORACLE;
                case "SQLITE": return DATABASETYPES.SQLITE;
                default:
                    return DATABASETYPES.MSSQL;
            }
        }

        static public SQLConsole.Data.DATABASETYPES getProviderFromInt(int typeid)
        {
            switch (typeid)
            {
                case 1: return DATABASETYPES.MYSQL;
                case 2: return DATABASETYPES.MSSQL;

                case 3: return DATABASETYPES.MSSQL2005;

                case 4: return DATABASETYPES.MSACCESS;
                case 5: return DATABASETYPES.ORACLE;
                case 6: return DATABASETYPES.SQLITE;
                default:
                    return DATABASETYPES.MSSQL;
            }
        }
    }


}



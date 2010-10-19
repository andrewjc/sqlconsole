using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using SQLConsole.Data;
using SQLConsole.Data.DatabaseProviders;


namespace SQLConsole.Data
{
    public class TableCache
    {
        private Hashtable _tableArray;
        private Database _db;
        private bool EnableTableCache = true;
        public TableCache(Database dbObj)
        {
            this._tableArray = new Hashtable();
            this._db = dbObj;
            // Load the table cache for the given database.
            deserializeCache();
        }

        public ATable getCachedTable(string tablename)
        {
            if (tablename == null) return null;
            System.Diagnostics.Debug.WriteLine("Getting Cached Table: " + tablename);
            ATable returnTable = null;
            //tablename = this._db.GetDatabaseConnector().DoTopLevelSqlTranslations(ref tablename);
            if ((this._tableArray.ContainsKey(tablename) == true) && (EnableTableCache == true))
            {
                returnTable = (ATable)_tableArray[tablename];
            }
            else
            {
                returnTable = cacheTable(tablename);
                if (returnTable == null) return null;
                serializeCache();
            }
            returnTable.setDatabaseObject(this._db);
            return returnTable;
        }

        public bool isCached(ATable tbl)
        {
            foreach (ATable ctbl in this._tableArray.Values)
            {
                if (ctbl.name.Equals(tbl.name)) return true;
            }
            return false;
        }

        public void removeFromCache(string tablename)
        {
            this._tableArray.Remove(tablename);
            this.serializeCache();

        }

        public ATable cacheTable(string tablename)
        {
           // if (this._tableArray.ContainsKey(tablename)) this._tableArray.Remove(tablename);
            // Query the database to get information about this table.
            ATable newCacheObject = null;
            System.Diagnostics.Debug.WriteLine("Caching Table: " + tablename);
            try
            {
                ATable cacheObject = new ATable(tablename);
                FieldDescriptor[] fieldinfo = this._db.GetDatabaseProvider().getTableDescription(tablename);
                if (fieldinfo == null)
                {
                    throw (new Exception("Error getting table."));
                }           

            foreach (FieldDescriptor fInfo in fieldinfo)
            {
                cacheObject.addField(fInfo.name, fInfo.type, fInfo.maxlen, fInfo.defaultval, fInfo.modifiers);
            }
            this._tableArray.Add(tablename, cacheObject);
            newCacheObject = cacheObject;
            }
            catch(Exception e)
            {
            	System.Diagnostics.Debug.Print(e.Message);
            }
            return newCacheObject;
        }

        public void serializeCache()
        {
            FileStream fs = null;
            try
            {
                BinaryFormatter s = new BinaryFormatter();
                string cacheFN = "Cache.";
                if (this._db.GetDatabaseConnector().svrinfo._server != null)
                    cacheFN += this._db.GetDatabaseConnector().svrinfo._server.Replace("\\", "").Replace(":", "").Replace(".", "");
                if (this._db.GetDatabaseConnector().svrinfo._database != null)
                cacheFN += this._db.GetDatabaseConnector().svrinfo._database.Replace("\\", "").Replace(":", "").Replace(".", "") + ".t";
                fs = new FileStream(cacheFN, FileMode.OpenOrCreate, FileAccess.Write);
                s.Serialize(fs, this._tableArray);
                fs.Close();
            }
            catch(Exception e) 
            {
				System.Diagnostics.Debug.Print(e.Message);
                fs.Close();
            }
        }

        public void deserializeCache()
        {
            try
            {
                BinaryFormatter s = new BinaryFormatter();
                
                string cacheFN = "Cache.";
                if (this._db.GetDatabaseConnector().svrinfo._server != null)
                    cacheFN += this._db.GetDatabaseConnector().svrinfo._server.Replace("\\", "").Replace(":", "").Replace(".", "");
                if (this._db.GetDatabaseConnector().svrinfo._database != null)
                cacheFN += this._db.GetDatabaseConnector().svrinfo._database.Replace("\\", "").Replace(":", "").Replace(".", "") + ".t";
                
                
                FileStream fs = new FileStream(cacheFN, FileMode.OpenOrCreate, FileAccess.Read);
                try
                {
                    if(fs.Length > 0)
                    this._tableArray = (Hashtable)s.Deserialize(fs);
                }
                catch(Exception e) { 
                	System.Diagnostics.Debug.Print(e.Message);
                    // rebuild and reserialize...
                    fs.Close();
                    this.rebuildAll();
                    this.serializeCache();
                }
                fs.Close();
            }
            catch { }
        }

        public void rebuildAll()
        {
            // This function deletes all cache data, causing it to be rebuilt
            this._tableArray.Clear();
        }

    }
}

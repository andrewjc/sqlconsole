using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using SQLConsole.Data;
using SQLConsole.Data.DatabaseProviders;
using SQLConsole.Data.ProviderConverters;
using System.Data.Odbc;
namespace SQLConsole.Data
{

    public class ThreadExecutionQueue
    {
        private LLDBA dbo;
        private object returnObject = null;
        private int returnMutex = 0;
        private long startTime = 0;
        const double TIME_MAX_THREAD_USAGE = 1; //3 seconds.
        private string lastQuery = "";
        AutoResetEvent releaseMutex = new AutoResetEvent(false);
        
        public void Initialize(LLDBA dbo) {
            this.dbo = dbo;
            
            LLDBA.waitForm.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            LLDBA.waitForm.Location = new System.Drawing.Point(200, 200);
        }

        public void BeginExecuteReader(OdbcCommand reader)
        {
            System.Diagnostics.Debug.Print("starting execute reader");
            this.lastQuery = reader.CommandText;
            //this._thread = new Thread(new ParameterizedThreadStart(_InternalThreadExecuteReader));
            this.returnMutex = 0;
            //this._thread.Start(reader);

            ThreadPool.QueueUserWorkItem(new WaitCallback(_InternalThreadExecuteReader), reader);
        }
        public void BeginExecuteNonQuery(OdbcCommand reader)
        {
            System.Diagnostics.Debug.Print("starting execute non query");
            this.lastQuery = reader.CommandText;
            //this._thread = new Thread(new ParameterizedThreadStart(_InternalThreadExecuteNonQuery));
            this.returnMutex = 0;
            //this._thread.Start(reader);
            ThreadPool.QueueUserWorkItem(new WaitCallback(_InternalThreadExecuteNonQuery), reader);
        }
        private void _InternalThreadExecuteReader(object reader)
        {
            try
            {
                int usingPrimaryHMST = 0;
                this.startTime = System.Environment.TickCount;
                if (this.dbo.hmst_status == true)
                    ((OdbcCommand)reader).Connection = this.dbo.getNewRawConnection().GetRawConnectionObject();
                else
                {
                    this.dbo.hmst_status = true;
                    usingPrimaryHMST = 1;
                }
                this.returnObject = ((OdbcCommand)reader).ExecuteReader();
                if (usingPrimaryHMST == 1) this.dbo.hmst_status = false;
                this.returnMutex = 1;
                this.releaseMutex.Set();
            }
            catch (Exception e)
            {
                if ((e.Message.IndexOf("pending local transaction") > 0) | (e.Message.IndexOf("busy with results for another hstmt") > 0)) //quick fix.
                {
                    ((OdbcCommand)reader).Connection = this.dbo.getNewRawConnection().GetRawConnectionObject();
                    this.startTime = System.Environment.TickCount;
                    this.returnObject = ((OdbcCommand)reader).ExecuteReader();
                    this.returnMutex = 1;
                    this.releaseMutex.Set();
                    return;
                }
                else
                    throw (e);
            }
        }

        private void _InternalThreadExecuteNonQuery(object reader)
        {
            try {
            int usingPrimaryHMST = 0;
            this.startTime = System.Environment.TickCount;
            if (this.dbo.hmst_status == true)
                ((OdbcCommand)reader).Connection = this.dbo.getNewRawConnection().GetRawConnectionObject();
            else
            {
                this.dbo.hmst_status = true;
                usingPrimaryHMST = 1;
            }
            this.returnObject = ((OdbcCommand)reader).ExecuteNonQuery();
            if (usingPrimaryHMST == 1) this.dbo.hmst_status = false;
            this.returnMutex = 1;
            this.releaseMutex.Set();
            }
            catch (Exception e)
            {

                if ((e.Message.IndexOf("pending local transaction") > 0) | (e.Message.IndexOf("busy with results for another hstmt") > 0)) //quick fix.
                {
                    ((OdbcCommand)reader).Connection = this.dbo.getNewRawConnection().GetRawConnectionObject();
                    this.startTime = System.Environment.TickCount;
                    this.returnObject = ((OdbcCommand)reader).ExecuteNonQuery();
                    this.returnMutex = 1;
                    this.releaseMutex.Set();
                    return;
                }
                else
                    throw (e);
            }
        }

        public void BeginExecuteScalar(OdbcCommand reader)
        {
            System.Diagnostics.Debug.Print("starting execute scalar");
            this.lastQuery = reader.CommandText;
            //this._thread = new Thread(new ParameterizedThreadStart(_InternalThreadExecuteScalar));
            this.returnMutex = 0;
            //this._thread.Start(reader);
            ThreadPool.QueueUserWorkItem(new WaitCallback(_InternalThreadExecuteScalar), reader);
        }



        private void _InternalThreadExecuteScalar(object reader)
        {
            try {

            int usingPrimaryHMST = 0;
            this.startTime = System.Environment.TickCount;
            if (this.dbo.hmst_status == true)
                ((OdbcCommand)reader).Connection = this.dbo.getNewRawConnection().GetRawConnectionObject();
            else
            {
                this.dbo.hmst_status = true;
                usingPrimaryHMST = 1;
            }
            this.returnObject = ((OdbcCommand)reader).ExecuteScalar();
            if (usingPrimaryHMST == 1) this.dbo.hmst_status = false;
            this.returnMutex = 1;
            this.releaseMutex.Set();
            }
            catch (Exception e)
            {
                if ((e.Message.IndexOf("pending local transaction") > 0) | (e.Message.IndexOf("busy with results for another hstmt") > 0)) //quick fix.
                {
                    ((OdbcCommand)reader).Connection = this.dbo.getNewRawConnection().GetRawConnectionObject();
                    this.startTime = System.Environment.TickCount;
                    this.returnObject = ((OdbcCommand)reader).ExecuteScalar();
                    this.returnMutex = 1;
                    this.releaseMutex.Set();
                    return;
                }
                else
                    throw (e);
            }
        }

        public void BeginOpen(OdbcConnection conn)
        {
            System.Diagnostics.Debug.Print("starting open");
            //this._thread = new Thread(new ParameterizedThreadStart(_InternalThreadOpen));
            //this._thread.Start(conn);
            ThreadPool.QueueUserWorkItem(new WaitCallback(_InternalThreadOpen), conn);
        }



        private void _InternalThreadOpen(object conn)
        {
            this.startTime = System.Environment.TickCount;
            try
            {
                ((OdbcConnection)conn).Open();
            }
            catch (Exception e)
            {
            	System.Diagnostics.Debug.Print(e.Message);
                this.returnObject = null;
                this.returnMutex = 1;
                this.releaseMutex.Set();
                return;

            }
            this.returnObject = 1;
            this.returnMutex = 1;
            this.releaseMutex.Set();
        }

        public OdbcDataReader GetResponse()
        {
           
                while (this.returnMutex == 0)
                {
                    if (this.startTime > 0)
                    {
                    if ((System.Environment.TickCount - this.startTime)/1000 > ThreadExecutionQueue.TIME_MAX_THREAD_USAGE)
                    {
                        long test = (System.Environment.TickCount - this.startTime)/1000;
                        LLDBA.waitForm.ShowA();
                        //this.waitForm.Show();
                        System.Diagnostics.Debug.Print("sql: " + this.lastQuery);
                        System.Diagnostics.Debug.Print("start wait " + DateTime.Now.ToLongTimeString());
                        this.releaseMutex.WaitOne();
                        System.Diagnostics.Debug.Print("end wait " + DateTime.Now.ToLongTimeString());
                    }
                    }
                }
                this.startTime = 0;
                LLDBA.waitForm.DelayWait();
                if (this.returnObject != null)
                    return (OdbcDataReader)this.returnObject;
                else
                    return null;
           
        }
        public object GetResponseA()
        {
            while (this.returnMutex == 0)
            {
                if (this.startTime > 0)
                {
                    if ((int)(System.Environment.TickCount - this.startTime)/1000 > ThreadExecutionQueue.TIME_MAX_THREAD_USAGE)
                    {
                        long test = (System.Environment.TickCount - this.startTime)/1000;
                        LLDBA.waitForm.ShowA();
                        System.Diagnostics.Debug.Print("sql: " + this.lastQuery);
                        System.Diagnostics.Debug.Print("start wait " + DateTime.Now.ToLongTimeString());

                        this.releaseMutex.WaitOne();
                        System.Diagnostics.Debug.Print("end wait " + DateTime.Now.ToLongTimeString());

                    }
                }
            }
            this.startTime = 0;
            LLDBA.waitForm.DelayWait();
            if (this.returnObject != null)
                return this.returnObject;
            else
                return null;

        }
    }
}

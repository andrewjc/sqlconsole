using System;
using System.Collections.Generic;
using SQLConsole.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace sqlconsole
{
    static class Queries
    {
        public static int recordsetViewMode = 0; //0 = table, 1 = cardview

        private static OutputMethod_Template _recordsetOutputMethod;
        public static OutputMethod_Template recordsetOutputMethod {
            get
            {
                if(PropertyBag.GetProperty("data.output") == null)
                    PropertyBag.SetProperty("data.output", "screen");
                string proto = PropertyBag.GetProperty("data.output").Trim();
                if (proto.Contains("://"))
                {
                    proto = proto.Substring(0, proto.IndexOf("://"));
                    return OutputManager.FromProtocol(proto);
                }
                else return new QueryOutputMethods.OutputMethod_Screen();


            }
            set
            {
                if(value != null)
                    _recordsetOutputMethod = value;
            }
        }
        public static bool benchMarkMode = false;
        public static string sqlQuery = "";
        public static void singleQueryScreen()
        {
            Console.Title = "Specify SQL Query";

            Console.Clear();
            Boolean exit = false;
            while (!exit)
            {
                Console.Write("SQL Query:>");
                string query = Console.ReadLine();
                if (!String.IsNullOrEmpty(query.Trim()))
                {
                    if (query.Trim() == "exit") { exit = true; break; }
                    Queries.executeDirect(query);
                }
            }
        }

        internal static void executeDirect(string parameters)
        {
            string sqlsrc = parameters;
            byte[] chars = System.Text.Encoding.ASCII.GetBytes(sqlsrc);

            // contained sql?
            if (parameters.Trim().Contains("{") && parameters.Trim().LastIndexOf("}") > (parameters.Length - 10))
            {
                sqlsrc = sqlsrc.Substring(sqlsrc.IndexOf("{")+1);
                sqlsrc = sqlsrc.Substring(0, sqlsrc.LastIndexOf("}"));

            }
            System.Data.Odbc.OdbcDataReader odr;
            try
            {
                odr = Program.dbo.GetDatabaseConnector().getResult(sqlsrc);
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid query specified. Reason:" + e.Message);
                return;
            }

            System.Collections.Hashtable fieldLengthHash = new System.Collections.Hashtable();
            if (odr.HasRows == false) return;
            System.Data.DataTable dt = odr.GetSchemaTable();
            foreach (System.Data.DataRow d in dt.Rows)
            {
                string r = (string)d[0];
                if(String.IsNullOrEmpty(r) == false)
                fieldLengthHash.Add(r, 1);
            }

            // Get field lengths...
            System.Collections.ArrayList rows = new System.Collections.ArrayList();

            while (odr.Read())
            {
                System.Collections.ArrayList row = new System.Collections.ArrayList();
                foreach (System.Data.DataRow d in dt.Rows)
                {
                    string r = (string)d[0];
                    if (String.IsNullOrEmpty(r) == false)
                    {
                        int currentLen = (int)fieldLengthHash[r];

                        AbstractRowField rowObj = new AbstractRowField();
                        rowObj.fieldname = r;

                        object value = odr[r];

                        if (value is System.Byte[])
                        {
                            value = System.Text.Encoding.UTF8.GetString((byte[])value);
                        }
                        else if (value is Int32)
                        {
                            value = Convert.ToString(value, CultureInfo.CurrentCulture);
                        }
                        else if (value is string || value is String)
                        {
                            value = (string)value;
                        }
                        if (Convert.ToString(value, CultureInfo.CurrentCulture).Length > currentLen)
                        {
                            fieldLengthHash[r] = Convert.ToString(value, CultureInfo.CurrentCulture).Length;
                        }

                        rowObj.fieldvalue = Convert.ToString(value, CultureInfo.CurrentCulture);
                        row.Add(rowObj);
                    }
                }
                rows.Add(row);
            }

            // Build column list...
            System.Collections.ArrayList columnList = new System.Collections.ArrayList();
            foreach (System.Data.DataRow d in dt.Rows)
            {
                string r = (string)d[0];
                if (String.IsNullOrEmpty(r) == false)
                {
                    columnList.Add(r);

                    int columnLength = r.Length;
                    int rowLength = (int)fieldLengthHash[r];
                    if (columnLength > rowLength)
                        fieldLengthHash[r] = columnLength;
                }
                else
                {
                    Console.WriteLine("[WARN] Atleast 1 or more columns do not have logical names. Consider giving column aliases in your SQL statement.");
                }
            }

            // Build row collection...
            System.Collections.ArrayList rowList = new System.Collections.ArrayList();
            rowList = rows;

            Queries.recordsetOutputMethod.output(columnList, rowList, fieldLengthHash);

        }

        internal static void benchMark(string sql)
        {
            throw new NotImplementedException();
        }

        internal static void setCurrentQuery(string p)
        {
            throw new NotImplementedException();
        }
    }
}

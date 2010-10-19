using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OrionDB;

namespace System.Data.OrionDB.ProviderConverters
{
    // This is now redundant. These features have been moved into db providers 
    // instead. 
    //  Code here will be removed shortly.

    /*
    public class TypeConverters
    {

        public static string FieldToTypeStringA(AField fieldstruct, DATABASETYPES dbtype)
        {
            string dTypeStr = "";
            switch (fieldstruct.type)
            {
                case ABSTRACTDATATYPES.AString:
                    if((dbtype == DATABASETYPES.MSSQL)||(dbtype == DATABASETYPES.MSSQL2005)) {
                        dTypeStr = "nvarchar";
                    }
                    else if(dbtype == DATABASETYPES.MYSQL) {
                        dTypeStr = "varchar";
                    }
                    else if (dbtype == DATABASETYPES.MSACCESS)
                    {
                        dTypeStr = "TEXT";
                    }
                    break;
                case ABSTRACTDATATYPES.ASmallInteger:
                    if ((dbtype == DATABASETYPES.MSSQL) || (dbtype == DATABASETYPES.MSSQL2005))
                    {
                        dTypeStr = "integer";
                    }
                    else if (dbtype == DATABASETYPES.MYSQL)
                    {
                        dTypeStr = "integer";
                    }
                    else if (dbtype == DATABASETYPES.MSACCESS)
                    {   
                        dTypeStr = "number";
                    }
                    break;
                case ABSTRACTDATATYPES.ABool:
                    if ((dbtype == DATABASETYPES.MSSQL) || (dbtype == DATABASETYPES.MSSQL2005))
                    {
                        dTypeStr = "bit";
                    }
                    else if (dbtype == DATABASETYPES.MYSQL)
                    {
                        dTypeStr = "boolean";
                    }
                    else if (dbtype == DATABASETYPES.MSACCESS)
                    {
                        dTypeStr = "boolean";
                    }
                    break;
                case ABSTRACTDATATYPES.AData:
                    if ((dbtype == DATABASETYPES.MSSQL) || (dbtype == DATABASETYPES.MSSQL2005))
                    {
                        dTypeStr = "image";
                    }
                    else if (dbtype == DATABASETYPES.MYSQL)
                    {
                        dTypeStr = "blob";
                    }
                    else if (dbtype == DATABASETYPES.MSACCESS)
                    {
                        dTypeStr = "image";
                    }
                    break;
            }
            if ((fieldstruct.maxsize != -1) && (fieldstruct.type != ABSTRACTDATATYPES.ASmallInteger) && (fieldstruct.type != ABSTRACTDATATYPES.ABool) && (fieldstruct.type != ABSTRACTDATATYPES.AData))
            {
                dTypeStr += "(" + fieldstruct.maxsize + ")";
            }
            if(fieldstruct.modifiers != null) {
                foreach (System.Data.OrionDB.ABSTRACTFIELDMODIFIERS fieldMod in fieldstruct.modifiers)
                {
                    switch (fieldMod)
                    {
                        case ABSTRACTFIELDMODIFIERS.AutoIncrement:
                            if (dbtype == DATABASETYPES.MSACCESS)
                            {
                                // Access does things a bit different, so set to autonumber
                                dTypeStr = "COUNTER";
                            }
                            else if (dbtype == (DATABASETYPES.MSSQL | DATABASETYPES.MSSQL2005))
                            {
                                dTypeStr += " IDENTITY(1,1)";
                            }
                            else if (dbtype == DATABASETYPES.MYSQL)
                            {
                                dTypeStr += " auto_increment";
                            }
                            break;
                        case ABSTRACTFIELDMODIFIERS.PrimaryKey:
                            if (dbtype == DATABASETYPES.MSACCESS)
                            {
                                dTypeStr += " CONSTRAINT PrimaryKey PRIMARY KEY";
                            }
                            else if (dbtype == (DATABASETYPES.MSSQL | DATABASETYPES.MSSQL2005))
                            {
                                dTypeStr += " PRIMARY KEY";
                            }
                            else if (dbtype == DATABASETYPES.MYSQL)
                            {
                                // This is done differently in mysql. it is set at the end of the statement
                            }
                            break;
                        case ABSTRACTFIELDMODIFIERS.NotNull:
                            if (dbtype == DATABASETYPES.MSACCESS)
                            {
                                dTypeStr += " NOT NULL";
                            }
                            else if (dbtype == (DATABASETYPES.MSSQL | DATABASETYPES.MSSQL2005))
                            {
                                dTypeStr += " NOT NULL";
                            }
                            else if (dbtype == DATABASETYPES.MYSQL)
                            {
                                // This is done differently in mysql. it is set at the end of the statement
                                dTypeStr += " NOT NULL";
                            }
                            break;
                        case ABSTRACTFIELDMODIFIERS.Clustered:
                            if (dbtype == (DATABASETYPES.MSSQL | DATABASETYPES.MSSQL2005))
                            {
                                dTypeStr += " CLUSTERED";
                            }
                            break;
                    }
                }
            }
            return dTypeStr;
        }
    }
     * 
     * **/
}

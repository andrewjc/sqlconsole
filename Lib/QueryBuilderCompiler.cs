using System;
using System.Collections.Generic;
using System.Text;
using SQLConsole.Data;

namespace SQLConsole.Data.ProviderConverters
{
    public class QueryBuilderCompiler
    {
        public static QueryBuilder CreateQueryBuilder(string csql, Database dbObj)
        {
            // This function will create a querybuilder object from commonsql
            QueryBuilder newQuery = new QueryBuilder(dbObj);
            
            // normalize commonsql:
          
            int offset = 0;
            int iQuote = 0;
            if (csql.IndexOf(";") <= 0) csql += ";";
            for (int i = 0; i < (csql.Length-offset); i++)
            {
                if (csql[i] == '\'' && iQuote == 0) 
                    iQuote = 1;
                else
                    if (csql[i] == '\'' && iQuote == 1)
                    {
                        if(csql[i-1] != '\'' && csql[i-1] != '\\')
                        iQuote = 0;
                    }
                    else if (iQuote == 0)
                    {
                        if ((csql[i + offset].Equals('(') || csql[i + offset].Equals(')')) && (!csql[(i + offset) - 1].Equals(' ')))
                        { // space to the left
                            csql = csql.Insert((i + (offset++)), " ");
                        }
                        if ((csql[i + offset].Equals('(') || csql[i + offset].Equals(')')) && (!csql[(i + offset) + 1].Equals(' ')))
                        { // space to the left
                            csql = csql.Insert((i + (offset++)) + 1, " ");
                        }
                    }
            }
            

            // endnormalize

            string[] sqlStatements = csql.Split(';');
            foreach (string sqlStr in sqlStatements)
            {
                if (!sqlStr.Trim().Equals(""))
                {
                    //if (sqlStr.Substring(sqlStr.Length - 1).Equals(";"))
                    //    sqlStr = sqlStr.Substring(0, sqlStr.Length - 1);
                    string[] tokens = sqlStr.Split(' ');
                    switch (tokens[0].ToLower())
                    {
                        case "drop":
                            if (tokens[1].ToLower().Equals("table"))
                                newQuery.setType(ABSTRACTQUERYTYPES.DropQuery);

                            if (tokens[1].ToLower().Equals("database"))
                                newQuery.setType(ABSTRACTQUERYTYPES.DropQuery);
                            newQuery.addSource(tokens[2].ToLower());
                            newQuery.addField(new AField("droptype", tokens[1].ToLower()));
                            break;
                        case "insert":
                            insertQueryHandler(ref newQuery, sqlStr);
                            break;
                        case "delete":
                            deleteQueryHandler(ref newQuery, sqlStr);
                            break;
                        case "show":
                            break;
                        case "select":
                            selectQueryHandler(ref newQuery, sqlStr);
                            break;
                        case "update":
                            updateQueryHandler(ref newQuery, sqlStr);
                            break;
                        case "alter":
                            alterQueryHandler(ref newQuery, sqlStr);
                            break;
                    }
                }
            }
            return newQuery;
        }

        static void deleteQueryHandler(ref QueryBuilder newQuery, string sqlStr)
        {
            // delete from table where silegjilsejgil=seiljsiegljseg
            // delete from table;
            string[] tokenList = new string[] { "delete", "where" };
            System.Collections.ArrayList tokenised = new System.Collections.ArrayList();
            newQuery.setType(ABSTRACTQUERYTYPES.DeleteQuery);

           
            // build the token list
            tokenised = SQLConsole.Data.utils.tokenizeList(sqlStr, tokenList);
            foreach (tokenItem tokenitem in tokenised)
            {
                string tokenString = tokenitem.tokenValue;
                string comString = tokenitem.tokenName;
                string paramString = tokenString.Substring(tokenString.IndexOf(comString) + comString.Length + 1).Trim();
                switch (comString)
                {
                    case "delete":
                        paramString = paramString.Substring("from ".Length);
                        string tableAlias = ""; string tablename = "";
                        if (paramString.IndexOf(' ') != -1)
                        {
                            tableAlias = paramString.Substring(paramString.IndexOf(' ') + 1);
                            tablename = paramString.Substring(0, paramString.IndexOf(' '));
                        }
                        else
                            tablename = paramString.Trim();
                        newQuery.addSource(new ATable(tablename, tableAlias));
                        break;
                    case "where":
                        newQuery.conditionalString = paramString;
                        break;
                }
            }
        }


     


        static void selectQueryHandler(ref QueryBuilder newQuery, string sqlStr) {
            string[] tokenList = new string[] { "select", "from", "inner join",  "where", "group by", "order by", "limit" };
            string tmp = "";
            for(int i=0;i<=tokenList.Length-1;i++) {
                tmp = tokenList[i];
                sqlStr = sqlStr.Replace(tmp, "[" + tmp + "]");
                tokenList[i]="["+tmp+"]";
            }
            System.Collections.ArrayList tokenised = new System.Collections.ArrayList();
            newQuery.setType(ABSTRACTQUERYTYPES.SelectQuery);
            newQuery.QueryLimit.lStart = -1;
            newQuery.QueryLimit.lLimit = -1;
            


            // build the token list
            tokenised = SQLConsole.Data.utils.tokenizeList(sqlStr, tokenList);








            foreach (tokenItem tokenitem in tokenised)
            {
                string tokenString = tokenitem.tokenValue;
                string comString = tokenitem.tokenName;

               

                string paramString = tokenString.Substring(tokenString.IndexOf(tokenitem.tokenName) + tokenitem.tokenName.Length + 1).Trim();

                if(paramString.Length > 10)
                if (paramString.Trim().ToLower().Substring(0, "distinct".Length) == "distinct")
                {
                    newQuery.distinct = true;
                    paramString = paramString.Trim().Substring("distinct ".Length);
                }
               
                //string comString = tokenString.Substring(0, tokenString.IndexOf(" ")).Trim();
                //string paramString = tokenString.Substring(tokenString.IndexOf(" ") + 1).Trim();
                string tableAlias = ""; string tablename = "";
                string[] fieldList;
                switch (comString)
                {
                    case "select":
                        string[] fieldnames = paramString.Split(',');
                        string realname = "";
                        string agg;
                        ABSTRACTAGGREGATE aggMode = ABSTRACTAGGREGATE.None;
                        foreach (string field in fieldnames)
                        {
                            realname = field.Trim();
                            if (field.IndexOf(")") > 0)
                            {
                                realname = field.Substring(field.IndexOf("(") + 1, field.IndexOf(")") - field.IndexOf("(") - 1);
                                realname = realname.Trim();
                                agg = field.Substring(0, field.IndexOf("(") - 1);
                                switch (agg)
                                {
                                    case "count":
                                        aggMode = ABSTRACTAGGREGATE.Count;
                                        break;
                                    case "max":
                                        aggMode = ABSTRACTAGGREGATE.Max;
                                        break;
                                    case "min":
                                        aggMode = ABSTRACTAGGREGATE.Min;
                                        break;
                                }
                            }
                            if (realname.IndexOf(".") > 0)
                                newQuery.addField(new AField(realname.Substring(realname.IndexOf(".") + 1), null, realname.Substring(0, realname.IndexOf("."))));
                            else
                            {
                                if (realname == "*")
                                {
                                    // deal with this seperately.
                                    newQuery.addField(new AField(realname, null));
                                }
                                else
                                {
                                    // Work out what source this field belongs to...
                                    string pureName = realname.Substring(realname.IndexOf(".") + 1);

                                    string[] sources = {};
                                    for (int i = 0; i < tokenised.Count; i++)
                                    {
                                        if (((tokenItem)tokenised[i]).tokenName == "from")
                                        {
                                            string sourcesList = ((tokenItem)tokenised[i]).tokenValue;
                                            sourcesList = sourcesList.Substring(sourcesList.IndexOf(((tokenItem)tokenised[i]).tokenName) + ((tokenItem)tokenised[i]).tokenName.Length + 1);
                                            sourcesList.Trim();
                                            string[] sources2 = sourcesList.Split(',');
                                            sources = (string[])utils.ConcatenateArrays(sources, sources2);
                                        }
                                        else if (((tokenItem)tokenised[i]).tokenName == "inner join") {
                                            string sourcesList = ((tokenItem)tokenised[i]).tokenValue;
                                            sourcesList = sourcesList.Substring(sourcesList.IndexOf(((tokenItem)tokenised[i]).tokenName) + ((tokenItem)tokenised[i]).tokenName.Length + 1);
                                            sourcesList = sourcesList.Substring(0, sourcesList.IndexOf(" on "));
                                            sourcesList.Trim();
                                            string[] sources2 = { sourcesList };
                                            sources = (string[])utils.ConcatenateArrays(sources, sources2);
                                        }
                                        else if (((tokenItem)tokenised[i]).tokenName == "outer join") {
                                            string sourcesList = ((tokenItem)tokenised[i]).tokenValue;
                                            sourcesList = sourcesList.Substring(sourcesList.IndexOf(((tokenItem)tokenised[i]).tokenName) + ((tokenItem)tokenised[i]).tokenName.Length + 1);
                                            sourcesList = sourcesList.Substring(0, sourcesList.IndexOf(" on "));
                                            sourcesList.Trim();
                                            string[] sources2 = { sourcesList };
                                            sources = (string[])utils.ConcatenateArrays(sources, sources2);
                                        }
                                        else if (((tokenItem)tokenised[i]).tokenName == "join") {
                                            string sourcesList = ((tokenItem)tokenised[i]).tokenValue;
                                            sourcesList = sourcesList.Substring(sourcesList.IndexOf(((tokenItem)tokenised[i]).tokenName) + ((tokenItem)tokenised[i]).tokenName.Length + 1);
                                            
                                            if(sourcesList.IndexOf(" on ") > 0)
                                                sourcesList = sourcesList.Substring(0, sourcesList.IndexOf(" on "));
                                            
                                            sourcesList.Trim();
                                            string[] sources2 = { sourcesList };
                                            sources = (string[])utils.ConcatenateArrays(sources, sources2);
                                        }
                                    }
                                    AField newField = null;
                                    foreach (string source in sources)
                                    {
                                        ATable src = null;

                                        if (source.IndexOf(" ") > 0)
                                            src = newQuery.getDatabaseObject().getTableObject(source.Substring(0, source.IndexOf(" ")));
                                        else
                                            src = newQuery.getDatabaseObject().getTableObject(source);
                                        if (src == null)
                                            throw (new Exception("Bad table name exception"));
                                        if (src.getFieldByName(pureName) != null)
                                        {
                                            if (newField != null) throw (new Exception("Ambiguous field name: " + realname + ". Field could belong to more than one source table."));

                                            newField = src.getFieldByName(pureName);
                                            if (source.IndexOf(" ") > 0)
                                            {
                                                newField.owner = source.Substring(source.IndexOf(" ")+1);
                                            }
                                        }


                                    }

                                    if (newField == null)
                                        throw (new Exception("Unable to determine source for field: " + realname));
                                    newQuery.addField(newField);

                                    //newQuery.addField(new AField(realname, null, (ATable)((QueryBuilder.SOURCEBINDING)newQuery.getSourceList()[0]).sourceObject));
                                    //newQuery.addField(new AField(realname,null,""));
                                }

                            }
                            if (realname.IndexOf(".") > 0)
                                newQuery.getField(realname.Substring(realname.IndexOf(".") + 1), realname.Substring(0, realname.IndexOf("."))).function = aggMode;
                            else
                                newQuery.getField(realname).function = aggMode;
                        }

                        break;
                    case "distinct":
                        newQuery.distinct = true;
                        break;
                    case "from":
                        
                        if (paramString.IndexOf(' ') != -1) {
                            tableAlias = paramString.Substring(paramString.IndexOf(' ') + 1);
                            tablename = paramString.Substring(0, paramString.IndexOf(' '));
                        }
                        else
                            tablename = paramString.Trim();
                        newQuery.addSource(new ATable(tablename, tableAlias));
                        break;
                    case "inner join":
                        System.Collections.ArrayList joinList = new System.Collections.ArrayList();
                        paramString = "inner join " + paramString;
                        if (paramString.ToLower().IndexOf("inner join", "inner join".Length + 1) >= 2)
                        {
                        while (paramString.Length >= 0)
                        {
                            string tmp2 = paramString;
                            if (paramString.ToLower().IndexOf("inner join", "inner join".Length + 1) >= 2)
                                tmp2 = paramString.ToLower().Substring(0, paramString.ToLower().IndexOf("inner join", "inner join".Length + 2)).Trim();
                            else
                            {
                                joinList.Add(tmp2);
                                break;
                            }
                            joinList.Add(tmp2);
                            paramString = paramString.ToLower().Substring(paramString.ToLower().IndexOf("inner join", "inner join".Length + 2)).Trim();

                        }
                        }
                        else
                        {
                            joinList.Add(paramString);
                        }


                        foreach (string tmpstr in joinList)
                        {
                            paramString = tmpstr.Substring("inner join".Length+1);
                            // inner join anothertable on sjlieglij=sliejgil
                            string lTmp = paramString.Substring(0, paramString.IndexOf(" on "));
                            if (lTmp.IndexOf(' ') != -1)
                            {
                                tableAlias = lTmp.Substring(lTmp.IndexOf(' ')).Trim();
                                tablename = lTmp.Substring(0, lTmp.IndexOf(' ')).Trim();
                            }
                            else
                                tablename = lTmp.Trim();

                            lTmp = paramString.Substring(paramString.IndexOf(" on") + " on".Length + 1);
                            string srcTable = ""; string dstTable = "";
                            string srcField = ""; string dstField = "";


                            srcTable = lTmp.Substring(0, lTmp.IndexOf('='));
                            dstTable = lTmp.Substring(lTmp.IndexOf('=') + 1);
                            if (srcTable.IndexOf('.') > 0)
                            {
                                // Assume specifed table.
                                srcField = srcTable.Substring(srcTable.IndexOf(".") + 1);
                                srcTable = srcTable.Substring(0, srcTable.IndexOf("."));
                            }
                            if (dstTable.IndexOf('.') > 0)
                            {
                                // Assume specifed table.
                                dstField = dstTable.Substring(dstTable.IndexOf(".") + 1);
                                dstTable = dstTable.Substring(0, dstTable.IndexOf("."));
                            }

                            newQuery.addSource(new ATable(tablename, tableAlias), ABSTRACTSOURCEBINDTYPES.INNERJOIN, ((QueryBuilder.SOURCEBINDING)newQuery.getSourceList()[newQuery.getSourceList().Count - 1]).sourceObject, srcField, dstField);
                        }
                        //newQuery.addSource(new ATable(tablename, tableAlias), ABSTRACTSOURCEBINDTYPES.INNERJOIN, null, lTmp.Substring(0, lTmp.IndexOf('=')), lTmp.Substring(lTmp.IndexOf('=')+1));
                        break;

                    case "join":
                        joinList = new System.Collections.ArrayList();
                        paramString = "join " + paramString;
                        if (paramString.ToLower().IndexOf("join", "join".Length + 1) >= 2)
                        {
                            while (paramString.Length >= 0)
                            {
                                string tmp2 = paramString;
                                if (paramString.ToLower().IndexOf("join", "join".Length + 1) >= 2)
                                    tmp2 = paramString.ToLower().Substring(0, paramString.ToLower().IndexOf("join", "join".Length + 2)).Trim();
                                else
                                {
                                    joinList.Add(tmp2);
                                    break;
                                }
                                joinList.Add(tmp2);
                                paramString = paramString.ToLower().Substring(paramString.ToLower().IndexOf("join", "join".Length + 2)).Trim();

                            }
                        }
                        else
                        {
                            joinList.Add(paramString);
                        }


                        foreach (string tmpstr in joinList)
                        {
                            paramString = tmpstr.Substring("join".Length + 1);
                            // inner join anothertable on sjlieglij=sliejgil
                            string lTmp = paramString.Substring(0, paramString.IndexOf(" on "));
                            if (lTmp.IndexOf(' ') != -1)
                            {
                                tableAlias = lTmp.Substring(lTmp.IndexOf(' ')).Trim();
                                tablename = lTmp.Substring(0, lTmp.IndexOf(' ')).Trim();
                            }
                            else
                                tablename = lTmp.Trim();

                            lTmp = paramString.Substring(paramString.IndexOf(" on") + " on".Length + 1);
                            string srcTable = ""; string dstTable = "";
                            string srcField = ""; string dstField = "";


                            srcTable = lTmp.Substring(0, lTmp.IndexOf('='));
                            dstTable = lTmp.Substring(lTmp.IndexOf('=') + 1);
                            if (srcTable.IndexOf('.') > 0)
                            {
                                // Assume specifed table.
                                srcField = srcTable.Substring(srcTable.IndexOf(".") + 1);
                                srcTable = srcTable.Substring(0, srcTable.IndexOf("."));
                            }
                            if (dstTable.IndexOf('.') > 0)
                            {
                                // Assume specifed table.
                                dstField = dstTable.Substring(dstTable.IndexOf(".") + 1);
                                dstTable = dstTable.Substring(0, dstTable.IndexOf("."));
                            }

                            newQuery.addSource(new ATable(tablename, tableAlias), ABSTRACTSOURCEBINDTYPES.INNERJOIN, ((QueryBuilder.SOURCEBINDING)newQuery.getSourceList()[newQuery.getSourceList().Count - 1]).sourceObject, srcField, dstField);
                        }
                        //newQuery.addSource(new ATable(tablename, tableAlias), ABSTRACTSOURCEBINDTYPES.INNERJOIN, null, lTmp.Substring(0, lTmp.IndexOf('=')), lTmp.Substring(lTmp.IndexOf('=')+1));
                        break;

                    
                    
                    case "where":
                        // where b.id = sjilegj, b.Name = fjff
                        // where b.id = silegjlsg and 
                        newQuery.conditionalString = paramString;
                        break;
                    case "group by":
                        fieldList = paramString.Split(',');
                        foreach(string ff in fieldList) {
                            newQuery.getField(ff).GroupBy = true;
                        }
                        break;
                    case "order by":
                        fieldList = paramString.Split(',');

                       
                        foreach (string ff in fieldList)
                        {
                            /* patch...  
                            bugfix: orderby where the field is not in the select field list
                            * ie: select * from [table] order by field
                            */
                            string fname = null;
                            string fOrderType = null;
                            if (ff.IndexOf(' ') > 0)
                            {
                                ff.Trim();
                                fname = ff.Substring(0, ff.IndexOf(' '));
                                fOrderType = ff.Substring(ff.IndexOf(' ') + 1);
                            }
                            else
                                fname = ff;

                            if (newQuery.getField(fname) == null)
                            {
                                if (fname.IndexOf(".") > 0)
                                {
                                    string tblName = ((ATable)newQuery.getSourceTableByAlias(fname.Substring(0, fname.IndexOf(".")))).name;
                                    AField fld = newQuery.getDatabaseObject().getTableObject(tblName).getFieldByName((fname.Substring(fname.IndexOf(".") + 1)));
                                    fld.owner = fname.Substring(0, fname.IndexOf("."));
                                    newQuery.addField(fld);
                                }
                                else
                                newQuery.addField(newQuery.getDatabaseObject().GetTableCache().getCachedTable(((ATable)((QueryBuilder.SOURCEBINDING)newQuery.getSourceList()[0]).sourceObject).name).getFieldByName(fname));
                            }
                            if(fOrderType != null) {
                                if (fOrderType.ToLower() == "asc")
                                    newQuery.getField(fname).OrderBy = ABSTRACTORDERTYPE.Ascending;
                                else
                                    newQuery.getField(fname).OrderBy = ABSTRACTORDERTYPE.Descending;
                            }
                            else { //default to order by asc
                                if (fname.IndexOf(".")>0)
                                {
                                    fname = fname.Substring(fname.IndexOf(".")+1);
                                }
                                if (fname == newQuery.getField(fname).name)
                                {
                                    newQuery.getField(fname).OrderBy = ABSTRACTORDERTYPE.Ascending;
                                }
                            }
                        }
                        break;
                    case "limit":
                        newQuery.QueryLimit.lStart = 0;
                        if (paramString.IndexOf(',') > 0)
                        {
                            // format limit 0,5
                            // format limit offset, count
                            newQuery.QueryLimit.lStart = Convert.ToInt32(paramString.Substring(0, paramString.IndexOf(',')));
                            newQuery.QueryLimit.lLimit = Convert.ToInt32(paramString.Substring(paramString.IndexOf(',') + 1));
                        }
                        else
                            newQuery.QueryLimit.lLimit = Convert.ToInt32(paramString);
                        break;
                }
            }
        }

        

        static void insertQueryHandler(ref QueryBuilder newQuery, string sqlStr)
        {
            try
            {
                newQuery.setType(ABSTRACTQUERYTYPES.InsertQuery);
                string[] tokens = sqlStr.Split(' ');
                System.Collections.Queue fieldQueue = new System.Collections.Queue(15);
                for (int i = 0; i < tokens.Length; i++)
                {
                    string tokenPeek = tokens[i];
                    if (tokens[i].Equals("into")) //Next token will be the tablename
                        newQuery.addSource(tokens[++i]);
                    if ((tokens[i].Equals("(")) && (!tokens[i - 2].Equals("values")))
                    { //fieldlist
                        // process fieldlist
                        string fieldname;
                        i++;    //just move forward to the tagset.
                        while (!tokens[i].Equals(")"))
                        {
                            fieldname = tokens[i++];
                            //if (fieldname.Trim().Substring(fieldname.Length - 1).Equals(","))
                            //    fieldname = fieldname.Trim().Substring(0, fieldname.Trim().Length - 1);
                            foreach(string field in fieldname.Split(','))
                                if(field.Trim().Length > 0)
                            fieldQueue.Enqueue(field);
                        }

                        string test = tokens[i + 1];

                    }
                    else if ((tokens[i + 1].Equals("(")) && (tokens[i].ToLower().Equals("values")))
                    { //valuelist
                        // process valuelist
                        i++; i++;
                        string restOfString = "";
                        while (i < tokens.Length)
                            restOfString += tokens[i++] + " ";

                        int strquoteon = 0;
                        string fieldVal = "";
                        bool quotedType = false;
                        for (int x = 0; x < restOfString.Length; x++)
                        {
                            if ((restOfString[x].Equals('\'')) && (strquoteon == 0))
                            {
                                strquoteon = 1;
                                quotedType = true;
                                //x++;    //skip the quote
                                fieldVal = "";

                                //fieldVal += restOfString[x];
                            }
                            else if ((strquoteon == 0) && ((restOfString[x].Equals(',')) || (restOfString[x].Equals(')'))))
                            {
                                string fieldname = (string)fieldQueue.Dequeue();
                                // Make sure we're not quoting.

                                newQuery.addField(new AField(fieldname, fieldVal.Trim()));
                                fieldVal = "";
                                quotedType = false;
                            }
                            else if (x > 0)
                            {
                                if (restOfString[x].Equals('\'') && restOfString[x + 1].Equals('\''))
                                {
                                    // escaped quote
                                    fieldVal += "''";
                                    x = x + 1;
                                }
                                // case for if it appears like an 
                                // escaped  quote, but it is not
                                // eg: '\\' as the value

                                else if (restOfString[x].Equals('\\') && restOfString[x + 1].Equals('\'') && !restOfString[x - 1].Equals('\''))
                                {
                                    // escaped quote
                                    fieldVal += "\\'";
                                    x = x + 1;
                                }
                                else
                                {
                                    if (restOfString[x].Equals('\''))
                                    {
                                        strquoteon = 0;
                                        //fieldVal += restOfString[x];
                                    }
                                    else
                                    {
                                        if (!((strquoteon == 0) && (quotedType == true)))
                                            fieldVal += restOfString[x];
                                    }
                                }

                                
                            }
                            else
                            {

                                fieldVal += restOfString[x];
                            }

                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
            	System.Diagnostics.Debug.Print(e.Message);
                newQuery = null;
            }
        }


        static void updateQueryHandler(ref QueryBuilder newQuery, string sqlStr)
        {

            string[] tokenList = new string[] { "update", "set", "where", "nullterminator" };
            System.Collections.ArrayList tokenised = new System.Collections.ArrayList();
            
            newQuery.setType(ABSTRACTQUERYTYPES.UpdateQuery);

         
            // build the token list
            tokenised = SQLConsole.Data.utils.tokenizeList(sqlStr, tokenList);
            foreach (tokenItem tokenitem in tokenised)
            {
                string tokenString = tokenitem.tokenValue;

                string comString = tokenitem.tokenName;
                string paramString = tokenString.Substring(tokenString.IndexOf(comString) + comString.Length + 1).Trim();

                //string comString = tokenString.Substring(0, tokenString.IndexOf(" ")).Trim();
                //string paramString = tokenString.Substring(tokenString.IndexOf(" ") + 1).Trim();
                string tablename = "";
                switch (comString)
                {
                    case "update":
                        tablename = paramString;
                        newQuery.addSource(tablename);
                        break;
                    case "set":
                        // set something=something, ilsjeiljg='sejil=gji\'seg', sjegiiljseg=slegijl

                        string curKey = ""; string curVal = "";
                        bool strQuoting = false;
                        bool doKey = false;
                        bool doAdd = false;
                        for (int i = 0; i <= paramString.Length; i++)
                        {
                            if (!doKey)
                            {
                                if (i < paramString.Length)
                                {
                                    if (paramString[i].Equals('='))
                                        doKey = true;

                                    else if (!paramString[i].Equals(',') && !paramString[i].Equals(' '))
                                        curKey += paramString[i];
                                }
                            }
                            else
                            {
                                if (i < paramString.Length)
                                {
                                    if ((paramString[i].Equals('\'')) && (!paramString[i - 1].Equals('\\')))
                                    {
                                        if (strQuoting)
                                        {
                                            strQuoting = false;
                                            doAdd = true;
                                        }
                                        else
                                        {
                                            strQuoting = true; i++;
                                        }
                                    }
                                    else if ((paramString[i].Equals(',')) && (strQuoting == false))
                                    {
                                        doAdd = true;
                                        i++;
                                    }
                                }
                                else doAdd = true;
                                if (doAdd)
                                {
                                    string fieldname = curKey;
                                    string fieldValue = curVal;
                                    strQuoting = false;
                                    doAdd = false;
                                    doKey = false;
                                    newQuery.addField(new AField(fieldname.Trim(), fieldValue.Trim()));
                                    curKey = "";
                                    curVal = "";
                                }
                                else
                                {
                                    curVal += paramString[i];
                                }
                            
                            }


                        }

                        break;
                    case "where":
                        newQuery.conditionalString = paramString;
                        break;
                }
            }

        }

        static void alterQueryHandler(ref QueryBuilder newQuery, string sqlStr)
        {
            newQuery.setType(ABSTRACTQUERYTYPES.AlterQuery);
            // alter table testTable drop column password
            newQuery.conditionalString = sqlStr;
        }


    }
}

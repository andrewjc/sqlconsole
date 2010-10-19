using System;
using System.Collections.Generic;
using System.Text;
using dbal.AbstractObjects;

namespace dbal.ProviderConverters
{
    class ProviderQueryConverters
    {
        public static QueryBuilder CreateQueryBuilder(string csql, Database dbObj)
        {
            // This function will create a querybuilder object from commonsql
            QueryBuilder newQuery = new QueryBuilder(dbObj);
            
            // normalize commonsql:
            int offset = 0;
            if (csql.IndexOf(";") <= 0) csql += ";";
            for (int i = 0; i < (csql.Length-offset); i++)
            {
                if ((csql[i + offset].Equals('(') || csql[i + offset].Equals(')')) && (!csql[(i + offset) - 1].Equals(' ')))
                { // space to the left
                    csql = csql.Insert((i+(offset++)), " ");
                }
                if ((csql[i + offset].Equals('(') || csql[i + offset].Equals(')')) && (!csql[(i + offset) + 1].Equals(' ')))
                { // space to the left
                    csql = csql.Insert((i + (offset++)) + 1, " ");
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
            string[] tokenList = new string[] { "delete from", "where", "nullterminator" };
            string tmp = "";
            System.Collections.ArrayList tokenised = new System.Collections.ArrayList();
            System.Collections.Hashtable tokenized = new System.Collections.Hashtable();
            newQuery.setType(ABSTRACTQUERYTYPES.DeleteQuery);

            for (int i = 0; i < tokenList.Length; i++)
            {
                if (sqlStr.IndexOf(tokenList[i]) >= 0)
                {
                    // locate next match
                    int nextmatch = -1;
                    for (int x = i + 1; x < tokenList.Length; x++)
                    {
                        if (sqlStr.IndexOf(tokenList[x]) >= 0)
                        {
                            nextmatch = x;
                            break;
                        }
                    }
                    if (nextmatch == -1)
                    {
                        tmp = sqlStr.Substring(sqlStr.IndexOf(tokenList[i]));
                    }
                    else
                    {
                        if (sqlStr.IndexOf(tokenList[nextmatch]) >= 0)
                            tmp = sqlStr.Substring(sqlStr.IndexOf(tokenList[i]), (sqlStr.IndexOf(tokenList[nextmatch]) - sqlStr.IndexOf(tokenList[i])));
                    }


                    //tokenised.Add(tmp);
                    tokenized.Add(tokenList[i], tmp.Trim());
                }
            }
            foreach (string tokenkey in tokenized.Keys)
            {
                string tokenString = (string)tokenized[tokenkey];
                string comString = tokenkey;
                string paramString = tokenString.Substring(tokenString.IndexOf(tokenkey) + tokenkey.Length + 1).Trim();
                switch (comString)
                {
                    case "delete from":
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
            string[] tokenList = new string[] { "select", "from", "inner join", "where", "group by", "order by", "limit", "nullterminator" };
            string tmp = "";
            System.Collections.ArrayList tokenised = new System.Collections.ArrayList();
            System.Collections.Hashtable tokenized = new System.Collections.Hashtable();
            newQuery.setType(ABSTRACTQUERYTYPES.SelectQuery);
            newQuery.QueryLimit.lStart = -1;
            newQuery.QueryLimit.lLimit = -1;
            for(int i=0;i<tokenList.Length;i++) {
                if(sqlStr.ToLower().IndexOf(tokenList[i]) >= 0) {
                    // locate next match
                    int nextmatch = -1;
                    for (int x = i+1; x < tokenList.Length; x++)
                    {
                        if (sqlStr.ToLower().IndexOf(tokenList[x]) >= 0)
                        {
                            nextmatch = x;
                            break;
                        }
                    }
                    if (nextmatch == -1)
                    {
                        tmp = sqlStr.Substring(sqlStr.ToLower().IndexOf(tokenList[i]));
                    }
                    else
                    {
                        if (sqlStr.ToLower().IndexOf(tokenList[nextmatch]) >= 0)
                            tmp = sqlStr.Substring(sqlStr.ToLower().IndexOf(tokenList[i]), (sqlStr.ToLower().IndexOf(tokenList[nextmatch])-sqlStr.ToLower().IndexOf(tokenList[i])));                    
                    }

                    // perform a quick check on the last token to see if it has another
                    // embedded instance of the current token... if so perform chopping
                    // and add each instance into the tokenized list.

                    /*
                    if (tmp.ToLower().IndexOf(tokenList[i], tokenList[i].Length + 1) >= 2)
                    {
                        while (tmp.Length >= 0)
                        {
                            string tmp2 = null;
                            if (tmp.ToLower().IndexOf(tokenList[i], tokenList[i].Length + 1) >= 2)
                                tmp2 = tmp.ToLower().Substring(0, tmp.IndexOf(tokenList[i], tokenList[i].Length + 2)).Trim();
                            else
                            {
                                tokenized.Add(tokenList[i], tmp);
                                break;
                            }
                            tokenized.Add(tokenList[i], tmp2);
                            tmp = tmp.ToLower().Substring(tmp.IndexOf(tokenList[i], tokenList[i].Length + 2)).Trim();

                        }
                    }
                    else
                    {
                     * */
                        tokenized.Add(tokenList[i], tmp.Trim());
                   // }
         
                    //tokenised.Add(tmp);
                  //  tokenized.Add(tokenList[i], tmp.Trim());

                    
                    
                }
            }

            foreach (string tokenkey in tokenized.Keys)
            {
                string tokenString = (string)tokenized[tokenkey];

                string comString = tokenkey;
                string paramString = tokenString.Substring(tokenString.IndexOf(tokenkey)+tokenkey.Length+1).Trim();

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
                            realname = field;
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
                                newQuery.addField(new AField(realname, null, (ATable)((QueryBuilder.SOURCEBINDING)newQuery.getSourceList()[0]).sourceObject));

                            if (realname.IndexOf(".") > 0)
                                newQuery.getField(realname.Substring(realname.IndexOf(".") + 1), realname.Substring(0, realname.IndexOf("."))).function = aggMode;
                            else
                                newQuery.getField(realname).function = aggMode;
                        }

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
                            if (newQuery.getField(ff) == null)
                            {
                                newQuery.addField(newQuery.getDatabaseObject().GetTableCache().getCachedTable(((ATable)((QueryBuilder.SOURCEBINDING)newQuery.getSourceList()[0]).sourceObject).name).getFieldByName(ff));
                            }
                            if(ff.IndexOf(' ') > 0) {
                                ff.Trim();
                                string fname = ff.Substring(0, ff.IndexOf(' '));
                                string fOrderType = ff.Substring(ff.IndexOf(' ') + 1);
                                if (fOrderType.ToLower() == "asc")
                                    newQuery.getField(fname).OrderBy = ABSTRACTORDERTYPE.Ascending;
                                else
                                    newQuery.getField(fname).OrderBy = ABSTRACTORDERTYPE.Descending;
                            }
                            else { //default to order by asc
                                if (ff == newQuery.getField(ff).name)
                                {
                                    newQuery.getField(ff).OrderBy = ABSTRACTORDERTYPE.Ascending;
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
                            if (fieldname.Trim().Substring(fieldname.Length - 1).Equals(","))
                                fieldname = fieldname.Trim().Substring(0, fieldname.Trim().Length - 1);
                            foreach(string field in fieldname.Split(','))
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
                            if ((restOfString[x].Equals('\'')) & (strquoteon == 0))
                            {
                                strquoteon = 1;
                                quotedType = true;
                                //x++;    //skip the quote
                                fieldVal = "";

                                //fieldVal += restOfString[x];
                            }
                            else if ((strquoteon == 0) & ((restOfString[x].Equals(',')) | (restOfString[x].Equals(')'))))
                            {
                                string fieldname = (string)fieldQueue.Dequeue();
                                // Make sure we're not quoting.

                                newQuery.addField(new AField(fieldname, fieldVal.Trim()));
                                fieldVal = "";
                                quotedType = false;
                            }
                            else if (x > 0)
                            {
                                if ((restOfString[x].Equals('\'')) & !((restOfString[x - 1].Equals('\\'))))
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
                newQuery = null;
            }
        }


        static void updateQueryHandler(ref QueryBuilder newQuery, string sqlStr)
        {

            string[] tokenList = new string[] { "update", "set", "where", "nullterminator" };
            string tmp = "";
            System.Collections.ArrayList tokenised = new System.Collections.ArrayList();
            System.Collections.Hashtable tokenized = new System.Collections.Hashtable();
            newQuery.setType(ABSTRACTQUERYTYPES.UpdateQuery);

            for (int i = 0; i < tokenList.Length; i++)
            {
                if (sqlStr.IndexOf(tokenList[i]) >= 0)
                {
                    if (sqlStr.IndexOf(tokenList[i + 1]) >= 0)
                        tmp = sqlStr.Substring(sqlStr.IndexOf(tokenList[i]), (sqlStr.IndexOf(tokenList[i + 1]) - sqlStr.IndexOf(tokenList[i])));
                    else
                        tmp = sqlStr.Substring(sqlStr.IndexOf(tokenList[i]));

                    tokenized.Add(tokenList[i], tmp);
                }
            }

            foreach (string tokenkey in tokenized.Keys)
            {
                string tokenString = (string)tokenized[tokenkey];

                string comString = tokenkey;
                string paramString = tokenString.Substring(tokenString.IndexOf(tokenkey) + tokenkey.Length + 1).Trim();

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
                        for (int i = 0; i < paramString.Length+1; i++)
                        {
                            if (!doKey)
                            {

                                if (paramString[i].Equals('='))                             
                                    doKey = true;

                                else if (!paramString[i].Equals(',') && !paramString[i].Equals(' ')) 
                                    curKey += paramString[i];
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

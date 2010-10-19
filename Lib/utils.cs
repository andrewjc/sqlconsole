using System;
using System.Collections.Generic;
using System.Text;

namespace SQLConsole.Data
{
    public struct tokenItem
    {
        public string tokenName;
        public string tokenValue;
    }
    public class utils
    {

        public static Array ConcatenateArrays(params Array[] arrays)
        {
            if (arrays==null)
            {
                throw new ArgumentNullException("arrays");
            }
            if (arrays.Length==0)
            {
                throw new ArgumentException("No arrays specified");
            }

            Type type = arrays[0].GetType().GetElementType();
            int totalLength = arrays[0].Length;
            for (int i=1; i < arrays.Length; i++)
            {
                if (arrays[i].GetType().GetElementType() != type)
                {
                    throw new ArgumentException("Arrays must all be of the same type");
                }
                totalLength += arrays[i].Length;
            }

            Array ret = Array.CreateInstance(type, totalLength);
            int index=0;
            foreach (Array array in arrays)
            {
                Array.Copy (array, 0, ret, index, array.Length);
                index += array.Length;
            }
            return ret;
           
        }


        public static string[] split_QF(string str, string delim)
        {
            // replace all occurances of the delim with \0\1\2\3\4
            System.Collections.ArrayList al = new System.Collections.ArrayList();
            int currentPos = 0;
            int? tmp = 0;
            string str_temp = str;
            while (currentPos < str.Length)
            {
                tmp = strsearch_QF(str_temp, delim);
                if (tmp != null)
                {
                    al.Add(str_temp.Substring(0, (int)tmp));
                    str_temp = str_temp.Substring(((int)tmp) + 1);
                }
                else
                {
                    al.Add(str_temp);
                    break;
                }
            }
            string[] returnArray = (string[])al.ToArray(typeof(string));

            return returnArray;
        }
        static int? strsearch_QF(string str, string needle)
        {
            int quoteStat = 0;
            int currentPos = 0;
            int tmp = 0;
            while (currentPos < str.Length)
            {
                // quote handling
                tmp = 0;
                if (str[currentPos] == Char.Parse("'"))
                {
                    quoteStat = (quoteStat == 0) ? 1 : 0;
                }
                if (quoteStat == 0)
                {
                    // check first letter
                    if (str.ToLower()[currentPos] == needle[0])
                    {

                        // check other letters
                        for (int i = 0; i < needle.Length; i++)
                        {
                            if ((str.Length > (currentPos + i)))
                            {
                                if (str.ToLower()[currentPos + i] != needle[i]) //doesnt match
                                {
                                    currentPos++;
                                    tmp = 0;
                                    break;
                                }
                                tmp = 1;
                            }
                            else { tmp = 0; currentPos++; }
                        }
                        if (tmp == 1)
                        {
                            return currentPos;
                        }
                    }
                }
                currentPos++;
            }
            return null;
        }


        public static System.Collections.ArrayList tokenizeList(string sqlstr, string[] tokens)
        {
            /* This function will build a tokenized table taking into account quoted sections... */
            System.Collections.ArrayList tokenCollection = new System.Collections.ArrayList();
            int currentToken = 0; //start pos
            int quoteStat = 0; //start quote off.
            int currentPos = 0;
            int tmp = 0;
            while (currentPos < sqlstr.Length)
            {
                // quote handling
                if (sqlstr[currentPos] == Char.Parse("'"))
                {
                    quoteStat = (quoteStat == 0) ? 1 : 0;
                }
                if (quoteStat == 0)
                {
                    // check that the current token is in the string. if not increase currentToken
                    if (strsearch_QF(sqlstr, tokens[currentToken]+" ") == null)
                    {
                        currentToken++;
                        if (tokens.Length <= currentToken) break;
                        continue;
                    }


                    // check first letter
                    if (sqlstr.ToLower()[currentPos] == tokens[currentToken][0])
                    {
                        // check other letters
                        for (int i = 0; i < tokens[currentToken].Length; i++)
                        {
                            if (sqlstr.ToLower()[currentPos + i] != tokens[currentToken][i]) //doesnt match
                            {
                                currentPos++;
                                tmp = 0;
                                break;
                            }else
                            {
                                // only true if the very next character after the currentpos is a space
                                if(i == (tokens[currentToken].Length-1))
                                if (sqlstr[currentPos+i + 1] != ' ')
                                {
                                    currentPos++;
                                    tmp = 0;
                                    break;
                                }
                                else
                                    tmp = 1;
                            }
                        }
                        if (tmp == 1)
                        {
                            // valid token position
                            string currentTokenString = sqlstr.Substring(currentPos);

                            // locate the end of this token... denoted by the next token in the list

                            int scanPos = currentPos + tokens[currentToken].Length + 1;
                            int quoteStat2 = 0;
                            bool scanSuccess = false;
                            while ((scanPos < sqlstr.Length) && !scanSuccess)
                            {
                                if (sqlstr[scanPos] == Char.Parse("'"))
                                {
                                    quoteStat2 = (quoteStat2 == 0) ? 1 : 0;
                                }
                                if (quoteStat2 == 0)
                                {
                                    for (int i = currentToken; i < tokens.Length; i++)
                                    {
                                        if (tokens[i][0] == sqlstr.ToLower()[scanPos])
                                        {
                                            // Check other positions
                                            int foundok = 0;
                                            for (int x = 0; x < tokens[i].Length; x++)
                                            {
                                                if (sqlstr.Length <= (scanPos + x))
                                                {
                                                    foundok = 0;
                                                    break;
                                                }
                                                if (sqlstr.ToLower()[scanPos + x] != tokens[i][x]) //doesnt match
                                                {
                                                    foundok = 0;
                                                    break;
                                                }
                                                else
                                                {
                                                    //if (i == (tokens[currentToken].Length - 1))
                                                    //    if (sqlstr[currentPos + i + 1] != ' ')
                                                    //    {
                                                    //        foundok = 0;
                                                    //        break;
                                                    //    }
                                                    //    else
                                                            foundok = 1;
                                                }

                                            }
                                            if (foundok == 1)
                                            {
                                                // scanpos should now indicate the start of the next token.
                                                scanSuccess = true;
                                                scanPos--;
                                                scanPos--;
                                                break;
                                            }
                                        }
                                    }

                                }
                                scanPos++;
                            }
                            if (scanPos > 0)
                            {
                                currentTokenString = sqlstr.Substring(currentPos, scanPos - currentPos);
                            }
                            tokenItem t = new tokenItem();
                            t.tokenName = tokens[currentToken];
                            t.tokenValue = currentTokenString;
                            t.tokenValue = t.tokenValue.Substring(t.tokenName.Length);

                            if (t.tokenName[0] == '[' && t.tokenName[ t.tokenName.Length-1 ]==']')
                            {
                                t.tokenName = t.tokenName.Substring(1, t.tokenName.Length - 2);
                            }
                            t.tokenValue = t.tokenName + t.tokenValue;
                            
                            tokenCollection.Add(t);
                            currentPos += t.tokenValue.Length+1;
                            if (currentPos + tokens[currentToken].Length + 1 > sqlstr.Length) break;
                            //if (sqlstr.ToLower().IndexOf(tokens[currentToken], currentPos + tokens[currentToken].Length + 1) > 0)
                            //{

                            //}
                            //else 
                            currentToken++;
                        }
                    }
                    else
                    {
                        currentPos++;
                    }
                }
                else
                    currentPos++;
            }
            return tokenCollection;
        }

        public static string MSSQL_AddSlashes(string str)
        {
            return str.Replace("'", @"''");
            //return str.Replace("'", @"''");
        }
        public static string MYSQL_AddSlashes(string str)
        {
            return str.Replace(@"'", @"\'");
            //return str.Replace("'", @"''");
        }
        public static string Local_AddSlashes(string str,SQLConsole.Data.DATABASETYPES dbType ) {
            if ((dbType == DATABASETYPES.MSSQL2005) || (dbType == DATABASETYPES.MSSQL || dbType == DATABASETYPES.SQLITE))
            {
                return MSSQL_AddSlashes(str);
            }
            else if (dbType == DATABASETYPES.MYSQL)
            {
                return MYSQL_AddSlashes(str);
            }
            else return null;
        }


        public static string MSSQL_StripSlashes(string str)
        {
            return str.Replace("''", @"'");
            //return str.Replace("'", @"''");
        }
        public static string MYSQL_StripSlashes(string str)
        {
            return str.Replace(@"\'", @"'");
            //return str.Replace("'", @"''");
        }
        public static string Local_StripSlashes(string str, SQLConsole.Data.DATABASETYPES dbType)
        {
            if ((dbType == DATABASETYPES.MSSQL2005) || (dbType == DATABASETYPES.MSSQL))
            {
                return MSSQL_StripSlashes(str);
            }
            else if (dbType == DATABASETYPES.MYSQL)
            {
                return MYSQL_StripSlashes(str);
            }
            else return null;
        }


        public static object[] arrayIntersect(object[] arrayA, object[] arrayB)//returns ONLY the first found strings in common between array A and array B
        {
            //count the length of both arrays
            int intArrayA = arrayA.Length,
            intArrayB = arrayB.Length;
            //Find Max array size if all vallues from shortest match all the values from the biggest
            int intMaxOutput = 0;
            object[] longArray, shortArray;
            if (intArrayA >= intArrayB)
            {
                intMaxOutput = intArrayB;
                shortArray = new object[intArrayB];//declare its size
                shortArray = arrayB;//transfer all value
                longArray = new object[intArrayA];//declare its size
                longArray = arrayA;//transfer all values
            }
            else//intArrayB is bigger
            {
                intMaxOutput = intArrayA;
                shortArray = new object[intArrayA];//declare its size
                shortArray = arrayA;//transfer all value
                longArray = new object[intArrayB];//declare its size
                longArray = arrayB;//transfer all values
            }
            //array for storing the strings
            object[] strArrayOutput = new object[intMaxOutput];
            //If all possible values matches then the shortest array will determin the max number of matches. 
            int shortCount = 0;//count through short array
            int longCount = 0;//count through long array
            int intOutput = 0;//starts the counter for each value that could be found

            /*
            * !!!side not to self!!!
            * since we know which of the arrays is the smallest
            * we only need to count through the while statement as many times
            * as the shortest array length, testing it each time against the other array
            * each time a value is found it needs to be stored into the array output
            * 
            */
            //take care of any null values in the short array
            while (shortCount >= 0 && shortCount < shortArray.Length)
            {
                if (shortArray[shortCount] == null)
                {
                    shortArray[shortCount] = "";
                    shortCount++;
                }
                else
                {
                    shortCount++;
                }
            }
            //take care of any null values in the long array
            while (longCount >= 0 && longCount < longArray.Length)
            {
                if (longArray[longCount] == null)
                {
                    longArray[longCount] = "";
                    longCount++;
                }
                else
                {
                    longCount++;
                }
            }
            //reset counters
            longCount = 0;
            shortCount = 0;
            //begin array checking
            while (shortCount >= 0 && shortCount < intMaxOutput)
            {
                //If longCount is at max but shortCount is not
                if (longCount >= (longArray.Length - 1) && shortCount != (shortArray.Length - 1))
                {
                    //test to make sure these values do not match
                    if (shortArray[shortCount] != longArray[longCount])
                    {
                        //we are still testing values
                        longCount = 0;//reset the long counter
                        shortCount++;//update the short counter
                    }
                    else
                    {
                        //something matches
                        strArrayOutput[intOutput] = shortArray[shortCount];
                        intOutput++;//incriment the output array
                        longCount = 0;//reset the long counter
                        shortCount++;//update the short counter
                    }
                }
                if (shortCount >= (shortArray.Length - 1) && longCount >= (longArray.Length - 1))
                {
                    //we have left the boudns of this array
                    //do nothing
                    shortCount++;//this will end the while statment
                }
                else
                {
                    //test for match
                    if (shortArray[shortCount] != longArray[longCount])
                    {
                        //if the two array strings do not match
                        longCount++;
                        //increase the count of the longer array
                    }
                    else
                    {
                        //if the two array strings do match
                        strArrayOutput[intOutput] = shortArray[shortCount];//assign the value to the return array
                        intOutput++;//incriment the output array
                        shortCount++;//incriment the A counter
                        longCount = 0;//reset the B counter
                    }
                }
            }
            return strArrayOutput;
        }


        public static System.Collections.ArrayList intDiff(System.Collections.ArrayList i1, System.Collections.ArrayList i2)
        {
            System.Collections.ArrayList tempList = new System.Collections.ArrayList(10);
            foreach (SQLConsole.Data.AField fld in i1)
            {
                int itag = 0;
                foreach (SQLConsole.Data.AField fldn in i2)
                {
                    if (fld.name == fldn.name) itag = 1;
                }
                if (itag == 0) tempList.Add(fld);
            }
            return tempList;
        }

        public static void setRegEntry(string cell, string keystr, string value)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(cell, true);
            if (key == null) { utils.createRegKey(cell); key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(cell, true); }
            key.SetValue(keystr, value);
            key.Close();
        }

        public static string getRegEntry(string cell, string key)
        {
            return getRegEntry(cell, key, "");
        }

        public static string getRegEntry(string cell, string keystr, string defaultval)
        {
            // Attempt to open the key
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(cell);

            // If the return value is null, the key doesn't exist
            if (key == null)
            {
                utils.createRegKey(cell);
            }
            if (key.GetValue(keystr) != null)
            {
                return (string)key.GetValue(keystr);
            }
            else return defaultval;

        }

        public static void createRegKey(string cellname)
        {
            Microsoft.Win32.RegistryKey f = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(cellname);
            if (f == null)
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(cellname);
        }

    }

    public class AbstractRowField
    {
        public string fieldname;
        public string fieldvalue;
        public string fieldlength;

        
        public AbstractRowField() {
        	
        
        }
        
        public AbstractRowField(string fieldname, string fieldvalue) {
        	
        	this.fieldname = fieldname;
        	this.fieldvalue = fieldvalue;
        }
    }
}

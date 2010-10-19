using System;
using System.Collections.Generic;
using System.Text;

using System.Xml.Serialization;
namespace SQLConsole.Data
{
    [XmlRoot("serverconfig")]
    public class settingsClass
    {
        [XmlElement("userid")]
        public string userid;
        [XmlElement("pass")]
        public string password;
        [XmlElement("host")]
        public string server;
        [XmlElement("database")]
        public string database;
        [XmlElement("dbtype")]
        public SQLConsole.Data.DATABASETYPES dbtype;
    }
}

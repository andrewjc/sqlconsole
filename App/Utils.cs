using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLConsole.Data;
namespace sqlconsole
{
    static class Utils
    {
        public static string getFriendlyNameFromPreset(string filename)
        {
            string newname = "";
            settingsClass settings = new settingsClass();

            System.Xml.Serialization.XmlSerializer s = new System.Xml.Serialization.XmlSerializer(typeof(settingsClass));
            System.IO.TextReader w = new System.IO.StreamReader(filename);

            settings = (settingsClass)s.Deserialize(w);
            w.Close();

            newname = settings.database + "@" + settings.server + " ("+settings.dbtype.ToString()+")";


            return newname;
        }
        public static settingsClass loadSettingsFile(string filename)
        {
            settingsClass settings = new settingsClass();

            System.Xml.Serialization.XmlSerializer s = new System.Xml.Serialization.XmlSerializer(typeof(settingsClass));
            System.IO.TextReader w = new System.IO.StreamReader(filename);

            settings = (settingsClass)s.Deserialize(w);
            w.Close();

            return settings;

        }
    }
}

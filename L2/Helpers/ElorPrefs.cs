using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace ELOR.Laney.Helpers {

    // Возможно, велосипед, но я не стал гуглить о существующих решениях для хранения типизированных настроек в xml.
    public class ElorPrefs {
        private const string ROOT = "elorPrefs";
        private const string INT = "int";
        private const string LONG = "long";
        private const string FLOAT = "float";
        private const string DOUBLE = "double";
        private const string STRING = "string";
        private const string BOOL = "bool";
        public static string SerializeToXML(Dictionary<string, object> preferences) {
            var doc = new XmlDocument();
            var root = doc.CreateElement(ROOT);
            doc.AppendChild(root);

            foreach (var pref in preferences) {
                string type = "";
                string rvalue = "";

                if (pref.Value is int int32) {
                    type = INT;
                    rvalue = int32.ToString();
                } else if (pref.Value is long int64) {
                    type = LONG;
                    rvalue = int64.ToString();
                } else if (pref.Value is float sf) {
                    type = FLOAT;
                    rvalue = sf.ToString().Replace(",", ".");
                } else if (pref.Value is double df) {
                    type = DOUBLE;
                    rvalue = df.ToString().Replace(",", ".");
                } else if (pref.Value is string str) {
                    type = STRING;
                    rvalue = str;
                } else if (pref.Value is bool boolean) {
                    type = BOOL;
                    rvalue = boolean.ToString().ToLower();
                }

                var node = doc.CreateElement(type);

                var value = doc.CreateNode(XmlNodeType.Text, type, null);
                value.Value = rvalue;
                node.AppendChild(value);

                var key = doc.CreateAttribute("key");
                key.Value = pref.Key;
                node.Attributes.Append(key);

                root.AppendChild(node);
            }
            return doc.OuterXml;
        }

        public static Dictionary<string, object> Deserialize(string str) {
            var doc = new XmlDocument();
            doc.LoadXml(str);

            var root = doc.FirstChild;
            if (root.Name != ROOT) throw new XmlException($"Invalid root! First element in XML document must be \"{ROOT}\".");

            return ParseNodes(root.ChildNodes);
        }

        private static Dictionary<string, object> ParseNodes(XmlNodeList nodes) {
            Dictionary<string, object> preferences = new Dictionary<string, object>();
            ushort i = 0;
            foreach (XmlNode node in nodes) {
                var type = node.Name;
                var keynode = node.Attributes.GetNamedItem("key");
                if (keynode == null) throw new XmlException($"Cannot find a \"key\" attribute! Tag: {type}, index: {i}");

                string key = keynode.Value;
                if (String.IsNullOrWhiteSpace(key)) throw new XmlException($"\"key\" attribute is empty! Tag: {type}, index: {i}");

                if (node.ChildNodes.Count == 0) {
                    i++;
                    continue;
                }

                object value = ParseValue(type, key, node.ChildNodes);
                if (value != null) preferences.Add(key, value);
                i++;
            }

            return preferences;
        }

        private static object ParseValue(string type, string key, XmlNodeList childNodes) {
            if (childNodes.Count > 1 || childNodes[0].NodeType != XmlNodeType.Text) throw new XmlException($"Invalid value! Value must be one text node (for example: <int key=\"sample\">123</int>). Key: {key}");
            var value = childNodes[0].Value;

            bool parsed = false;
            switch (type) {
                case INT:
                    int integer = 0;
                    parsed = Int32.TryParse(value, out integer);
                    if (!parsed) throw new ArgumentException($"Invalid value: cannot parse value to {type}! Key: {key}");
                    return integer;
                case LONG:
                    long int64 = 0;
                    parsed = Int64.TryParse(value, out int64);
                    if (!parsed) throw new ArgumentException($"Invalid value: cannot parse value to {type}! Key: {key}");
                    return int64;
                case FLOAT:
                    float sf = 0f;
                    parsed = float.TryParse(value, CultureInfo.InvariantCulture, out sf);
                    if (!parsed) throw new ArgumentException($"Invalid value: cannot parse value to {type}! Key: {key}");
                    return sf;
                case DOUBLE:
                    double df = 0;
                    parsed = Double.TryParse(value, CultureInfo.InvariantCulture, out df);
                    if (!parsed) throw new ArgumentException($"Invalid value: cannot parse value to {type}! Key: {key}");
                    return df;
                case BOOL:
                    bool boolean = false;
                    parsed = Boolean.TryParse(value, out boolean);
                    if (!parsed) throw new ArgumentException($"Invalid value: cannot parse value to {type}! Key: {key}");
                    return boolean;
                case STRING:
                    return value;
                default:
                    return null;
            }
        }
    }
}
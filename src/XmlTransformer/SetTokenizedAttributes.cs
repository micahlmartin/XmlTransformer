using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace XmlTransformer
{
    public class SetTokenizedAttributes : AttributeTransform
    {
        public static readonly string Token = "Token";
        public static readonly string TokenNumber = "TokenNumber";
        public static readonly string XPathWithIndex = "XPathWithIndex";
        public static readonly string ParameterAttribute = "Parameter";
        public static readonly string XpathLocator = "XpathLocator";
        public static readonly string XPathWithLocator = "XPathWithLocator";
        private static Regex s_dirRegex = (Regex)null;
        private static Regex s_parentAttribRegex = (Regex)null;
        private static Regex s_tokenFormatRegex = (Regex)null;
        private SetTokenizedAttributeStorage storageDictionary;
        private bool fInitStorageDictionary;
        private XmlAttribute tokenizeValueCurrentXmlAttribute;

        protected SetTokenizedAttributeStorage TransformStorage
        {
            get
            {
                if (this.storageDictionary == null && !this.fInitStorageDictionary)
                {
                    this.storageDictionary = this.GetService<SetTokenizedAttributeStorage>();
                    this.fInitStorageDictionary = true;
                }
                return this.storageDictionary;
            }
        }

        internal static Regex DirRegex
        {
            get
            {
                if (SetTokenizedAttributes.s_dirRegex == null)
                    SetTokenizedAttributes.s_dirRegex = new Regex("\\G\\{%(\\s*(?<attrname>\\w+(?=\\W))(\\s*(?<equal>=)\\s*'(?<attrval>[^']*)'|\\s*(?<equal>=)\\s*(?<attrval>[^\\s%>]*)|(?<equal>)(?<attrval>\\s*?)))*\\s*?%\\}");
                return SetTokenizedAttributes.s_dirRegex;
            }
        }

        internal static Regex ParentAttributeRegex
        {
            get
            {
                if (SetTokenizedAttributes.s_parentAttribRegex == null)
                    SetTokenizedAttributes.s_parentAttribRegex = new Regex("\\G\\$\\((?<tagname>[\\w:\\.]+)\\)");
                return SetTokenizedAttributes.s_parentAttribRegex;
            }
        }

        internal static Regex TokenFormatRegex
        {
            get
            {
                if (SetTokenizedAttributes.s_tokenFormatRegex == null)
                    SetTokenizedAttributes.s_tokenFormatRegex = new Regex("\\G\\#\\((?<tagname>[\\w:\\.]+)\\)");
                return SetTokenizedAttributes.s_tokenFormatRegex;
            }
        }

        static SetTokenizedAttributes()
        {
        }

        protected override void Apply()
        {
            bool fTokenizeParameter = false;
            SetTokenizedAttributeStorage transformStorage = this.TransformStorage;
            List<Dictionary<string, string>> parameters = (List<Dictionary<string, string>>)null;
            if (transformStorage != null)
            {
                fTokenizeParameter = transformStorage.EnableTokenizeParameters;
                if (fTokenizeParameter)
                    parameters = transformStorage.DictionaryList;
            }
            foreach (XmlAttribute transformAttribute in this.TransformAttributes)
            {
                XmlAttribute targetAttribute = this.TargetNode.Attributes.GetNamedItem(transformAttribute.Name) as XmlAttribute;
                string str = this.TokenizeValue(targetAttribute, transformAttribute, fTokenizeParameter, parameters);
                if (targetAttribute != null)
                {
                    targetAttribute.Value = str;
                }
                else
                {
                    XmlAttribute node = (XmlAttribute)transformAttribute.Clone();
                    node.Value = str;
                    this.TargetNode.Attributes.Append(node);
                }
                this.Log.LogMessage(MessageType.Verbose, "Set '{0}' attribute", new object[1]
        {
          (object) transformAttribute.Name
        });
            }
            if (this.TransformAttributes.Count > 0)
                this.Log.LogMessage(MessageType.Verbose, "Set {0} attributes", new object[1]
        {
          (object) this.TransformAttributes.Count
        });
            else
                this.Log.LogWarning("No attributes found to set", new object[0]);
        }

        protected string GetAttributeValue(string attributeName)
        {
            string str = (string)null;
            XmlAttribute xmlAttribute = this.TargetNode.Attributes.GetNamedItem(attributeName) as XmlAttribute;
            if (xmlAttribute == null && string.Compare(attributeName, this.tokenizeValueCurrentXmlAttribute.Name, StringComparison.OrdinalIgnoreCase) != 0)
                xmlAttribute = this.TransformNode.Attributes.GetNamedItem(attributeName) as XmlAttribute;
            if (xmlAttribute != null)
                str = xmlAttribute.Value;
            return str;
        }

        protected string EscapeDirRegexSpecialCharacter(string value, bool escape)
        {
            if (escape)
                return value.Replace("'", "&apos;");
            else
                return value.Replace("&apos;", "'");
        }

        protected static string SubstituteKownValue(string transformValue, Regex patternRegex, string patternPrefix, SetTokenizedAttributes.GetValueCallback getValueDelegate)
        {
            int num1 = 0;
            List<System.Text.RegularExpressions.Match> list = new List<System.Text.RegularExpressions.Match>();
            do
            {
                num1 = transformValue.IndexOf(patternPrefix, num1, StringComparison.OrdinalIgnoreCase);
                if (num1 > -1)
                {
                    System.Text.RegularExpressions.Match match = patternRegex.Match(transformValue, num1);
                    if (match.Success)
                    {
                        list.Add(match);
                        num1 = match.Index + match.Length;
                    }
                    else
                        ++num1;
                }
            }
            while (num1 > -1);
            StringBuilder stringBuilder = new StringBuilder(transformValue.Length);
            if (list.Count > 0)
            {
                stringBuilder.Remove(0, stringBuilder.Length);
                int startIndex = 0;
                int num2 = 0;
                foreach (System.Text.RegularExpressions.Match match in list)
                {
                    stringBuilder.Append(transformValue.Substring(startIndex, match.Index - startIndex));
                    string key = match.Groups["tagname"].Value;
                    string str = getValueDelegate(key);
                    if (str != null)
                        stringBuilder.Append(str);
                    else
                        stringBuilder.Append(match.Value);
                    startIndex = match.Index + match.Length;
                    ++num2;
                }
                stringBuilder.Append(transformValue.Substring(startIndex));
                transformValue = ((object)stringBuilder).ToString();
            }
            return transformValue;
        }

        private string GetXPathToAttribute(XmlAttribute xmlAttribute)
        {
            return this.GetXPathToAttribute(xmlAttribute, (IList<string>)null);
        }

        private string GetXPathToAttribute(XmlAttribute xmlAttribute, IList<string> locators)
        {
            string str1 = string.Empty;
            if (xmlAttribute != null)
            {
                string str2 = this.GetXPathToNode((XmlNode)xmlAttribute.OwnerElement);
                if (!string.IsNullOrEmpty(str2))
                {
                    StringBuilder stringBuilder = new StringBuilder(256);
                    if (locators != null && locators.Count != 0)
                    {
                        foreach (string attributeName in (IEnumerable<string>)locators)
                        {
                            string attributeValue = this.GetAttributeValue(attributeName);
                            if (!string.IsNullOrEmpty(attributeValue))
                            {
                                if (stringBuilder.Length != 0)
                                    stringBuilder.Append(" and ");
                                stringBuilder.Append(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "@{0}='{1}'", new object[2]
                {
                  (object) attributeName,
                  (object) attributeValue
                }));
                            }
                            else
                                throw new XmlTransformationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "No attribute '{0}' exists for the Match Locator", new object[1]
                {
                  (object) attributeName
                }));
                        }
                    }
                    if (stringBuilder.Length == 0)
                    {
                        for (int index = 0; index < this.TargetNodes.Count; ++index)
                        {
                            if (this.TargetNodes[index] == xmlAttribute.OwnerElement)
                            {
                                stringBuilder.Append((index + 1).ToString((IFormatProvider)CultureInfo.InvariantCulture));
                                break;
                            }
                        }
                    }
                    str2 = str2 + "[" + ((object)stringBuilder).ToString() + "]";
                }
                str1 = str2 + "/@" + xmlAttribute.Name;
            }
            return str1;
        }

        private string GetXPathToNode(XmlNode xmlNode)
        {
            if (xmlNode == null || xmlNode.NodeType == XmlNodeType.Document)
                return (string)null;
            else
                return this.GetXPathToNode(xmlNode.ParentNode) + "/" + xmlNode.Name;
        }

        private string TokenizeValue(XmlAttribute targetAttribute, XmlAttribute transformAttribute, bool fTokenizeParameter, List<Dictionary<string, string>> parameters)
        {
            this.tokenizeValueCurrentXmlAttribute = transformAttribute;
            string transformValue = transformAttribute.Value;
            string xpathToAttribute1 = this.GetXPathToAttribute(targetAttribute);
            string input = SetTokenizedAttributes.SubstituteKownValue(transformValue, SetTokenizedAttributes.ParentAttributeRegex, "$(", (SetTokenizedAttributes.GetValueCallback)(key => this.EscapeDirRegexSpecialCharacter(this.GetAttributeValue(key), true)));
            if (fTokenizeParameter && parameters != null)
            {
                StringBuilder stringBuilder = new StringBuilder(input.Length);
                int num1 = 0;
                List<System.Text.RegularExpressions.Match> list = new List<System.Text.RegularExpressions.Match>();
                do
                {
                    num1 = input.IndexOf("{%", num1, StringComparison.OrdinalIgnoreCase);
                    if (num1 > -1)
                    {
                        System.Text.RegularExpressions.Match match = SetTokenizedAttributes.DirRegex.Match(input, num1);
                        if (match.Success)
                        {
                            list.Add(match);
                            num1 = match.Index + match.Length;
                        }
                        else
                            ++num1;
                    }
                }
                while (num1 > -1);
                if (list.Count > 0)
                {
                    stringBuilder.Remove(0, stringBuilder.Length);
                    int startIndex = 0;
                    int num2 = 0;
                    foreach (System.Text.RegularExpressions.Match match in list)
                    {
                        stringBuilder.Append(input.Substring(startIndex, match.Index - startIndex));
                        CaptureCollection captures1 = match.Groups["attrname"].Captures;
                        if (captures1 != null && captures1.Count > 0)
                        {
                            CaptureCollection captures2 = match.Groups["attrval"].Captures;
                            Dictionary<string, string> paramDictionary = new Dictionary<string, string>(4, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
                            paramDictionary[SetTokenizedAttributes.XPathWithIndex] = xpathToAttribute1;
                            paramDictionary[SetTokenizedAttributes.TokenNumber] = num2.ToString((IFormatProvider)CultureInfo.InvariantCulture);
                            for (int index1 = 0; index1 < captures1.Count; ++index1)
                            {
                                string index2 = captures1[index1].Value;
                                string str = (string)null;
                                if (captures2 != null && index1 < captures2.Count)
                                    str = this.EscapeDirRegexSpecialCharacter(captures2[index1].Value, false);
                                paramDictionary[index2] = str;
                            }
                            string str1 = (string)null;
                            if (!paramDictionary.TryGetValue(SetTokenizedAttributes.Token, out str1))
                                str1 = this.storageDictionary.TokenFormat;
                            if (!string.IsNullOrEmpty(str1))
                                paramDictionary[SetTokenizedAttributes.Token] = str1;
                            int count = paramDictionary.Count;
                            string[] array = new string[count];
                            paramDictionary.Keys.CopyTo(array, 0);
                            for (int index1 = 0; index1 < count; ++index1)
                            {
                                string index2 = array[index1];
                                string str2 = SetTokenizedAttributes.SubstituteKownValue(paramDictionary[index2], SetTokenizedAttributes.TokenFormatRegex, "#(", (SetTokenizedAttributes.GetValueCallback)(key =>
                                {
                                    if (!paramDictionary.ContainsKey(key))
                                        return (string)null;
                                    else
                                        return paramDictionary[key];
                                }));
                                paramDictionary[index2] = str2;
                            }
                            if (paramDictionary.TryGetValue(SetTokenizedAttributes.Token, out str1))
                                stringBuilder.Append(str1);
                            string argumentString;
                            if (paramDictionary.TryGetValue(SetTokenizedAttributes.XpathLocator, out argumentString) && !string.IsNullOrEmpty(argumentString))
                            {
                                IList<string> locators = XmlArgumentUtility.SplitArguments(argumentString);
                                string xpathToAttribute2 = this.GetXPathToAttribute(targetAttribute, locators);
                                if (!string.IsNullOrEmpty(xpathToAttribute2))
                                    paramDictionary[SetTokenizedAttributes.XPathWithLocator] = xpathToAttribute2;
                            }
                            parameters.Add(paramDictionary);
                        }
                        startIndex = match.Index + match.Length;
                        ++num2;
                    }
                    stringBuilder.Append(input.Substring(startIndex));
                    input = ((object)stringBuilder).ToString();
                }
            }
            return input;
        }

        protected delegate string GetValueCallback(string key);
    }
}

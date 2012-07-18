using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    internal class XmlAttributePreservationDict
    {
        private List<string> orderedAttributes = new List<string>();
        private Dictionary<string, string> leadingSpaces = new Dictionary<string, string>();
        private string attributeNewLineString;
        private bool computedOneAttributePerLine;
        private bool oneAttributePerLine;

        private bool OneAttributePerLine
        {
            get
            {
                if (!this.computedOneAttributePerLine)
                {
                    this.computedOneAttributePerLine = true;
                    this.oneAttributePerLine = this.ComputeOneAttributePerLine();
                }
                return this.oneAttributePerLine;
            }
        }

        internal void ReadPreservationInfo(string elementStartTag)
        {
            XmlTextReader xmlTextReader = new XmlTextReader((TextReader)new StringReader(elementStartTag));
            WhitespaceTrackingTextReader trackingTextReader = new WhitespaceTrackingTextReader((TextReader)new StringReader(elementStartTag));
            xmlTextReader.Namespaces = false;
            xmlTextReader.Read();
            for (bool flag = xmlTextReader.MoveToFirstAttribute(); flag; flag = xmlTextReader.MoveToNextAttribute())
            {
                this.orderedAttributes.Add(xmlTextReader.Name);
                if (trackingTextReader.ReadToPosition(xmlTextReader.LineNumber, xmlTextReader.LinePosition))
                    this.leadingSpaces.Add(xmlTextReader.Name, trackingTextReader.PrecedingWhitespace);
            }
            int length = elementStartTag.Length;
            if (elementStartTag.EndsWith("/>", StringComparison.Ordinal))
                --length;
            if (!trackingTextReader.ReadToPosition(length))
                return;
            this.leadingSpaces.Add(string.Empty, trackingTextReader.PrecedingWhitespace);
        }

        internal void WritePreservedAttributes(XmlAttributePreservingWriter writer, XmlAttributeCollection attributes)
        {
            string newLineString = (string)null;
            if (this.attributeNewLineString != null)
                newLineString = writer.SetAttributeNewLineString(this.attributeNewLineString);
            try
            {
                foreach (string key in this.orderedAttributes)
                {
                    XmlAttribute xmlAttribute = attributes[key];
                    if (xmlAttribute != null)
                    {
                        if (this.leadingSpaces.ContainsKey(key))
                            writer.WriteAttributeWhitespace(this.leadingSpaces[key]);
                        xmlAttribute.WriteTo((XmlWriter)writer);
                    }
                }
                if (!this.leadingSpaces.ContainsKey(string.Empty))
                    return;
                writer.WriteAttributeTrailingWhitespace(this.leadingSpaces[string.Empty]);
            }
            finally
            {
                if (newLineString != null)
                    writer.SetAttributeNewLineString(newLineString);
            }
        }

        internal void UpdatePreservationInfo(XmlAttributeCollection updatedAttributes, XmlFormatter formatter)
        {
            if (updatedAttributes.Count == 0)
            {
                if (this.orderedAttributes.Count <= 0)
                    return;
                this.leadingSpaces.Clear();
                this.orderedAttributes.Clear();
            }
            else
            {
                Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
                foreach (string index in this.orderedAttributes)
                    dictionary[index] = false;
                foreach (XmlAttribute xmlAttribute in (XmlNamedNodeMap)updatedAttributes)
                {
                    if (!dictionary.ContainsKey(xmlAttribute.Name))
                        this.orderedAttributes.Add(xmlAttribute.Name);
                    dictionary[xmlAttribute.Name] = true;
                }
                bool flag1 = true;
                string str = (string)null;
                foreach (string key in this.orderedAttributes)
                {
                    bool flag2 = dictionary[key];
                    if (!flag2)
                    {
                        if (this.leadingSpaces.ContainsKey(key))
                        {
                            string space = this.leadingSpaces[key];
                            if (flag1)
                            {
                                if (str == null)
                                    str = space;
                            }
                            else if (this.ContainsNewLine(space))
                                str = space;
                            this.leadingSpaces.Remove(key);
                        }
                    }
                    else if (str != null)
                    {
                        if (flag1 || !this.leadingSpaces.ContainsKey(key) || !this.ContainsNewLine(this.leadingSpaces[key]))
                            this.leadingSpaces[key] = str;
                        str = (string)null;
                    }
                    else if (!this.leadingSpaces.ContainsKey(key))
                    {
                        if (flag1)
                            this.leadingSpaces[key] = " ";
                        else if (this.OneAttributePerLine)
                            this.leadingSpaces[key] = this.GetAttributeNewLineString(formatter);
                        else
                            this.EnsureAttributeNewLineString(formatter);
                    }
                    flag1 = flag1 && !flag2;
                }
            }
        }

        private bool ComputeOneAttributePerLine()
        {
            if (this.leadingSpaces.Count <= 1)
                return false;
            bool flag = true;
            foreach (string key in this.orderedAttributes)
            {
                if (flag)
                    flag = false;
                else if (this.leadingSpaces.ContainsKey(key) && !this.ContainsNewLine(this.leadingSpaces[key]))
                    return false;
            }
            return true;
        }

        private bool ContainsNewLine(string space)
        {
            return space.IndexOf("\n", StringComparison.Ordinal) >= 0;
        }

        public string GetAttributeNewLineString(XmlFormatter formatter)
        {
            if (this.attributeNewLineString == null)
                this.attributeNewLineString = this.ComputeAttributeNewLineString(formatter);
            return this.attributeNewLineString;
        }

        private string ComputeAttributeNewLineString(XmlFormatter formatter)
        {
            string str = this.LookAheadForNewLineString();
            if (str != null)
                return str;
            if (formatter != null)
                return formatter.CurrentAttributeIndent;
            else
                return (string)null;
        }

        private string LookAheadForNewLineString()
        {
            foreach (string space in this.leadingSpaces.Values)
            {
                if (this.ContainsNewLine(space))
                    return space;
            }
            return (string)null;
        }

        private void EnsureAttributeNewLineString(XmlFormatter formatter)
        {
            this.GetAttributeNewLineString(formatter);
        }
    }
}

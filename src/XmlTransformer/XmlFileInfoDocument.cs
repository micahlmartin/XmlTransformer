using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    public class XmlFileInfoDocument : XmlDocument
    {
        private bool _firstLoad = true;
        private Encoding _textEncoding;
        private XmlTextReader _reader;
        private XmlAttributePreservationProvider _preservationProvider;
        private string _fileName;
        private int _lineNumberOffset;
        private int _linePositionOffset;

        internal bool HasErrorInfo
        {
            get
            {
                return this._reader != null;
            }
        }

        internal string FileName
        {
            get
            {
                return this._fileName;
            }
        }

        private int CurrentLineNumber
        {
            get
            {
                if (this._reader == null)
                    return 0;
                else
                    return this._reader.LineNumber + this._lineNumberOffset;
            }
        }

        private int CurrentLinePosition
        {
            get
            {
                if (this._reader == null)
                    return 0;
                else
                    return this._reader.LinePosition + this._linePositionOffset;
            }
        }

        private bool FirstLoad
        {
            get
            {
                return this._firstLoad;
            }
        }

        private XmlAttributePreservationProvider PreservationProvider
        {
            get
            {
                return this._preservationProvider;
            }
        }

        private new Encoding TextEncoding
        {
            get
            {
                if (this._textEncoding != null)
                    return this._textEncoding;
                if (this.HasChildNodes)
                {
                    XmlDeclaration xmlDeclaration = this.FirstChild as XmlDeclaration;
                    if (xmlDeclaration != null)
                    {
                        string encoding = xmlDeclaration.Encoding;
                        if (encoding.Length > 0)
                            return Encoding.GetEncoding(encoding);
                    }
                }
                return (Encoding)null;
            }
        }

        public override void Load(string filename)
        {
            this.LoadFromFileName(filename);
            this._firstLoad = false;
        }

        public override void Load(Stream inStream)
        {
            this.LoadFromTextReader((TextReader)new StreamReader(inStream, true));
            this._firstLoad = false;
        }

        public override void Load(TextReader txtReader)
        {
            this.LoadFromTextReader(txtReader);
            this._firstLoad = false;
        }

        public override void Load(XmlReader reader)
        {
            this._reader = reader as XmlTextReader;
            if (this._reader != null)
                this._fileName = this._reader.BaseURI;
            base.Load(reader);
            if (this._reader != null)
                this._textEncoding = this._reader.Encoding;
            this._firstLoad = false;
        }

        private void LoadFromFileName(string filename)
        {
            this._fileName = filename;
            StreamReader streamReader = (StreamReader)null;
            try
            {
                if (this.PreserveWhitespace)
                    this._preservationProvider = new XmlAttributePreservationProvider(filename);
                streamReader = new StreamReader(filename, true);
                this.LoadFromTextReader((TextReader)streamReader);
            }
            finally
            {
                if (this._preservationProvider != null)
                {
                    this._preservationProvider.Close();
                    this._preservationProvider = (XmlAttributePreservationProvider)null;
                }
                if (streamReader != null)
                {
                    streamReader.Close();
                    streamReader.Dispose();
                }
            }
        }

        private void LoadFromTextReader(TextReader textReader)
        {
            StreamReader streamReader = textReader as StreamReader;
            if (streamReader != null)
            {
                FileStream fileStream = streamReader.BaseStream as FileStream;
                if (fileStream != null)
                    this._fileName = fileStream.Name;
                this._textEncoding = this.GetEncodingFromStream(streamReader.BaseStream);
            }
            this._reader = new XmlTextReader(this._fileName, textReader);
            base.Load((XmlReader)this._reader);
            if (this._textEncoding != null)
                return;
            this._textEncoding = this._reader.Encoding;
        }

        private Encoding GetEncodingFromStream(Stream stream)
        {
            Encoding encoding = (Encoding)null;
            if (stream.CanSeek)
            {
                byte[] buffer = new byte[3];
                stream.Read(buffer, 0, buffer.Length);
                if ((int)buffer[0] == 239 && (int)buffer[1] == 187 && (int)buffer[2] == 191)
                    encoding = Encoding.UTF8;
                else if ((int)buffer[0] == 254 && (int)buffer[1] == (int)byte.MaxValue)
                    encoding = Encoding.BigEndianUnicode;
                else if ((int)buffer[0] == (int)byte.MaxValue && (int)buffer[1] == 254)
                    encoding = Encoding.Unicode;
                else if ((int)buffer[0] == 43 && (int)buffer[1] == 47 && (int)buffer[2] == 118)
                    encoding = Encoding.UTF7;
                stream.Seek(0L, SeekOrigin.Begin);
            }
            return encoding;
        }

        internal XmlNode CloneNodeFromOtherDocument(XmlNode element)
        {
            XmlTextReader xmlTextReader = this._reader;
            string str = this._fileName;
            XmlNode xmlNode = (XmlNode)null;
            try
            {
                IXmlLineInfo xmlLineInfo = element as IXmlLineInfo;
                if (xmlLineInfo != null)
                {
                    this._reader = new XmlTextReader((TextReader)new StringReader(element.OuterXml));
                    this._lineNumberOffset = xmlLineInfo.LineNumber - 1;
                    this._linePositionOffset = xmlLineInfo.LinePosition - 2;
                    this._fileName = element.OwnerDocument.BaseURI;
                    xmlNode = this.ReadNode((XmlReader)this._reader);
                }
                else
                {
                    this._fileName = (string)null;
                    this._reader = (XmlTextReader)null;
                    xmlNode = this.ReadNode((XmlReader)new XmlTextReader((TextReader)new StringReader(element.OuterXml)));
                }
            }
            finally
            {
                this._lineNumberOffset = 0;
                this._linePositionOffset = 0;
                this._fileName = str;
                this._reader = xmlTextReader;
            }
            return xmlNode;
        }

        public override void Save(string filename)
        {
            XmlWriter w = (XmlWriter)null;
            try
            {
                if (this.PreserveWhitespace)
                {
                    XmlFormatter.Format((XmlDocument)this);
                    w = (XmlWriter)new XmlAttributePreservingWriter(filename, this.TextEncoding);
                }
                else
                    w = (XmlWriter)new XmlTextWriter(filename, this.TextEncoding)
                    {
                        Formatting = Formatting.Indented
                    };
                this.WriteTo(w);
            }
            finally
            {
                if (w != null)
                {
                    w.Flush();
                    w.Close();
                }
            }
        }

        public override XmlElement CreateElement(string prefix, string localName, string namespaceURI)
        {
            if (this.HasErrorInfo)
                return (XmlElement)new XmlFileInfoDocument.XmlFileInfoElement(prefix, localName, namespaceURI, this);
            else
                return base.CreateElement(prefix, localName, namespaceURI);
        }

        public override XmlAttribute CreateAttribute(string prefix, string localName, string namespaceURI)
        {
            if (this.HasErrorInfo)
                return (XmlAttribute)new XmlFileInfoDocument.XmlFileInfoAttribute(prefix, localName, namespaceURI, this);
            else
                return base.CreateAttribute(prefix, localName, namespaceURI);
        }

        internal bool IsNewNode(XmlNode node)
        {
            XmlFileInfoDocument.XmlFileInfoElement xmlFileInfoElement = this.FindContainingElement(node) as XmlFileInfoDocument.XmlFileInfoElement;
            if (xmlFileInfoElement != null)
                return !xmlFileInfoElement.IsOriginal;
            else
                return false;
        }

        private XmlElement FindContainingElement(XmlNode node)
        {
            while (node != null && !(node is XmlElement))
                node = node.ParentNode;
            return node as XmlElement;
        }

        private class XmlFileInfoElement : XmlElement, IXmlLineInfo, IXmlFormattableAttributes
        {
            private int lineNumber;
            private int linePosition;
            private bool isOriginal;
            private XmlAttributePreservationDict preservationDict;

            public int LineNumber
            {
                get
                {
                    return this.lineNumber;
                }
            }

            public int LinePosition
            {
                get
                {
                    return this.linePosition;
                }
            }

            public bool IsOriginal
            {
                get
                {
                    return this.isOriginal;
                }
            }

            string IXmlFormattableAttributes.AttributeIndent
            {
                get
                {
                    return this.preservationDict.GetAttributeNewLineString((XmlFormatter)null);
                }
            }

            internal XmlFileInfoElement(string prefix, string localName, string namespaceUri, XmlFileInfoDocument document)
                : base(prefix, localName, namespaceUri, (XmlDocument)document)
            {
                this.lineNumber = document.CurrentLineNumber;
                this.linePosition = document.CurrentLinePosition;
                this.isOriginal = document.FirstLoad;
                if (document.PreservationProvider != null)
                    this.preservationDict = document.PreservationProvider.GetDictAtPosition(this.lineNumber, this.linePosition - 1);
                if (this.preservationDict != null)
                    return;
                this.preservationDict = new XmlAttributePreservationDict();
            }

            public override void WriteTo(XmlWriter w)
            {
                string prefix = this.Prefix;
                if (!string.IsNullOrEmpty(this.NamespaceURI))
                    prefix = w.LookupPrefix(this.NamespaceURI) ?? this.Prefix;
                w.WriteStartElement(prefix, this.LocalName, this.NamespaceURI);
                if (this.HasAttributes)
                {
                    XmlAttributePreservingWriter preservingWriter = w as XmlAttributePreservingWriter;
                    if (preservingWriter == null || this.preservationDict == null)
                        this.WriteAttributesTo(w);
                    else
                        this.WritePreservedAttributesTo(preservingWriter);
                }
                if (this.IsEmpty)
                {
                    w.WriteEndElement();
                }
                else
                {
                    this.WriteContentTo(w);
                    w.WriteFullEndElement();
                }
            }

            private void WriteAttributesTo(XmlWriter w)
            {
                XmlAttributeCollection attributes = this.Attributes;
                for (int index = 0; index < attributes.Count; ++index)
                    attributes[index].WriteTo(w);
            }

            private void WritePreservedAttributesTo(XmlAttributePreservingWriter preservingWriter)
            {
                this.preservationDict.WritePreservedAttributes(preservingWriter, this.Attributes);
            }

            public bool HasLineInfo()
            {
                return true;
            }

            void IXmlFormattableAttributes.FormatAttributes(XmlFormatter formatter)
            {
                this.preservationDict.UpdatePreservationInfo(this.Attributes, formatter);
            }
        }

        private class XmlFileInfoAttribute : XmlAttribute, IXmlLineInfo
        {
            private int lineNumber;
            private int linePosition;

            public int LineNumber
            {
                get
                {
                    return this.lineNumber;
                }
            }

            public int LinePosition
            {
                get
                {
                    return this.linePosition;
                }
            }

            internal XmlFileInfoAttribute(string prefix, string localName, string namespaceUri, XmlFileInfoDocument document)
                : base(prefix, localName, namespaceUri, (XmlDocument)document)
            {
                this.lineNumber = document.CurrentLineNumber;
                this.linePosition = document.CurrentLinePosition;
            }

            public bool HasLineInfo()
            {
                return true;
            }
        }
    }
}

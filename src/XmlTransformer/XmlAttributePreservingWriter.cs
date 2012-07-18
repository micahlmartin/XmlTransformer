using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    internal class XmlAttributePreservingWriter : XmlWriter
    {
        private XmlTextWriter xmlWriter;
        private XmlAttributePreservingWriter.AttributeTextWriter textWriter;

        public override WriteState WriteState
        {
            get
            {
                return this.xmlWriter.WriteState;
            }
        }

        public XmlAttributePreservingWriter(string fileName, Encoding encoding)
            : this(encoding == null ? (TextWriter)new StreamWriter(fileName) : (TextWriter)new StreamWriter(fileName, false, encoding))
        {
        }

        public XmlAttributePreservingWriter(TextWriter textWriter)
        {
            this.textWriter = new XmlAttributePreservingWriter.AttributeTextWriter(textWriter);
            this.xmlWriter = new XmlTextWriter((TextWriter)this.textWriter);
        }

        public void WriteAttributeWhitespace(string whitespace)
        {
            if (this.WriteState == WriteState.Attribute)
                this.WriteEndAttribute();
            else if (this.WriteState != WriteState.Element)
                throw new InvalidOperationException();
            this.textWriter.AttributeLeadingWhitespace = whitespace;
        }

        public void WriteAttributeTrailingWhitespace(string whitespace)
        {
            if (this.WriteState == WriteState.Attribute)
                this.WriteEndAttribute();
            else if (this.WriteState != WriteState.Element)
                throw new InvalidOperationException();
            this.textWriter.Write(whitespace);
        }

        public string SetAttributeNewLineString(string newLineString)
        {
            string attributeNewLineString = this.textWriter.AttributeNewLineString;
            if (newLineString == null && this.xmlWriter.Settings != null)
                newLineString = this.xmlWriter.Settings.NewLineChars;
            if (newLineString == null)
                newLineString = "\r\n";
            this.textWriter.AttributeNewLineString = newLineString;
            return attributeNewLineString;
        }

        private bool IsOnlyWhitespace(string whitespace)
        {
            foreach (char c in whitespace)
            {
                if (!char.IsWhiteSpace(c))
                    return false;
            }
            return true;
        }

        public override void Close()
        {
            this.xmlWriter.Close();
        }

        public override void Flush()
        {
            this.xmlWriter.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            return this.xmlWriter.LookupPrefix(ns);
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            this.xmlWriter.WriteBase64(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            this.xmlWriter.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            this.xmlWriter.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.xmlWriter.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            this.xmlWriter.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            this.xmlWriter.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteEndAttribute()
        {
            this.xmlWriter.WriteEndAttribute();
            this.textWriter.EndAttribute();
        }

        public override void WriteEndDocument()
        {
            this.xmlWriter.WriteEndDocument();
        }

        public override void WriteEndElement()
        {
            this.xmlWriter.WriteEndElement();
        }

        public override void WriteEntityRef(string name)
        {
            this.xmlWriter.WriteEntityRef(name);
        }

        public override void WriteFullEndElement()
        {
            this.xmlWriter.WriteFullEndElement();
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this.xmlWriter.WriteProcessingInstruction(name, text);
        }

        public override void WriteRaw(string data)
        {
            this.xmlWriter.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.xmlWriter.WriteRaw(buffer, index, count);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this.textWriter.StartAttribute();
            this.xmlWriter.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartDocument(bool standalone)
        {
            this.xmlWriter.WriteStartDocument(standalone);
        }

        public override void WriteStartDocument()
        {
            this.xmlWriter.WriteStartDocument();
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.xmlWriter.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteString(string text)
        {
            this.xmlWriter.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            this.xmlWriter.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            this.xmlWriter.WriteWhitespace(ws);
        }

        private class AttributeTextWriter : TextWriter
        {
            private int lineNumber = 1;
            private int linePosition = 1;
            private int maxLineLength = 160;
            private string newLineString = "\r\n";
            private XmlAttributePreservingWriter.AttributeTextWriter.State state;
            private StringBuilder writeBuffer;
            private TextWriter baseWriter;
            private string leadingWhitespace;

            public string AttributeLeadingWhitespace
            {
                set
                {
                    this.leadingWhitespace = value;
                }
            }

            public string AttributeNewLineString
            {
                get
                {
                    return this.newLineString;
                }
                set
                {
                    this.newLineString = value;
                }
            }

            public int MaxLineLength
            {
                get
                {
                    return this.maxLineLength;
                }
                set
                {
                    this.maxLineLength = value;
                }
            }

            public override Encoding Encoding
            {
                get
                {
                    return this.baseWriter.Encoding;
                }
            }

            public AttributeTextWriter(TextWriter baseWriter)
                : base((IFormatProvider)CultureInfo.InvariantCulture)
            {
                this.baseWriter = baseWriter;
            }

            public void StartAttribute()
            {
                this.ChangeState(XmlAttributePreservingWriter.AttributeTextWriter.State.WaitingForAttributeLeadingSpace);
            }

            public void EndAttribute()
            {
                this.WriteQueuedAttribute();
            }

            public override void Write(char value)
            {
                this.UpdateState(value);
                switch (this.state)
                {
                    case XmlAttributePreservingWriter.AttributeTextWriter.State.Writing:
                    case XmlAttributePreservingWriter.AttributeTextWriter.State.FlushingBuffer:
                        this.ReallyWriteCharacter(value);
                        break;
                    case XmlAttributePreservingWriter.AttributeTextWriter.State.WaitingForAttributeLeadingSpace:
                        if ((int)value == 32)
                        {
                            this.ChangeState(XmlAttributePreservingWriter.AttributeTextWriter.State.ReadingAttribute);
                            break;
                        }
                        else
                            goto case 0;
                    case XmlAttributePreservingWriter.AttributeTextWriter.State.ReadingAttribute:
                    case XmlAttributePreservingWriter.AttributeTextWriter.State.Buffering:
                        this.writeBuffer.Append(value);
                        break;
                }
            }

            private void UpdateState(char value)
            {
                switch (value)
                {
                    case ' ':
                        if (this.state != XmlAttributePreservingWriter.AttributeTextWriter.State.Writing)
                            break;
                        this.ChangeState(XmlAttributePreservingWriter.AttributeTextWriter.State.Buffering);
                        break;
                    case '/':
                        break;
                    case '>':
                        if (this.state != XmlAttributePreservingWriter.AttributeTextWriter.State.Buffering)
                            break;
                        string str = ((object)this.writeBuffer).ToString();
                        if (str.EndsWith(" /", StringComparison.Ordinal))
                            this.writeBuffer.Remove(str.LastIndexOf(' '), 1);
                        this.ChangeState(XmlAttributePreservingWriter.AttributeTextWriter.State.Writing);
                        break;
                    default:
                        if (this.state != XmlAttributePreservingWriter.AttributeTextWriter.State.Buffering)
                            break;
                        this.ChangeState(XmlAttributePreservingWriter.AttributeTextWriter.State.Writing);
                        break;
                }
            }

            private void ChangeState(XmlAttributePreservingWriter.AttributeTextWriter.State newState)
            {
                if (this.state == newState)
                    return;
                XmlAttributePreservingWriter.AttributeTextWriter.State state = this.state;
                this.state = newState;
                if (this.StateRequiresBuffer(newState))
                {
                    this.CreateBuffer();
                }
                else
                {
                    if (!this.StateRequiresBuffer(state))
                        return;
                    this.FlushBuffer();
                }
            }

            private bool StateRequiresBuffer(XmlAttributePreservingWriter.AttributeTextWriter.State state)
            {
                if (state != XmlAttributePreservingWriter.AttributeTextWriter.State.Buffering)
                    return state == XmlAttributePreservingWriter.AttributeTextWriter.State.ReadingAttribute;
                else
                    return true;
            }

            private void CreateBuffer()
            {
                if (this.writeBuffer != null)
                    return;
                this.writeBuffer = new StringBuilder();
            }

            private void FlushBuffer()
            {
                if (this.writeBuffer == null)
                    return;
                XmlAttributePreservingWriter.AttributeTextWriter.State state = this.state;
                try
                {
                    this.state = XmlAttributePreservingWriter.AttributeTextWriter.State.FlushingBuffer;
                    this.Write(((object)this.writeBuffer).ToString());
                    this.writeBuffer = (StringBuilder)null;
                }
                finally
                {
                    this.state = state;
                }
            }

            private void ReallyWriteCharacter(char value)
            {
                this.baseWriter.Write(value);
                if ((int)value == 10)
                {
                    ++this.lineNumber;
                    this.linePosition = 1;
                }
                else
                    ++this.linePosition;
            }

            private void WriteQueuedAttribute()
            {
                if (this.leadingWhitespace != null)
                {
                    this.writeBuffer.Insert(0, this.leadingWhitespace);
                    this.leadingWhitespace = (string)null;
                }
                else if (this.linePosition + this.writeBuffer.Length + 1 > this.MaxLineLength)
                    this.writeBuffer.Insert(0, this.AttributeNewLineString);
                else
                    this.writeBuffer.Insert(0, ' ');
                this.ChangeState(XmlAttributePreservingWriter.AttributeTextWriter.State.Writing);
            }

            public override void Flush()
            {
                this.baseWriter.Flush();
            }

            public override void Close()
            {
                this.baseWriter.Close();
            }

            private enum State
            {
                Writing,
                WaitingForAttributeLeadingSpace,
                ReadingAttribute,
                Buffering,
                FlushingBuffer,
            }
        }
    }
}

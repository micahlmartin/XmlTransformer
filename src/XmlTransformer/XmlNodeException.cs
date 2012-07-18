using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    [Serializable]
    public class XmlNodeException : XmlTransformationException
    {
        private XmlFileInfoDocument document;
        private IXmlLineInfo lineInfo;

        public bool HasErrorInfo
        {
            get
            {
                return this.lineInfo != null;
            }
        }

        public string FileName
        {
            get
            {
                if (this.document == null)
                    return (string)null;
                else
                    return this.document.FileName;
            }
        }

        public int LineNumber
        {
            get
            {
                if (this.lineInfo == null)
                    return 0;
                else
                    return this.lineInfo.LineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                if (this.lineInfo == null)
                    return 0;
                else
                    return this.lineInfo.LinePosition;
            }
        }

        public XmlNodeException(Exception innerException, XmlNode node)
            : base(innerException.Message, innerException)
        {
            this.lineInfo = node as IXmlLineInfo;
            this.document = node.OwnerDocument as XmlFileInfoDocument;
        }

        public XmlNodeException(string message, XmlNode node)
            : base(message)
        {
            this.lineInfo = node as IXmlLineInfo;
            this.document = node.OwnerDocument as XmlFileInfoDocument;
        }

        public static Exception Wrap(Exception ex, XmlNode node)
        {
            if (ex is XmlNodeException)
                return ex;
            else
                return (Exception)new XmlNodeException(ex, node);
        }
    }
}

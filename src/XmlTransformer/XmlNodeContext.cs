using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    internal class XmlNodeContext
    {
        private XmlNode node;

        public XmlNode Node
        {
            get
            {
                return this.node;
            }
        }

        public bool HasLineInfo
        {
            get
            {
                return this.node is IXmlLineInfo;
            }
        }

        public int LineNumber
        {
            get
            {
                IXmlLineInfo xmlLineInfo = this.node as IXmlLineInfo;
                if (xmlLineInfo != null)
                    return xmlLineInfo.LineNumber;
                else
                    return 0;
            }
        }

        public int LinePosition
        {
            get
            {
                IXmlLineInfo xmlLineInfo = this.node as IXmlLineInfo;
                if (xmlLineInfo != null)
                    return xmlLineInfo.LinePosition;
                else
                    return 0;
            }
        }

        public XmlNodeContext(XmlNode node)
        {
            this.node = node;
        }
    }
}

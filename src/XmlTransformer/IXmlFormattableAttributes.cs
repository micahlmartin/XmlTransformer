using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    internal interface IXmlFormattableAttributes
    {
        string AttributeIndent { get; }

        void FormatAttributes(XmlFormatter formatter);
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    internal class InsertBefore : InsertBase
    {
        protected override void Apply()
        {
            this.SiblingElement.ParentNode.InsertBefore(this.TransformNode, (XmlNode)this.SiblingElement);
            this.Log.LogMessage(MessageType.Verbose, string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Inserted '{0}' elemen", new object[1]
      {
        (object) this.TransformNode.Name
      }), new object[0]);
        }
    }
}

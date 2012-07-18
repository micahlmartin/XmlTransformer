using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    internal class InsertAfter : InsertBase
    {
        protected override void Apply()
        {
            this.SiblingElement.ParentNode.InsertAfter(this.TransformNode, (XmlNode)this.SiblingElement);
            this.Log.LogMessage(MessageType.Verbose, string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Inserted '{0}' element", new object[1]
      {
        (object) this.TransformNode.Name
      }), new object[0]);
        }
    }
}

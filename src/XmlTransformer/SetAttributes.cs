using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    public class SetAttributes : AttributeTransform
    {
        protected override void Apply()
        {
            foreach (XmlAttribute xmlAttribute1 in this.TransformAttributes)
            {
                XmlAttribute xmlAttribute2 = this.TargetNode.Attributes.GetNamedItem(xmlAttribute1.Name) as XmlAttribute;
                if (xmlAttribute2 != null)
                    xmlAttribute2.Value = xmlAttribute1.Value;
                else
                    this.TargetNode.Attributes.Append((XmlAttribute)xmlAttribute1.Clone());
                this.Log.LogMessage(MessageType.Verbose, "Set {0} attributes", new object[1]
        {
          (object) xmlAttribute1.Name
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
    }
}

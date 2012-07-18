using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    public class RemoveAttributes : AttributeTransform
    {
        protected override void Apply()
        {
            foreach (XmlAttribute node in this.TargetAttributes)
            {
                this.TargetNode.Attributes.Remove(node);
                this.Log.LogMessage(MessageType.Verbose, "Removed {0} attributes", new object[1]
        {
          (object) node.Name
        });
            }
            if (this.TargetAttributes.Count > 0)
                this.Log.LogMessage(MessageType.Verbose, "Removed {0} attributes", new object[1]
        {
          (object) this.TargetAttributes.Count
        });
            else
                this.Log.LogWarning(this.TargetNode, "No attributes found to remove", new object[0]);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    internal class Remove : Transform
    {
        protected override void Apply()
        {
            CommonErrors.WarnIfMultipleTargets(this.Log, this.TransformNameShort, this.TargetNodes, this.ApplyTransformToAllTargetNodes);
            this.RemoveNode();
        }

        protected void RemoveNode()
        {
            CommonErrors.ExpectNoArguments(this.Log, this.TransformNameShort, this.ArgumentString);
            this.TargetNode.ParentNode.RemoveChild(this.TargetNode);
            this.Log.LogMessage(MessageType.Verbose, "Removed {0} attributes", new object[1]
      {
        (object) this.TargetNode.Name
      });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    internal class Replace : Transform
    {
        protected override void Apply()
        {
            CommonErrors.ExpectNoArguments(this.Log, this.TransformNameShort, this.ArgumentString);
            CommonErrors.WarnIfMultipleTargets(this.Log, this.TransformNameShort, this.TargetNodes, this.ApplyTransformToAllTargetNodes);
            this.TargetNode.ParentNode.ReplaceChild(this.TransformNode, this.TargetNode);
            this.Log.LogMessage(MessageType.Verbose, "Replaced '{0}' element", new object[1]
      {
        (object) this.TargetNode.Name
      });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    internal class Insert : Transform
    {
        public Insert()
            : base(TransformFlags.UseParentAsTargetNode, MissingTargetMessage.Error)
        {
        }

        protected override void Apply()
        {
            CommonErrors.ExpectNoArguments(this.Log, this.TransformNameShort, this.ArgumentString);
            this.TargetNode.AppendChild(this.TransformNode);
            this.Log.LogMessage(MessageType.Verbose, "Inserted '{0}' element", new object[1]
      {
        (object) this.TransformNode.Name
      });
        }
    }
}

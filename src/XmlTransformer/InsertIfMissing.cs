using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    internal class InsertIfMissing : Insert
    {
        protected override void Apply()
        {
            CommonErrors.ExpectNoArguments(this.Log, this.TransformNameShort, this.ArgumentString);
            if (this.TargetChildNodes != null && this.TargetChildNodes.Count != 0)
                return;
            this.TargetNode.AppendChild(this.TransformNode);
            this.Log.LogMessage(MessageType.Verbose, "Inserted '{0}' element", new object[1]
      {
        (object) this.TransformNode.Name
      });
        }
    }
}

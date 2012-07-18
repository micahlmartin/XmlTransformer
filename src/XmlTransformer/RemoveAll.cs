using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    internal class RemoveAll : Remove
    {
        public RemoveAll()
        {
            this.ApplyTransformToAllTargetNodes = true;
        }

        protected override void Apply()
        {
            this.RemoveNode();
        }
    }
}

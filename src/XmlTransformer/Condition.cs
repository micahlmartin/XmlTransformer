using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    public sealed class Condition : Locator
    {
        protected override string ConstructPredicate()
        {
            this.EnsureArguments(1, 1);
            return this.Arguments[0];
        }
    }
}

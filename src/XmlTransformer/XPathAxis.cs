using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    public enum XPathAxis
    {
        Child,
        Descendant,
        Parent,
        Ancestor,
        FollowingSibling,
        PrecedingSibling,
        Following,
        Preceding,
        Self,
        DescendantOrSelf,
        AncestorOrSelf,
    }
}

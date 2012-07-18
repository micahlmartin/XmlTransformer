using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    public sealed class Match : Locator
    {
        protected override string ConstructPredicate()
        {
            this.EnsureArguments(1);
            string str1 = (string)null;
            foreach (string name in (IEnumerable<string>)this.Arguments)
            {
                XmlAttribute xmlAttribute = this.CurrentElement.Attributes.GetNamedItem(name) as XmlAttribute;
                if (xmlAttribute != null)
                {
                    string str2 = string.Format((IFormatProvider)CultureInfo.InvariantCulture, "@{0}='{1}'", new object[2]
          {
            (object) xmlAttribute.Name,
            (object) xmlAttribute.Value
          });
                    str1 = str1 != null ? str1 + " and " + str2 : str2;
                }
                else
                    throw new XmlTransformationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "No attribute '{0}' exists for the Match Locator", new object[1]
          {
            (object) name
          }));
            }
            return str1;
        }
    }
}

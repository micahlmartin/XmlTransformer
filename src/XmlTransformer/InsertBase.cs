using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    internal abstract class InsertBase : Transform
    {
        private XmlElement siblingElement;

        protected XmlElement SiblingElement
        {
            get
            {
                if (this.siblingElement == null)
                {
                    if (this.Arguments == null || this.Arguments.Count == 0)
                        throw new XmlTransformationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "{0} requires an XPath argument", new object[1]
            {
              (object) this.GetType().Name
            }));
                    else if (this.Arguments.Count > 1)
                    {
                        throw new XmlTransformationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Too many arguments to {0}", new object[1]
            {
              (object) this.GetType().Name
            }));
                    }
                    else
                    {
                        string xpath = this.Arguments[0];
                        XmlNodeList xmlNodeList = this.TargetNode.SelectNodes(xpath);
                        if (xmlNodeList.Count == 0)
                        {
                            throw new XmlTransformationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "'{0}' does not evaluate to an element", new object[1]
              {
                (object) xpath
              }));
                        }
                        else
                        {
                            this.siblingElement = xmlNodeList[0] as XmlElement;
                            if (this.siblingElement == null)
                                throw new XmlTransformationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "'{0}' does not evaluate to an element", new object[1]
                {
                  (object) xpath
                }));
                        }
                    }
                }
                return this.siblingElement;
            }
        }

        internal InsertBase()
            : base(TransformFlags.UseParentAsTargetNode, MissingTargetMessage.Error)
        {
        }
    }
}

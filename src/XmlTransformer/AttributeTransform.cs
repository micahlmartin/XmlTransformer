using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    public abstract class AttributeTransform : Transform
    {
        private XmlNode transformAttributeSource;
        private XmlNodeList transformAttributes;
        private XmlNode targetAttributeSource;
        private XmlNodeList targetAttributes;

        protected XmlNodeList TransformAttributes
        {
            get
            {
                if (this.transformAttributes == null || this.transformAttributeSource != this.TransformNode)
                {
                    this.transformAttributeSource = this.TransformNode;
                    this.transformAttributes = this.GetAttributesFrom(this.TransformNode);
                }
                return this.transformAttributes;
            }
        }

        protected XmlNodeList TargetAttributes
        {
            get
            {
                if (this.targetAttributes == null || this.targetAttributeSource != this.TargetNode)
                {
                    this.targetAttributeSource = this.TargetNode;
                    this.targetAttributes = this.GetAttributesFrom(this.TargetNode);
                }
                return this.targetAttributes;
            }
        }

        protected AttributeTransform()
            : base(TransformFlags.ApplyTransformToAllTargetNodes)
        {
        }

        private XmlNodeList GetAttributesFrom(XmlNode node)
        {
            if (this.Arguments == null || this.Arguments.Count == 0)
                return this.GetAttributesFrom(node, "*", false);
            if (this.Arguments.Count == 1)
                return this.GetAttributesFrom(node, this.Arguments[0], true);
            foreach (string str in (IEnumerable<string>)this.Arguments)
                this.GetAttributesFrom(node, str, true);
            return this.GetAttributesFrom(node, this.Arguments, false);
        }

        private XmlNodeList GetAttributesFrom(XmlNode node, string argument, bool warnIfEmpty)
        {
            return this.GetAttributesFrom(node, (IList<string>)new string[1]
      {
        argument
      }, (warnIfEmpty ? 1 : 0) != 0);
        }

        private XmlNodeList GetAttributesFrom(XmlNode node, IList<string> arguments, bool warnIfEmpty)
        {
            string[] array = new string[arguments.Count];
            arguments.CopyTo(array, 0);
            string xpath = "@" + string.Join("|@", array);
            XmlNodeList xmlNodeList = node.SelectNodes(xpath);
            if (xmlNodeList.Count == 0 && warnIfEmpty && arguments.Count == 1)
                this.Log.LogWarning("Argument '{0}' did not match any attributes", new object[1]
        {
          (object) arguments[0]
        });
            return xmlNodeList;
        }
    }
}

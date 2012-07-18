using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    public abstract class Locator
    {
        private string argumentString;
        private IList<string> arguments;
        private string parentPath;
        private XmlElementContext context;
        private XmlTransformationLogger logger;

        protected virtual string ParentPath
        {
            get
            {
                return this.parentPath;
            }
        }

        protected XmlNode CurrentElement
        {
            get
            {
                return (XmlNode)this.context.Element;
            }
        }

        protected virtual string NextStepNodeTest
        {
            get
            {
                if (!string.IsNullOrEmpty(this.CurrentElement.NamespaceURI) && string.IsNullOrEmpty(this.CurrentElement.Prefix))
                    return "_defaultNamespace:" + this.CurrentElement.LocalName;
                else
                    return this.CurrentElement.Name;
            }
        }

        protected virtual XPathAxis NextStepAxis
        {
            get
            {
                return XPathAxis.Child;
            }
        }

        protected XmlTransformationLogger Log
        {
            get
            {
                if (this.logger == null)
                {
                    this.logger = this.context.GetService<XmlTransformationLogger>();
                    if (this.logger != null)
                        this.logger.CurrentReferenceNode = (XmlNode)this.context.LocatorAttribute;
                }
                return this.logger;
            }
        }

        protected string ArgumentString
        {
            get
            {
                return this.argumentString;
            }
        }

        protected IList<string> Arguments
        {
            get
            {
                if (this.arguments == null && this.argumentString != null)
                    this.arguments = XmlArgumentUtility.SplitArguments(this.argumentString);
                return this.arguments;
            }
        }

        protected virtual string ConstructPath()
        {
            return this.AppendStep(this.ParentPath, this.NextStepAxis, this.NextStepNodeTest, this.ConstructPredicate());
        }

        protected string AppendStep(string basePath, string stepNodeTest)
        {
            return this.AppendStep(basePath, XPathAxis.Child, stepNodeTest, string.Empty);
        }

        protected string AppendStep(string basePath, XPathAxis stepAxis, string stepNodeTest)
        {
            return this.AppendStep(basePath, stepAxis, stepNodeTest, string.Empty);
        }

        protected string AppendStep(string basePath, string stepNodeTest, string predicate)
        {
            return this.AppendStep(basePath, XPathAxis.Child, stepNodeTest, predicate);
        }

        protected string AppendStep(string basePath, XPathAxis stepAxis, string stepNodeTest, string predicate)
        {
            return this.EnsureTrailingSlash(basePath) + this.GetAxisString(stepAxis) + stepNodeTest + this.EnsureBracketedPredicate(predicate);
        }

        protected virtual string ConstructPredicate()
        {
            return string.Empty;
        }

        protected void EnsureArguments()
        {
            this.EnsureArguments(1);
        }

        protected void EnsureArguments(int min)
        {
            if (this.Arguments != null && this.Arguments.Count >= min)
                return;
            throw new XmlTransformationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "{0} requires at least {1} arguments", new object[2] { (object) this.GetType().Name, (object) min }));
        }

        protected void EnsureArguments(int min, int max)
        {
            if (min == max && (this.Arguments == null || this.Arguments.Count != min))
            {
                throw new XmlTransformationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "{0} requires exactly {1} arguments", new object[2] { (object) this.GetType().Name, (object) min }));
            }
            else
            {
                this.EnsureArguments(min);
                if (this.Arguments.Count <= max)
                    return;
                throw new XmlTransformationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Too many arguments for {0}", new object[1]
        {
          (object) this.GetType().Name
        }));
            }
        }

        internal string ConstructPath(string parentPath, XmlElementContext context, string argumentString)
        {
            string str = string.Empty;
            if (this.parentPath == null && this.context == null)
            {
                if (this.argumentString == null)
                {
                    try
                    {
                        this.parentPath = parentPath;
                        this.context = context;
                        this.argumentString = argumentString;
                        str = this.ConstructPath();
                    }
                    finally
                    {
                        this.parentPath = (string)null;
                        this.context = (XmlElementContext)null;
                        this.argumentString = (string)null;
                        this.arguments = (IList<string>)null;
                        this.ReleaseLogger();
                    }
                }
            }
            return str;
        }

        internal string ConstructParentPath(string parentPath, XmlElementContext context, string argumentString)
        {
            string str = string.Empty;
            if (this.parentPath == null && this.context == null)
            {
                if (this.argumentString == null)
                {
                    try
                    {
                        this.parentPath = parentPath;
                        this.context = context;
                        this.argumentString = argumentString;
                        str = this.ParentPath;
                    }
                    finally
                    {
                        this.parentPath = (string)null;
                        this.context = (XmlElementContext)null;
                        this.argumentString = (string)null;
                        this.arguments = (IList<string>)null;
                        this.ReleaseLogger();
                    }
                }
            }
            return str;
        }

        private void ReleaseLogger()
        {
            if (this.logger == null)
                return;
            this.logger.CurrentReferenceNode = (XmlNode)null;
            this.logger = (XmlTransformationLogger)null;
        }

        private string GetAxisString(XPathAxis stepAxis)
        {
            switch (stepAxis)
            {
                case XPathAxis.Child:
                    return string.Empty;
                case XPathAxis.Descendant:
                    return "descendant::";
                case XPathAxis.Parent:
                    return "parent::";
                case XPathAxis.Ancestor:
                    return "ancestor::";
                case XPathAxis.FollowingSibling:
                    return "following-sibling::";
                case XPathAxis.PrecedingSibling:
                    return "preceding-sibling::";
                case XPathAxis.Following:
                    return "following::";
                case XPathAxis.Preceding:
                    return "preceding::";
                case XPathAxis.Self:
                    return "self::";
                case XPathAxis.DescendantOrSelf:
                    return "/";
                case XPathAxis.AncestorOrSelf:
                    return "ancestor-or-self::";
                default:
                    return string.Empty;
            }
        }

        private string EnsureTrailingSlash(string basePath)
        {
            if (!basePath.EndsWith("/", StringComparison.Ordinal))
                basePath = basePath + "/";
            return basePath;
        }

        private string EnsureBracketedPredicate(string predicate)
        {
            if (string.IsNullOrEmpty(predicate))
                return string.Empty;
            if (!predicate.StartsWith("[", StringComparison.Ordinal))
                predicate = "[" + predicate;
            if (!predicate.EndsWith("]", StringComparison.Ordinal))
                predicate = predicate + "]";
            return predicate;
        }
    }
}

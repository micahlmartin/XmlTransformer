using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace XmlTransformer
{
    internal class XmlElementContext : XmlNodeContext
    {
        private XmlElementContext parentContext;
        private string xpath;
        private string parentXPath;
        private XmlDocument xmlTargetDoc;
        private IServiceProvider serviceProvider;
        private XmlNode transformNodes;
        private XmlNodeList targetNodes;
        private XmlNodeList targetParents;
        private XmlAttribute transformAttribute;
        private XmlAttribute locatorAttribute;
        private XmlNamespaceManager namespaceManager;
        private static Regex nameAndArgumentsRegex;

        public XmlElement Element
        {
            get
            {
                return this.Node as XmlElement;
            }
        }

        public string XPath
        {
            get
            {
                if (this.xpath == null)
                    this.xpath = this.ConstructXPath();
                return this.xpath;
            }
        }

        public string ParentXPath
        {
            get
            {
                if (this.parentXPath == null)
                    this.parentXPath = this.ConstructParentXPath();
                return this.parentXPath;
            }
        }

        public int TransformLineNumber
        {
            get
            {
                IXmlLineInfo xmlLineInfo = this.transformAttribute as IXmlLineInfo;
                if (xmlLineInfo != null)
                    return xmlLineInfo.LineNumber;
                else
                    return this.LineNumber;
            }
        }

        public int TransformLinePosition
        {
            get
            {
                IXmlLineInfo xmlLineInfo = this.transformAttribute as IXmlLineInfo;
                if (xmlLineInfo != null)
                    return xmlLineInfo.LinePosition;
                else
                    return this.LinePosition;
            }
        }

        public XmlAttribute TransformAttribute
        {
            get
            {
                return this.transformAttribute;
            }
        }

        public XmlAttribute LocatorAttribute
        {
            get
            {
                return this.locatorAttribute;
            }
        }

        internal XmlNode TransformNode
        {
            get
            {
                if (this.transformNodes == null)
                    this.transformNodes = this.CreateCloneInTargetDocument((XmlNode)this.Element);
                return this.transformNodes;
            }
        }

        internal XmlNodeList TargetNodes
        {
            get
            {
                if (this.targetNodes == null)
                    this.targetNodes = this.GetTargetNodes(this.XPath);
                return this.targetNodes;
            }
        }

        internal XmlNodeList TargetParents
        {
            get
            {
                if (this.targetParents == null && this.parentContext != null)
                    this.targetParents = this.GetTargetNodes(this.ParentXPath);
                return this.targetParents;
            }
        }

        private XmlDocument TargetDocument
        {
            get
            {
                return this.xmlTargetDoc;
            }
        }

        private Regex NameAndArgumentsRegex
        {
            get
            {
                if (XmlElementContext.nameAndArgumentsRegex == null)
                    XmlElementContext.nameAndArgumentsRegex = new Regex("\\A\\s*(?<name>\\w+)(\\s*\\((?<arguments>.*)\\))?\\s*\\Z", RegexOptions.Compiled | RegexOptions.Singleline);
                return XmlElementContext.nameAndArgumentsRegex;
            }
        }

        static XmlElementContext()
        {
        }

        public XmlElementContext(XmlElementContext parent, XmlElement element, XmlDocument xmlTargetDoc, IServiceProvider serviceProvider)
            : base((XmlNode)element)
        {
            this.parentContext = parent;
            this.xmlTargetDoc = xmlTargetDoc;
            this.serviceProvider = serviceProvider;
        }

        public T GetService<T>() where T : class
        {
            if (this.serviceProvider != null)
                return this.serviceProvider.GetService(typeof(T)) as T;
            else
                return default(T);
        }

        public Transform ConstructTransform(out string argumentString)
        {
            try
            {
                return this.CreateObjectFromAttribute<Transform>(out argumentString, out this.transformAttribute);
            }
            catch (Exception ex)
            {
                throw this.WrapException(ex);
            }
        }

        private string ConstructXPath()
        {
            try
            {
                string parentPath = this.parentContext == null ? string.Empty : this.parentContext.XPath;
                string argumentString;
                return this.CreateLocator(out argumentString).ConstructPath(parentPath, this, argumentString);
            }
            catch (Exception ex)
            {
                throw this.WrapException(ex);
            }
        }

        private string ConstructParentXPath()
        {
            try
            {
                string parentPath = this.parentContext == null ? string.Empty : this.parentContext.XPath;
                string argumentString;
                return this.CreateLocator(out argumentString).ConstructParentPath(parentPath, this, argumentString);
            }
            catch (Exception ex)
            {
                throw this.WrapException(ex);
            }
        }

        private Locator CreateLocator(out string argumentString)
        {
            Locator locator = this.CreateObjectFromAttribute<Locator>(out argumentString, out this.locatorAttribute);
            if (locator == null)
            {
                argumentString = (string)null;
                locator = (Locator)DefaultLocator.Instance;
            }
            return locator;
        }

        private XmlNode CreateCloneInTargetDocument(XmlNode sourceNode)
        {
            XmlFileInfoDocument fileInfoDocument = this.TargetDocument as XmlFileInfoDocument;
            XmlNode node = fileInfoDocument == null ? this.TargetDocument.ReadNode((XmlReader)new XmlTextReader((TextReader)new StringReader(sourceNode.OuterXml))) : fileInfoDocument.CloneNodeFromOtherDocument(sourceNode);
            this.ScrubTransformAttributesAndNamespaces(node);
            return node;
        }

        private void ScrubTransformAttributesAndNamespaces(XmlNode node)
        {
            if (node.Attributes != null)
            {
                List<XmlAttribute> list = new List<XmlAttribute>();
                foreach (XmlAttribute xmlAttribute in (XmlNamedNodeMap)node.Attributes)
                {
                    if (xmlAttribute.NamespaceURI == XmlTransformation.TransformNamespace)
                        list.Add(xmlAttribute);
                    else if (xmlAttribute.Prefix.Equals("xmlns") || xmlAttribute.Name.Equals("xmlns"))
                        list.Add(xmlAttribute);
                    else
                        xmlAttribute.Prefix = string.Empty;
                }
                foreach (XmlAttribute node1 in list)
                    node.Attributes.Remove(node1);
            }
            foreach (XmlNode node1 in node.ChildNodes)
                this.ScrubTransformAttributesAndNamespaces(node1);
        }

        private XmlNodeList GetTargetNodes(string xpath)
        {
            this.GetNamespaceManager();
            return this.TargetDocument.SelectNodes(xpath, this.GetNamespaceManager());
        }

        private Exception WrapException(Exception ex)
        {
            return XmlNodeException.Wrap(ex, (XmlNode)this.Element);
        }

        private Exception WrapException(Exception ex, XmlNode node)
        {
            return XmlNodeException.Wrap(ex, node);
        }

        private XmlNamespaceManager GetNamespaceManager()
        {
            if (this.namespaceManager == null)
            {
                XmlNodeList xmlNodeList = this.Element.SelectNodes("namespace::*");
                if (xmlNodeList.Count > 0)
                {
                    this.namespaceManager = new XmlNamespaceManager(this.Element.OwnerDocument.NameTable);
                    foreach (XmlAttribute xmlAttribute in xmlNodeList)
                    {
                        string str = string.Empty;
                        int num = xmlAttribute.Name.IndexOf(':');
                        this.namespaceManager.AddNamespace(num < 0 ? "_defaultNamespace" : xmlAttribute.Name.Substring(num + 1), xmlAttribute.Value);
                    }
                }
                else
                    this.namespaceManager = new XmlNamespaceManager(this.GetParentNameTable());
            }
            return this.namespaceManager;
        }

        private XmlNameTable GetParentNameTable()
        {
            if (this.parentContext == null)
                return this.Element.OwnerDocument.NameTable;
            else
                return this.parentContext.GetNamespaceManager().NameTable;
        }

        private string ParseNameAndArguments(string name, out string arguments)
        {
            arguments = (string)null;
            System.Text.RegularExpressions.Match match = this.NameAndArgumentsRegex.Match(name);
            if (!match.Success)
                throw new XmlTransformationException("Transform and Locator attributes must contain only a type name, or a type name followed by a list of attributes in parentheses.");
            if (match.Groups["arguments"].Success)
            {
                CaptureCollection captures = match.Groups["arguments"].Captures;
                if (captures.Count == 1 && !string.IsNullOrEmpty(captures[0].Value))
                    arguments = captures[0].Value;
            }
            return match.Groups["name"].Captures[0].Value;
        }

        private ObjectType CreateObjectFromAttribute<ObjectType>(out string argumentString, out XmlAttribute objectAttribute) where ObjectType : class
        {
            objectAttribute = this.Element.Attributes.GetNamedItem(typeof(ObjectType).Name, XmlTransformation.TransformNamespace) as XmlAttribute;
            try
            {
                if (objectAttribute != null)
                {
                    string typeName = this.ParseNameAndArguments(objectAttribute.Value, out argumentString);
                    if (!string.IsNullOrEmpty(typeName))
                        return this.GetService<NamedTypeFactory>().Construct<ObjectType>(typeName);
                }
            }
            catch (Exception ex)
            {
                throw this.WrapException(ex, (XmlNode)objectAttribute);
            }
            argumentString = (string)null;
            return default(ObjectType);
        }

        internal bool HasTargetNode(out XmlElementContext failedContext, out bool existedInOriginal)
        {
            failedContext = (XmlElementContext)null;
            existedInOriginal = false;
            if (this.TargetNodes.Count != 0)
                return true;
            failedContext = this;
            while (failedContext.parentContext != null && failedContext.parentContext.TargetNodes.Count == 0)
                failedContext = failedContext.parentContext;
            existedInOriginal = this.ExistedInOriginal(failedContext.XPath);
            return false;
        }

        internal bool HasTargetParent(out XmlElementContext failedContext, out bool existedInOriginal)
        {
            failedContext = (XmlElementContext)null;
            existedInOriginal = false;
            if (this.TargetParents.Count != 0)
                return true;
            failedContext = this;
            while (failedContext.parentContext != null && !string.IsNullOrEmpty(failedContext.parentContext.ParentXPath) && failedContext.parentContext.TargetParents.Count == 0)
                failedContext = failedContext.parentContext;
            existedInOriginal = this.ExistedInOriginal(failedContext.XPath);
            return false;
        }

        private bool ExistedInOriginal(string xpath)
        {
            IXmlOriginalDocumentService service = this.GetService<IXmlOriginalDocumentService>();
            if (service != null)
            {
                XmlNodeList xmlNodeList = service.SelectNodes(xpath, this.GetNamespaceManager());
                if (xmlNodeList != null && xmlNodeList.Count > 0)
                    return true;
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    internal class XmlFormatter
    {
        private LinkedList<string> indents = new LinkedList<string>();
        private LinkedList<string> attributeIndents = new LinkedList<string>();
        private string currentIndent = string.Empty;
        private string defaultTab = "\t";
        private XmlFileInfoDocument document;
        private string originalFileName;
        private string currentAttributeIndent;
        private string oneTab;
        private XmlNode currentNode;
        private XmlNode previousNode;

        private XmlNode CurrentNode
        {
            get
            {
                return this.currentNode;
            }
            set
            {
                this.previousNode = this.currentNode;
                this.currentNode = value;
            }
        }

        private XmlNode PreviousNode
        {
            get
            {
                return this.previousNode;
            }
        }

        private string PreviousIndent
        {
            get
            {
                return this.indents.Last.Value;
            }
        }

        private string CurrentIndent
        {
            get
            {
                if (this.currentIndent == null)
                    this.currentIndent = this.ComputeCurrentIndent();
                return this.currentIndent;
            }
        }

        public string CurrentAttributeIndent
        {
            get
            {
                if (this.currentAttributeIndent == null)
                    this.currentAttributeIndent = this.ComputeCurrentAttributeIndent();
                return this.currentAttributeIndent;
            }
        }

        private string OneTab
        {
            get
            {
                if (this.oneTab == null)
                    this.oneTab = this.ComputeOneTab();
                return this.oneTab;
            }
        }

        public string DefaultTab
        {
            get
            {
                return this.defaultTab;
            }
            set
            {
                this.defaultTab = value;
            }
        }

        private XmlFormatter(XmlFileInfoDocument document)
        {
            this.document = document;
            this.originalFileName = document.FileName;
        }

        public static void Format(XmlDocument document)
        {
            XmlFileInfoDocument document1 = document as XmlFileInfoDocument;
            if (document1 == null)
                return;
            new XmlFormatter(document1).FormatLoop((XmlNode)document1);
        }

        private void FormatLoop(XmlNode parentNode)
        {
            for (int index = 0; index < parentNode.ChildNodes.Count; ++index)
            {
                XmlNode node = parentNode.ChildNodes[index];
                this.CurrentNode = node;
                switch (node.NodeType)
                {
                    case XmlNodeType.Element:
                        index += this.HandleElement(node);
                        break;
                    case XmlNodeType.Entity:
                    case XmlNodeType.Comment:
                        index += this.EnsureNodeIndent(node, false);
                        break;
                    case XmlNodeType.Whitespace:
                        index += this.HandleWhiteSpace(node);
                        break;
                }
            }
        }

        private void FormatAttributes(XmlNode node)
        {
            IXmlFormattableAttributes formattableAttributes = node as IXmlFormattableAttributes;
            if (formattableAttributes == null)
                return;
            formattableAttributes.FormatAttributes(this);
        }

        private int HandleElement(XmlNode node)
        {
            int num = this.HandleStartElement(node);
            this.ReorderNewItemsAtEnd(node);
            this.FormatLoop(node);
            this.CurrentNode = node;
            return num + this.HandleEndElement(node);
        }

        private void ReorderNewItemsAtEnd(XmlNode node)
        {
            if (this.IsNewNode(node))
                return;
            XmlNode node1 = node.LastChild;
            if (node1 == null || node1.NodeType == XmlNodeType.Whitespace)
                return;
            XmlNode xmlNode = (XmlNode)null;
            for (; node1 != null; node1 = node1.PreviousSibling)
            {
                switch (node1.NodeType)
                {
                    case XmlNodeType.Element:
                        if (this.IsNewNode(node1))
                            continue;
                        else
                            goto label_8;
                    case XmlNodeType.Whitespace:
                        xmlNode = node1;
                        goto label_8;
                    default:
                        goto label_8;
                }
            }
        label_8:
            if (xmlNode == null)
                return;
            node.RemoveChild(xmlNode);
            node.AppendChild(xmlNode);
        }

        private int HandleStartElement(XmlNode node)
        {
            int num = this.EnsureNodeIndent(node, false);
            this.FormatAttributes(node);
            this.PushIndent();
            return num;
        }

        private int HandleEndElement(XmlNode node)
        {
            int num = 0;
            this.PopIndent();
            if (!((XmlElement)node).IsEmpty)
                num = this.EnsureNodeIndent(node, true);
            return num;
        }

        private int HandleWhiteSpace(XmlNode node)
        {
            int num = 0;
            if (this.IsWhiteSpace(this.PreviousNode))
            {
                XmlNode oldChild = this.PreviousNode;
                if (this.FindLastNewLine(node.OuterXml) < 0 && this.FindLastNewLine(this.PreviousNode.OuterXml) >= 0)
                    oldChild = node;
                oldChild.ParentNode.RemoveChild(oldChild);
                num = -1;
            }
            string indentFromWhiteSpace = this.GetIndentFromWhiteSpace(node);
            if (indentFromWhiteSpace != null)
                this.SetIndent(indentFromWhiteSpace);
            return num;
        }

        private int EnsureNodeIndent(XmlNode node, bool indentBeforeEnd)
        {
            int num = 0;
            if (this.NeedsIndent(node, this.PreviousNode))
            {
                if (indentBeforeEnd)
                {
                    this.InsertIndentBeforeEnd(node);
                }
                else
                {
                    this.InsertIndentBefore(node);
                    num = 1;
                }
            }
            return num;
        }

        private string GetIndentFromWhiteSpace(XmlNode node)
        {
            string outerXml = node.OuterXml;
            int lastNewLine = this.FindLastNewLine(outerXml);
            if (lastNewLine >= 0)
                return outerXml.Substring(lastNewLine);
            else
                return (string)null;
        }

        private int FindLastNewLine(string whitespace)
        {
            for (int index = whitespace.Length - 1; index >= 0; --index)
            {
                switch (whitespace[index])
                {
                    case '\t':
                    case ' ':
                        continue;
                    case '\n':
                        if (index > 0 && (int)whitespace[index - 1] == 13)
                            return index - 1;
                        else
                            return index;
                    case '\r':
                        return index;
                    default:
                        return -1;
                }
            }
            return -1;
        }

        private void SetIndent(string indent)
        {
            if (this.currentIndent != null && this.currentIndent.Equals(indent))
                return;
            this.currentIndent = indent;
            this.oneTab = (string)null;
            this.currentAttributeIndent = (string)null;
        }

        private void PushIndent()
        {
            this.indents.AddLast(new LinkedListNode<string>(this.CurrentIndent));
            this.currentIndent = (string)null;
            this.attributeIndents.AddLast(new LinkedListNode<string>(this.currentAttributeIndent));
            this.currentAttributeIndent = (string)null;
        }

        private void PopIndent()
        {
            if (this.indents.Count <= 0)
                throw new InvalidOperationException();
            this.currentIndent = this.indents.Last.Value;
            this.indents.RemoveLast();
            this.currentAttributeIndent = this.attributeIndents.Last.Value;
            this.attributeIndents.RemoveLast();
        }

        private bool NeedsIndent(XmlNode node, XmlNode previousNode)
        {
            if (this.IsWhiteSpace(previousNode) || this.IsText(previousNode))
                return false;
            if (!this.IsNewNode(node))
                return this.IsNewNode(previousNode);
            else
                return true;
        }

        private bool IsWhiteSpace(XmlNode node)
        {
            if (node != null)
                return node.NodeType == XmlNodeType.Whitespace;
            else
                return false;
        }

        public bool IsText(XmlNode node)
        {
            if (node != null)
                return node.NodeType == XmlNodeType.Text;
            else
                return false;
        }

        private bool IsNewNode(XmlNode node)
        {
            if (node != null)
                return this.document.IsNewNode(node);
            else
                return false;
        }

        private void InsertIndentBefore(XmlNode node)
        {
            node.ParentNode.InsertBefore((XmlNode)this.document.CreateWhitespace(this.CurrentIndent), node);
        }

        private void InsertIndentBeforeEnd(XmlNode node)
        {
            node.AppendChild((XmlNode)this.document.CreateWhitespace(this.CurrentIndent));
        }

        private string ComputeCurrentIndent()
        {
            return this.LookAheadForIndent() ?? this.PreviousIndent + this.OneTab;
        }

        private string LookAheadForIndent()
        {
            if (this.currentNode.ParentNode == null)
                return (string)null;
            foreach (XmlNode node in this.currentNode.ParentNode.ChildNodes)
            {
                if (this.IsWhiteSpace(node) && node.NextSibling != null)
                {
                    string outerXml = node.OuterXml;
                    int lastNewLine = this.FindLastNewLine(outerXml);
                    if (lastNewLine >= 0)
                        return outerXml.Substring(lastNewLine);
                }
            }
            return (string)null;
        }

        private string ComputeOneTab()
        {
            if (this.indents.Count < 0)
                return this.DefaultTab;
            LinkedListNode<string> linkedListNode = this.indents.Last;
            for (LinkedListNode<string> previous = linkedListNode.Previous; previous != null; previous = linkedListNode.Previous)
            {
                if (linkedListNode.Value.StartsWith(previous.Value, StringComparison.Ordinal))
                    return linkedListNode.Value.Substring(previous.Value.Length);
                linkedListNode = previous;
            }
            return this.ConvertIndentToTab(linkedListNode.Value);
        }

        private string ConvertIndentToTab(string indent)
        {
            for (int index = 0; index < indent.Length - 1; ++index)
            {
                switch (indent[index])
                {
                    case '\n':
                    case '\r':
                        goto case '\n';
                    default:
                        return indent.Substring(index + 1);
                }
            }
            return this.DefaultTab;
        }

        private string ComputeCurrentAttributeIndent()
        {
            return this.LookForSiblingIndent(this.CurrentNode) ?? this.CurrentIndent + this.OneTab;
        }

        private string LookForSiblingIndent(XmlNode currentNode)
        {
            bool flag = true;
            string str = (string)null;
            foreach (XmlNode xmlNode in currentNode.ParentNode.ChildNodes)
            {
                if (xmlNode == currentNode)
                {
                    flag = false;
                }
                else
                {
                    IXmlFormattableAttributes formattableAttributes = xmlNode as IXmlFormattableAttributes;
                    if (formattableAttributes != null)
                        str = formattableAttributes.AttributeIndent;
                }
                if (!flag && str != null)
                    return str;
            }
            return (string)null;
        }
    }
}

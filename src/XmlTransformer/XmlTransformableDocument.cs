using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    public class XmlTransformableDocument : XmlFileInfoDocument, IXmlOriginalDocumentService
    {
        private XmlDocument xmlOriginal;

        public bool IsChanged
        {
            get
            {
                if (this.xmlOriginal == null)
                    return false;
                else
                    return !this.IsXmlEqual(this.xmlOriginal, (XmlDocument)this);
            }
        }

        internal void OnBeforeChange()
        {
            if (this.xmlOriginal != null)
                return;
            this.CloneOriginalDocument();
        }

        internal void OnAfterChange()
        {
        }

        private void CloneOriginalDocument()
        {
            this.xmlOriginal = (XmlDocument)this.Clone();
        }

        private bool IsXmlEqual(XmlDocument xmlOriginal, XmlDocument xmlTransformed)
        {
            return false;
        }

        XmlNodeList IXmlOriginalDocumentService.SelectNodes(string xpath, XmlNamespaceManager nsmgr)
        {
            if (this.xmlOriginal != null)
                return this.xmlOriginal.SelectNodes(xpath, nsmgr);
            else
                return (XmlNodeList)null;
        }
    }
}

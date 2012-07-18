using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    public class XmlTransformationLogger
    {
        private bool hasLoggedErrors;
        private IXmlTransformationLogger externalLogger;
        private XmlNode currentReferenceNode;
        private bool fSupressWarnings;

        internal bool HasLoggedErrors
        {
            get
            {
                return this.hasLoggedErrors;
            }
            set
            {
                this.hasLoggedErrors = false;
            }
        }

        internal XmlNode CurrentReferenceNode
        {
            get
            {
                return this.currentReferenceNode;
            }
            set
            {
                this.currentReferenceNode = value;
            }
        }

        public bool SupressWarnings
        {
            get
            {
                return this.fSupressWarnings;
            }
            set
            {
                this.fSupressWarnings = value;
            }
        }

        internal XmlTransformationLogger(IXmlTransformationLogger logger)
        {
            this.externalLogger = logger;
        }

        internal void LogErrorFromException(Exception ex)
        {
            this.hasLoggedErrors = true;
            if (this.externalLogger == null)
                throw ex;
            XmlNodeException xmlNodeException = ex as XmlNodeException;
            if (xmlNodeException != null && xmlNodeException.HasErrorInfo)
                this.externalLogger.LogErrorFromException((Exception)xmlNodeException, this.ConvertUriToFileName(xmlNodeException.FileName), xmlNodeException.LineNumber, xmlNodeException.LinePosition);
            else
                this.externalLogger.LogErrorFromException(ex);
        }

        public void LogMessage(string message, params object[] messageArgs)
        {
            if (this.externalLogger == null)
                return;
            this.externalLogger.LogMessage(message, messageArgs);
        }

        public void LogMessage(MessageType type, string message, params object[] messageArgs)
        {
            if (this.externalLogger == null)
                return;
            this.externalLogger.LogMessage(type, message, messageArgs);
        }

        public void LogWarning(string message, params object[] messageArgs)
        {
            if (this.SupressWarnings)
                this.LogMessage(message, messageArgs);
            else if (this.CurrentReferenceNode != null)
            {
                this.LogWarning(this.CurrentReferenceNode, message, messageArgs);
            }
            else
            {
                if (this.externalLogger == null)
                    return;
                this.externalLogger.LogWarning(message, messageArgs);
            }
        }

        public void LogWarning(XmlNode referenceNode, string message, params object[] messageArgs)
        {
            if (this.SupressWarnings)
            {
                this.LogMessage(message, messageArgs);
            }
            else
            {
                if (this.externalLogger == null)
                    return;
                string file = this.ConvertUriToFileName(referenceNode.OwnerDocument);
                IXmlLineInfo xmlLineInfo = referenceNode as IXmlLineInfo;
                if (xmlLineInfo != null)
                    this.externalLogger.LogWarning(file, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition, message, messageArgs);
                else
                    this.externalLogger.LogWarning(file, message, messageArgs);
            }
        }

        public void LogError(string message, params object[] messageArgs)
        {
            this.hasLoggedErrors = true;
            if (this.CurrentReferenceNode != null)
            {
                this.LogError(this.CurrentReferenceNode, message, messageArgs);
            }
            else
            {
                if (this.externalLogger == null)
                    throw new XmlTransformationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, message, messageArgs));
                this.externalLogger.LogError(message, messageArgs);
            }
        }

        public void LogError(XmlNode referenceNode, string message, params object[] messageArgs)
        {
            this.hasLoggedErrors = true;
            if (this.externalLogger == null)
                throw new XmlNodeException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, message, messageArgs), referenceNode);
            string file = this.ConvertUriToFileName(referenceNode.OwnerDocument);
            IXmlLineInfo xmlLineInfo = referenceNode as IXmlLineInfo;
            if (xmlLineInfo != null)
                this.externalLogger.LogError(file, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition, message, messageArgs);
            else
                this.externalLogger.LogError(file, message, messageArgs);
        }

        public void StartSection(string message, params object[] messageArgs)
        {
            if (this.externalLogger == null)
                return;
            this.externalLogger.StartSection(message, messageArgs);
        }

        public void StartSection(MessageType type, string message, params object[] messageArgs)
        {
            if (this.externalLogger == null)
                return;
            this.externalLogger.StartSection(type, message, messageArgs);
        }

        public void EndSection(string message, params object[] messageArgs)
        {
            if (this.externalLogger == null)
                return;
            this.externalLogger.EndSection(message, messageArgs);
        }

        public void EndSection(MessageType type, string message, params object[] messageArgs)
        {
            if (this.externalLogger == null)
                return;
            this.externalLogger.EndSection(type, message, messageArgs);
        }

        private string ConvertUriToFileName(XmlDocument xmlDocument)
        {
            XmlFileInfoDocument fileInfoDocument = xmlDocument as XmlFileInfoDocument;
            return this.ConvertUriToFileName(fileInfoDocument == null ? fileInfoDocument.BaseURI : fileInfoDocument.FileName);
        }

        private string ConvertUriToFileName(string fileName)
        {
            try
            {
                Uri uri = new Uri(fileName);
                if (uri.IsFile)
                {
                    if (string.IsNullOrEmpty(uri.Host))
                        fileName = uri.LocalPath;
                }
            }
            catch (UriFormatException ex)
            {
            }
            return fileName;
        }
    }
}

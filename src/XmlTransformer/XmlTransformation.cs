using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    public class XmlTransformation : IServiceProvider
    {
        internal static readonly string TransformNamespace = "http://schemas.microsoft.com/XML-Document-Transform";
        internal static readonly string SupressWarnings = "SupressWarnings";
        private ServiceContainer transformationServiceContainer = new ServiceContainer();
        private string transformFile;
        private XmlDocument xmlTransformation;
        private XmlDocument xmlTarget;
        private XmlTransformableDocument xmlTransformable;
        private XmlTransformationLogger logger;
        private NamedTypeFactory namedTypeFactory;
        private ServiceContainer documentServiceContainer;
        private bool hasTransformNamespace;

        static XmlTransformation()
        {
        }

        public XmlTransformation(string transformFile)
            : this(transformFile, true, (IXmlTransformationLogger)null)
        {
        }

        public XmlTransformation(string transform, IXmlTransformationLogger logger)
            : this(transform, true, logger)
        {
        }

        public XmlTransformation(string transform, bool isTransformAFile, IXmlTransformationLogger logger)
        {
            this.transformFile = transform;
            this.logger = new XmlTransformationLogger(logger);
            this.xmlTransformation = (XmlDocument)new XmlFileInfoDocument();
            if (isTransformAFile)
                this.xmlTransformation.Load(transform);
            else
                this.xmlTransformation.LoadXml(transform);
            this.InitializeTransformationServices();
            this.PreprocessTransformDocument();
        }

        private void InitializeTransformationServices()
        {
            this.namedTypeFactory = new NamedTypeFactory(this.transformFile);
            this.transformationServiceContainer.AddService(((object)this.namedTypeFactory).GetType(), (object)this.namedTypeFactory);
            this.transformationServiceContainer.AddService(this.logger.GetType(), (object)this.logger);
        }

        private void InitializeDocumentServices(XmlDocument document)
        {
            this.documentServiceContainer = new ServiceContainer();
            if (!(document is IXmlOriginalDocumentService))
                return;
            this.documentServiceContainer.AddService(typeof(IXmlOriginalDocumentService), (object)document);
        }

        private void ReleaseDocumentServices()
        {
            if (this.documentServiceContainer == null)
                return;
            this.documentServiceContainer.RemoveService(typeof(IXmlOriginalDocumentService));
            this.documentServiceContainer = (ServiceContainer)null;
        }

        private void PreprocessTransformDocument()
        {
            this.hasTransformNamespace = false;
            foreach (XmlNode xmlNode in this.xmlTransformation.SelectNodes("//namespace::*"))
            {
                if (xmlNode.Value.Equals(XmlTransformation.TransformNamespace, StringComparison.Ordinal))
                {
                    this.hasTransformNamespace = true;
                    break;
                }
            }
            if (!this.hasTransformNamespace)
                return;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager((XmlNameTable)new NameTable());
            nsmgr.AddNamespace("xdt", XmlTransformation.TransformNamespace);
            foreach (XmlNode xmlNode in this.xmlTransformation.SelectNodes("//xdt:*", nsmgr))
            {
                XmlElement element = xmlNode as XmlElement;
                if (element != null)
                {
                    XmlElementContext context = (XmlElementContext)null;
                    try
                    {
                        switch (element.LocalName)
                        {
                            case "Import":
                                context = this.CreateElementContext((XmlElementContext)null, element);
                                this.PreprocessImportElement(context);
                                continue;
                            default:
                                this.logger.LogWarning((XmlNode)element, "Unknown tag '{0}'", new object[1] { (object) element.Name });
                                continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Exception exception = ex;
                        if (context != null)
                            exception = this.WrapException(exception, (XmlNodeContext)context);
                        this.logger.LogErrorFromException(exception);
                        throw new XmlTransformationException("Fatal syntax error", exception);
                    }
                    finally
                    {
                    }
                }
            }
        }

        public void AddTransformationService(Type serviceType, object serviceInstance)
        {
            this.transformationServiceContainer.AddService(serviceType, serviceInstance);
        }

        public void RemoveTransformationService(Type serviceType)
        {
            this.transformationServiceContainer.RemoveService(serviceType);
        }

        public bool Apply(XmlDocument xmlTarget)
        {
            if (this.xmlTarget != null)
                return false;
            this.logger.HasLoggedErrors = false;
            this.xmlTarget = xmlTarget;
            this.xmlTransformable = xmlTarget as XmlTransformableDocument;
            try
            {
                if (this.hasTransformNamespace)
                {
                    this.InitializeDocumentServices(xmlTarget);
                    this.TransformLoop(this.xmlTransformation);
                }
                else
                    this.logger.LogMessage(MessageType.Normal, "The expected namespace {0} was not found in the transform file", new object[1]
          {
            (object) XmlTransformation.TransformNamespace
          });
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
            finally
            {
                this.ReleaseDocumentServices();
                this.xmlTarget = (XmlDocument)null;
                this.xmlTransformable = (XmlTransformableDocument)null;
            }
            return !this.logger.HasLoggedErrors;
        }

        private void TransformLoop(XmlDocument xmlSource)
        {
            this.TransformLoop(new XmlNodeContext((XmlNode)xmlSource));
        }

        private void TransformLoop(XmlNodeContext parentContext)
        {
            foreach (XmlNode xmlNode in parentContext.Node.ChildNodes)
            {
                XmlElement element = xmlNode as XmlElement;
                if (element != null)
                {
                    XmlElementContext elementContext = this.CreateElementContext(parentContext as XmlElementContext, element);
                    try
                    {
                        this.HandleElement(elementContext);
                    }
                    catch (Exception ex)
                    {
                        this.HandleException(ex, (XmlNodeContext)elementContext);
                    }
                }
            }
        }

        private XmlElementContext CreateElementContext(XmlElementContext parentContext, XmlElement element)
        {
            return new XmlElementContext(parentContext, element, this.xmlTarget, (IServiceProvider)this);
        }

        private void HandleException(Exception ex)
        {
            this.logger.LogErrorFromException(ex);
        }

        private void HandleException(Exception ex, XmlNodeContext context)
        {
            this.HandleException(this.WrapException(ex, context));
        }

        private Exception WrapException(Exception ex, XmlNodeContext context)
        {
            return XmlNodeException.Wrap(ex, context.Node);
        }

        private void HandleElement(XmlElementContext context)
        {
            string argumentString;
            Transform transform = context.ConstructTransform(out argumentString);
            if (transform != null)
            {
                bool supressWarnings = this.logger.SupressWarnings;
                XmlAttribute xmlAttribute = context.Element.Attributes.GetNamedItem(XmlTransformation.SupressWarnings, XmlTransformation.TransformNamespace) as XmlAttribute;
                if (xmlAttribute != null)
                    this.logger.SupressWarnings = Convert.ToBoolean(xmlAttribute.Value, (IFormatProvider)CultureInfo.InvariantCulture);
                try
                {
                    this.OnApplyingTransform();
                    transform.Execute(context, argumentString);
                    this.OnAppliedTransform();
                }
                catch (Exception ex)
                {
                    this.HandleException(ex, (XmlNodeContext)context);
                }
                finally
                {
                    this.logger.SupressWarnings = supressWarnings;
                }
            }
            this.TransformLoop((XmlNodeContext)context);
        }

        private void OnApplyingTransform()
        {
            if (this.xmlTransformable == null)
                return;
            this.xmlTransformable.OnBeforeChange();
        }

        private void OnAppliedTransform()
        {
            if (this.xmlTransformable == null)
                return;
            this.xmlTransformable.OnAfterChange();
        }

        private void PreprocessImportElement(XmlElementContext context)
        {
            string assemblyName = (string)null;
            string nameSpace = (string)null;
            string path = (string)null;
            foreach (XmlAttribute xmlAttribute in (XmlNamedNodeMap)context.Element.Attributes)
            {
                if (xmlAttribute.NamespaceURI.Length == 0)
                {
                    switch (xmlAttribute.Name)
                    {
                        case "assembly":
                            assemblyName = xmlAttribute.Value;
                            continue;
                        case "namespace":
                            nameSpace = xmlAttribute.Value;
                            continue;
                        case "path":
                            path = xmlAttribute.Value;
                            continue;
                    }
                }
                throw new XmlNodeException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Import tag does not support '{0}' attribute", new object[1] { (object) xmlAttribute.Name }), (XmlNode)xmlAttribute);
            }
            if (assemblyName != null && path != null)
                throw new XmlNodeException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Import tag cannot have both a 'path' and an 'assembly'", new object[0]), (XmlNode)context.Element);
            if (assemblyName == null && path == null)
                throw new XmlNodeException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Import tag must have a 'path' or an 'assembly'", new object[0]), (XmlNode)context.Element);
            if (nameSpace == null)
                throw new XmlNodeException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Import tag must have a 'namespace'", new object[0]), (XmlNode)context.Element);
            if (assemblyName != null)
                this.namedTypeFactory.AddAssemblyRegistration(assemblyName, nameSpace);
            else
                this.namedTypeFactory.AddPathRegistration(path, nameSpace);
        }

        public object GetService(Type serviceType)
        {
            object obj = (object)null;
            if (this.documentServiceContainer != null)
                obj = this.documentServiceContainer.GetService(serviceType);
            if (obj == null)
                obj = this.transformationServiceContainer.GetService(serviceType);
            return obj;
        }
    }
}

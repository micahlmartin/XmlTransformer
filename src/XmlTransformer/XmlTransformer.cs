using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace XmlTransformer
{
    public static class XmlTransformer
    {
        public static XElement MergeXml(XElement sourceDocument, XElement transformDocument)
        {
            if (sourceDocument == null)
                throw new ArgumentNullException("sourceDocument");

            if (transformDocument == null)
                return sourceDocument;

            return transformDocument.MergeWith(sourceDocument, GetConfigMappings());
        }

        public static void MergeFile(string sourceFile, string transformPath)
        {
            MergeFile(sourceFile, transformPath, sourceFile);
        }

        public static void MergeFile(string sourceFile, string transformPath, string destinationPath)
        {
            var xmlFragment = XElement.Load(sourceFile);
            var transformDocument = XElement.Load(transformPath);

            var mergedDocument = MergeXml(xmlFragment, transformDocument);
            mergedDocument.Save(destinationPath);
        }

        public static XmlElement MergeXml(XmlElement sourceDocument, XmlElement transformDocument)
        {
            var source = sourceDocument.ToXElement();
            var transform = transformDocument.ToXElement();

            return MergeXml(source, transform).ToXmlElement();
        }

        public static void TransformXml(string sourceFile, string transformFile)
        {
            TransformXml(sourceFile, transformFile, transformFile);
        }

        public static void TransformXml(string sourceFile, string transformFile, string destinationFile)
        {
            var transformer = new TransformXml
            {
                Source = sourceFile, 
                Destination = destinationFile,
                Transform = transformFile,
            };

            transformer.Execute();
        }

        private static IDictionary<XName, Action<XElement, XElement>> GetConfigMappings()
        {
            // REVIEW: This might be an edge case, but we're setting this rule for all xml files.
            // If someone happens to do a transform where the xml file has a configSections node
            // we will add it first. This is probably fine, but this is a config specific scenario
            return new Dictionary<XName, Action<XElement, XElement>>() {
                { "configSections" , (parent, element) => parent.AddFirst(element) }
            };
        }
    }
}

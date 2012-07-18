using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    public class TransformXml
    {
        public string Source { get; set; }
        public string Transform { get; set; }
        public string Destination { get; set; }
        public bool Execute()
        {
            bool flag = true;
            IXmlTransformationLogger logger = (IXmlTransformationLogger)new Log4netTransformationLogger();
            try
            {
                logger.StartSection(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Transforming Source File: {0}", new object[1] { (object) this.Source }), new object[0]);
                XmlTransformableDocument document = this.OpenSourceFile(this.Source);
                logger.LogMessage(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Applying Transform File: {0}", new object[1] { (object) this.Transform }), new object[0]);
                flag = this.OpenTransformFile(this.Transform, logger).Apply((XmlDocument)document);
                if (flag)
                {
                    logger.LogMessage(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Output File: {0}", new object[1] { (object) this.Destination }), new object[0]);
                    this.SaveTransformedFile(document, this.Destination);
                }
            }
            catch (XmlException ex)
            {
                Uri uri = new Uri(ex.SourceUri);
                logger.LogError(uri.LocalPath, ex.LineNumber, ex.LinePosition, ex.Message, new object[0]);
                flag = false;
            }
            catch (Exception ex)
            {
                logger.LogErrorFromException(ex);
                flag = false;
            }
            finally
            {
                logger.EndSection(string.Format((IFormatProvider)CultureInfo.CurrentCulture, flag ? "Transformation succeeded" : "Transformation failed", new object[0]), new object[0]);
            }
            return flag;
        }

        private void SaveTransformedFile(XmlTransformableDocument document, string destinationFile)
        {
            try
            {
                document.Save(destinationFile);
            }
            catch (XmlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Could not write Destination file: {0}", new object[1] { (object) ex.Message }), ex);
            }
        }

        private XmlTransformableDocument OpenSourceFile(string sourceFile)
        {
            try
            {
                XmlTransformableDocument transformableDocument = new XmlTransformableDocument();
                transformableDocument.PreserveWhitespace = true;
                transformableDocument.Load(sourceFile);
                return transformableDocument;
            }
            catch (XmlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Could not open Source file: {0}", new object[1] { (object) ex.Message }), ex);
            }
        }

        private XmlTransformation OpenTransformFile(string transformFile, IXmlTransformationLogger logger)
        {
            try
            {
                return new XmlTransformation(transformFile, logger);
            }
            catch (XmlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Could not open Transform file: {0}", new object[1] { (object) ex.Message }), ex);
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    public abstract class Transform
    {
        private MissingTargetMessage missingTargetMessage;
        private bool applyTransformToAllTargetNodes;
        private bool useParentAsTargetNode;
        private XmlTransformationLogger logger;
        private XmlElementContext context;
        private XmlNode currentTransformNode;
        private XmlNode currentTargetNode;
        private string argumentString;
        private IList<string> arguments;

        protected bool ApplyTransformToAllTargetNodes
        {
            get
            {
                return this.applyTransformToAllTargetNodes;
            }
            set
            {
                this.applyTransformToAllTargetNodes = value;
            }
        }

        protected bool UseParentAsTargetNode
        {
            get
            {
                return this.useParentAsTargetNode;
            }
            set
            {
                this.useParentAsTargetNode = value;
            }
        }

        protected MissingTargetMessage MissingTargetMessage
        {
            get
            {
                return this.missingTargetMessage;
            }
            set
            {
                this.missingTargetMessage = value;
            }
        }

        protected XmlNode TransformNode
        {
            get
            {
                if (this.currentTransformNode == null)
                    return this.context.TransformNode;
                else
                    return this.currentTransformNode;
            }
        }

        protected XmlNode TargetNode
        {
            get
            {
                if (this.currentTargetNode == null)
                {
                    IEnumerator enumerator = this.TargetNodes.GetEnumerator();
                    try
                    {
                        if (enumerator.MoveNext())
                            return (XmlNode)enumerator.Current;
                    }
                    finally
                    {
                        IDisposable disposable = enumerator as IDisposable;
                        if (disposable != null)
                            disposable.Dispose();
                    }
                }
                return this.currentTargetNode;
            }
        }

        protected XmlNodeList TargetNodes
        {
            get
            {
                if (this.UseParentAsTargetNode)
                    return this.context.TargetParents;
                else
                    return this.context.TargetNodes;
            }
        }

        protected XmlNodeList TargetChildNodes
        {
            get
            {
                return this.context.TargetNodes;
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
                        this.logger.CurrentReferenceNode = (XmlNode)this.context.TransformAttribute;
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

        private string TransformNameLong
        {
            get
            {
                if (!this.context.HasLineInfo)
                    return this.TransformNameShort;
                return string.Format((IFormatProvider)CultureInfo.CurrentCulture, "{0} (transform line {1}, {2})", (object)this.TransformName, (object)this.context.TransformLineNumber, (object)this.context.TransformLinePosition);
            }
        }

        internal string TransformNameShort
        {
            get
            {
                return string.Format((IFormatProvider)CultureInfo.CurrentCulture, "{0}", new object[1] { (object) this.TransformName });
            }
        }

        private string TransformName
        {
            get
            {
                return this.GetType().Name;
            }
        }

        protected Transform()
            : this(TransformFlags.None)
        {
        }

        protected Transform(TransformFlags flags)
            : this(flags, MissingTargetMessage.Warning)
        {
        }

        protected Transform(TransformFlags flags, MissingTargetMessage message)
        {
            this.missingTargetMessage = message;
            this.applyTransformToAllTargetNodes = (flags & TransformFlags.ApplyTransformToAllTargetNodes) == TransformFlags.ApplyTransformToAllTargetNodes;
            this.useParentAsTargetNode = (flags & TransformFlags.UseParentAsTargetNode) == TransformFlags.UseParentAsTargetNode;
        }

        protected abstract void Apply();

        protected T GetService<T>() where T : class
        {
            return this.context.GetService<T>();
        }

        internal void Execute(XmlElementContext context, string argumentString)
        {
            if (this.context != null || this.argumentString != null)
                return;
            bool flag1 = false;
            bool flag2 = false;
            try
            {
                this.context = context;
                this.argumentString = argumentString;
                this.arguments = (IList<string>)null;
                if (!this.ShouldExecuteTransform())
                    return;
                flag2 = true;
                this.Log.StartSection(MessageType.Verbose, "Executing {0}", new object[1] { (object) this.TransformNameLong });
                this.Log.LogMessage(MessageType.Verbose, "on {0}", new object[1] { (object) context.XPath });
                if (this.ApplyTransformToAllTargetNodes)
                    this.ApplyOnAllTargetNodes();
                else
                    this.ApplyOnce();
            }
            catch (Exception ex)
            {
                flag1 = true;
                if (context.TransformAttribute != null)
                    this.Log.LogErrorFromException(XmlNodeException.Wrap(ex, (XmlNode)context.TransformAttribute));
                else
                    this.Log.LogErrorFromException(ex);
            }
            finally
            {
                if (flag2)
                {
                    if (flag1)
                        this.Log.EndSection(MessageType.Verbose, "Error during {0}", new object[1] { (object) this.TransformNameShort });
                    else
                        this.Log.EndSection(MessageType.Verbose, "Done executing {0}", new object[1] { (object) this.TransformNameShort });
                }
                else
                    this.Log.LogMessage(MessageType.Normal, "Not executing {0}", new object[1] { (object) this.TransformNameLong });
                this.context = (XmlElementContext)null;
                this.argumentString = (string)null;
                this.arguments = (IList<string>)null;
                this.ReleaseLogger();
            }
        }

        private void ReleaseLogger()
        {
            if (this.logger == null)
                return;
            this.logger.CurrentReferenceNode = (XmlNode)null;
            this.logger = (XmlTransformationLogger)null;
        }

        private bool ApplyOnAllTargetNodes()
        {
            bool flag = false;
            XmlNode transformNode = this.TransformNode;
            foreach (XmlNode xmlNode in this.TargetNodes)
            {
                try
                {
                    this.currentTargetNode = xmlNode;
                    this.currentTransformNode = transformNode.Clone();
                    this.ApplyOnce();
                }
                catch (Exception ex)
                {
                    this.Log.LogErrorFromException(ex);
                    flag = true;
                }
            }
            this.currentTargetNode = (XmlNode)null;
            return flag;
        }

        private void ApplyOnce()
        {
            this.WriteApplyMessage(this.TargetNode);
            this.Apply();
        }

        private void WriteApplyMessage(XmlNode targetNode)
        {
            IXmlLineInfo xmlLineInfo = targetNode as IXmlLineInfo;
            if (xmlLineInfo != null)
                this.Log.LogMessage(MessageType.Verbose, "Applying to '{0}' element (no source line info)", (object)targetNode.Name, (object)xmlLineInfo.LineNumber, (object)xmlLineInfo.LinePosition);
            else
                this.Log.LogMessage(MessageType.Verbose, "Applying to '{0}' element (no source line info)", new object[1] { (object) targetNode.Name });
        }

        private bool ShouldExecuteTransform()
        {
            return this.HasRequiredTarget();
        }

        private bool HasRequiredTarget()
        {
            bool existedInOriginal = false;
            XmlElementContext failedContext;
            if (!this.UseParentAsTargetNode ? this.context.HasTargetNode(out failedContext, out existedInOriginal) : this.context.HasTargetParent(out failedContext, out existedInOriginal))
                return true;
            this.HandleMissingTarget(failedContext, existedInOriginal);
            return false;
        }

        private void HandleMissingTarget(XmlElementContext matchFailureContext, bool existedInOriginal)
        {
            string message = string.Format((IFormatProvider)CultureInfo.CurrentCulture, existedInOriginal ? "'{0}' did not find a match, because matching nodes in the source document were modified or removed by a previous transform" : "No element in the source document matches '{0}'", new object[1] { (object) matchFailureContext.XPath });
            switch (this.MissingTargetMessage)
            {
                case MissingTargetMessage.None:
                    this.Log.LogMessage(MessageType.Verbose, message, new object[0]);
                    break;
                case MissingTargetMessage.Information:
                    this.Log.LogMessage(MessageType.Normal, message, new object[0]);
                    break;
                case MissingTargetMessage.Warning:
                    this.Log.LogWarning(matchFailureContext.Node, message, new object[0]);
                    break;
                case MissingTargetMessage.Error:
                    throw new XmlNodeException(message, matchFailureContext.Node);
            }
        }
    }
}

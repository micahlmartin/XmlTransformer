using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    //internal class TaskTransformationLogger : IXmlTransformationLogger
    //{
    //    private readonly string indentStringPiece = "  ";
    //    private int indentLevel;
    //    private string indentString;
    //    private bool stackTrace;

    //    private string IndentString
    //    {
    //        get
    //        {
    //            if (this.indentString == null)
    //            {
    //                this.indentString = string.Empty;
    //                for (int index = 0; index < this.indentLevel; ++index)
    //                {
    //                    TaskTransformationLogger transformationLogger = this;
    //                    string str = transformationLogger.indentString + this.indentStringPiece;
    //                    transformationLogger.indentString = str;
    //                }
    //            }
    //            return this.indentString;
    //        }
    //    }

    //    private int IndentLevel
    //    {
    //        get
    //        {
    //            return this.indentLevel;
    //        }
    //        set
    //        {
    //            if (this.indentLevel == value)
    //                return;
    //            this.indentLevel = value;
    //            this.indentString = (string)null;
    //        }
    //    }

    //    void IXmlTransformationLogger.LogMessage(string message, params object[] messageArgs)
    //    {
    //        this.LogMessage(MessageType.Normal, message, messageArgs);
    //    }

    //    void IXmlTransformationLogger.LogMessage(MessageType type, string message, params object[] messageArgs)
    //    {
    //        MessageImportance importance;
    //        switch (type)
    //        {
    //            case MessageType.Normal:
    //                importance = MessageImportance.Normal;
    //                break;
    //            case MessageType.Verbose:
    //                importance = MessageImportance.Low;
    //                break;
    //            default:
    //                importance = MessageImportance.Normal;
    //                break;
    //        }
    //        this.loggingHelper.LogMessage(importance, this.IndentString + message, messageArgs);
    //    }

    //    void IXmlTransformationLogger.LogWarning(string message, params object[] messageArgs)
    //    {
    //        this.loggingHelper.LogWarning(message, messageArgs);
    //    }

    //    void IXmlTransformationLogger.LogWarning(string file, string message, params object[] messageArgs)
    //    {
    //        this.LogWarning(file, 0, 0, message, messageArgs);
    //    }

    //    void IXmlTransformationLogger.LogWarning(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
    //    {
    //        this.loggingHelper.LogWarning((string)null, (string)null, (string)null, file, lineNumber, linePosition, 0, 0, this.loggingHelper.FormatString(message, messageArgs), new object[0]);
    //    }

    //    void IXmlTransformationLogger.LogError(string message, params object[] messageArgs)
    //    {
    //        this.loggingHelper.LogError(message, messageArgs);
    //    }

    //    void IXmlTransformationLogger.LogError(string file, string message, params object[] messageArgs)
    //    {
    //        this.LogError(file, 0, 0, message, messageArgs);
    //    }

    //    void IXmlTransformationLogger.LogError(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
    //    {
    //        this.loggingHelper.LogError((string)null, (string)null, (string)null, file, lineNumber, linePosition, 0, 0, this.loggingHelper.FormatString(message, messageArgs), new object[0]);
    //    }

    //    void IXmlTransformationLogger.LogErrorFromException(Exception ex)
    //    {
    //        this.loggingHelper.LogErrorFromException(ex, this.stackTrace, this.stackTrace, (string)null);
    //    }

    //    void IXmlTransformationLogger.LogErrorFromException(Exception ex, string file)
    //    {
    //        this.loggingHelper.LogErrorFromException(ex, this.stackTrace, this.stackTrace, file);
    //    }

    //    void IXmlTransformationLogger.LogErrorFromException(Exception ex, string file, int lineNumber, int linePosition)
    //    {
    //        string message = ex.Message;
    //        if (this.stackTrace)
    //        {
    //            StringBuilder stringBuilder = new StringBuilder();
    //            for (Exception exception = ex; exception != null; exception = exception.InnerException)
    //            {
    //                stringBuilder.AppendFormat("{0} : {1}", (object)exception.GetType().Name, (object)exception.Message);
    //                stringBuilder.AppendLine();
    //                if (!string.IsNullOrEmpty(exception.StackTrace))
    //                    stringBuilder.Append(exception.StackTrace);
    //            }
    //            message = ((object)stringBuilder).ToString();
    //        }
    //        this.LogError(file, lineNumber, linePosition, message, new object[0]);
    //    }

    //    void IXmlTransformationLogger.StartSection(string message, params object[] messageArgs)
    //    {
    //        this.StartSection(MessageType.Normal, message, messageArgs);
    //    }

    //    void IXmlTransformationLogger.StartSection(MessageType type, string message, params object[] messageArgs)
    //    {
    //        this.LogMessage(type, message, messageArgs);
    //        ++this.IndentLevel;
    //    }

    //    void IXmlTransformationLogger.EndSection(string message, params object[] messageArgs)
    //    {
    //        this.EndSection(MessageType.Normal, message, messageArgs);
    //    }

    //    void IXmlTransformationLogger.EndSection(MessageType type, string message, params object[] messageArgs)
    //    {
    //        if (this.IndentLevel > 0)
    //            --this.IndentLevel;
    //        this.LogMessage(type, message, messageArgs);
    //    }
    //}
}

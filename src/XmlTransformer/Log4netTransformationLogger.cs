using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace XmlTransformer
{
    public class Log4netTransformationLogger : IXmlTransformationLogger
    {
        private ILog _logger = LogManager.GetLogger(typeof(Log4netTransformationLogger));
        private int _indentLevel;
        private string _indentString;
        private readonly string _indentStringPiece = "  ";
        
        private string IndentString
        {
            get
            {
                if (this._indentString == null)
                {
                    this._indentString = string.Empty;
                    for (int index = 0; index < this._indentLevel; ++index)
                    {
                        Log4netTransformationLogger transformationLogger = this;
                        string str = transformationLogger._indentString + this._indentStringPiece;
                        transformationLogger._indentString = str;
                    }
                }
                return this._indentString;
            }
        }
        private int IndentLevel
        {
            get
            {
                return this._indentLevel;
            }
            set
            {
                if (this._indentLevel == value)
                    return;
                this._indentLevel = value;
                this._indentString = (string)null;
            }
        }

        public void LogMessage(string message, params object[] messageArgs)
        {
            this.LogMessage(MessageType.Normal, message, messageArgs);
        }

        public void LogMessage(MessageType type, string message, params object[] messageArgs)
        {
            switch (type)
            {
                case MessageType.Normal:
                    _logger.InfoFormat(IndentString + message, messageArgs);
                    break;
                case MessageType.Verbose:
                    _logger.DebugFormat(IndentString + message, messageArgs);
                    break;
                default:
                    _logger.InfoFormat(IndentString + message, messageArgs);
                    break;
            }
        }

        public void LogWarning(string message, params object[] messageArgs)
        {
            _logger.WarnFormat(message, messageArgs);
        }

        public void LogWarning(string file, string message, params object[] messageArgs)
        {
            _logger.WarnFormat(message, messageArgs);
        }

        public void LogWarning(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            _logger.WarnFormat(message, messageArgs);
        }

        public void LogError(string message, params object[] messageArgs)
        {
            _logger.ErrorFormat(message, messageArgs);
        }

        public void LogError(string file, string message, params object[] messageArgs)
        {
            _logger.ErrorFormat(message, messageArgs);
        }

        public void LogError(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            _logger.ErrorFormat(message, messageArgs);
        }

        public void LogErrorFromException(Exception ex)
        {
            _logger.ErrorFormat(ex.Message, ex);
        }

        public void LogErrorFromException(Exception ex, string file)
        {
            _logger.ErrorFormat(ex.Message, ex);
        }

        public void LogErrorFromException(Exception ex, string file, int lineNumber, int linePosition)
        {
            _logger.Error(ex.Message, ex);
        }

        public void StartSection(string message, params object[] messageArgs)
        {
            StartSection(MessageType.Normal, message, messageArgs);
        }

        public void StartSection(MessageType type, string message, params object[] messageArgs)
        {
            LogMessage(type, message, messageArgs);
            ++this.IndentLevel;
        }

        public void EndSection(string message, params object[] messageArgs)
        {
            EndSection(MessageType.Normal, message, messageArgs);
        }

        public void EndSection(MessageType type, string message, params object[] messageArgs)
        {
            if (IndentLevel > 0)
                --IndentLevel;
            LogMessage(type, message, messageArgs);
        }
    }
}

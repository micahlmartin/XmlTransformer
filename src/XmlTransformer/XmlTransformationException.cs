using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    [Serializable]
    public class XmlTransformationException : Exception
    {
        internal XmlTransformationException(string message)
            : base(message)
        {
        }

        internal XmlTransformationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

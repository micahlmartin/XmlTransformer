using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlTransformer
{
    internal static class CommonErrors
    {
        internal static void ExpectNoArguments(XmlTransformationLogger log, string transformName, string argumentString)
        {
            if (string.IsNullOrEmpty(argumentString))
                return;
            log.LogWarning("{0} does not expect arguments; ignoring", new object[1]
      {
        (object) transformName
      });
        }

        internal static void WarnIfMultipleTargets(XmlTransformationLogger log, string transformName, XmlNodeList targetNodes, bool applyTransformToAllTargets)
        {
            if (targetNodes.Count <= 1)
                return;
            log.LogWarning("Found multiple target elements, but the '{0}' Transform only applies to the first match", new object[1]
      {
        (object) transformName
      });
        }
    }
}

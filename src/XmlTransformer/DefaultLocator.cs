using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    internal sealed class DefaultLocator : Locator
    {
        private static DefaultLocator instance;

        internal static DefaultLocator Instance
        {
            get
            {
                if (DefaultLocator.instance == null)
                    DefaultLocator.instance = new DefaultLocator();
                return DefaultLocator.instance;
            }
        }

        static DefaultLocator()
        {
        }
    }
}

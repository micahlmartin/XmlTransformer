using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    internal static class XmlArgumentUtility
    {
        internal static IList<string> SplitArguments(string argumentString)
        {
            if (argumentString.IndexOf(',') == -1)
            {
                return (IList<string>)new string[1]
        {
          argumentString
        };
            }
            else
            {
                List<string> list = new List<string>();
                list.AddRange((IEnumerable<string>)argumentString.Split(new char[1]
        {
          ','
        }));
                IList<string> arguments = XmlArgumentUtility.RecombineArguments((IList<string>)list, ',');
                XmlArgumentUtility.TrimStrings(arguments);
                return arguments;
            }
        }

        private static IList<string> RecombineArguments(IList<string> arguments, char separator)
        {
            List<string> list = new List<string>();
            string str1 = (string)null;
            int num = 0;
            foreach (string str2 in (IEnumerable<string>)arguments)
            {
                str1 = str1 != null ? str1 + (object)separator + str2 : str2;
                num += XmlArgumentUtility.CountParens(str2);
                if (num == 0)
                {
                    list.Add(str1);
                    str1 = (string)null;
                }
            }
            if (str1 != null)
                list.Add(str1);
            if (arguments.Count != list.Count)
                arguments = (IList<string>)list;
            return arguments;
        }

        private static void TrimStrings(IList<string> arguments)
        {
            for (int index = 0; index < arguments.Count; ++index)
                arguments[index] = arguments[index].Trim();
        }

        private static int CountParens(string str)
        {
            int num = 0;
            foreach (char ch in str)
            {
                switch (ch)
                {
                    case '(':
                        ++num;
                        break;
                    case ')':
                        --num;
                        break;
                }
            }
            return num;
        }
    }
}

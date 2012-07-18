using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    internal class XmlAttributePreservationProvider
    {
        private StreamReader streamReader;
        private PositionTrackingTextReader reader;

        public XmlAttributePreservationProvider(string fileName)
        {
            this.streamReader = new StreamReader((Stream)File.OpenRead(fileName));
            this.reader = new PositionTrackingTextReader((TextReader)this.streamReader);
        }

        public XmlAttributePreservationDict GetDictAtPosition(int lineNumber, int linePosition)
        {
            if (this.reader.ReadToPosition(lineNumber, linePosition))
            {
                StringBuilder stringBuilder = new StringBuilder();
                int num;
                do
                {
                    num = this.reader.Read();
                    stringBuilder.Append((char)num);
                }
                while (num > 0 && (int)(ushort)num != 62);
                if (num > 0)
                {
                    XmlAttributePreservationDict preservationDict = new XmlAttributePreservationDict();
                    preservationDict.ReadPreservationInfo(((object)stringBuilder).ToString());
                    return preservationDict;
                }
            }
            return (XmlAttributePreservationDict)null;
        }

        public void Close()
        {
            if (this.streamReader == null)
                return;
            this.streamReader.Close();
            this.streamReader.Dispose();
            this.streamReader = (StreamReader)null;
        }
    }
}

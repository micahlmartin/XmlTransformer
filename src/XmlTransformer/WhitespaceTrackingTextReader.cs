using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    internal class WhitespaceTrackingTextReader : PositionTrackingTextReader
    {
        private StringBuilder precedingWhitespace = new StringBuilder();

        public string PrecedingWhitespace
        {
            get
            {
                return ((object)this.precedingWhitespace).ToString();
            }
        }

        public WhitespaceTrackingTextReader(TextReader reader)
            : base(reader)
        {
        }

        public override int Read()
        {
            int character = base.Read();
            this.UpdateWhitespaceTracking(character);
            return character;
        }

        private void UpdateWhitespaceTracking(int character)
        {
            if (char.IsWhiteSpace((char)character))
                this.AppendWhitespaceCharacter(character);
            else
                this.ResetWhitespaceString();
        }

        private void AppendWhitespaceCharacter(int character)
        {
            this.precedingWhitespace.Append((char)character);
        }

        private void ResetWhitespaceString()
        {
            this.precedingWhitespace = new StringBuilder();
        }
    }
}

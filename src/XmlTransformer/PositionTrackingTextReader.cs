using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XmlTransformer
{
    internal class PositionTrackingTextReader : TextReader
    {
        private int lineNumber = 1;
        private int linePosition = 1;
        private int characterPosition = 1;
        private const int newlineCharacter = 10;
        private TextReader internalReader;

        public PositionTrackingTextReader(TextReader textReader)
        {
            this.internalReader = textReader;
        }

        public override int Read()
        {
            int character = this.internalReader.Read();
            this.UpdatePosition(character);
            return character;
        }

        public override int Peek()
        {
            return this.internalReader.Peek();
        }

        public bool ReadToPosition(int lineNumber, int linePosition)
        {
            while (this.lineNumber < lineNumber && this.Peek() != -1)
                this.ReadLine();
            while (this.linePosition < linePosition && this.Peek() != -1)
                this.Read();
            if (this.lineNumber == lineNumber)
                return this.linePosition == linePosition;
            else
                return false;
        }

        public bool ReadToPosition(int characterPosition)
        {
            while (this.characterPosition < characterPosition && this.Peek() != -1)
                this.Read();
            return this.characterPosition == characterPosition;
        }

        private void UpdatePosition(int character)
        {
            if (character == 10)
            {
                ++this.lineNumber;
                this.linePosition = 1;
            }
            else
                ++this.linePosition;
            ++this.characterPosition;
        }
    }
}

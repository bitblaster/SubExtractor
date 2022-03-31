using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    public class OcrCharacter : IEquatable<OcrCharacter>, IComparable<OcrCharacter>
    {
        public const Char UnmatchedValue = '\0';
        public static readonly OcrCharacter Unmatched = new OcrCharacter(UnmatchedValue);

        public OcrCharacter(char value)
        {
            this.Value = value;
        }

        public OcrCharacter(char ocr, bool isItalic)
            : this(ocr)
        {
            this.Italic = isItalic;
        }

        public char Value { get; private set; }
        public bool Italic { get; private set; }

        public override string ToString()
        {
            if(this.Italic)
            {
                return string.Format("Italic \'{0}\'", this.Value);
            }
            else
            {
                return string.Format("\'{0}\'", this.Value);
            }
        }

        public static bool operator ==(OcrCharacter c1, OcrCharacter c2)
        {
            if((object)c1 == null)
            {
                return ((object)c2 == null);
            }
            return c1.Equals(c2);
        }

        public static bool operator !=(OcrCharacter c1, OcrCharacter c2)
        {
            return !(c1 == c2);
        }

        public bool Equals(OcrCharacter other)
        {
            if((object)other == null)
            {
                return false;
            }
            return (this.Value == other.Value) && (this.Italic == other.Italic);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as OcrCharacter);
        }

        public override int GetHashCode()
        {
            unchecked { return this.Value.GetHashCode() + this.Italic.GetHashCode(); }
        }

        public int CompareTo(OcrCharacter other)
        {
            if(!this.Italic && other.Italic)
            {
                return -1;
            }
            if(this.Italic && !other.Italic)
            {
                return 1;
            }
            return this.Value.CompareTo(other.Value);
        }
    }
}

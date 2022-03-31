using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DvdSubOcr
{
    public class RectangleWithColors
    {
        SortedDictionary<int, object> colors = new SortedDictionary<int, object>();

        public RectangleWithColors(Rectangle rect, int colorIndex)
        {
            this.Rectangle = rect;
            this.ColorIndexes = colors.Keys;
            this.colors.Add(colorIndex, null);
        }

        public Rectangle Rectangle { get; private set; }
        public ICollection<int> ColorIndexes { get; private set; }

        public void Add(Rectangle rect, int colorIndex)
        {
            this.Rectangle = Rectangle.Union(this.Rectangle, rect);
            if(!this.colors.ContainsKey(colorIndex))
            {
                this.colors.Add(colorIndex, null);
            }
        }

        public void TrimRectangle(int xTrim, int yTrim)
        {
            this.Rectangle = Rectangle.Inflate(this.Rectangle, -xTrim, -yTrim);
        }

        public void MergeWith(RectangleWithColors other)
        {
            this.Rectangle = Rectangle.Union(this.Rectangle, other.Rectangle);
            foreach(int colorIndex in other.ColorIndexes)
            {
                if(!this.colors.ContainsKey(colorIndex))
                {
                    this.colors.Add(colorIndex, null);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0} Colors {1}", this.Rectangle,
                this.ColorIndexes.Aggregate("", (concat, current) => (concat.Length == 0) ?
                    current.ToString() : concat + ", " + current.ToString()));
        }
    }
}

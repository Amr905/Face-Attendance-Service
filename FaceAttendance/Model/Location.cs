using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceAttendance.Model
{
    public class Location
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Location(int top, int left, int width, int height)
        {
            Top = top;
            Left = left;
            Width = width;
            Height = height;
        }

    }
}

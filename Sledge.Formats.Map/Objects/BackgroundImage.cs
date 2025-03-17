using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Sledge.Formats.Map.Objects
{
    public class BackgroundImage
    {
        public ViewportType Viewport { get; set; }
        public string Path { get; set; }
        public double Scale { get; set; }
        public byte Luminance { get; set; }
        public FilterMode Filter { get; set; }
        public bool InvertColours { get; set; }
        public Vector2 Offset { get; set; }
    }
}

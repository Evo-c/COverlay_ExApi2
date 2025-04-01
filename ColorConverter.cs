using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cOverlay
{
    public static class ColorConverter
    {
        public static System.Numerics.Vector4 ToVector4(Color color)
        {
            return new System.Numerics.Vector4(
                color.R / 255f,
                color.G / 255f,
                color.B / 255f,
                color.A / 255f
            );
        }

        public static Color FromVector4(System.Numerics.Vector4 colorArray)
        {
            var color = System.Drawing.Color.FromArgb(
                (int)(colorArray[3] * 255), // Alpha
                (int)(colorArray[0] * 255), // Red
                (int)(colorArray[1] * 255), // Green
                (int)(colorArray[2] * 255)  // Blue
            );
            return color;
        }
    }
}

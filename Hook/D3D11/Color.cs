using System;

namespace Hook.D3D11
{
    [Serializable]
    public class Color
    {
        public static Color White = new Color(255, 255, 255);
        public static Color Red = new Color(255, 0, 0);

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        // Divide by 25 to combat glow post-process effect.
        public float Rf => (R / 255f) / 25f;
        public float Gf => (G / 255f) / 25f;
        public float Bf => (B / 255f) / 25f;

        public Color() { }
        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public override bool Equals(object obj)
        {
            var color = (Color) obj;

            return color != null && Equals(color);
        }

        protected bool Equals(Color other)
        {
            return R == other.R && G == other.G && B == other.B;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = R.GetHashCode();
                hashCode = (hashCode * 397) ^ G.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                return hashCode;
            }
        }
    }
}

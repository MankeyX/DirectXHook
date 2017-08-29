using System;
using SharpDX.DXGI;

namespace Hook.D3D11
{
    [Serializable]
    public class ModelParameters
    {
        public string Name { get; set; }
        public int IndexCount { get; set; }
        public int IndexByteWidth { get; set; }
        public int Stride { get; set; }
        public int VertexByteWidth { get; set; }
        public Format Format { get; set; }
        public bool Enabled { get; set; }
        public Color Color { get; set; } = new Color(100, 100, 100);

        public ModelParameters() { }
        public ModelParameters(int indexCount, int indexByteWidth, int stride, 
                               int vertexByteWidth, Format format, Color color)
        {
            IndexCount = indexCount;
            IndexByteWidth = indexByteWidth;
            Stride = stride;
            Format = format;
            VertexByteWidth = vertexByteWidth;
            Color = color;
        }

        public override bool Equals(object obj)
        {
            var modelParameters = obj as ModelParameters;

            return modelParameters != null 
                && modelParameters.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash *= 23 + IndexCount.GetHashCode();
                hash *= 23 + IndexByteWidth.GetHashCode();
                hash *= 23 + Stride.GetHashCode();
                hash *= 23 + VertexByteWidth.GetHashCode();
                hash *= 23 + Format.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"Name: {Name}\n" +
                   $"Index Count: {IndexCount}\n" +
                   $"Index Buffer Byte Width: {IndexByteWidth}\n" +
                   $"Stride: {Stride}\n" +
                   $"Vertex Buffer Byte Width: {VertexByteWidth}\n" +
                   $"Format: {Format}";
        }
    }
}

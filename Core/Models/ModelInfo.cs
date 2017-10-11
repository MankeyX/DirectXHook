using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core.Models
{
    [Serializable]
    public class ModelInfo : IComparable<ModelInfo>, INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        public int IndexCount { get; set; }
        public int IndexByteWidth { get; set; }
        public int Stride { get; set; }
        public int VertexByteWidth { get; set; }
        public int Format { get; set; }
        public Color Color { get; set; } = new Color(0, 0, 100);

        private bool _enabled = true;
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                NotifyPropertyChanged();
            }
        }
        
        public ModelInfo() { }
        public ModelInfo(int indexCount, int indexByteWidth, int stride, 
                               int vertexByteWidth, int format, Color color)
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
            var modelParameters = obj as ModelInfo;

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

        public int CompareTo(ModelInfo other)
        {
            if (ReferenceEquals(this, other))
                return 0;
            if (ReferenceEquals(null, other))
                return 1;

            var indexCountComparison = IndexCount.CompareTo(other.IndexCount);
            if (indexCountComparison != 0)
                return indexCountComparison;

            var indexByteWidthComparison = IndexByteWidth.CompareTo(other.IndexByteWidth);
            if (indexByteWidthComparison != 0)
                return indexByteWidthComparison;

            var strideComparison = Stride.CompareTo(other.Stride);
            if (strideComparison != 0)
                return strideComparison;

            var vertexByteWidthComparison = VertexByteWidth.CompareTo(other.VertexByteWidth);
            if (vertexByteWidthComparison != 0)
                return vertexByteWidthComparison;
            
            return Format.CompareTo(other.Format);
        }

        #region INotifyPropertyChanged
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}

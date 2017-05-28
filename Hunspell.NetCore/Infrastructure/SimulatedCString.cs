using System.Text;

namespace Hunspell.NetCore.Infrastructure
{
    internal sealed class SimulatedCString
    {
        public SimulatedCString(string text)
        {
            _buffer = StringBuilderPool.Get(text);
        }

        private StringBuilder _buffer;

        private string _toStringCache = null;

        public char this[int index]
        {
            get
            {
                return index < 0 || index >= _buffer.Length ? '\0' : _buffer[index];
            }
            set
            {
                _toStringCache = null;
                _buffer[index] = value;
            }
        }

        public int BufferLength => _buffer.Length;

        public void WriteChars(string text, int destinationIndex)
        {
            _toStringCache = null;
            _buffer.WriteChars(text, destinationIndex);
        }

        public void WriteChars(int sourceIndex, string text, int destinationIndex)
        {
            _toStringCache = null;
            _buffer.WriteChars(sourceIndex, text, destinationIndex);
        }

        public void Assign(string text)
        {
            _toStringCache = null;
            _buffer.Clear();
            _buffer.Append(text);
        }

        public string Substring(int index) => ToString().Substring(index);

        internal StringSlice Subslice(int index) => ToString().Subslice(index);

        public void Destroy()
        {
            if (_buffer != null)
            {
                StringBuilderPool.Return(_buffer);
            }

            _toStringCache = null;
            _buffer = null;
        }

        public override string ToString()
        {
            return _toStringCache ?? (_toStringCache = _buffer.ToStringTerminated());
        }

        public static implicit operator string(SimulatedCString cString)
        {
            return cString?.ToString();
        }
    }
}

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hunspell.NetCore.Tests.Utilities
{
    public class Utf16StringLineReader : IHunspellLineReader
    {
        private static readonly char[] _lineBreakChars = new[]{'\n','\r'};

        private int _position = 0;

        private string Content { get; }

        public Utf16StringLineReader(string text)
        {
            Content = text;
        }

        public Encoding CurrentEncoding => Encoding.UTF8;

        public Task<string> ReadLineAsync()
        {
            string result;
            if (Content == null || _position >= Content.Length)
            {
                result = null;
            }
            else
            {
                var startPosition = _position;
                _position = Content.IndexOfAny(_lineBreakChars, _position);
                if (_position < 0)
                {
                    _position = Content.Length;
                }

                result = Content.Substring(startPosition, _position - startPosition);
                for(;_position < Content.Length && _lineBreakChars.Contains(Content[_position]); _position++)
                {
                    ;
                }
            }

            return Task.FromResult(result);
        }

        public string ReadLine()
        {
            return ReadLineAsync().Result;
        }
    }
}

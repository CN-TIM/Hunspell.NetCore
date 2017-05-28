using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if !NO_ASYNC

#endif

namespace Hunspell.NetCore
{
    public sealed class StaticEncodingLineReader : IHunspellLineReader, IDisposable
    {
        public StaticEncodingLineReader(Stream stream, Encoding encoding)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            this._stream = stream;
            _reader = new StreamReader(stream, encoding, true);
        }

        private readonly Stream _stream;
        private readonly StreamReader _reader;

        public Encoding CurrentEncoding => _reader.CurrentEncoding;

#if !NO_IO_FILE
        public static List<string> ReadLines(string filePath, Encoding defaultEncoding)
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StaticEncodingLineReader(stream, defaultEncoding))
            {
                return reader.ReadLines().ToList();
            }
        }

#if !NO_ASYNC
        public static async Task<List<string>> ReadLinesAsync(string filePath, Encoding defaultEncoding)
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StaticEncodingLineReader(stream, defaultEncoding))
            {
                return await reader.ReadLinesAsync().ConfigureAwait(false);
            }
        }
#endif

#endif

        public string ReadLine()
        {
            return _reader.ReadLine();
        }

#if !NO_ASYNC
        public Task<string> ReadLineAsync()
        {
            return reader.ReadLineAsync();
        }
#endif

        public void Dispose()
        {
            _reader.Dispose();
            _stream.Dispose();
        }
    }
}

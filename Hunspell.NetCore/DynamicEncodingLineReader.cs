using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hunspell.NetCore.Infrastructure;

namespace Hunspell.NetCore
{
    public sealed class DynamicEncodingLineReader : IHunspellLineReader, IDisposable
    {
        static DynamicEncodingLineReader()
        {
            _preambleEncodings =
                new Encoding[]
                {
                    new UnicodeEncoding(true, true)
                    ,new UnicodeEncoding(false, true)
                    ,Encoding.UTF8
                };

            _maxPreambleBytes = _preambleEncodings.Max(e => e.GetPreamble().Length);
        }

        private static readonly Regex _setEncodingRegex = new Regex(
            @"^[\t ]*SET[\t ]+([^\t ]+)[\t ]*$", RegexOptions.CultureInvariant);    //netstandard1.1 doesn't have RegexOptions.Compiled

        private static readonly Encoding[] _preambleEncodings;

        private static readonly int _maxPreambleBytes;

        public DynamicEncodingLineReader(Stream stream, Encoding initialEncoding)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (initialEncoding == null)
            {
                throw new ArgumentNullException(nameof(initialEncoding));
            }

            this._stream = stream;
            _encoding = initialEncoding;
            _decoder = initialEncoding.GetDecoder();
        }

        private readonly Stream _stream;
        private Encoding _encoding;
        private Decoder _decoder;

        private readonly int _bufferMaxSize = 4096;
        private byte[] _buffer = null;
        private int _bufferIndex = -1;
        private bool _hasCheckedForPreamble = false;

        public Encoding CurrentEncoding => _encoding;

        public static List<string> ReadLines(string filePath, Encoding defaultEncoding)
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new DynamicEncodingLineReader(stream, defaultEncoding))
            {
                return reader.ReadLines().ToList();
            }
        }

        public static async Task<List<string>> ReadLinesAsync(string filePath, Encoding defaultEncoding)
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new DynamicEncodingLineReader(stream, defaultEncoding))
            {
                return await reader.ReadLinesAsync().ConfigureAwait(false);
            }
        }

        public string ReadLine()
        {
            if (!_hasCheckedForPreamble)
            {
                ReadPreamble();
            }

            var builder = StringBuilderPool.Get();
            char[] readChars = null;
            while ((readChars = ReadNextChars()) != null)
            {
                if (ProcessCharsForLine(readChars, builder))
                {
                    break;
                }
            }

            if (readChars == null && builder.Length == 0)
            {
                return null;
            }

            return ProcessLine(StringBuilderPool.GetStringAndReturn(builder));
        }

        public async Task<string> ReadLineAsync()
        {
            if (!_hasCheckedForPreamble)
            {
                await ReadPreambleAsync().ConfigureAwait(false);
            }

            var builder = StringBuilderPool.Get();
            char[] readChars = null;
            while ((readChars = await ReadNextCharsAsync().ConfigureAwait(false)) != null)
            {
                if (ProcessCharsForLine(readChars, builder))
                {
                    break;
                }
            }

            if (readChars == null && builder.Length == 0)
            {
                return null;
            }

            return ProcessLine(StringBuilderPool.GetStringAndReturn(builder));
        }

        private bool ProcessCharsForLine(char[] readChars, StringBuilder builder)
        {
            var firstNonLineBreakCharacter = -1;
            var lastNonLineBreakCharacter = -1;

            for (var i = 0; i < readChars.Length; i++)
            {
                var charValue = readChars[i];
                if (charValue != '\r' && charValue != '\n')
                {
                    firstNonLineBreakCharacter = i;
                    break;
                }
            }

            for (var i = readChars.Length - 1; i >= 0; i--)
            {
                var charValue = readChars[i];
                if (charValue != '\r' && charValue != '\n')
                {
                    lastNonLineBreakCharacter = i;
                    break;
                }
            }

            if (firstNonLineBreakCharacter == -1 || lastNonLineBreakCharacter == -1)
            {
                return true;
            }
            else
            {
                builder.Append(readChars, firstNonLineBreakCharacter, lastNonLineBreakCharacter - firstNonLineBreakCharacter + 1);
                return lastNonLineBreakCharacter != readChars.Length - 1;
            }
        }

        private char[] ReadNextChars()
        {
            var maxBytes = _encoding.GetMaxByteCount(1);
            var bytesReadIntoCharBuffer = 0;
            var charOutBuffer = new char[_encoding.GetMaxCharCount(maxBytes)];

            while (bytesReadIntoCharBuffer < maxBytes)
            {
                var nextBytes = ReadBytes(1);
                if (nextBytes == null || nextBytes.Length == 0)
                {
                    return null;
                }

                bytesReadIntoCharBuffer += nextBytes.Length;

                var charsProduced = TryDecode(nextBytes, charOutBuffer);
                if (charsProduced > 0)
                {
                    if (charOutBuffer.Length != charsProduced)
                    {
                        Array.Resize(ref charOutBuffer, charsProduced);
                    }

                    return charOutBuffer;
                }
            }

            return null;
        }

        private async Task<char[]> ReadNextCharsAsync()
        {
            var maxBytes = _encoding.GetMaxByteCount(1);
            var bytesConsumed = 0;
            var charOutBuffer = new char[_encoding.GetMaxCharCount(maxBytes)];

            while (bytesConsumed < maxBytes)
            {
                var nextBytes = await ReadBytesAsync(1).ConfigureAwait(false);
                if (nextBytes == null || nextBytes.Length == 0)
                {
                    return null;
                }

                bytesConsumed += nextBytes.Length;

                var charsProduced = TryDecode(nextBytes, charOutBuffer);
                if (charsProduced > 0)
                {
                    if (charOutBuffer.Length != charsProduced)
                    {
                        Array.Resize(ref charOutBuffer, charsProduced);
                    }

                    return charOutBuffer;
                }
            }

            return null;
        }

        private int TryDecode(byte[] bytes, char[] chars)
        {
            int bytesConverted;
            int charsProduced;
            bool completed;

            _decoder.Convert(
                    bytes,
                    0,
                    bytes.Length,
                    chars,
                    0,
                    chars.Length,
                    false,
                    out bytesConverted,
                    out charsProduced,
                    out completed);

            return charsProduced;
        }

        private bool ReadPreamble()
        {
            var possiblePreambleBytes = ReadBytes(_maxPreambleBytes);
            return HandlePreambleBytes(possiblePreambleBytes);
        }

        private async Task<bool> ReadPreambleAsync()
        {
            var possiblePreambleBytes = await ReadBytesAsync(_maxPreambleBytes).ConfigureAwait(false);
            return HandlePreambleBytes(possiblePreambleBytes);
        }

        private bool HandlePreambleBytes(byte[] possiblePreambleBytes)
        {
            if (possiblePreambleBytes == null || possiblePreambleBytes.Length == 0)
            {
                return false;
            }

            int? bytesToRestore = null;
            foreach (var candidateEncoding in _preambleEncodings)
            {
                var encodingPreamble = candidateEncoding.GetPreamble();
                if (encodingPreamble == null || encodingPreamble.Length == 0)
                {
                    continue;
                }

                if (
                    possiblePreambleBytes.Length >= encodingPreamble.Length
                    && ArrayComparer<byte>.Default.Equals(possiblePreambleBytes, 0, encodingPreamble, 0, encodingPreamble.Length)
                )
                {
                    bytesToRestore = possiblePreambleBytes.Length - encodingPreamble.Length;
                    _encoding = candidateEncoding;
                    break;
                }
            }

            RevertReadBytes(bytesToRestore ?? possiblePreambleBytes.Length);

            _hasCheckedForPreamble = true;
            return true;
        }

        private byte[] ReadBytes(int count)
        {
            var result = new byte[count];
            var resultOffset = 0;
            var bytesNeeded = result.Length;

            while (bytesNeeded > 0)
            {
                if (!PrepareBuffer())
                {
                    return null;
                }

                HandleReadBytesIncrement(result, ref bytesNeeded, ref resultOffset);
            }

            return result;
        }

        private async Task<byte[]> ReadBytesAsync(int count)
        {
            var result = new byte[count];
            var resultOffset = 0;
            var bytesNeeded = result.Length;

            while (bytesNeeded > 0)
            {
                if (!await PrepareBufferAsync().ConfigureAwait(false))
                {
                    return null;
                }

                HandleReadBytesIncrement(result, ref bytesNeeded, ref resultOffset);
            }

            return result;
        }

        private void HandleReadBytesIncrement(byte[] result, ref int bytesNeeded, ref int resultOffset)
        {
            var bytesLeftInBuffer = _buffer.Length - _bufferIndex;
            if (bytesNeeded >= bytesLeftInBuffer)
            {
                Buffer.BlockCopy(_buffer, _bufferIndex, result, resultOffset, bytesLeftInBuffer);
                _bufferIndex = _buffer.Length;
                resultOffset += bytesLeftInBuffer;
                bytesNeeded -= bytesLeftInBuffer;
            }
            else
            {
                Buffer.BlockCopy(_buffer, _bufferIndex, result, resultOffset, bytesNeeded);
                _bufferIndex += bytesNeeded;
                resultOffset += bytesNeeded;
                bytesNeeded = 0;
            }
        }

        private bool PrepareBuffer()
        {
            if (_buffer != null && _bufferIndex < _buffer.Length)
            {
                return true;
            }

            _buffer = new byte[_bufferMaxSize];
            var readBytesCount = _stream.Read(_buffer, 0, _buffer.Length);

            return HandlePrepareBufferRead(readBytesCount);
        }

        private async Task<bool> PrepareBufferAsync()
        {
            if (_buffer != null && _bufferIndex < _buffer.Length)
            {
                return true;
            }

            _buffer = new byte[_bufferMaxSize];
            var readBytesCount = await _stream.ReadAsync(_buffer, 0, _buffer.Length).ConfigureAwait(false);

            return HandlePrepareBufferRead(readBytesCount);
        }

        private bool HandlePrepareBufferRead(int readBytesCount)
        {
            if (readBytesCount != _buffer.Length)
            {
                Array.Resize(ref _buffer, readBytesCount);
            }

            _bufferIndex = 0;
            return readBytesCount != 0;
        }

        private void RevertReadBytes(int count)
        {
            if (count == 0)
            {
                return;
            }

            if (_buffer == null)
            {
                throw new InvalidOperationException();
            }

            var revertedIndex = _bufferIndex - count;
            if (revertedIndex < 0 || revertedIndex >= _buffer.Length)
            {
                throw new InvalidOperationException();
            }

            _bufferIndex = revertedIndex;
        }

        private string ProcessLine(string rawLine)
        {
            var setEncodingMatch = _setEncodingRegex.Match(rawLine);
            if (setEncodingMatch.Success)
            {
                ChangeEncoding(setEncodingMatch.Groups[1].Value);
            }

            return rawLine;
        }

        private void ChangeEncoding(string encodingName)
        {
            var newEncoding = EncodingEx.GetEncodingByName(encodingName);
            if (newEncoding == null || ReferenceEquals(newEncoding, _encoding) || _encoding.Equals(newEncoding))
            {
                return;
            }

            _decoder = newEncoding.GetDecoder();
            _encoding = newEncoding;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}

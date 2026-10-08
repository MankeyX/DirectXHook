using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Core.Serialization;

namespace Core.Ipc
{
    /// <summary>
    /// Sends and receives newline delimited JSON messages over a stream (a named pipe between the UI and the hook).
    /// </summary>
    public class MessageChannel : IDisposable
    {
        private readonly Stream _stream;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly object _writeLock = new object();

        public event Action<PipeMessage> MessageReceived;

        public MessageChannel(Stream stream)
        {
            var encoding = new UTF8Encoding(false);

            _stream = stream;
            _reader = new StreamReader(stream, encoding);
            _writer = new StreamWriter(stream, encoding) { AutoFlush = true };
        }

        public void Send(PipeMessage message)
        {
            var json = JsonSerializer.Serialize(message, CoreJsonContext.Default.PipeMessage);

            lock (_writeLock)
            {
                _writer.WriteLine(json);
            }
        }

        /// <summary>
        /// Reads messages until the other side disconnects.
        /// </summary>
        public void ReadMessages()
        {
            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                var message = JsonSerializer.Deserialize(line, CoreJsonContext.Default.PipeMessage);

                if (message != null)
                    MessageReceived?.Invoke(message);
            }
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}

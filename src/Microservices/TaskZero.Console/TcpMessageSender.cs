using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace TaskZero.Console
{
    public class TcpMessageSender : IMessageSender
    {
        private readonly IEventStoreConnection _connection;
        private readonly string _inputStream;

        public TcpMessageSender(IEventStoreConnection connection, string inputStream)
        {
            _connection = connection;
            _inputStream = inputStream;
        }

        public async Task<string> Post<T>(string path, T data)
        {
            if (data is Dictionary<string, String>)
            {

                await _connection.AppendToStreamAsync(_inputStream, ExpectedVersion.Any,
                    new EventData(Guid.NewGuid(), path, true, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
                        null));
                return "ok";
            }

            return "ko";
        }
    }
}

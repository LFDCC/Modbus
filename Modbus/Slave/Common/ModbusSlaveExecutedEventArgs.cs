namespace Modbus
{
    public class ModbusSlaveExecutedEventArgs : PluginEventArgs
    {
        public ModbusSlaveExecutedEventArgs(
          IDependencyClient client,
          IModbusResponse response,
          Protocol protocol,
          IModbusRequest request)
        {
            this.Client = client;
            this.Response = response;
            this.Protocol = protocol;
            this.Request = request;
        }

        public IDependencyClient Client { get; }

        public IModbusResponse Response { get; }

        public Protocol Protocol { get; }

        public IModbusRequest Request { get; }
    }
}

namespace Modbus
{
    public class ModbusSlaveExecutingEventArgs : PermitEventArgs
    {
        public ModbusSlaveExecutingEventArgs(
          IModbusRequest request,
          Protocol protocol,
          IDependencyClient client)
        {
            this.IsPermitOperation = true;
            this.Request = request;
            this.Protocol = protocol;
            this.Client = client;
        }

        public IDependencyClient Client { get; }

        public ModbusErrorCode ErrorCode { get; set; }

        public Protocol Protocol { get; }

        public IModbusRequest Request { get; }
    }
}

namespace Modbus
{
    internal class InternalModbusResponse : IModbusResponse
    {
        public InternalModbusResponse(
          ReadOnlyMemory<byte> data,
          FunctionCode functionCode,
          ModbusErrorCode errorCode,
          IModbusRequest request,
          byte slaveId)
        {
            this.Data = data;
            this.FunctionCode = functionCode;
            this.ErrorCode = errorCode;
            this.Request = request;
            this.SlaveId = slaveId;
        }

        public ReadOnlyMemory<byte> Data { get; set; }

        public ReadOnlyMemory<byte> ResponseMemory { get; set; }

        public FunctionCode FunctionCode { get; set; }

        public ModbusErrorCode ErrorCode { get; set; }

        public IModbusRequest Request { get; }

        public byte SlaveId { get; }

        public bool IsSuccess => this.ErrorCode == ModbusErrorCode.Success;
    }
}

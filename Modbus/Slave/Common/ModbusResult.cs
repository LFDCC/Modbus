namespace Modbus
{
    public readonly struct ModbusResult
    {
        public ModbusResult(ReadOnlyMemory<byte> data, ModbusErrorCode errorCode)
        {
            this.Data = data;
            this.ErrorCode = errorCode;
        }

        public ReadOnlyMemory<byte> Data { get; }

        public ModbusErrorCode ErrorCode { get; }
    }
}

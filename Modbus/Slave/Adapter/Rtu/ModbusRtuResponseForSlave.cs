namespace Modbus
{
    internal class ModbusRtuResponseForSlave : ModbusRtuBase, IBytesBuilder, IRequestInfo
    {
        private readonly ModbusErrorCode m_errorCode;

        public ModbusRtuResponseForSlave(ModbusRtuBase request, ModbusResult result)
        {
            this.StartingAddress = request.StartingAddress;
            this.Quantity = request.Quantity;
            this.ReadStartAddress = request.ReadStartAddress;
            this.ReadQuantity = request.ReadQuantity;
            this.SlaveId = request.SlaveId;
            this.FunctionCode = request.FunctionCode;
            this.Data = result.Data;
            this.m_errorCode = result.ErrorCode;
        }

        public int MaxLength => 1024;

        public ReadOnlyMemory<byte> ResponseMemory { get; private set; }

        public void Build<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
        {
            int maxLength = this.MaxLength;
            BytesWriter writer2 = new BytesWriter(writer.GetMemory(maxLength));
            byte functionCode = (byte)this.FunctionCode;
            if (this.m_errorCode == ModbusErrorCode.Success)
            {
                if (this.FunctionCode <= FunctionCode.ReadInputRegisters || this.FunctionCode == FunctionCode.ReadWriteMultipleRegisters)
                {
                    WriterExtension.WriteValue<BytesWriter, byte>(ref writer2, this.SlaveId);
                    WriterExtension.WriteValue<BytesWriter, byte>(ref writer2, functionCode);
                    WriterExtension.WriteValue<BytesWriter, byte>(ref writer2, (byte)this.Data.Length);
                    writer2.Write(this.Data.Span);
                }
                else if (this.FunctionCode == FunctionCode.WriteSingleCoil || this.FunctionCode == FunctionCode.WriteSingleRegister)
                {
                    WriterExtension.WriteValue<BytesWriter, byte>(ref writer2, this.SlaveId);
                    WriterExtension.WriteValue<BytesWriter, byte>(ref writer2, functionCode);
                    WriterExtension.WriteValue<BytesWriter, ushort>(ref writer2, this.StartingAddress, EndianType.Big);
                    writer2.Write(this.Data.Span);
                }
                else if (this.FunctionCode == FunctionCode.WriteMultipleCoils || this.FunctionCode == FunctionCode.WriteMultipleRegisters)
                {
                    WriterExtension.WriteValue<BytesWriter, byte>(ref writer2, this.SlaveId);
                    WriterExtension.WriteValue<BytesWriter, byte>(ref writer2, functionCode);
                    WriterExtension.WriteValue<BytesWriter, ushort>(ref writer2, this.StartingAddress, EndianType.Big);
                    WriterExtension.WriteValue<BytesWriter, ushort>(ref writer2, this.Quantity, EndianType.Big);
                }
            }
            else
            {
                byte num = functionCode.SetBit(7, true);
                WriterExtension.WriteValue<BytesWriter, byte>(ref writer2, this.SlaveId);
                WriterExtension.WriteValue<BytesWriter, byte>(ref writer2, num);
                WriterExtension.WriteValue<BytesWriter, byte>(ref writer2, (byte)this.m_errorCode);
            }
            this.Crc = TouchSocketModbusUtility.ToModbusCrcValue(writer2.Span);
            WriterExtension.WriteValue<BytesWriter, ushort>(ref writer2, this.Crc, EndianType.Big);
            this.ResponseMemory = (ReadOnlyMemory<byte>)writer2.Span.ToArray();
            int writtenCount = (int)writer2.WrittenCount;
            writer.Advance(writtenCount);
        }
    }
}

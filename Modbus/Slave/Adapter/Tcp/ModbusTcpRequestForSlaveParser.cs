namespace Modbus
{
    internal static class ModbusTcpRequestForSlaveParser
    {
        public const int HeaderLength = 8;

        public static FilterResult Filter<TReader>(
          ref TReader reader,
          ref ModbusTcpRequestForSlave request)
          where TReader : IBytesReader
        {
            if (reader.BytesRemaining < 8L)
                return FilterResult.Cache;
            long bytesRead = reader.BytesRead;
            ReadOnlySpan<byte> span1 = reader.GetSpan(8);
            int count = (int)TouchSocketBitConverter.BigEndian.To<ushort>(span1.Slice(4)) - 2;
            if (count < 0)
            {
                reader.Advance(1);
                return FilterResult.GoOn;
            }
            int num = 8 + count;
            if (reader.BytesRemaining < (long)num)
            {
                reader.BytesRead = bytesRead;
                return FilterResult.Cache;
            }
            ModbusTcpRequestForSlave tcpRequestForSlave = new ModbusTcpRequestForSlave(TouchSocketBitConverter.BigEndian.To<ushort>(span1), TouchSocketBitConverter.BigEndian.To<ushort>(span1.Slice(2)), span1[6], (FunctionCode)span1[7]);
            reader.Advance(8);
            ReadOnlySpan<byte> span2 = reader.GetSpan(count);
            if (tcpRequestForSlave.FunctionCode <= FunctionCode.ReadInputRegisters)
            {
                if (count < 4)
                {
                    reader.BytesRead = bytesRead;
                    reader.Advance(1);
                    return FilterResult.GoOn;
                }
                tcpRequestForSlave.StartingAddress = TouchSocketBitConverter.BigEndian.To<ushort>(span2);
                tcpRequestForSlave.Quantity = TouchSocketBitConverter.BigEndian.To<ushort>(span2.Slice(2));
            }
            else if (tcpRequestForSlave.FunctionCode == FunctionCode.WriteSingleCoil || tcpRequestForSlave.FunctionCode == FunctionCode.WriteSingleRegister)
            {
                if (count < 4)
                {
                    reader.BytesRead = bytesRead;
                    reader.Advance(1);
                    return FilterResult.GoOn;
                }
                tcpRequestForSlave.StartingAddress = TouchSocketBitConverter.BigEndian.To<ushort>(span2);
                tcpRequestForSlave.Data = (ReadOnlyMemory<byte>)span2.Slice(2).ToArray();
            }
            else if (tcpRequestForSlave.FunctionCode == FunctionCode.WriteMultipleCoils || tcpRequestForSlave.FunctionCode == FunctionCode.WriteMultipleRegisters)
            {
                if (count < 5)
                {
                    reader.BytesRead = bytesRead;
                    reader.Advance(1);
                    return FilterResult.GoOn;
                }
                tcpRequestForSlave.StartingAddress = TouchSocketBitConverter.BigEndian.To<ushort>(span2);
                tcpRequestForSlave.Quantity = TouchSocketBitConverter.BigEndian.To<ushort>(span2.Slice(2));
                tcpRequestForSlave.Data = (ReadOnlyMemory<byte>)span2.Slice(5).ToArray();
            }
            else if (tcpRequestForSlave.FunctionCode == FunctionCode.ReadWriteMultipleRegisters)
            {
                if (count < 9)
                {
                    reader.BytesRead = bytesRead;
                    reader.Advance(1);
                    return FilterResult.GoOn;
                }
                tcpRequestForSlave.ReadStartAddress = TouchSocketBitConverter.BigEndian.To<ushort>(span2);
                tcpRequestForSlave.ReadQuantity = TouchSocketBitConverter.BigEndian.To<ushort>(span2.Slice(2));
                tcpRequestForSlave.StartingAddress = TouchSocketBitConverter.BigEndian.To<ushort>(span2.Slice(4));
                tcpRequestForSlave.Quantity = TouchSocketBitConverter.BigEndian.To<ushort>(span2.Slice(6));
                tcpRequestForSlave.Data = (ReadOnlyMemory<byte>)span2.Slice(9).ToArray();
            }
            else
            {
                reader.BytesRead = bytesRead;
                reader.Advance(1);
                return FilterResult.GoOn;
            }
            reader.Advance(count);
            request = tcpRequestForSlave;
            return FilterResult.Success;
        }
    }
}

namespace Modbus
{
    internal static class ModbusRtuRequestForSlaveParser
    {
        public static FilterResult Filter<TReader>(
          ref TReader reader,
          ref ModbusRtuRequestForSlave request)
          where TReader : IBytesReader
        {
            if (reader.BytesRemaining < 2L)
                return FilterResult.Cache;
            long bytesRead = reader.BytesRead;
            byte num1 = ReaderExtension.ReadValue<TReader, byte>(ref reader);
            FunctionCode functionCode = (FunctionCode)ReaderExtension.ReadValue<TReader, byte>(ref reader);
            switch (functionCode)
            {
                case (FunctionCode)0:
                case FunctionCode.ReadCoils:
                case FunctionCode.ReadDiscreteInputs:
                case FunctionCode.ReadHoldingRegisters:
                case FunctionCode.ReadInputRegisters:
                    int num2 = 6;
                    if (reader.BytesRemaining < (long)num2)
                    {
                        reader.BytesRead = bytesRead;
                        return FilterResult.Cache;
                    }
                    ushort num3 = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                    ushort num4 = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                    ushort modbusCrcValue1 = TouchSocketModbusUtility.ToModbusCrcValue(reader.TotalSequence.Slice(bytesRead, reader.BytesRead - bytesRead));
                    ushort num5 = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                    if ((int)num5 != (int)modbusCrcValue1)
                        throw new Exception("crc验证失败");
                    request = new ModbusRtuRequestForSlave();
                    request.SlaveId = num1;
                    request.FunctionCode = functionCode;
                    request.StartingAddress = num3;
                    request.Quantity = num4;
                    request.Crc = num5;
                    return FilterResult.Success;
                case FunctionCode.WriteSingleCoil:
                case FunctionCode.WriteSingleRegister:
                    int num6 = 6;
                    if (reader.BytesRemaining < (long)num6)
                    {
                        reader.BytesRead = bytesRead;
                        return FilterResult.Cache;
                    }
                    ushort num7 = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                    byte[] numArray = new byte[2];
                    Span<byte> span = (Span<byte>)numArray;
                    reader.Read(span);
                    ushort modbusCrcValue2 = TouchSocketModbusUtility.ToModbusCrcValue(reader.TotalSequence.Slice(bytesRead, reader.BytesRead - bytesRead));
                    ushort num8 = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                    if ((int)num8 != (int)modbusCrcValue2)
                        throw new Exception("crc验证失败");
                    request = new ModbusRtuRequestForSlave();
                    request.SlaveId = num1;
                    request.FunctionCode = functionCode;
                    request.Data = (ReadOnlyMemory<byte>)numArray;
                    request.StartingAddress = num7;
                    request.Crc = num8;
                    return FilterResult.Success;
                case FunctionCode.WriteMultipleCoils:
                case FunctionCode.WriteMultipleRegisters:
                    if (reader.BytesRemaining < 5L)
                    {
                        reader.BytesRead = bytesRead;
                        return FilterResult.Cache;
                    }
                    ushort num9 = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                    ushort num10 = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                    int num11 = (int)ReaderExtension.ReadValue<TReader, byte>(ref reader) + 2;
                    if (reader.BytesRemaining < (long)num11)
                    {
                        reader.BytesRead = bytesRead;
                        return FilterResult.Cache;
                    }
                    byte[] array1 = ReaderExtension.ReadToSpan<TReader>(ref reader, num11 - 2).ToArray();
                    ushort modbusCrcValue3 = TouchSocketModbusUtility.ToModbusCrcValue(reader.TotalSequence.Slice(bytesRead, reader.BytesRead - bytesRead));
                    ushort num12 = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                    if ((int)num12 != (int)modbusCrcValue3)
                        throw new Exception("crc验证失败");
                    request = new ModbusRtuRequestForSlave();
                    request.SlaveId = num1;
                    request.FunctionCode = functionCode;
                    request.Data = (ReadOnlyMemory<byte>)array1;
                    request.StartingAddress = num9;
                    request.Quantity = num10;
                    request.Crc = num12;
                    return FilterResult.Success;
                case FunctionCode.ReadWriteMultipleRegisters:
                    if (reader.BytesRemaining < 9L)
                    {
                        reader.BytesRead = bytesRead;
                        return FilterResult.Cache;
                    }
                    ushort num13 = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                    ushort num14 = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                    ushort num15 = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                    ushort num16 = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                    int num17 = (int)ReaderExtension.ReadValue<TReader, byte>(ref reader) + 2;
                    if (reader.BytesRemaining < (long)num17)
                    {
                        reader.BytesRead = bytesRead;
                        return FilterResult.Cache;
                    }
                    byte[] array2 = ReaderExtension.ReadToSpan<TReader>(ref reader, num17 - 2).ToArray();
                    ushort modbusCrcValue4 = TouchSocketModbusUtility.ToModbusCrcValue(reader.TotalSequence.Slice(bytesRead, reader.BytesRead - bytesRead));
                    ushort num18 = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
                    if ((int)num18 != (int)modbusCrcValue4)
                        throw new Exception("crc验证失败");
                    request = new ModbusRtuRequestForSlave();
                    request.SlaveId = num1;
                    request.FunctionCode = functionCode;
                    request.Data = (ReadOnlyMemory<byte>)array2;
                    request.Quantity = num16;
                    request.StartingAddress = num15;
                    request.ReadQuantity = num14;
                    request.ReadStartAddress = num13;
                    request.Crc = num18;
                    return FilterResult.Success;
                default:
                    throw new Exception("无法识别的功能码");
            }
        }
    }
}

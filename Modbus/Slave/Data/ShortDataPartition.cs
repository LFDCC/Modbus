namespace Modbus
{
    public sealed class ShortDataPartition : DataPartition<short>
    {
        public ShortDataPartition(int startingAddress, Memory<short> values)
          : base(startingAddress, values)
        {
        }

        public ShortDataPartition(int startingAddress, int quantity)
          : this(startingAddress, (Memory<short>)new short[quantity])
        {
        }

        public override ModbusResult Read(int startingAddress, int quantity)
        {
            using (this.LockSlim.CreateReadLock())
            {
                if (startingAddress < this.StartingAddress)
                    return new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.AddressInvalid);
                if (startingAddress + quantity > this.Quantity + this.StartingAddress)
                    return new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.AddressInvalid);
                var writer = new ValueByteBlock(quantity * 2);
                try
                {
                    Span<short> span = this.Values.Slice(startingAddress - this.StartingAddress, quantity).Span;
                    for (int index = 0; index < span.Length; ++index)
                    {
                        short num = span[index];
                        WriterExtension.WriteValue<ValueByteBlock, short>(ref writer, num, EndianType.Big);
                    }
                    return new ModbusResult((ReadOnlyMemory<byte>)writer.ToArray<ValueByteBlock>(), ModbusErrorCode.Success);
                }
                finally
                {
                    writer.Dispose();
                }
            }
        }

        public override ModbusResult Write(int startingAddress, short value)
        {
            using (this.LockSlim.CreateWriteLock())
            {
                if (startingAddress < this.StartingAddress)
                    return new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.AddressInvalid);
                if (startingAddress >= this.Quantity + this.StartingAddress)
                    return new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.AddressInvalid);
                this.Values.Span[startingAddress - this.StartingAddress] = value;
                return new ModbusResult(TouchSocketBitConverter.BigEndian.GetBytes<short>(value), ModbusErrorCode.Success);
            }
        }

        public override ModbusResult Write(int startingAddress, ReadOnlySpan<short> values)
        {
            using (this.LockSlim.CreateWriteLock())
            {
                if (startingAddress < this.StartingAddress)
                    return new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.AddressInvalid);
                if (startingAddress + values.Length > this.Quantity + this.StartingAddress)
                    return new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.AddressInvalid);
                for (int index = 0; index < values.Length; ++index)
                    this.Values.Span[startingAddress - this.StartingAddress + index] = values[index];
                return new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.Success);
            }
        }
    }
}

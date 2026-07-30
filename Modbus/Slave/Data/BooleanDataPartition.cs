namespace Modbus
{
    public sealed class BooleanDataPartition : DataPartition<bool>
    {
        public BooleanDataPartition(int startingAddress, Memory<bool> values)
          : base(startingAddress, values)
        {
        }

        public BooleanDataPartition(int startingAddress, int quantity)
          : this(startingAddress, (Memory<bool>)new bool[quantity])
        {
        }

        public override ModbusResult Read(int startingAddress, int quantity)
        {
            using (this.LockSlim.CreateReadLock())
            {
                if (startingAddress < this.StartingAddress)
                    return new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.AddressInvalid);
                return startingAddress + quantity > this.Quantity + this.StartingAddress ? new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.AddressInvalid) : new ModbusResult(TouchSocketBitConverter.ConvertValues<bool, byte>((ReadOnlySpan<bool>)this.Values.Slice(startingAddress - this.StartingAddress, quantity).Span), ModbusErrorCode.Success);
            }
        }

        public override ModbusResult Write(int startingAddress, bool value)
        {
            using (this.LockSlim.CreateWriteLock())
            {
                if (startingAddress < this.StartingAddress)
                    return new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.AddressInvalid);
                if (startingAddress >= this.Quantity + this.StartingAddress)
                    return new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.AddressInvalid);
                this.Values.Span[startingAddress - this.StartingAddress] = value;
                return new ModbusResult(TouchSocketModbusUtility.BoolToBytes(value), ModbusErrorCode.Success);
            }
        }

        public override ModbusResult Write(int startingAddress, ReadOnlySpan<bool> values)
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

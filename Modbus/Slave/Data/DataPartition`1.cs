namespace Modbus
{
    public abstract class DataPartition<T>
    {
        public DataPartition(int startingAddress, Memory<T> values)
        {
            this.StartingAddress = startingAddress;
            this.Values = values;
        }

        public ReaderWriterLockSlim LockSlim { get; } = new ReaderWriterLockSlim();

        public int StartingAddress { get; }

        public Memory<T> Values { get; }

        public int Quantity => this.Values.Length;

        public abstract ModbusResult Read(int startingAddress, int quantity);

        public abstract ModbusResult Write(int startingAddress, T value);

        public abstract ModbusResult Write(int startingAddress, ReadOnlySpan<T> values);
    }
}

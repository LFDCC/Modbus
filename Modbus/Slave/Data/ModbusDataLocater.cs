namespace Modbus
{
    public class ModbusDataLocater : IModbusDataLocater
    {
        public ModbusDataLocater(
          int coilsQuantity,
          int discreteInputsQuantity,
          int holdingRegistersQuantity,
          int inputRegistersQuantity)
        {
            this.Coils = new BooleanDataPartition(0, coilsQuantity);
            this.DiscreteInputs = new BooleanDataPartition(0, discreteInputsQuantity);
            this.HoldingRegisters = new ShortDataPartition(0, holdingRegistersQuantity);
            this.InputRegisters = new ShortDataPartition(0, inputRegistersQuantity);
        }

        public ModbusDataLocater() { }

        public BooleanDataPartition Coils { get; set; }

        public BooleanDataPartition DiscreteInputs { get; set; }

        public ShortDataPartition HoldingRegisters { get; set; }

        public ShortDataPartition InputRegisters { get; set; }

        public virtual Task<ModbusResult> ExecuteAsync(
          IModbusRequest modbusRequest,
          CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult<ModbusResult>(this.PrivateExecute(modbusRequest));
        }

        private ModbusResult PrivateExecute(IModbusRequest modbusRequest)
        {
            FunctionCode functionCode = modbusRequest.FunctionCode;
            if ((uint)functionCode <= 15U)
            {
                switch (functionCode)
                {
                    case FunctionCode.ReadCoils:
                        return this.Coils.Read((int)modbusRequest.StartingAddress, (int)modbusRequest.Quantity);
                    case FunctionCode.ReadDiscreteInputs:
                        return this.DiscreteInputs.Read((int)modbusRequest.StartingAddress, (int)modbusRequest.Quantity);
                    case FunctionCode.ReadHoldingRegisters:
                        return this.HoldingRegisters.Read((int)modbusRequest.StartingAddress, (int)modbusRequest.Quantity);
                    case FunctionCode.ReadInputRegisters:
                        return this.InputRegisters.Read((int)modbusRequest.StartingAddress, (int)modbusRequest.Quantity);
                    case FunctionCode.WriteSingleCoil:
                        return this.Coils.Write((int)modbusRequest.StartingAddress, Convert.ToBoolean(TouchSocketBitConverter.Default.To<ushort>(modbusRequest.Data.Span)));
                    case FunctionCode.WriteSingleRegister:
                        return this.HoldingRegisters.Write((int)modbusRequest.StartingAddress, TouchSocketBitConverter.BigEndian.To<short>(modbusRequest.Data.Span));
                    case FunctionCode.WriteMultipleCoils:
                        ReadOnlyMemory<bool> readOnlyMemory = TouchSocketBitConverter.Default.ToValues<bool>(modbusRequest.Data.Span).Slice(0, (int)modbusRequest.Quantity);
                        return this.Coils.Write((int)modbusRequest.StartingAddress, readOnlyMemory.Span);
                }
            }
            else
            {
                switch (functionCode)
                {
                    case FunctionCode.WriteMultipleRegisters:
                        return this.HoldingRegisters.Write((int)modbusRequest.StartingAddress, (ReadOnlySpan<short>)TouchSocketBitConverter.BigEndian.ToValues<short>(modbusRequest.Data.Span).ToArray());
                    case FunctionCode.ReadWriteMultipleRegisters:
                        if (!(modbusRequest is IModbusReadWriteRequest readWriteRequest))
                            return new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.TaskError);
                        if ((int)modbusRequest.StartingAddress + (int)modbusRequest.Quantity > this.HoldingRegisters.Quantity + this.HoldingRegisters.StartingAddress)
                            return new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.AddressInvalid);
                        if ((int)readWriteRequest.ReadStartAddress + (int)readWriteRequest.ReadQuantity > this.HoldingRegisters.Quantity + this.HoldingRegisters.StartingAddress)
                            return new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.AddressInvalid);
                        ModbusResult modbusResult = this.HoldingRegisters.Write((int)modbusRequest.StartingAddress, (ReadOnlySpan<short>)TouchSocketBitConverter.BigEndian.ToValues<short>(modbusRequest.Data.Span).ToArray());
                        return modbusResult.ErrorCode != ModbusErrorCode.Success ? modbusResult : this.HoldingRegisters.Read((int)readWriteRequest.ReadStartAddress, (int)readWriteRequest.ReadQuantity);
                }
            }
            return new ModbusResult(new ReadOnlyMemory<byte>(), ModbusErrorCode.TaskError);
        }
    }
}

namespace Modbus
{
    public interface IModbusDataLocater
    {
        BooleanDataPartition Coils { get; set; }

        BooleanDataPartition DiscreteInputs { get; set; }

        ShortDataPartition HoldingRegisters { get; set; }

        ShortDataPartition InputRegisters { get; set; }

        Task<ModbusResult> ExecuteAsync(
          IModbusRequest modbusRequest,
          CancellationToken cancellationToken = default(CancellationToken));
    }
}

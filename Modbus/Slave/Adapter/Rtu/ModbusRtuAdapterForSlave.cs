namespace Modbus
{
    internal class ModbusRtuAdapterForSlave : CustomDataHandlingAdapter<ModbusRtuRequestForSlave>
    {
        public override bool CanSendRequestInfo => true;

        protected override FilterResult Filter<TReader>(
          ref TReader reader,
          bool _,
          ref ModbusRtuRequestForSlave request)
        {
            return ModbusRtuRequestForSlaveParser.Filter<TReader>(ref reader, ref request);
        }
    }
}

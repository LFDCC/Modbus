namespace Modbus
{
    internal class ModbusTcpAdapterForSlave : CustomDataHandlingAdapter<ModbusTcpRequestForSlave>
    {
        public override bool CanSendRequestInfo => true;

        protected override FilterResult Filter<TReader>(
          ref TReader reader,
          bool _,
          ref ModbusTcpRequestForSlave request)
        {
            return ModbusTcpRequestForSlaveParser.Filter<TReader>(ref reader, ref request);
        }
    }
}

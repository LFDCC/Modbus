namespace Modbus
{
    public class ModbusTcpSlave :
      TcpServiceBase<ModbusTcpSlaveSessionClient>,
      IModbusTcpSlave,
      IServiceBase,
      ISetupConfigObject,
      IResolverConfigObject,
      IConfigObject,
      IDependencyObject,
      IDisposableObject,
      IDisposable,
      ILoggerObject,
      IPluginObject,
      IResolverObject,
      IModbusSlave
    {
        protected override ModbusTcpSlaveSessionClient NewClient()
        {
            return (ModbusTcpSlaveSessionClient)new ModbusTcpSlave.PrivateModbusTcpSlaveSessionClient();
        }

        private class PrivateModbusTcpSlaveSessionClient : ModbusTcpSlaveSessionClient
        {
        }
    }
}

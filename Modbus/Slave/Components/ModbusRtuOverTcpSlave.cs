namespace Modbus
{
    public class ModbusRtuOverTcpSlave :
      TcpServiceBase<ModbusRtuOverTcpSlaveSessionClient>,
      IModbusRtuOverTcpSlave,
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
        protected override ModbusRtuOverTcpSlaveSessionClient NewClient()
        {
            return (ModbusRtuOverTcpSlaveSessionClient)new ModbusRtuOverTcpSlave.PrivateModbusRtuOverTcpSlaveSessionClient();
        }

        private class PrivateModbusRtuOverTcpSlaveSessionClient : ModbusRtuOverTcpSlaveSessionClient
        {
        }
    }
}

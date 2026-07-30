namespace Modbus
{
    public static class ModbusSlaveExtension
    {
        public static IModbusSlavePoint GetSlavePointBySlaveId(
          this IModbusSlave modbusSlave,
          byte slaveId)
        {
            foreach (IPlugin plugin in modbusSlave.PluginManager.Plugins)
            {
                if (plugin is IModbusSlavePoint slavePointBySlaveId && (int)slavePointBySlaveId.SlaveId == (int)slaveId)
                    return slavePointBySlaveId;
            }
            return (IModbusSlavePoint)null;
        }

    }
}

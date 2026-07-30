namespace Modbus
{
    internal static class ModbusPluginRaiseExtension
    {
        public static ValueTask<bool> RaiseIModbusSlaveExecutedPluginAsync(
            this IPluginManager pluginManager,
            IResolver resolver,
            IModbusSlavePoint sender,
            ModbusSlaveExecutedEventArgs e)
        {
            return pluginManager.RaiseAsync(typeof(IModbusSlaveExecutedPlugin), resolver, (object)sender, (PluginEventArgs)e);
        }

        public static ValueTask<bool> RaiseIModbusSlaveExecutingPluginAsync(
            this IPluginManager pluginManager,
            IResolver resolver,
            IModbusSlavePoint sender,
            ModbusSlaveExecutingEventArgs e)
        {
            return pluginManager.RaiseAsync(typeof(IModbusSlaveExecutingPlugin), resolver, (object)sender, (PluginEventArgs)e);
        }
    }
}

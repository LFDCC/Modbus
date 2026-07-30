namespace Modbus
{
    /// <summary>
    /// 从站点（SlavePoint）相关的扩展方法。
    /// </summary>
    public static class ModbusSlavePointExtension
    {
        /// <summary>
        /// 向 Modbus 从站（Tcp/Udp/Rtu 等）注册一个从站点。
        /// 该方法会基于 <see cref="ModbusSlavePointOption"/> 创建 <see cref="ModbusSlavePoint"/> 并加入插件管理器。
        /// </summary>
        /// <param name="pluginManager">从站对象的插件管理器（例如 <c>ModbusTcpSlave.PluginManager</c>）。</param>
        /// <param name="configure">用于配置站点号、数据区等参数的回调。</param>
        /// <returns>同一个插件管理器，便于链式调用。</returns>
        public static IPluginManager AddModbusSlavePoint(
            this IPluginManager pluginManager,
            Action<ModbusSlavePointOption> configure)
        {
            if (pluginManager == null)
            {
                throw new ArgumentNullException(nameof(pluginManager));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var option = new ModbusSlavePointOption();
            configure(option);
            pluginManager.Add<ModbusSlavePoint>(new ModbusSlavePoint(option));
            return pluginManager;
        }
    }
}

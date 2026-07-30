namespace Modbus;

/// <summary>
/// 针对 <see cref="ISetupConfigObject"/> 的配置扩展。
/// Modbus 主站与从站均实现该接口，因此在此统一定义一次即可同时惠及两端，
/// 调用处无需显式 new TouchSocketConfig()。
/// </summary>
public static class SetupConfigObjectExtension
{
    /// <summary>
    /// 以 lambda 形式配置并初始化实现 <see cref="ISetupConfigObject"/> 的对象（如 Modbus 主站/从站），
    /// 调用处无需显式引用 TouchSocketConfig。
    /// </summary>
    /// <param name="configObject">配置对象（主站或从站实例）</param>
    /// <param name="configure">配置回调，参数为内部新建的 TouchSocketConfig 实例</param>
    /// <returns>初始化任务</returns>
    public static Task SetupAsync(this ISetupConfigObject configObject, Action<TouchSocketConfig> configure)
    {
        var config = new TouchSocketConfig();
        configure?.Invoke(config);
        return configObject.SetupAsync(config);
    }

    /// <summary>
    /// 以 lambda 形式配置并初始化，等价于 <see cref="SetupAsync(ISetupConfigObject, Action{TouchSocketConfig})"/>。
    /// 命名更贴近“配置”语义，按需选用。
    /// </summary>
    /// <param name="configObject">配置对象（主站或从站实例）</param>
    /// <param name="configure">配置回调，参数为内部新建的 TouchSocketConfig 实例</param>
    /// <returns>初始化任务</returns>
    public static Task SetConfig(this ISetupConfigObject configObject, Action<TouchSocketConfig> configure)
        => configObject.SetupAsync(configure);
}

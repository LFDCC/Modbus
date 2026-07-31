using System;
using System.Threading;
using System.Threading.Tasks;

using Modbus;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Example.SlaveExceptions
{
    /// <summary>
    /// 从站异常捕获示例。
    /// 演示 Master 端如何区分以下异常场景：
    /// 1. SlaveId 不存在 —— Slave 静默丢弃请求，Master 超时。通过 catch(TimeoutException) + master.Online 判断。
    /// 2. 非法数据地址 —— Slave 返回 Modbus 异常码，Master 抛 ModbusResponseException。
    /// 3. 链路断开 —— Master 发送时或等待响应时连接已断，Online == false。
    /// 4. 正常读写 —— 对比基线。
    /// </summary>
    public class Driver
    {
        private const int ServerPort = 51010;
        private const byte ExistingSlaveId = 1;

        private static async Task<int> Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => cts.Cancel();

            try { await RunAsync(cts.Token).ConfigureAwait(false); }
            catch (Exception e) { Console.WriteLine($"运行出错: {e.Message}"); }

            Console.WriteLine("按任意键退出...");
            if (!Console.IsInputRedirected) { Console.ReadKey(); }
            return 0;
        }

        private static async Task RunAsync(CancellationToken ct)
        {
            Console.WriteLine("=== 从站异常捕获示例 ===\n");

            // --- 从站初始化 ---
            var dataLocater = new ModbusDataLocater(20, 20, 20, 20);
            dataLocater.HoldingRegisters.Write(0, new short[] { 100, 200, 300, 400 });

            var slave = new ModbusTcpSlave();
            await slave.SetupAsync(config =>
            {
                config.SetListenIPHosts(new IPHost[] { new IPHost($"127.0.0.1:{ServerPort}") });
                config.ConfigurePlugins(a =>
                {
                    a.AddModbusSlavePoint(options =>
                    {
                        options.SlaveId = ExistingSlaveId;
                        options.IgnoreSlaveId = false; // 严格匹配 SlaveId
                        options.DataLocater = dataLocater;
                    });
                });
            }).ConfigureAwait(false);
            await slave.StartAsync(CancellationToken.None).ConfigureAwait(false);
            Console.WriteLine($"[从站] 监听 127.0.0.1:{ServerPort}, SlaveId={ExistingSlaveId} (仅保持寄存器 0-3 有效)\n");

            // --- 主站初始化 ---
            using var master = new ModbusTcpMaster();
            await master.SetupAsync(config =>
                config.SetRemoteIPHost(new IPHost($"127.0.0.1:{ServerPort}"))).ConfigureAwait(false);
            await master.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
            Console.WriteLine($"[主站] 已连接, Online={master.Online}\n");

            try
            {
                // ========== 场景 1：正常读写（基线） ==========
                await Scenario1_NormalRead(master).ConfigureAwait(false);

                // ========== 场景 2：SlaveId 不存在 ==========
                await Scenario2_NonExistentSlaveId(master).ConfigureAwait(false);

                // ========== 场景 3：非法数据地址 ==========
                await Scenario3_IllegalAddress(master).ConfigureAwait(false);

                // ========== 场景 4：链路断开 ==========
                await Scenario4_LinkDisconnected(master, slave).ConfigureAwait(false);
            }
            finally
            {
                // 场景 4 可能已经关闭了 slave，这里安全清理
                try
                {
                    await slave.StopAsync(CancellationToken.None).ConfigureAwait(false);
                }
                catch { /* slave 可能已停止 */ }
                slave.Dispose();
            }

            Console.WriteLine("\n=== 示例结束 ===");
        }

        // ====================================================================
        // 场景 1：正常读写（基线对照）
        // ====================================================================
        private static async Task Scenario1_NormalRead(ModbusTcpMaster master)
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("场景 1: 正常读写（SlaveId=1, 地址=0, 数量=4）");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            try
            {
                var response = await master.ReadHoldingRegistersAsync(
                    ExistingSlaveId, 0, 4, 1000, default).ConfigureAwait(false);

                var shorts = response.Data.ToShorts();
                Console.Write("  读取结果: ");
                for (int i = 0; i < shorts.Length; i++)
                {
                    Console.Write($"[{i}]={shorts[i]} ");
                }
                Console.WriteLine();
                Console.WriteLine("  ✓ 正常响应\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ 意外异常: {ex.GetType().Name}: {ex.Message}\n");
            }
        }

        // ====================================================================
        // 场景 2：SlaveId 不存在 → Slave 静默丢弃 → Master 超时
        // 核心逻辑：catch(TimeoutException) { if (master.Online) → 从站异常 }
        // ====================================================================
        private static async Task Scenario2_NonExistentSlaveId(ModbusTcpMaster master)
        {
            byte ghostSlaveId = 99; // 从站不存在此 SlaveId

            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"场景 2: SlaveId 不存在（SlaveId={ghostSlaveId}）");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            try
            {
                // 使用 500ms 超时快速失败
                var response = await master.ReadHoldingRegistersAsync(
                    ghostSlaveId, 0, 4, 500, default).ConfigureAwait(false);

                // 如果没超时但收到响应（理论上不会发生，因为 Slave 不回复）
                Console.WriteLine($"  收到响应（意外）: ErrorCode={response.ErrorCode}");
            }
            catch (TimeoutException ex)
            {
                // ★ 核心判断逻辑 ★
                Console.WriteLine($"  捕获 TimeoutException: {ex.Message}");
                Console.WriteLine($"  master.Online = {master.Online}");

                if (master.Online)
                {
                    Console.WriteLine("  → Master 在线但超时 → 从站异常");
                    Console.WriteLine("    原因分析:");
                    Console.WriteLine("    · 链路正常（TCP 连接存活）");
                    Console.WriteLine("    · 从站未回复 → SlaveId 不存在/从站未运行/从站忙");
                    Console.WriteLine("    · 区分方法: 先用已知 SlaveId 探测确认链路+从站正常");
                    Console.WriteLine("  ✗ 超时（从站无响应，可能是 SlaveId 不存在）\n");
                }
                else
                {
                    Console.WriteLine("  → Master 离线 → 链路异常（TCP 连接已断开）");
                    Console.WriteLine("  ✗ 超时（链路断开）\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ 其他异常: {ex.GetType().Name}: {ex.Message}\n");
            }
        }

        // ====================================================================
        // 场景 3：非法数据地址 → Slave 返回 Modbus 异常码 → ModbusResponseException
        // ====================================================================
        private static async Task Scenario3_IllegalAddress(ModbusTcpMaster master)
        {
            ushort illegalAddr = 100; // 超出 dataLocater 的 20 寄存器范围

            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"场景 3: 非法数据地址（SlaveId={ExistingSlaveId}, 地址={illegalAddr}）");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            try
            {
                var response = await master.ReadHoldingRegistersAsync(
                    ExistingSlaveId, illegalAddr, 4, 1000, default).ConfigureAwait(false);

                Console.WriteLine($"  收到响应（意外）: ErrorCode={response.ErrorCode}");
            }
            catch (ModbusResponseException ex)
            {
                Console.WriteLine($"  捕获 ModbusResponseException");
                Console.WriteLine($"  ErrorCode = {ex.ErrorCode} ({(byte)ex.ErrorCode})");
                Console.WriteLine($"  Message   = {ex.Message}");
                Console.WriteLine($"  master.Online = {master.Online}");

                switch (ex.ErrorCode)
                {
                    case ModbusErrorCode.AddressInvalid:
                        Console.WriteLine("  → 非法数据地址（地址超出从站寄存器范围）");
                        break;
                    case ModbusErrorCode.FunctionCodeNotDefined:
                        Console.WriteLine("  → 功能码不支持");
                        break;
                    case ModbusErrorCode.ValueInvalid:
                        Console.WriteLine("  → 非法数据值");
                        break;
                    case ModbusErrorCode.GatewayUnavailable:
                        Console.WriteLine("  → 网关下游从站无响应");
                        break;
                    default:
                        Console.WriteLine($"  → 其他 Modbus 异常: {ex.ErrorCode}");
                        break;
                }
                Console.WriteLine("  ✗ 从站返回 Modbus 异常码（非法地址）\n");
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"  捕获 TimeoutException: {ex.Message}");
                Console.WriteLine($"  master.Online = {master.Online}");
                if (master.Online)
                {
                    Console.WriteLine("  → 从站未返回异常码，直接超时（可能地址未触发异常逻辑）");
                }
                Console.WriteLine("  ✗ 超时\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ 其他异常: {ex.GetType().Name}: {ex.Message}\n");
            }
        }

        // ====================================================================
        // 场景 4：链路断开 → Online == false
        // ====================================================================
        private static async Task Scenario4_LinkDisconnected(ModbusTcpMaster master, ModbusTcpSlave slave)
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("场景 4: 链路断开（从站停止 → 主站读取）");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            // 先停止从站，模拟链路断开
            Console.WriteLine("  [操作] 停止从站...");
            await slave.StopAsync(CancellationToken.None).ConfigureAwait(false);
            Console.WriteLine($"  master.Online = {master.Online} (停止从站后，TCP 可能仍保持短暂存活)");

            // 等待一小段时间让 TCP 连接感知断开
            await Task.Delay(200).ConfigureAwait(false);
            Console.WriteLine($"  master.Online = {master.Online} (延迟 200ms 后)");

            try
            {
                var response = await master.ReadHoldingRegistersAsync(
                    ExistingSlaveId, 0, 4, 1000, default).ConfigureAwait(false);

                Console.WriteLine($"  收到响应（意外）: ErrorCode={response.ErrorCode}");
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"  捕获 TimeoutException: {ex.Message}");
                Console.WriteLine($"  master.Online = {master.Online}");

                if (!master.Online)
                {
                    Console.WriteLine("  → Master 离线 → 链路异常");
                    Console.WriteLine("    原因分析: TCP 连接已断开（对端关闭/网络中断）");
                    Console.WriteLine("    与场景 2 的区别: Online == false 即可判断为链路问题");
                }
                else
                {
                    Console.WriteLine("  → Master 仍在线 → TCP 连接尚未感知断开");
                    Console.WriteLine("    （TCP keepalive 可能需要更长时间才能检测到断开）");
                }
                Console.WriteLine("  ✗ 链路断开\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  捕获 {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"  master.Online = {master.Online}");
                Console.WriteLine("  ✗ 链路断开（其他异常形式）\n");
            }
        }
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Modbus;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Example.WriteMultipleCoils
{
    /// <summary>
    /// FC15 — 写多个线圈（Write Multiple Coils）示例。
    /// 批量写入线圈状态，然后回读验证。
    /// </summary>
    public class Driver
    {
        private const int ServerPort = 51007;
        private const byte SlaveId = 1;

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
            Console.WriteLine("=== FC15 写多个线圈（Write Multiple Coils） ===");

            // --- 从站初始化 ---
            var dataLocater = new ModbusDataLocater(20, 20, 20, 20);
            dataLocater.Coils.Write(0, new bool[16]);

            var slave = new ModbusTcpSlave();
            await slave.SetupAsync(config =>
            {
                config.SetListenIPHosts(new IPHost[] { new IPHost($"127.0.0.1:{ServerPort}") });
                config.ConfigurePlugins(a =>
                {
                    a.AddModbusSlavePoint(options =>
                    {
                        options.SlaveId = SlaveId;
                        options.IgnoreSlaveId = false;
                        options.DataLocater = dataLocater;
                    });
                });
            }).ConfigureAwait(false);
            await slave.StartAsync(CancellationToken.None).ConfigureAwait(false);
            Console.WriteLine($"[从站] 监听 127.0.0.1:{ServerPort}, SlaveId={SlaveId}");

            // --- 主站初始化 ---
            using var master = new ModbusTcpMaster();
            await master.SetupAsync(config =>
                config.SetRemoteIPHost(new IPHost($"127.0.0.1:{ServerPort}"))).ConfigureAwait(false);
            await master.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
            Console.WriteLine("[主站] 已连接");

            try
            {
                // 写入 10 个线圈
                var values = new bool[] { true, true, false, true, false, true, true, false, true, false };
                Console.WriteLine($"\n写入 {values.Length} 个线圈，起始地址=0");
                Console.WriteLine("写入值: " + string.Join(", ", values.Select(v => v ? "ON" : "OFF")));

                await master.WriteMultipleCoilsAsync(SlaveId, 0, new ReadOnlyMemory<bool>(values)).ConfigureAwait(false);

                // 回读验证
                var readBack = await master.ReadCoilsAsync(SlaveId, 0, (ushort)values.Length).ConfigureAwait(false);
                Console.WriteLine("\n回读验证:");
                for (int i = 0; i < values.Length; i++)
                {
                    bool match = values[i] == readBack.Span[i];
                    Console.WriteLine($"  线圈[{i}]: 写入={values[i]}, 读取={readBack.Span[i]} {(match ? "OK" : "MISMATCH")}");
                }

                // 带超时和取消的重载
                Console.WriteLine("\n带超时重载写入 (500ms):");
                var values2 = new bool[] { false, false, true, true };
                await master.WriteMultipleCoilsAsync(SlaveId, 10, new ReadOnlyMemory<bool>(values2), 500, ct).ConfigureAwait(false);

                var readBack2 = await master.ReadCoilsAsync(SlaveId, 10, 4).ConfigureAwait(false);
                Console.WriteLine($"  线圈[10..13]: {string.Join(", ", Enumerable.Range(0, 4).Select(i => readBack2.Span[i] ? "ON" : "OFF"))}");
            }
            finally
            {
                await slave.StopAsync(CancellationToken.None).ConfigureAwait(false);
                slave.Dispose();
            }

            Console.WriteLine("\n=== 示例结束 ===");
        }
    }
}

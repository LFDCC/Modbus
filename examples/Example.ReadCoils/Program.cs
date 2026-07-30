using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Modbus;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Example.ReadCoils
{
    /// <summary>
    /// FC01 — 读线圈（Read Coils）示例。
    /// 启动本地 TCP 从站，预置线圈数据，主站读取并打印。
    /// </summary>
    public class Driver
    {
        private const int ServerPort = 51001;
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
            Console.WriteLine("=== FC01 读线圈（Read Coils） ===");

            // --- 从站初始化 ---
            var dataLocater = new ModbusDataLocater(20, 20, 20, 20);
            dataLocater.Coils.Write(0, new bool[] { true, false, true, true, false, true, false, false });

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
                // 读取 8 个线圈，地址从 0 开始
                ushort startAddr = 0;
                ushort quantity = 8;

                Console.WriteLine($"\n读取线圈: 起始地址={startAddr}, 数量={quantity}");
                var coils = await master.ReadCoilsAsync(SlaveId, startAddr, quantity).ConfigureAwait(false);

                Console.WriteLine("结果:");
                for (int i = 0; i < coils.Length; i++)
                {
                    Console.WriteLine($"  地址 {startAddr + i}: {(coils.Span[i] ? "ON" : "OFF")}");
                }

                // 也支持带超时和取消令牌的重载
                Console.WriteLine("\n带超时重载读取 (500ms):");
                var coils2 = await master.ReadCoilsAsync(SlaveId, startAddr, quantity, 500, ct).ConfigureAwait(false);
                Console.WriteLine($"  读取到 {coils2.Length} 个线圈状态");
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

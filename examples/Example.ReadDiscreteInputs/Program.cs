using System;
using System.Threading;
using System.Threading.Tasks;

using Modbus;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Example.ReadDiscreteInputs
{
    /// <summary>
    /// FC02 — 读离散输入（Read Discrete Inputs）示例。
    /// 离散输入是只读的，从站预置后主站只能读取不能写入。
    /// </summary>
    public class Driver
    {
        private const int ServerPort = 51002;
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
            Console.WriteLine("=== FC02 读离散输入（Read Discrete Inputs） ===");

            // --- 从站初始化 ---
            var dataLocater = new ModbusDataLocater(20, 20, 20, 20);
            dataLocater.DiscreteInputs.Write(0, new bool[] { true, true, false, true, false, false, true, true });

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
                ushort startAddr = 0;
                ushort quantity = 8;

                Console.WriteLine($"\n读离散输入: 起始地址={startAddr}, 数量={quantity}");
                var inputs = await master.ReadDiscreteInputsAsync(SlaveId, startAddr, quantity).ConfigureAwait(false);

                Console.WriteLine("结果:");
                for (int i = 0; i < inputs.Length; i++)
                {
                    Console.WriteLine($"  地址 {startAddr + i}: {(inputs.Span[i] ? "ON" : "OFF")}");
                }

                // 读取部分
                Console.WriteLine("\n读取前 4 个:");
                var inputs2 = await master.ReadDiscreteInputsAsync(SlaveId, 0, 4).ConfigureAwait(false);
                for (int i = 0; i < inputs2.Length; i++)
                {
                    Console.WriteLine($"  地址 {i}: {(inputs2.Span[i] ? "ON" : "OFF")}");
                }
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

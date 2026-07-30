using System;
using System.Threading;
using System.Threading.Tasks;

using Modbus;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Example.ReadInputRegisters
{
    /// <summary>
    /// FC04 — 读输入寄存器（Read Input Registers）示例。
    /// 输入寄存器是只读的，常用于传感器数据。
    /// </summary>
    public class Driver
    {
        private const int ServerPort = 51004;
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
            Console.WriteLine("=== FC04 读输入寄存器（Read Input Registers） ===");

            // --- 从站初始化 ---
            var dataLocater = new ModbusDataLocater(20, 20, 20, 20);
            dataLocater.InputRegisters.Write(0, new short[] { 100, 200, 300, 400, 500, 600 });

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
                ushort quantity = 6;

                Console.WriteLine($"\n读输入寄存器: 起始地址={startAddr}, 数量={quantity}");
                var response = await master.ReadInputRegistersAsync(SlaveId, startAddr, quantity).ConfigureAwait(false);

                // 解析为 short[]
                var shorts = response.Data.ToShorts();
                Console.WriteLine("结果 (short[]):");
                for (int i = 0; i < shorts.Length; i++)
                {
                    Console.WriteLine($"  输入寄存器[{startAddr + i}] = {shorts[i]}");
                }

                // 解析为 ushort[]
                var ushorts = response.Data.ToUshorts();
                Console.WriteLine("\n结果 (ushort[]):");
                for (int i = 0; i < ushorts.Length; i++)
                {
                    Console.WriteLine($"  输入寄存器[{startAddr + i}] = {ushorts[i]}");
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

using System;
using System.Threading;
using System.Threading.Tasks;

using Modbus;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Example.WriteSingleRegister
{
    /// <summary>
    /// FC06 — 写单个寄存器（Write Single Register）示例。
    /// 演示 short / ushort 两种重载，写入后回读验证。
    /// </summary>
    public class Driver
    {
        private const int ServerPort = 51006;
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
            Console.WriteLine("=== FC06 写单个寄存器（Write Single Register） ===");

            // --- 从站初始化 ---
            var dataLocater = new ModbusDataLocater(20, 20, 20, 20);
            dataLocater.HoldingRegisters.Write(0, new short[] { 0, 0, 0, 0 });

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
                // 使用 short 重载写入
                Console.WriteLine("\n--- 使用 short 重载 ---");
                short valShort = 12345;
                Console.WriteLine($"写入 寄存器[0] = {valShort} (short)");
                await master.WriteSingleRegisterAsync(SlaveId, 0, valShort).ConfigureAwait(false);

                // 使用 ushort 重载写入
                Console.WriteLine("\n--- 使用 ushort 重载 ---");
                ushort valUshort = 60000;
                Console.WriteLine($"写入 寄存器[1] = {valUshort} (ushort)");
                await master.WriteSingleRegisterAsync(SlaveId, 1, valUshort).ConfigureAwait(false);

                // 写负数
                short valNeg = -100;
                Console.WriteLine($"写入 寄存器[2] = {valNeg} (short, 负数)");
                await master.WriteSingleRegisterAsync(SlaveId, 2, valNeg).ConfigureAwait(false);

                // 回读验证
                Console.WriteLine("\n--- 回读验证 ---");
                var response = await master.ReadHoldingRegistersAsync(SlaveId, 0, 4).ConfigureAwait(false);
                var shorts = response.Data.ToShorts();
                var ushorts = response.Data.ToUshorts();

                for (int i = 0; i < shorts.Length; i++)
                {
                    Console.WriteLine($"  寄存器[{i}] = {shorts[i]} (short) / {ushorts[i]} (ushort)");
                }

                // 带超时和取消的重载
                Console.WriteLine("\n--- 带超时重载 ---");
                await master.WriteSingleRegisterAsync(SlaveId, 3, (short)999, 500, ct).ConfigureAwait(false);
                var resp = await master.ReadHoldingRegistersAsync(SlaveId, 3, 1).ConfigureAwait(false);
                Console.WriteLine($"  寄存器[3] = {resp.Data.ToShort()}");
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

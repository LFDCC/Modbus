using System;
using System.Threading;
using System.Threading.Tasks;

using Modbus;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Example.ReadWriteMultipleRegisters
{
    /// <summary>
    /// FC23 — 读写多个寄存器（Read/Write Multiple Registers）示例。
    /// 在一次请求中同时完成写入和读取操作，常用于需要"写后立即读"的高效场景。
    /// </summary>
    public class Driver
    {
        private const int ServerPort = 51009;
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
            Console.WriteLine("=== FC23 读写多个寄存器（Read/Write Multiple Registers） ===");

            // --- 从站初始化 ---
            var dataLocater = new ModbusDataLocater(20, 20, 20, 20);
            dataLocater.HoldingRegisters.Write(0, new short[] { 11, 22, 33, 44, 55, 66, 77, 88, 99, 100 });

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
                // FC23: 在一次请求中
                //   - 读取: 起始地址 0, 数量 4 (读到原始数据 11, 22, 33, 44)
                //   - 写入: 起始地址 5, 写入 {999, 888, 777, 666}
                ushort readStart = 0;
                ushort readQty = 4;
                ushort writeStart = 5;
                var writeData = new short[] { 999, 888, 777, 666 };
                var writeBytes = writeData.ToMemoryBytes();

                Console.WriteLine($"\nFC23 操作:");
                Console.WriteLine($"  读取: 地址={readStart}, 数量={readQty}");
                Console.WriteLine($"  写入: 地址={writeStart}, 数据=[{string.Join(", ", writeData)}]");

                var response = await master.ReadWriteMultipleRegistersAsync(
                    SlaveId, readStart, readQty, writeStart, writeBytes).ConfigureAwait(false);

                // response.Data 是读取到的数据
                var readShorts = response.Data.ToShorts();
                Console.WriteLine($"\n读取结果: [{string.Join(", ", readShorts)}]");

                // 回读写入区域，验证写入成功
                Console.WriteLine("\n回读写入区域验证:");
                var verifyResp = await master.ReadHoldingRegistersAsync(SlaveId, writeStart, (ushort)writeData.Length).ConfigureAwait(false);
                var verifyShorts = verifyResp.Data.ToShorts();
                Console.WriteLine($"  寄存器[{writeStart}..{writeStart + writeData.Length - 1}]: [{string.Join(", ", verifyShorts)}]");

                // 带超时和取消的重载
                Console.WriteLine("\n--- 带超时重载 (800ms) ---");
                var resp2 = await master.ReadWriteMultipleRegistersAsync(
                    SlaveId, 0, 2, 0, new short[] { 111, 222 }.ToMemoryBytes(), 800, ct).ConfigureAwait(false);
                Console.WriteLine($"  读取结果: [{string.Join(", ", resp2.Data.ToShorts())}]");
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

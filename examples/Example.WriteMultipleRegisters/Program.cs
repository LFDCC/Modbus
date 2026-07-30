using System;
using System.Threading;
using System.Threading.Tasks;

using Modbus;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Example.WriteMultipleRegisters
{
    /// <summary>
    /// FC16 — 写多个寄存器（Write Multiple Registers）示例。
    /// 演示使用 ByteConverterExtension 将 short[]/ushort[] 序列化为字节后批量写入。
    /// </summary>
    public class Driver
    {
        private const int ServerPort = 51008;
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
            Console.WriteLine("=== FC16 写多个寄存器（Write Multiple Registers） ===");

            // --- 从站初始化 ---
            var dataLocater = new ModbusDataLocater(20, 20, 20, 20);
            dataLocater.HoldingRegisters.Write(0, new short[10]);

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
                // 方式1: 使用 short[] → ToMemoryBytes() → WriteMultipleRegistersAsync
                Console.WriteLine("\n--- 方式1: short[] → 字节序列 ---");
                var shortArr = new short[] { 100, 200, 300, -100, -200 };
                var bytesShort = shortArr.ToMemoryBytes();
                Console.WriteLine($"写入 {shortArr.Length} 个 short: [{string.Join(", ", shortArr)}]");
                await master.WriteMultipleRegistersAsync(SlaveId, 0, bytesShort).ConfigureAwait(false);

                // 回读
                var resp1 = await master.ReadHoldingRegistersAsync(SlaveId, 0, (ushort)shortArr.Length).ConfigureAwait(false);
                var readBack1 = resp1.Data.ToShorts();
                Console.WriteLine("回读: [" + string.Join(", ", readBack1) + "]");

                // 方式2: 使用 ushort[] → ToMemoryBytes()
                Console.WriteLine("\n--- 方式2: ushort[] → 字节序列 ---");
                var ushortArr = new ushort[] { 10000, 20000, 30000, 40000, 50000 };
                var bytesUshort = ushortArr.ToMemoryBytes();
                Console.WriteLine($"写入 {ushortArr.Length} 个 ushort: [{string.Join(", ", ushortArr)}]");
                await master.WriteMultipleRegistersAsync(SlaveId, 5, bytesUshort).ConfigureAwait(false);

                var resp2 = await master.ReadHoldingRegistersAsync(SlaveId, 5, (ushort)ushortArr.Length).ConfigureAwait(false);
                var readBack2 = resp2.Data.ToUshorts();
                Console.WriteLine("回读: [" + string.Join(", ", readBack2) + "]");

                // 方式3: 单个 int (跨 2 个寄存器)
                Console.WriteLine("\n--- 方式3: int 值跨 2 个寄存器 ---");
                int intVal = 123456;
                var intBytes = intVal.ToMemoryBytes();
                Console.WriteLine($"写入 int {intVal} 到寄存器[0..1] (4 字节)");
                await master.WriteMultipleRegistersAsync(SlaveId, 0, intBytes).ConfigureAwait(false);

                var resp3 = await master.ReadHoldingRegistersAsync(SlaveId, 0, 2).ConfigureAwait(false);
                int readInt = resp3.Data.ToInt();
                Console.WriteLine($"回读 int: {readInt}");

                // 方式4: 带超时和取消
                Console.WriteLine("\n--- 方式4: 带超时重载 ---");
                var data4 = new short[] { 1, 2, 3 }.ToMemoryBytes();
                await master.WriteMultipleRegistersAsync(SlaveId, 0, data4, 500, ct).ConfigureAwait(false);
                var resp4 = await master.ReadHoldingRegistersAsync(SlaveId, 0, 3).ConfigureAwait(false);
                Console.WriteLine("回读: [" + string.Join(", ", resp4.Data.ToShorts()) + "]");
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

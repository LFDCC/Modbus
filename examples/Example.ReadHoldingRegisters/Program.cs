using System;
using System.Threading;
using System.Threading.Tasks;

using Modbus;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Example.ReadHoldingRegisters
{
    /// <summary>
    /// FC03 — 读保持寄存器（Read Holding Registers）示例。
    /// 演示读取寄存器并使用 SpanMemoryExtension 安全解析为各种数值类型。
    /// </summary>
    public class Driver
    {
        private const int ServerPort = 51003;
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
            Console.WriteLine("=== FC03 读保持寄存器（Read Holding Registers） ===");

            // --- 从站初始化 ---
            var dataLocater = new ModbusDataLocater(20, 20, 20, 20);
            // 预置 8 个寄存器值 (short[])
            dataLocater.HoldingRegisters.Write(0, new short[] { 11, 22, 33, 44, 55, 66, 77, 88 });

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
                // 读取 8 个保持寄存器
                ushort startAddr = 0;
                ushort quantity = 8;

                Console.WriteLine($"\n读保持寄存器: 起始地址={startAddr}, 数量={quantity}");
                var response = await master.ReadHoldingRegistersAsync(SlaveId, startAddr, quantity).ConfigureAwait(false);

                // response.Data 是 ReadOnlyMemory<byte>，大端字节序列
                // 使用 SpanMemoryExtension 安全解析
                var data = response.Data;

                Console.WriteLine("原始字节 (大端):");
                for (int i = 0; i < data.Length; i++)
                {
                    Console.Write($"{data.Span[i]:X2} ");
                    if ((i + 1) % 2 == 0) Console.Write(" ");
                }
                Console.WriteLine();

                // 解析为 short 数组（默认大端）
                var shorts = data.ToShorts();
                Console.WriteLine("\n解析为 short[] (默认大端):");
                for (int i = 0; i < shorts.Length; i++)
                {
                    Console.WriteLine($"  寄存器[{startAddr + i}] = {shorts[i]}");
                }

                // 解析为 ushort 数组
                var ushorts = data.ToUshorts();
                Console.WriteLine("\n解析为 ushort[] (默认大端):");
                for (int i = 0; i < ushorts.Length; i++)
                {
                    Console.WriteLine($"  寄存器[{startAddr + i}] = {ushorts[i]}");
                }

                // 解析前 8 字节为 int (2 个 int, 每个占 4 字节)
                // 注意: async 方法中不能持有 ReadOnlySpan<byte> (ref struct) 局部变量，
                // 因此使用 ReadOnlyMemory<byte>.ToInts() 扩展方法而非 Span.ToInts()
                var ints = data.Slice(0, 8).ToInts();
                Console.WriteLine("\n解析前 8 字节为 int[] (默认大端):");
                for (int i = 0; i < ints.Length; i++)
                {
                    Console.WriteLine($"  int[{i}] = {ints[i]}");
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

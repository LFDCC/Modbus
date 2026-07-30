using System;
using System.Threading;
using System.Threading.Tasks;

using Modbus;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Example.WriteSingleCoil
{
    /// <summary>
    /// FC05 — 写单个线圈（Write Single Coil）示例。
    /// 向从站写入单个线圈状态（ON/OFF），然后回读验证。
    /// </summary>
    public class Driver
    {
        private const int ServerPort = 51005;
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
            Console.WriteLine("=== FC05 写单个线圈（Write Single Coil） ===");

            // --- 从站初始化 ---
            var dataLocater = new ModbusDataLocater(20, 20, 20, 20);
            // 初始全部 OFF
            dataLocater.Coils.Write(0, new bool[] { false, false, false, false });

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
                // 写入前先读取当前状态
                Console.WriteLine("\n--- 写入前 ---");
                var before = await master.ReadCoilsAsync(SlaveId, 0, 4).ConfigureAwait(false);
                for (int i = 0; i < before.Length; i++)
                    Console.WriteLine($"  线圈[{i}] = {(before.Span[i] ? "ON" : "OFF")}");

                // 写单个线圈: 地址 0 → ON
                Console.WriteLine("\n写入 线圈[0] = ON");
                await master.WriteSingleCoilAsync(SlaveId, 0, true).ConfigureAwait(false);

                // 写单个线圈: 地址 2 → ON
                Console.WriteLine("写入 线圈[2] = ON");
                await master.WriteSingleCoilAsync(SlaveId, 2, true).ConfigureAwait(false);

                // 回读验证
                Console.WriteLine("\n--- 写入后 ---");
                var after = await master.ReadCoilsAsync(SlaveId, 0, 4).ConfigureAwait(false);
                for (int i = 0; i < after.Length; i++)
                    Console.WriteLine($"  线圈[{i}] = {(after.Span[i] ? "ON" : "OFF")}");

                // 关闭
                Console.WriteLine("\n写入 线圈[0] = OFF");
                await master.WriteSingleCoilAsync(SlaveId, 0, false).ConfigureAwait(false);

                var final = await master.ReadCoilsAsync(SlaveId, 0, 4).ConfigureAwait(false);
                Console.WriteLine("最终状态:");
                for (int i = 0; i < final.Length; i++)
                    Console.WriteLine($"  线圈[{i}] = {(final.Span[i] ? "ON" : "OFF")}");
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

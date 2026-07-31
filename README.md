# Modbus

[![NuGet](https://img.shields.io/badge/nuget-LFDCC.Modbus-blue.svg)](https://www.nuget.org/packages/LFDCC.Modbus)

一套基于 [TouchSocket](https://gitee.com/RRQM_Home/TouchSocket) 实现的原生、全异步 Modbus 客户端/服务端组件。 NuGet 包 ID：`LFDCC.Modbus`。

- **主站（Master，即客户端）**：基于开源 [TouchSocket.Modbus](https://gitee.com/RRQM_Home/TouchSocket) 源码，由 `ModbusTcpMaster` / `ModbusUdpMaster` / `ModbusRtuMaster` / `ModbusRtuOverTcpMaster` / `ModbusRtuOverUdpMaster` 驱动。
- **从站（Slave，即服务端）**：基于 TouchSocket 原生实现，支持 TCP / UDP / RTU / RTU-over-TCP / RTU-over-UDP 五种传输。

## 特性

- 纯 TouchSocket 异步（`Task` / `CancellationToken`），无遗留同步接口。
- 主站所有读写方法为扩展方法（`ModbusMasterExtension`），默认超时 1000ms，亦支持按调用传入超时与取消令牌。
- 从站以「插件」形式挂在 `ModbusTcpSlave` 等宿主上，一个宿主可挂多个站点（`ModbusSlavePoint`），按 `SlaveId` 派发。
- 数据区 `ModbusDataLocater` 用 `BooleanDataPartition` / `ShortDataPartition` 分块管理线圈/离散输入/保持寄存器/输入寄存器。
- 内置安全的字节序转换扩展（`SpanMemoryExtension` / `ByteConverterExtension`），基于 BCL `BinaryPrimitives`

## 架构

```
Modbus/
├── Modbus.cs / ModbusRequest.cs / ModbusResponse.cs   根常量与请求/响应模型
├── Master/                  主站（客户端）
│   ├── ModbusTcpMaster.cs        TCP 主站
│   ├── ModbusUdpMaster.cs        UDP 主站
│   ├── ModbusRtuMaster.cs        串口 RTU 主站
│   ├── ModbusRtuOverTcpMaster.cs / ModbusRtuOverUdpMaster.cs
│   ├── Interfaces/IModbusMaster.cs   主站契约（SendModbusRequestAsync）
│   ├── Extensions/ModbusMasterExtension.cs   读/写/超时扩展方法
│   └── Handlers/                功能码处理管线（ModbusFunctionHandlerRegistry）
├── Slave/                   从站（服务端）
│   ├── ModbusTcpSlave.cs        TCP 从站（TcpServiceBase<ModbusTcpSlaveSessionClient>）
│   ├── ModbusUdpSlave.cs / ModbusRtuSlave.cs / ModbusRtuOverTcpSlave.cs / ModbusRtuOverUdpSlave.cs
│   ├── ModbusDataLocater.cs     数据区（4 段 Partition）
│   ├── ModbusSlavePoint.cs      单个从站点（插件）
│   ├── ModbusSlavePointOption.cs 站点配置（SlaveId / IgnoreSlaveId / DataLocater）
│   ├── BooleanDataPartition.cs / ShortDataPartition.cs   线圈 / 寄存器分区
│   └── ModbusSlavePointExtension.cs   AddModbusSlavePoint 便捷扩展
├── Extensions/
│   ├── SetupConfigObjectExtension.cs   SetupAsync(Action<TouchSocketConfig>) / SetConfig 别名
│   ├── SpanMemoryExtension.cs          ReadOnlyMemory<byte> → 数值类型读取（ToShorts/ToUshorts/ToInts…）
│   └── ByteConverterExtension.cs       数值类型 → ReadOnlyMemory<byte> 写入（ToMemoryBytes/ToSpanBytes）
└── （公共工具、异常、字节序转换等）

examples/                    按功能码拆分的独立控制台示例（每个自建主从回环）
├── Example.ReadCoils/                 FC01 读线圈
├── Example.ReadDiscreteInputs/        FC02 读离散输入
├── Example.ReadHoldingRegisters/      FC03 读保持寄存器
├── Example.ReadInputRegisters/        FC04 读输入寄存器
├── Example.WriteSingleCoil/           FC05 写单线圈
├── Example.WriteSingleRegister/       FC06 写单寄存器
├── Example.WriteMultipleCoils/        FC15 写多线圈
├── Example.WriteMultipleRegisters/    FC16 写多寄存器
└── Example.ReadWriteMultipleRegisters/ FC23 读写多寄存器
```

## 安装

```bash
dotnet add package LFDCC.Modbus
```
## 快速开始（TCP 主从回环）

完整可运行示例见 [`examples/`](examples/) 目录下按功能码拆分的 9 个控制台项目。以下为最小流程：

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Modbus;
using TouchSocket.Core;
using TouchSocket.Sockets;

class Program
{
    static async Task Main()
    {
        const int port = 51000;
        const byte slaveId = 1;

        // 1) 准备从站数据区并写入一些初始值
        var dataLocater = new ModbusDataLocater(10, 10, 10, 10); // 线圈/离散输入/保持寄存器/输入寄存器各 10 个
        dataLocater.HoldingRegisters.Write(0, new short[] { 11, 22, 33, 44, 55, 66, 77, 88 });
        dataLocater.Coils.Write(0, new bool[] { true, false, true, false, true });

        // 2) 启动 TCP 从站，注册站点（站号 1）
        var slave = new ModbusTcpSlave();
        await slave.SetupAsync(config =>
        {
            config.SetListenIPHosts(new IPHost[] { new IPHost($"127.0.0.1:{port}") });
            config.ConfigurePlugins(a => a.AddModbusSlavePoint(options =>
            {
                options.SlaveId = slaveId;
                options.IgnoreSlaveId = false;
                options.DataLocater = dataLocater;
            }));
        });
        await slave.StartAsync(CancellationToken.None);

        // 3) 创建 TCP 主站并连接
        using var master = new ModbusTcpMaster();
        await master.SetupAsync(config => config.SetRemoteIPHost(new IPHost($"127.0.0.1:{port}")));
        await master.ConnectAsync(CancellationToken.None);

        // 4) 读保持寄存器（默认 1000ms 超时），响应 Data 为大端字节序列
        var holding = await master.ReadHoldingRegistersAsync(slaveId, 0, 5);
        Console.WriteLine(string.Join(", ", holding.Data.ToUshorts())); // 11, 22, 33, 44, 55

        // 5) 写单寄存器（注意值类型为 short）
        await master.WriteSingleRegisterAsync(slaveId, 2, (short)999);

        // 6) 写多寄存器：先用 ByteConverterExtension.ToMemoryBytes() 把 ushort[] 编码为大端字节
        var writeBytes = new ushort[] { 100, 200, 300 }.ToMemoryBytes();
        await master.WriteMultipleRegistersAsync(slaveId, 5, writeBytes);

        await slave.StopAsync(CancellationToken.None);
    }
}
```

## 示例项目（examples/）

每个示例都是一个独立的控制台项目，自建 TCP 主从回环，互不依赖外部 Modbus 设备：

| 项目 | 功能码 | 端口 | 说明 |
| --- | --- | --- | --- |
| `Example.ReadCoils` | FC01 | 51001 | 读线圈，演示默认超时与显式超时两种重载 |
| `Example.ReadDiscreteInputs` | FC02 | 51002 | 读离散输入（只读数据区） |
| `Example.ReadHoldingRegisters` | FC03 | 51003 | 读保持寄存器，用 `ToShorts()`/`ToUshorts()`/`ToInts()` 解析 |
| `Example.ReadInputRegisters` | FC04 | 51004 | 读输入寄存器 |
| `Example.WriteSingleCoil` | FC05 | 51005 | 写单线圈 + 回读验证 |
| `Example.WriteSingleRegister` | FC06 | 51006 | 写单寄存器 short/ushort 重载 + 带超时重载 |
| `Example.WriteMultipleCoils` | FC15 | 51007 | 批量写线圈 + 回读验证 |
| `Example.WriteMultipleRegisters` | FC16 | 51008 | 四种写入方式：`short[]`/`ushort[]`/`int`/带超时 |
| `Example.ReadWriteMultipleRegisters` | FC23 | 51009 | 一次请求同时写入并读取寄存器 |

运行示例：

```bash
dotnet run --project examples/Example.ReadHoldingRegisters -c Release
```

## 配置（Setup）扩展方法

```csharp
// 从站
await slave.SetupAsync(config =>
{
    config.SetListenIPHosts(new IPHost[] { new IPHost($"127.0.0.1:{port}") });
    config.ConfigurePlugins(a => a.AddModbusSlavePoint(o => { o.SlaveId = 1; o.DataLocater = dataLocater; }));
});

// 主站（SetConfig 等价于 SetupAsync）
await master.SetConfig(config => config.SetRemoteIPHost(new IPHost($"127.0.0.1:{port}")));
```

## 主站读写 API（`ModbusMasterExtension`）

读写方法均为 `IModbusMaster` 的扩展方法，按功能码命名。每个方法都提供 **两个重载**：

- **默认超时重载**：不传超时，使用内置默认 `1000ms`。
- **显式超时重载**：`(…, int millisecondsTimeout, CancellationToken cancellationToken = default)`，可覆盖默认超时、传入取消令牌。

| 方法 | 返回 | 说明 |
| --- | --- | --- |
| `ReadCoilsAsync(slaveId, start, qty)` | `ReadOnlyMemory<bool>` | 读线圈（FC1） |
| `ReadDiscreteInputsAsync(slaveId, start, qty)` | `ReadOnlyMemory<bool>` | 读离散输入（FC2） |
| `ReadHoldingRegistersAsync(slaveId, start, qty)` | `IModbusResponse` | 读保持寄存器（FC3），`response.Data` 为大端字节 |
| `ReadInputRegistersAsync(slaveId, start, qty)` | `IModbusResponse` | 读输入寄存器（FC4） |
| `WriteSingleCoilAsync(slaveId, start, bool)` | `IModbusResponse` | 写单线圈（FC5） |
| `WriteSingleRegisterAsync(slaveId, start, short)` | `IModbusResponse` | 写单寄存器（FC6），值类型为 `short`（亦提供 `ushort` 的显式超时重载） |
| `WriteMultipleCoilsAsync(slaveId, start, ReadOnlyMemory<bool>)` | `IModbusResponse` | 写多线圈（FC15） |
| `WriteMultipleRegistersAsync(slaveId, start, ReadOnlyMemory<byte>)` | `IModbusResponse` | 写多寄存器（FC16），入参为大端字节序列 |
| `ReadWriteMultipleRegistersAsync(slaveId, readStart, readQty, writeStart, ReadOnlyMemory<byte>)` | `IModbusResponse` | 读写多寄存器（FC23） |

### 字节序与安全转换扩展

**`SpanMemoryExtension`（读取：字节 → 数值）**

| 方法 | 说明 |
| --- | --- |
| `ReadOnlyMemory<byte>.ToShort(EndianType=Big)` / `.ToShorts(...)` | 大端字节 → `short` / `short[]` |
| `ReadOnlyMemory<byte>.ToUshort(EndianType=Big)` / `.ToUshorts(...)` | 大端字节 → `ushort` / `ushort[]` |
| `ReadOnlyMemory<byte>.ToInt(...)` / `.ToInts(...)` | 大端字节 → `int` / `int[]`（占 2 个寄存器） |
| `ReadOnlyMemory<byte>.ToUint(...)` / `.ToUints(...)` | 大端字节 → `uint` / `uint[]` |
| `ReadOnlyMemory<byte>.ToFloat(...)` / `.ToFloats(...)` | 大端字节 → `float` / `float[]` |
| `ReadOnlyMemory<byte>.ToLong(...)` / `.ToLongs(...)` | 大端字节 → `long` / `long[]`（占 4 个寄存器） |
| `ReadOnlyMemory<byte>.ToUlong(...)` / `.ToUlongs(...)` | 大端字节 → `ulong` / `ulong[]` |
| `ReadOnlyMemory<byte>.ToDouble(...)` / `.ToDoubles(...)` | 大端字节 → `double` / `double[]` |
| `ReadOnlyMemory<byte>.ToByte()` / `.ToBytes()` | 单字节 / 字节数组 |

> `ReadOnlySpan<byte>` 同名重载亦提供，但**不可在 async 方法中持有**（ref struct 限制），异步场景请用 `ReadOnlyMemory<byte>` 重载。

**`ByteConverterExtension`（写入：数值 → 字节）**

| 方法 | 说明 |
| --- | --- |
| `short.ToMemoryBytes(EndianType=Big)` | `short` → 大端 2 字节 |
| `short[].ToMemoryBytes(...)` | `short[]` → 大端字节序列 |
| `ushort.ToMemoryBytes(...)` / `ushort[].ToMemoryBytes(...)` | `ushort` 版本 |
| `int.ToMemoryBytes(...)` / `int[].ToMemoryBytes(...)` | `int` → 4 字节（2 个寄存器） |
| `uint.ToMemoryBytes(...)` / `uint[].ToMemoryBytes(...)` | `uint` 版本 |
| `float.ToMemoryBytes(...)` / `float[].ToMemoryBytes(...)` | `float` → 4 字节 |
| `long.ToMemoryBytes(...)` / `long[].ToMemoryBytes(...)` | `long` → 8 字节（4 个寄存器） |
| `ulong.ToMemoryBytes(...)` / `ulong[].ToMemoryBytes(...)` | `ulong` 版本 |
| `double.ToMemoryBytes(...)` / `double[].ToMemoryBytes(...)` | `double` → 8 字节 |
| `bool.ToMemoryBytes(...)` | `bool` → 2 字节（线圈值 0xFF00/0x0000） |

使用示例：

```csharp
// 读取：response.Data → ushort[]
var values = response.Data.ToUshorts();            // 默认大端
var shorts = response.Data.ToShorts(EndianType.Little);

// 写入：ushort[] → 大端字节
var bytes = new ushort[] { 100, 200, 300 }.ToMemoryBytes();
await master.WriteMultipleRegistersAsync(1, 0, bytes);

// int 占 2 个寄存器
var intBytes = ((int)123456).ToMemoryBytes();
await master.WriteMultipleRegistersAsync(1, 0, intBytes);
var readBack = (await master.ReadHoldingRegistersAsync(1, 0, 2)).Data.ToInt();
```

## 超时与取消

主站读写采用**单一正交机制**：

- **默认超时**：所有默认重载内置 `1000ms` 超时（`ModbusMasterExtension` 内部用 `new CancellationTokenSource(1000)` 链接到调用方传入的 `CancellationToken`）。
- **per-call 覆盖**：显式超时重载的 `millisecondsTimeout > 0` 覆盖默认；`<= 0` 回退默认。
- **`CancellationToken`**：外部/全局**取消**（循环控制、UI 关闭、请求断连等）。

```csharp
// 1) 循环采集：默认 1000ms 超时
while (!cts.IsCancellationRequested)
    var resp = await master.ReadHoldingRegistersAsync(1, 0, 10, cts.Token);

// 2) 单次覆盖：5s 超时，覆盖默认 1000ms
var r = await master.ReadHoldingRegistersAsync(1, 0, 10, 5000, cts.Token);

// 3) 彻底不限超时（仅受取消令牌控制）
var r2 = await master.ReadHoldingRegistersAsync(1, 0, 10, 0, cts.Token);
```

超时 → `TimeoutException`；令牌取消 → `OperationCanceledException`。**不会关闭底层 socket**：超时/取消只让 `await` 抛异常，连接保持打开、可继续复用；要断连需自行 `CloseAsync` 或 `Dispose`。超时后迟到的响应因事务槽已清理而丢弃，不会串到后续请求。

## 从站配置

### 站点注册（AddModbusSlavePoint）

每个从站宿主（如 `ModbusTcpSlave`）通过插件管理多个站点。`AddModbusSlavePoint` 便捷扩展会基于 `ModbusSlavePointOption` 创建 `ModbusSlavePoint` 并加入插件管理器：

```csharp
slave.SetupAsync(new TouchSocketConfig()
    .SetListenIPHosts(new IPHost[] { new IPHost(502) })
    .ConfigurePlugins(a => a.AddModbusSlavePoint(options =>
    {
        options.SlaveId = 1;             // 站点号（unitId）
        options.IgnoreSlaveId = false;   // false=校验站号；true=忽略（广播式）
        options.DataLocater = new ModbusDataLocater(10, 10, 10, 10);
    })));
```

### 数据区（ModbusDataLocater）

| 构造 / 属性 | 说明 |
| --- | --- |
| `new ModbusDataLocater(coils, discreteInputs, holdingRegisters, inputRegisters)` | 4 段分区，地址均从 0 起，参数为各段数量 |
| `new ModbusDataLocater()` + 显式分区 | 可指定各分区 `StartingAddress`（如 `new ShortDataPartition(1000, 10)` 表示从 1000 起 10 个） |
| `HoldingRegisters.Write(addr, short)` / `Write(addr, ReadOnlySpan<short>)` | 写单个 / 多个保持寄存器 |
| `Coils.Write(addr, bool)` / `Write(addr, ReadOnlySpan<bool>)` | 写单个 / 多个线圈 |
| `*.Read(addr, qty)` | 读取（返回 `ModbusResult`，含字节 `Data` 与 `ErrorCode`） |

分区类型：`BooleanDataPartition`（线圈 / 离散输入）、`ShortDataPartition`（保持寄存器 / 输入寄存器）。

### 多传输

从站除 `ModbusTcpSlave` 外，还有 `ModbusUdpSlave`、`ModbusRtuSlave`（串口）、`ModbusRtuOverTcpSlave`、`ModbusRtuOverUdpSlave`；主站对应 `ModbusUdpMaster`、`ModbusRtuMaster`、`ModbusRtuOverTcpMaster`、`ModbusRtuOverUdpMaster`。它们的 Setup/Start/Connect 与读写 API 完全一致，仅是底层传输不同（串口主站/从站需在 `TouchSocketConfig` 中配置串口参数，具体见各 `SerialPortClientBase` 配置项）。

## 构建 / 打包

```bash
# 构建（本机离线环境建议加以下两个参数以避免节点复用锁）
dotnet build Modbus.sln -c Release -p:UseSharedCompilation=false -p:MSBuildEnableNodeReuse=false

# 打包
dotnet pack Modbus/Modbus.csproj -c Release
```
## 许可

[MIT](LICENSE.txt)

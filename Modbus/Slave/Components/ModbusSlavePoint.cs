using System.Runtime.CompilerServices;

namespace Modbus
{
    internal class ModbusSlavePoint :
      PluginBase,
      ITcpReceivedPlugin,
      IPlugin,
      IDisposableObject,
      IDisposable,
      IUdpReceivedPlugin,
      ISerialReceivedPlugin,
      IModbusSlavePoint
    {
        public ModbusSlavePoint(

        ModbusSlavePointOption option)
        {
            this.IgnoreSlaveId = option.IgnoreSlaveId;
            this.DataLocater = option.DataLocater ?? throw new ArgumentNullException(nameof(DataLocater));
            this.SlaveId = option.SlaveId;
        }

        public bool IgnoreSlaveId { get; }

        public IModbusDataLocater DataLocater { get; }

        public byte SlaveId { get; }

        public async Task OnSerialReceived(ISerialPortSession client, ReceivedDataEventArgs e)
        {
            ModbusRtuSlave clientSender;
            if (e.RequestInfo is ModbusRtuBase request)
            {
                clientSender = client as ModbusRtuSlave;
                if (clientSender != null && (this.IgnoreSlaveId || (int)this.SlaveId == (int)request.SlaveId))
                {
                    ModbusSlaveExecutingEventArgs args = new ModbusSlaveExecutingEventArgs((IModbusRequest)request, TouchSocketModbusUtility.ModbusRtu, (IDependencyClient)client);
                    await this.RaiseModbusSlaveExecuting(client.Resolver, args).ConfigureDefaultAwait();
                    ModbusResult result;
                    if (args.IsPermitOperation)
                        result = await this.DataLocater.ExecuteAsync((IModbusRequest)request, client.ClosedToken).ConfigureDefaultAwait<ModbusResult>();
                    else
                        result = new ModbusResult(new ReadOnlyMemory<byte>(), args.ErrorCode);
                    await clientSender.InternalSendAsync(new ModbusRtuResponseForSlave(request, result)).ConfigureDefaultAwait();
                    e.Handled = true;
                    await this.RaiseModbusSlaveExecuted(client.Resolver, new ModbusSlaveExecutedEventArgs((IDependencyClient)client, (IModbusResponse)new InternalModbusResponse(result.Data, request.FunctionCode, result.ErrorCode, (IModbusRequest)request, request.SlaveId), TouchSocketModbusUtility.ModbusRtu, (IModbusRequest)request)).ConfigureDefaultAwait();
                    return;
                }
            }
            await e.InvokeNext().ConfigureDefaultAwait();
        }

        public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
        {
            ModbusSlaveExecutingEventArgs args;
            ModbusResult result;
            if (e.RequestInfo is ModbusTcpBase request && client is ModbusTcpSlaveSessionClient modbusTcpSlave)
            {
                if (this.IgnoreSlaveId || (int)this.SlaveId == (int)request.SlaveId)
                {
                    args = new ModbusSlaveExecutingEventArgs((IModbusRequest)request, TouchSocketModbusUtility.ModbusTcp, (IDependencyClient)client);
                    await this.RaiseModbusSlaveExecuting(client.Resolver, args).ConfigureDefaultAwait();
                    if (args.IsPermitOperation)
                        result = await this.DataLocater.ExecuteAsync((IModbusRequest)request, client.ClosedToken).ConfigureDefaultAwait<ModbusResult>();
                    else
                        result = new ModbusResult(new ReadOnlyMemory<byte>(), args.ErrorCode);
                    await modbusTcpSlave.InternalSendAsync(new ModbusTcpResponseForSlave(request, result)).ConfigureDefaultAwait();
                    e.Handled = true;
                    await this.RaiseModbusSlaveExecuted(client.Resolver, new ModbusSlaveExecutedEventArgs((IDependencyClient)client, (IModbusResponse)new InternalModbusResponse(result.Data, request.FunctionCode, result.ErrorCode, (IModbusRequest)request, request.SlaveId), TouchSocketModbusUtility.ModbusTcp, (IModbusRequest)request)).ConfigureDefaultAwait();
                    return;
                }
            }
            else
            {
                ModbusRtuOverTcpSlaveSessionClient modbusRtuOverTcp;
                if (e.RequestInfo is ModbusRtuRequestForSlave requestRtu)
                {
                    modbusRtuOverTcp = client as ModbusRtuOverTcpSlaveSessionClient;
                    if (modbusRtuOverTcp != null && (this.IgnoreSlaveId || (int)this.SlaveId == (int)requestRtu.SlaveId))
                    {
                        args = new ModbusSlaveExecutingEventArgs((IModbusRequest)requestRtu, TouchSocketModbusUtility.ModbusRtuOverTcp, (IDependencyClient)client);
                        await this.RaiseModbusSlaveExecuting(client.Resolver, args).ConfigureDefaultAwait();
                        if (args.IsPermitOperation)
                            result = await this.DataLocater.ExecuteAsync((IModbusRequest)requestRtu, client.ClosedToken).ConfigureDefaultAwait<ModbusResult>();
                        else
                            result = new ModbusResult(new ReadOnlyMemory<byte>(), args.ErrorCode);
                        await modbusRtuOverTcp.InternalSendAsync(new ModbusRtuResponseForSlave((ModbusRtuBase)requestRtu, result), CancellationToken.None).ConfigureDefaultAwait();
                        e.Handled = true;
                        await this.RaiseModbusSlaveExecuted(client.Resolver, new ModbusSlaveExecutedEventArgs((IDependencyClient)client, (IModbusResponse)new InternalModbusResponse(result.Data, requestRtu.FunctionCode, result.ErrorCode, (IModbusRequest)requestRtu, requestRtu.SlaveId), TouchSocketModbusUtility.ModbusRtuOverTcp, (IModbusRequest)requestRtu)).ConfigureDefaultAwait();
                        return;
                    }
                }
            }
            await e.InvokeNext().ConfigureDefaultAwait();
        }

        public async Task OnUdpReceived(IUdpSessionBase client, UdpReceivedDataEventArgs e)
        {
            ModbusSlaveExecutingEventArgs args;
            ModbusResult result;
            if (e.RequestInfo is ModbusTcpBase request && client is ModbusUdpSlave modbusUdpSlave)
            {
                if (this.IgnoreSlaveId || (int)this.SlaveId == (int)request.SlaveId)
                {
                    args = new ModbusSlaveExecutingEventArgs((IModbusRequest)request, TouchSocketModbusUtility.ModbusUdp, (IDependencyClient)client);
                    await this.RaiseModbusSlaveExecuting(client.Resolver, args).ConfigureDefaultAwait();
                    if (args.IsPermitOperation)
                        result = await this.DataLocater.ExecuteAsync((IModbusRequest)request, CancellationToken.None).ConfigureDefaultAwait<ModbusResult>();
                    else
                        result = new ModbusResult(new ReadOnlyMemory<byte>(), args.ErrorCode);
                    await modbusUdpSlave.InternalSendAsync(e.EndPoint, new ModbusTcpResponseForSlave(request, result)).ConfigureDefaultAwait();
                    e.Handled = true;
                    await this.RaiseModbusSlaveExecuted(client.Resolver, new ModbusSlaveExecutedEventArgs((IDependencyClient)client, (IModbusResponse)new InternalModbusResponse(result.Data, request.FunctionCode, result.ErrorCode, (IModbusRequest)request, request.SlaveId), TouchSocketModbusUtility.ModbusUdp, (IModbusRequest)request)).ConfigureDefaultAwait();
                    return;
                }
            }
            else
            {
                ModbusRtuOverUdpSlave modbusRtuOverUdpSlave;
                if (e.RequestInfo is ModbusRtuBase requestRtu)
                {
                    modbusRtuOverUdpSlave = client as ModbusRtuOverUdpSlave;
                    if (modbusRtuOverUdpSlave != null && (this.IgnoreSlaveId || (int)this.SlaveId == (int)requestRtu.SlaveId))
                    {
                        args = new ModbusSlaveExecutingEventArgs((IModbusRequest)requestRtu, TouchSocketModbusUtility.ModbusRtuOverUdp, (IDependencyClient)client);
                        await this.RaiseModbusSlaveExecuting(client.Resolver, args).ConfigureDefaultAwait();
                        if (args.IsPermitOperation)
                            result = await this.DataLocater.ExecuteAsync((IModbusRequest)requestRtu, CancellationToken.None).ConfigureDefaultAwait<ModbusResult>();
                        else
                            result = new ModbusResult(new ReadOnlyMemory<byte>(), args.ErrorCode);
                        await modbusRtuOverUdpSlave.InternalSendAsync(e.EndPoint, new ModbusRtuResponseForSlave(requestRtu, result)).ConfigureDefaultAwait();
                        e.Handled = true;
                        await this.RaiseModbusSlaveExecuted(client.Resolver, new ModbusSlaveExecutedEventArgs((IDependencyClient)client, (IModbusResponse)new InternalModbusResponse(result.Data, requestRtu.FunctionCode, result.ErrorCode, (IModbusRequest)requestRtu, requestRtu.SlaveId), TouchSocketModbusUtility.ModbusRtuOverUdp, (IModbusRequest)requestRtu)).ConfigureDefaultAwait();
                        return;
                    }
                }
            }
            await e.InvokeNext().ConfigureDefaultAwait();
        }

        private async Task RaiseModbusSlaveExecuted(IResolver resolver, ModbusSlaveExecutedEventArgs e)
        {
            await this.PluginManager.RaiseIModbusSlaveExecutedPluginAsync(resolver, (IModbusSlavePoint)this, e).ConfigureDefaultAwait<bool>();
        }

        private async Task RaiseModbusSlaveExecuting(
          IResolver resolver,
          ModbusSlaveExecutingEventArgs e)
        {
            await this.PluginManager.RaiseIModbusSlaveExecutingPluginAsync(resolver, (IModbusSlavePoint)this, e).ConfigureDefaultAwait<bool>();
        }
    }
}

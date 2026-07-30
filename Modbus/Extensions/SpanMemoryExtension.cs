namespace Modbus;

/// <summary>
/// <see cref="ReadOnlySpan{T}"/> / <see cref="ReadOnlyMemory{T}"/> → 数值类型的便捷读取扩展。
/// <para>
/// 底层使用 BCL <see cref="BinaryPrimitives"/>，不依赖 <c>TouchSocketBitConverter.To&lt;T&gt;</c>
/// （4.2.18 版本存在 <see cref="AccessViolationException"/>）。
/// 字节序通过 <see cref="EndianType"/> 参数控制，默认 <see cref="EndianType.Big"/>（Modbus 标准）。
/// </para>
/// <para>
/// <b>EndianType 四种模式对应的字节序（以 32-bit ABCD 为例）：</b>
/// <list type="table">
///   <item><term><see cref="EndianType.Big"/></term><description>ABCD — 标准大端，Modbus 默认</description></item>
///   <item><term><see cref="EndianType.Little"/></term><description>DCBA — 标准小端</description></item>
///   <item><term><see cref="EndianType.BigSwap"/></term><description>BADC — 字内字节交换</description></item>
///   <item><term><see cref="EndianType.LittleSwap"/></term><description>CDAB — 字交换</description></item>
/// </list>
/// </para>
/// <example>
/// <code>
/// var val  = span.ToUshort();                          // 默认大端
/// var val2 = span.ToUshort(EndianType.LittleSwap);     // CDAB 字序
/// var arr  = memory.ToUshorts();                       // 批量大端
/// </code>
/// </example>
/// </summary>
public static class SpanMemoryExtension
{
    #region byte

    /// <summary>
    /// 取首字节。
    /// </summary>
    public static byte ToByte(this ReadOnlySpan<byte> span)
        => span[0];

    /// <summary>
    /// 取首字节（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static byte ToByte(this ReadOnlyMemory<byte> memory)
        => memory.Span.ToByte();

    /// <summary>
    /// 原样导出字节数组。
    /// </summary>
    public static byte[] ToBytes(this ReadOnlySpan<byte> span)
        => span.ToArray();

    /// <summary>
    /// 原样导出字节数组（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static byte[] ToBytes(this ReadOnlyMemory<byte> memory)
        => memory.Span.ToBytes();

    #endregion

    #region ushort

    /// <summary>
    /// 从起始位置读取一个 <see cref="ushort"/>（2 字节）。
    /// </summary>
    /// <param name="span">至少包含 2 字节的源跨度</param>
    /// <param name="endian">字节序，默认 <see cref="EndianType.Big"/></param>
    public static ushort ToUshort(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
        => endian == EndianType.Big || endian == EndianType.LittleSwap
            ? BinaryPrimitives.ReadUInt16BigEndian(span)
            : BinaryPrimitives.ReadUInt16LittleEndian(span);

    /// <summary>
    /// 从起始位置读取一个 <see cref="ushort"/>（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static ushort ToUshort(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToUshort(endian);

    /// <summary>
    /// 将整个跨度解析为 <see cref="ushort"/> 数组（每 2 字节一个元素）。
    /// </summary>
    public static ushort[] ToUshorts(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
    {
        var count = span.Length / 2;
        var result = new ushort[count];
        for (var i = 0; i < count; i++)
            result[i] = span.Slice(i * 2, 2).ToUshort(endian);
        return result;
    }

    /// <summary>
    /// 将整个内存解析为 <see cref="ushort"/> 数组（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static ushort[] ToUshorts(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToUshorts(endian);

    #endregion

    #region short

    /// <summary>
    /// 从起始位置读取一个有符号 <see cref="short"/>（2 字节）。
    /// </summary>
    public static short ToShort(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
        => endian == EndianType.Big || endian == EndianType.LittleSwap
            ? BinaryPrimitives.ReadInt16BigEndian(span)
            : BinaryPrimitives.ReadInt16LittleEndian(span);

    /// <summary>
    /// 从起始位置读取一个有符号 <see cref="short"/>（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static short ToShort(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToShort(endian);

    /// <summary>
    /// 将整个跨度解析为有符号 <see cref="short"/> 数组（每 2 字节一个元素）。
    /// </summary>
    public static short[] ToShorts(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
    {
        var count = span.Length / 2;
        var result = new short[count];
        for (var i = 0; i < count; i++)
            result[i] = span.Slice(i * 2, 2).ToShort(endian);
        return result;
    }

    /// <summary>
    /// 将整个内存解析为有符号 <see cref="short"/> 数组（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static short[] ToShorts(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToShorts(endian);

    #endregion

    #region uint

    /// <summary>
    /// 从起始位置读取一个 <see cref="uint"/>（4 字节，跨 2 个寄存器）。
    /// </summary>
    public static uint ToUint(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
    {
        return endian switch
        {
            EndianType.Big        => BinaryPrimitives.ReadUInt32BigEndian(span),
            EndianType.Little     => BinaryPrimitives.ReadUInt32LittleEndian(span),
            EndianType.BigSwap    => ReadBigSwapUInt32(span),
            EndianType.LittleSwap => ReadLittleSwapUInt32(span),
            _ => BinaryPrimitives.ReadUInt32BigEndian(span)
        };
    }

    /// <summary>
    /// 从起始位置读取一个 <see cref="uint"/>（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static uint ToUint(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToUint(endian);

    /// <summary>
    /// 将整个跨度解析为 <see cref="uint"/> 数组（每 4 字节一个元素）。
    /// </summary>
    public static uint[] ToUints(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
    {
        var count = span.Length / 4;
        var result = new uint[count];
        for (var i = 0; i < count; i++)
            result[i] = span.Slice(i * 4, 4).ToUint(endian);
        return result;
    }

    /// <summary>
    /// 将整个内存解析为 <see cref="uint"/> 数组（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static uint[] ToUints(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToUints(endian);

    #endregion

    #region int

    /// <summary>
    /// 从起始位置读取一个有符号 <see cref="int"/>（4 字节，跨 2 个寄存器）。
    /// </summary>
    public static int ToInt(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
    {
        return endian switch
        {
            EndianType.Big        => BinaryPrimitives.ReadInt32BigEndian(span),
            EndianType.Little     => BinaryPrimitives.ReadInt32LittleEndian(span),
            EndianType.BigSwap    => (int)ReadBigSwapUInt32(span),
            EndianType.LittleSwap => (int)ReadLittleSwapUInt32(span),
            _ => BinaryPrimitives.ReadInt32BigEndian(span)
        };
    }

    /// <summary>
    /// 从起始位置读取一个有符号 <see cref="int"/>（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static int ToInt(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToInt(endian);

    /// <summary>
    /// 将整个跨度解析为有符号 <see cref="int"/> 数组（每 4 字节一个元素）。
    /// </summary>
    public static int[] ToInts(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
    {
        var count = span.Length / 4;
        var result = new int[count];
        for (var i = 0; i < count; i++)
            result[i] = span.Slice(i * 4, 4).ToInt(endian);
        return result;
    }

    /// <summary>
    /// 将整个内存解析为有符号 <see cref="int"/> 数组（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static int[] ToInts(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToInts(endian);

    #endregion

    #region float

    /// <summary>
    /// 从起始位置读取一个 <see cref="float"/>（IEEE754 单精度，4 字节，跨 2 个寄存器）。
    /// </summary>
    public static float ToFloat(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
    {
        return endian switch
        {
            EndianType.Big        => BinaryPrimitives.ReadSingleBigEndian(span),
            EndianType.Little     => BinaryPrimitives.ReadSingleLittleEndian(span),
            EndianType.BigSwap    => BitConverter.UInt32BitsToSingle(ReadBigSwapUInt32(span)),
            EndianType.LittleSwap => BitConverter.UInt32BitsToSingle(ReadLittleSwapUInt32(span)),
            _ => BinaryPrimitives.ReadSingleBigEndian(span)
        };
    }

    /// <summary>
    /// 从起始位置读取一个 <see cref="float"/>（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static float ToFloat(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToFloat(endian);

    /// <summary>
    /// 将整个跨度解析为 <see cref="float"/> 数组（每 4 字节一个元素）。
    /// </summary>
    public static float[] ToFloats(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
    {
        var count = span.Length / 4;
        var result = new float[count];
        for (var i = 0; i < count; i++)
            result[i] = span.Slice(i * 4, 4).ToFloat(endian);
        return result;
    }

    /// <summary>
    /// 将整个内存解析为 <see cref="float"/> 数组（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static float[] ToFloats(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToFloats(endian);

    #endregion

    #region long

    /// <summary>
    /// 从起始位置读取一个有符号 <see cref="long"/>（8 字节，跨 4 个寄存器）。
    /// </summary>
    public static long ToLong(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
    {
        return endian switch
        {
            EndianType.Big        => BinaryPrimitives.ReadInt64BigEndian(span),
            EndianType.Little     => BinaryPrimitives.ReadInt64LittleEndian(span),
            EndianType.BigSwap    => (long)ReadBigSwapUInt64(span),
            EndianType.LittleSwap => (long)ReadLittleSwapUInt64(span),
            _ => BinaryPrimitives.ReadInt64BigEndian(span)
        };
    }

    /// <summary>
    /// 从起始位置读取一个有符号 <see cref="long"/>（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static long ToLong(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToLong(endian);

    /// <summary>
    /// 将整个跨度解析为有符号 <see cref="long"/> 数组（每 8 字节一个元素）。
    /// </summary>
    public static long[] ToLongs(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
    {
        var count = span.Length / 8;
        var result = new long[count];
        for (var i = 0; i < count; i++)
            result[i] = span.Slice(i * 8, 8).ToLong(endian);
        return result;
    }

    /// <summary>
    /// 将整个内存解析为有符号 <see cref="long"/> 数组（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static long[] ToLongs(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToLongs(endian);

    #endregion

    #region ulong

    /// <summary>
    /// 从起始位置读取一个 <see cref="ulong"/>（8 字节，跨 4 个寄存器）。
    /// </summary>
    public static ulong ToUlong(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
    {
        return endian switch
        {
            EndianType.Big        => BinaryPrimitives.ReadUInt64BigEndian(span),
            EndianType.Little     => BinaryPrimitives.ReadUInt64LittleEndian(span),
            EndianType.BigSwap    => ReadBigSwapUInt64(span),
            EndianType.LittleSwap => ReadLittleSwapUInt64(span),
            _ => BinaryPrimitives.ReadUInt64BigEndian(span)
        };
    }

    /// <summary>
    /// 从起始位置读取一个 <see cref="ulong"/>（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static ulong ToUlong(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToUlong(endian);

    /// <summary>
    /// 将整个跨度解析为 <see cref="ulong"/> 数组（每 8 字节一个元素）。
    /// </summary>
    public static ulong[] ToUlongs(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
    {
        var count = span.Length / 8;
        var result = new ulong[count];
        for (var i = 0; i < count; i++)
            result[i] = span.Slice(i * 8, 8).ToUlong(endian);
        return result;
    }

    /// <summary>
    /// 将整个内存解析为 <see cref="ulong"/> 数组（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static ulong[] ToUlongs(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToUlongs(endian);

    #endregion

    #region double

    /// <summary>
    /// 从起始位置读取一个 <see cref="double"/>（IEEE754 双精度，8 字节，跨 4 个寄存器）。
    /// </summary>
    public static double ToDouble(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
    {
        return endian switch
        {
            EndianType.Big        => BinaryPrimitives.ReadDoubleBigEndian(span),
            EndianType.Little     => BinaryPrimitives.ReadDoubleLittleEndian(span),
            EndianType.BigSwap    => BitConverter.UInt64BitsToDouble(ReadBigSwapUInt64(span)),
            EndianType.LittleSwap => BitConverter.UInt64BitsToDouble(ReadLittleSwapUInt64(span)),
            _ => BinaryPrimitives.ReadDoubleBigEndian(span)
        };
    }

    /// <summary>
    /// 从起始位置读取一个 <see cref="double"/>（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static double ToDouble(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToDouble(endian);

    /// <summary>
    /// 将整个跨度解析为 <see cref="double"/> 数组（每 8 字节一个元素）。
    /// </summary>
    public static double[] ToDoubles(this ReadOnlySpan<byte> span, EndianType endian = EndianType.Big)
    {
        var count = span.Length / 8;
        var result = new double[count];
        for (var i = 0; i < count; i++)
            result[i] = span.Slice(i * 8, 8).ToDouble(endian);
        return result;
    }

    /// <summary>
    /// 将整个内存解析为 <see cref="double"/> 数组（兼容 <see cref="ReadOnlyMemory{T}"/>）。
    /// </summary>
    public static double[] ToDoubles(this ReadOnlyMemory<byte> memory, EndianType endian = EndianType.Big)
        => memory.Span.ToDoubles(endian);

    #endregion

    #region 内部辅助 — swap 读取

    /// <summary>
    /// BigSwap (BADC)：每个 16-bit word 内字节交换后按大端读。
    /// 即 [B,A,D,C] → 先交换为 [A,B,C,D] → 大端读。
    /// </summary>
    internal static uint ReadBigSwapUInt32(ReadOnlySpan<byte> span)
    {
        Span<byte> tmp = stackalloc byte[4];
        tmp[0] = span[1]; tmp[1] = span[0]; // word0: BA → AB
        tmp[2] = span[3]; tmp[3] = span[2]; // word1: DC → CD
        return BinaryPrimitives.ReadUInt32BigEndian(tmp);
    }

    /// <summary>
    /// LittleSwap (CDAB)：交换两个 16-bit word 后按大端读。
    /// 即 [C,D,A,B] → 先交换为 [A,B,C,D] → 大端读。
    /// </summary>
    internal static uint ReadLittleSwapUInt32(ReadOnlySpan<byte> span)
    {
        Span<byte> tmp = stackalloc byte[4];
        tmp[0] = span[2]; tmp[1] = span[3]; // word1 → word0
        tmp[2] = span[0]; tmp[3] = span[1]; // word0 → word1
        return BinaryPrimitives.ReadUInt32BigEndian(tmp);
    }

    /// <summary>
    /// BigSwap (BADCFEHG)：每个 16-bit word 内字节交换后按大端读。
    /// </summary>
    internal static ulong ReadBigSwapUInt64(ReadOnlySpan<byte> span)
    {
        Span<byte> tmp = stackalloc byte[8];
        tmp[0] = span[1]; tmp[1] = span[0];
        tmp[2] = span[3]; tmp[3] = span[2];
        tmp[4] = span[5]; tmp[5] = span[4];
        tmp[6] = span[7]; tmp[7] = span[6];
        return BinaryPrimitives.ReadUInt64BigEndian(tmp);
    }

    /// <summary>
    /// LittleSwap (CDABGHEF)：交换每对 16-bit word 后按大端读。
    /// </summary>
    internal static ulong ReadLittleSwapUInt64(ReadOnlySpan<byte> span)
    {
        Span<byte> tmp = stackalloc byte[8];
        tmp[0] = span[2]; tmp[1] = span[3];
        tmp[2] = span[0]; tmp[3] = span[1];
        tmp[4] = span[6]; tmp[5] = span[7];
        tmp[6] = span[4]; tmp[7] = span[5];
        return BinaryPrimitives.ReadUInt64BigEndian(tmp);
    }

    #endregion
}

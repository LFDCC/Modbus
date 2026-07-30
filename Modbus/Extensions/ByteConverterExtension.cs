namespace Modbus;

/// <summary>
/// 数值类型 -> 字节序列的便捷写入扩展。
/// <para>
/// 底层使用 BCL <see cref="BinaryPrimitives"/>，不依赖 <c>TouchSocketBitConverter</c>。
/// 字节序通过 <see cref="EndianType"/> 参数控制，默认 <see cref="EndianType.Big"/>（Modbus 标准）。
/// </para>
/// <example>
/// <code>
/// var mem  = ((short)0x1234).ToMemoryBytes();                       // 默认大端
/// var mem2 = ((ushort)0x1234).ToMemoryBytes(EndianType.LittleSwap); // CDAB 字序
/// var arr  = new short[] { 1, 2, 3 }.ToMemoryBytes();               // 批量大端
/// </code>
/// </example>
/// </summary>
public static class ByteConverterExtension
{
    #region 单值 -> ReadOnlyMemory<byte>

    /// <summary>
    /// 将单个 <see cref="short"/> 序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存（2 字节）。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this short value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[2];
        WriteInt16(bytes, value, endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="ushort"/> 序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存（2 字节）。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this ushort value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[2];
        WriteUInt16(bytes, value, endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="int"/> 序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存（4 字节）。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this int value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[4];
        WriteInt32(bytes, value, endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="uint"/> 序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存（4 字节）。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this uint value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[4];
        WriteUInt32(bytes, value, endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="float"/> 序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存（4 字节）。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this float value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[4];
        WriteUInt32(bytes, BitConverter.SingleToUInt32Bits(value), endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="long"/> 序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存（8 字节）。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this long value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[8];
        WriteInt64(bytes, value, endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="ulong"/> 序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存（8 字节）。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this ulong value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[8];
        WriteUInt64(bytes, value, endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="double"/> 序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存（8 字节）。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this double value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[8];
        WriteUInt64(bytes, BitConverter.DoubleToUInt64Bits(value), endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="byte"/> 包装为 <see cref="ReadOnlyMemory{T}"/> 字节内存（1 字节）。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this byte value)
    {
        var bytes = new byte[1];
        bytes[0] = value;
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="bool"/> 包装为 <see cref="ReadOnlyMemory{T}"/> 字节内存（1 字节，0x00 或 0xFF）。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this bool value)
    {
        var bytes = new byte[1];
        bytes[0] = value ? (byte)0xFF : (byte)0x00;
        return new ReadOnlyMemory<byte>(bytes);
    }

    #endregion

    #region 单值 -> ReadOnlySpan<byte>

    /// <summary>
    /// 将单个 <see cref="short"/> 序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度（2 字节）。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this short value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[2];
        WriteInt16(bytes, value, endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="ushort"/> 序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度（2 字节）。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this ushort value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[2];
        WriteUInt16(bytes, value, endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="int"/> 序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度（4 字节）。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this int value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[4];
        WriteInt32(bytes, value, endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="uint"/> 序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度（4 字节）。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this uint value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[4];
        WriteUInt32(bytes, value, endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="float"/> 序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度（4 字节）。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this float value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[4];
        WriteUInt32(bytes, BitConverter.SingleToUInt32Bits(value), endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="long"/> 序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度（8 字节）。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this long value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[8];
        WriteInt64(bytes, value, endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="ulong"/> 序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度（8 字节）。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this ulong value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[8];
        WriteUInt64(bytes, value, endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="double"/> 序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度（8 字节）。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this double value, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[8];
        WriteUInt64(bytes, BitConverter.DoubleToUInt64Bits(value), endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="byte"/> 包装为 <see cref="ReadOnlySpan{T}"/> 字节跨度（1 字节）。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this byte value)
    {
        var bytes = new byte[1];
        bytes[0] = value;
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将单个 <see cref="bool"/> 包装为 <see cref="ReadOnlySpan{T}"/> 字节跨度（1 字节，0x00 或 0xFF）。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this bool value)
    {
        var bytes = new byte[1];
        bytes[0] = value ? (byte)0xFF : (byte)0x00;
        return new ReadOnlySpan<byte>(bytes);
    }

    #endregion

    #region 数组 -> ReadOnlyMemory<byte>

    /// <summary>
    /// 将 <see cref="short"/> 数组序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this short[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 2];
        for (var i = 0; i < array.Length; i++)
            WriteInt16(bytes.AsSpan(i * 2, 2), array[i], endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="ushort"/> 数组序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this ushort[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 2];
        for (var i = 0; i < array.Length; i++)
            WriteUInt16(bytes.AsSpan(i * 2, 2), array[i], endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="int"/> 数组序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this int[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 4];
        for (var i = 0; i < array.Length; i++)
            WriteInt32(bytes.AsSpan(i * 4, 4), array[i], endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="uint"/> 数组序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this uint[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 4];
        for (var i = 0; i < array.Length; i++)
            WriteUInt32(bytes.AsSpan(i * 4, 4), array[i], endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="float"/> 数组序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this float[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 4];
        for (var i = 0; i < array.Length; i++)
            WriteUInt32(bytes.AsSpan(i * 4, 4), BitConverter.SingleToUInt32Bits(array[i]), endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="long"/> 数组序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this long[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 8];
        for (var i = 0; i < array.Length; i++)
            WriteInt64(bytes.AsSpan(i * 8, 8), array[i], endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="ulong"/> 数组序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this ulong[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 8];
        for (var i = 0; i < array.Length; i++)
            WriteUInt64(bytes.AsSpan(i * 8, 8), array[i], endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="double"/> 数组序列化为 <see cref="ReadOnlyMemory{T}"/> 字节内存。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this double[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 8];
        for (var i = 0; i < array.Length; i++)
            WriteUInt64(bytes.AsSpan(i * 8, 8), BitConverter.DoubleToUInt64Bits(array[i]), endian);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="byte"/> 数组包装为 <see cref="ReadOnlyMemory{T}"/> 字节内存（零拷贝）。
    /// </summary>
    public static ReadOnlyMemory<byte> ToMemoryBytes(this byte[] array)
        => new ReadOnlyMemory<byte>(array);

    /// <summary>
    /// 将 <see cref="bool"/> 数组包装为 <see cref="ReadOnlyMemory{T}"/> 布尔内存。
    /// </summary>
    public static ReadOnlyMemory<bool> ToMemoryBools(this bool[] array)
        => new ReadOnlyMemory<bool>(array);

    #endregion

    #region 数组 -> ReadOnlySpan<byte>

    /// <summary>
    /// 将 <see cref="short"/> 数组序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this short[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 2];
        for (var i = 0; i < array.Length; i++)
            WriteInt16(bytes.AsSpan(i * 2, 2), array[i], endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="ushort"/> 数组序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this ushort[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 2];
        for (var i = 0; i < array.Length; i++)
            WriteUInt16(bytes.AsSpan(i * 2, 2), array[i], endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="int"/> 数组序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this int[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 4];
        for (var i = 0; i < array.Length; i++)
            WriteInt32(bytes.AsSpan(i * 4, 4), array[i], endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="uint"/> 数组序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this uint[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 4];
        for (var i = 0; i < array.Length; i++)
            WriteUInt32(bytes.AsSpan(i * 4, 4), array[i], endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="float"/> 数组序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this float[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 4];
        for (var i = 0; i < array.Length; i++)
            WriteUInt32(bytes.AsSpan(i * 4, 4), BitConverter.SingleToUInt32Bits(array[i]), endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="long"/> 数组序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this long[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 8];
        for (var i = 0; i < array.Length; i++)
            WriteInt64(bytes.AsSpan(i * 8, 8), array[i], endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="ulong"/> 数组序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this ulong[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 8];
        for (var i = 0; i < array.Length; i++)
            WriteUInt64(bytes.AsSpan(i * 8, 8), array[i], endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="double"/> 数组序列化为 <see cref="ReadOnlySpan{T}"/> 字节跨度。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this double[] array, EndianType endian = EndianType.Big)
    {
        var bytes = new byte[array.Length * 8];
        for (var i = 0; i < array.Length; i++)
            WriteUInt64(bytes.AsSpan(i * 8, 8), BitConverter.DoubleToUInt64Bits(array[i]), endian);
        return new ReadOnlySpan<byte>(bytes);
    }

    /// <summary>
    /// 将 <see cref="byte"/> 数组包装为 <see cref="ReadOnlySpan{T}"/> 字节跨度（零拷贝）。
    /// </summary>
    public static ReadOnlySpan<byte> ToSpanBytes(this byte[] array)
        => new ReadOnlySpan<byte>(array);

    /// <summary>
    /// 将 <see cref="bool"/> 数组包装为 <see cref="ReadOnlySpan{T}"/> 布尔跨度。
    /// </summary>
    public static ReadOnlySpan<bool> ToSpanBools(this bool[] array)
        => new ReadOnlySpan<bool>(array);

    #endregion

    #region 内部辅助 - 按字节序写入

    internal static void WriteUInt16(Span<byte> dest, ushort value, EndianType endian)
    {
        if (endian == EndianType.Big || endian == EndianType.LittleSwap)
            BinaryPrimitives.WriteUInt16BigEndian(dest, value);
        else
            BinaryPrimitives.WriteUInt16LittleEndian(dest, value);
    }

    internal static void WriteInt16(Span<byte> dest, short value, EndianType endian)
        => WriteUInt16(dest, (ushort)value, endian);

    internal static void WriteUInt32(Span<byte> dest, uint value, EndianType endian)
    {
        switch (endian)
        {
            case EndianType.Big:
                BinaryPrimitives.WriteUInt32BigEndian(dest, value);
                break;
            case EndianType.Little:
                BinaryPrimitives.WriteUInt32LittleEndian(dest, value);
                break;
            case EndianType.BigSwap:
                WriteBigSwapUInt32(dest, value);
                break;
            case EndianType.LittleSwap:
                WriteLittleSwapUInt32(dest, value);
                break;
            default:
                BinaryPrimitives.WriteUInt32BigEndian(dest, value);
                break;
        }
    }

    internal static void WriteInt32(Span<byte> dest, int value, EndianType endian)
        => WriteUInt32(dest, (uint)value, endian);

    internal static void WriteUInt64(Span<byte> dest, ulong value, EndianType endian)
    {
        switch (endian)
        {
            case EndianType.Big:
                BinaryPrimitives.WriteUInt64BigEndian(dest, value);
                break;
            case EndianType.Little:
                BinaryPrimitives.WriteUInt64LittleEndian(dest, value);
                break;
            case EndianType.BigSwap:
                WriteBigSwapUInt64(dest, value);
                break;
            case EndianType.LittleSwap:
                WriteLittleSwapUInt64(dest, value);
                break;
            default:
                BinaryPrimitives.WriteUInt64BigEndian(dest, value);
                break;
        }
    }

    internal static void WriteInt64(Span<byte> dest, long value, EndianType endian)
        => WriteUInt64(dest, (ulong)value, endian);

    /// <summary>
    /// BigSwap (BADC)：先大端写，再交换每个 16-bit word 内字节。
    /// </summary>
    internal static void WriteBigSwapUInt32(Span<byte> dest, uint value)
    {
        BinaryPrimitives.WriteUInt32BigEndian(dest, value);
        (dest[0], dest[1]) = (dest[1], dest[0]); // word0: AB -> BA
        (dest[2], dest[3]) = (dest[3], dest[2]); // word1: CD -> DC
    }

    /// <summary>
    /// LittleSwap (CDAB)：先大端写，再交换两个 16-bit word。
    /// </summary>
    internal static void WriteLittleSwapUInt32(Span<byte> dest, uint value)
    {
        BinaryPrimitives.WriteUInt32BigEndian(dest, value);
        (dest[0], dest[1], dest[2], dest[3]) = (dest[2], dest[3], dest[0], dest[1]);
    }

    /// <summary>
    /// BigSwap (BADCFEHG)：先大端写，再交换每个 16-bit word 内字节。
    /// </summary>
    internal static void WriteBigSwapUInt64(Span<byte> dest, ulong value)
    {
        BinaryPrimitives.WriteUInt64BigEndian(dest, value);
        (dest[0], dest[1]) = (dest[1], dest[0]);
        (dest[2], dest[3]) = (dest[3], dest[2]);
        (dest[4], dest[5]) = (dest[5], dest[4]);
        (dest[6], dest[7]) = (dest[7], dest[6]);
    }

    /// <summary>
    /// LittleSwap (CDABGHEF)：先大端写，再交换每对 16-bit word。
    /// </summary>
    internal static void WriteLittleSwapUInt64(Span<byte> dest, ulong value)
    {
        BinaryPrimitives.WriteUInt64BigEndian(dest, value);
        (dest[0], dest[1], dest[2], dest[3]) = (dest[2], dest[3], dest[0], dest[1]);
        (dest[4], dest[5], dest[6], dest[7]) = (dest[6], dest[7], dest[4], dest[5]);
    }

    #endregion
}

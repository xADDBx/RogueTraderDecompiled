using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking.Settings;

[MemoryPackable(GenerateType.Object)]
public sealed class IntSettingNetData : TypedBaseSettingNetData<int>, IMemoryPackable<IntSettingNetData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class IntSettingNetDataFormatter : MemoryPackFormatter<IntSettingNetData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref IntSettingNetData value)
		{
			IntSettingNetData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref IntSettingNetData value)
		{
			IntSettingNetData.Deserialize(ref reader, ref value);
		}
	}

	[JsonConstructor]
	private IntSettingNetData()
	{
	}

	[MemoryPackConstructor]
	public IntSettingNetData(byte index, int value)
		: base(index, value)
	{
	}

	static IntSettingNetData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<IntSettingNetData>())
		{
			MemoryPackFormatterProvider.Register(new IntSettingNetDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<IntSettingNetData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<IntSettingNetData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref IntSettingNetData? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.Index, in value.Value);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref IntSettingNetData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		byte value2;
		int value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<byte, int>(out value2, out value3);
			}
			else
			{
				value2 = value.Index;
				value3 = value.Value;
				reader.ReadUnmanaged<byte>(out value2);
				reader.ReadUnmanaged<int>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(IntSettingNetData), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = 0;
			}
			else
			{
				value2 = value.Index;
				value3 = value.Value;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<byte>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new IntSettingNetData(value2, value3);
	}
}

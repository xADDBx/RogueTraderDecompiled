using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking.Settings;

[MemoryPackable(GenerateType.Object)]
public sealed class BoolSettingNetData : TypedBaseSettingNetData<bool>, IMemoryPackable<BoolSettingNetData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BoolSettingNetDataFormatter : MemoryPackFormatter<BoolSettingNetData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BoolSettingNetData value)
		{
			BoolSettingNetData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BoolSettingNetData value)
		{
			BoolSettingNetData.Deserialize(ref reader, ref value);
		}
	}

	[JsonConstructor]
	private BoolSettingNetData()
	{
	}

	[MemoryPackConstructor]
	public BoolSettingNetData(byte index, bool value)
		: base(index, value)
	{
	}

	static BoolSettingNetData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BoolSettingNetData>())
		{
			MemoryPackFormatterProvider.Register(new BoolSettingNetDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BoolSettingNetData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BoolSettingNetData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BoolSettingNetData? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BoolSettingNetData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		byte value2;
		bool value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<byte, bool>(out value2, out value3);
			}
			else
			{
				value2 = value.Index;
				value3 = value.Value;
				reader.ReadUnmanaged<byte>(out value2);
				reader.ReadUnmanaged<bool>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BoolSettingNetData), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = false;
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
					reader.ReadUnmanaged<bool>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new BoolSettingNetData(value2, value3);
	}
}

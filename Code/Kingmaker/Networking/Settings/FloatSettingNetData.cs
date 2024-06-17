using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking.Settings;

[MemoryPackable(GenerateType.Object)]
public sealed class FloatSettingNetData : TypedBaseSettingNetData<float>, IMemoryPackable<FloatSettingNetData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class FloatSettingNetDataFormatter : MemoryPackFormatter<FloatSettingNetData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref FloatSettingNetData value)
		{
			FloatSettingNetData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref FloatSettingNetData value)
		{
			FloatSettingNetData.Deserialize(ref reader, ref value);
		}
	}

	[JsonConstructor]
	private FloatSettingNetData()
	{
	}

	[MemoryPackConstructor]
	public FloatSettingNetData(byte index, float value)
		: base(index, value)
	{
	}

	static FloatSettingNetData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<FloatSettingNetData>())
		{
			MemoryPackFormatterProvider.Register(new FloatSettingNetDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<FloatSettingNetData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<FloatSettingNetData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref FloatSettingNetData? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref FloatSettingNetData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		byte value2;
		float value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<byte, float>(out value2, out value3);
			}
			else
			{
				value2 = value.Index;
				value3 = value.Value;
				reader.ReadUnmanaged<byte>(out value2);
				reader.ReadUnmanaged<float>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(FloatSettingNetData), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = 0f;
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
					reader.ReadUnmanaged<float>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new FloatSettingNetData(value2, value3);
	}
}

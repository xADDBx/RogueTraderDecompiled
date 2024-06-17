using Kingmaker.Settings.Entities;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking.Settings;

[MemoryPackable(GenerateType.Object)]
public sealed class EnumSettingNetData : TypedBaseSettingNetData<int>, IMemoryPackable<EnumSettingNetData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class EnumSettingNetDataFormatter : MemoryPackFormatter<EnumSettingNetData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref EnumSettingNetData value)
		{
			EnumSettingNetData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EnumSettingNetData value)
		{
			EnumSettingNetData.Deserialize(ref reader, ref value);
		}
	}

	[JsonConstructor]
	private EnumSettingNetData()
	{
	}

	[MemoryPackConstructor]
	public EnumSettingNetData(byte index, int value)
		: base(index, value)
	{
	}

	public override void ForceSet()
	{
		((ISettingsEntityEnum)PhotonManager.Settings.SettingsForSync[Index]).SetValueAndConfirm(Value);
	}

	static EnumSettingNetData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EnumSettingNetData>())
		{
			MemoryPackFormatterProvider.Register(new EnumSettingNetDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EnumSettingNetData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EnumSettingNetData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref EnumSettingNetData? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref EnumSettingNetData? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EnumSettingNetData), 2, memberCount);
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
		value = new EnumSettingNetData(value2, value3);
	}
}

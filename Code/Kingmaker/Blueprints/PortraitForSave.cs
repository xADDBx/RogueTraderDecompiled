using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Blueprints;

[MemoryPackable(GenerateType.Object)]
public sealed class PortraitForSave : IMemoryPackable<PortraitForSave>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PortraitForSaveFormatter : MemoryPackFormatter<PortraitForSave>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PortraitForSave value)
		{
			PortraitForSave.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PortraitForSave value)
		{
			PortraitForSave.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintPortrait m_Blueprint;

	[JsonProperty]
	[MemoryPackInclude]
	private PortraitData m_Data;

	[JsonProperty]
	[MemoryPackInclude]
	private bool m_IsMainCharacter;

	[MemoryPackIgnore]
	public PortraitData Data
	{
		get
		{
			if (!m_Blueprint)
			{
				return m_Data;
			}
			return m_Blueprint.Data;
		}
	}

	[MemoryPackIgnore]
	public bool IsMainCharacter => m_IsMainCharacter;

	public PortraitForSave(BlueprintPortrait blueprint, bool isMainCharacter)
	{
		m_Blueprint = blueprint;
		m_IsMainCharacter = isMainCharacter;
	}

	public PortraitForSave(PortraitData data, bool isMainCharacter)
	{
		m_Data = data;
		m_IsMainCharacter = isMainCharacter;
	}

	[JsonConstructor]
	[MemoryPackConstructor]
	public PortraitForSave()
	{
	}

	static PortraitForSave()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PortraitForSave>())
		{
			MemoryPackFormatterProvider.Register(new PortraitForSaveFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PortraitForSave[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PortraitForSave>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PortraitForSave? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WriteValue(in value.m_Blueprint);
		writer.WritePackable(in value.m_Data);
		writer.WriteUnmanaged(in value.m_IsMainCharacter);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PortraitForSave? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintPortrait value2;
		PortraitData value3;
		bool value4;
		if (memberCount == 3)
		{
			if (value != null)
			{
				value2 = value.m_Blueprint;
				value3 = value.m_Data;
				value4 = value.m_IsMainCharacter;
				reader.ReadValue(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadUnmanaged<bool>(out value4);
				goto IL_00c8;
			}
			value2 = reader.ReadValue<BlueprintPortrait>();
			value3 = reader.ReadPackable<PortraitData>();
			reader.ReadUnmanaged<bool>(out value4);
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PortraitForSave), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
				value4 = false;
			}
			else
			{
				value2 = value.m_Blueprint;
				value3 = value.m_Data;
				value4 = value.m_IsMainCharacter;
			}
			if (memberCount != 0)
			{
				reader.ReadValue(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<bool>(out value4);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00c8;
			}
		}
		value = new PortraitForSave
		{
			m_Blueprint = value2,
			m_Data = value3,
			m_IsMainCharacter = value4
		};
		return;
		IL_00c8:
		value.m_Blueprint = value2;
		value.m_Data = value3;
		value.m_IsMainCharacter = value4;
	}
}

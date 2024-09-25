using System;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintUnitReference : BlueprintReference<BlueprintUnit>, IMemoryPackable<BlueprintUnitReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintUnitReferenceFormatter : MemoryPackFormatter<BlueprintUnitReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintUnitReference value)
		{
			BlueprintUnitReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintUnitReference value)
		{
			BlueprintUnitReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintUnitReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintUnitReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintUnitReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintUnitReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintUnitReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintUnitReference? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteString(value.guid);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintUnitReference? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string text;
		if (memberCount == 1)
		{
			if (value != null)
			{
				text = value.guid;
				text = reader.ReadString();
				goto IL_0068;
			}
			text = reader.ReadString();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintUnitReference), 1, memberCount);
				return;
			}
			text = ((value != null) ? value.guid : null);
			if (memberCount != 0)
			{
				text = reader.ReadString();
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0068;
			}
		}
		value = new BlueprintUnitReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

using System;
using Kingmaker.Visual.Sound;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintUnitAsksListReference : BlueprintReference<BlueprintUnitAsksList>, IMemoryPackable<BlueprintUnitAsksListReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintUnitAsksListReferenceFormatter : MemoryPackFormatter<BlueprintUnitAsksListReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintUnitAsksListReference value)
		{
			BlueprintUnitAsksListReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintUnitAsksListReference value)
		{
			BlueprintUnitAsksListReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintUnitAsksListReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintUnitAsksListReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintUnitAsksListReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintUnitAsksListReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintUnitAsksListReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintUnitAsksListReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintUnitAsksListReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintUnitAsksListReference), 1, memberCount);
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
		value = new BlueprintUnitAsksListReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

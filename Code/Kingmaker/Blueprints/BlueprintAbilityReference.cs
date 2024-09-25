using System;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintAbilityReference : BlueprintReference<BlueprintAbility>, IMemoryPackable<BlueprintAbilityReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintAbilityReferenceFormatter : MemoryPackFormatter<BlueprintAbilityReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintAbilityReference value)
		{
			BlueprintAbilityReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintAbilityReference value)
		{
			BlueprintAbilityReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintAbilityReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintAbilityReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintAbilityReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintAbilityReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintAbilityReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintAbilityReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintAbilityReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintAbilityReference), 1, memberCount);
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
		value = new BlueprintAbilityReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

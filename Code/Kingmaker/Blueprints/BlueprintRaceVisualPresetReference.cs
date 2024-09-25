using System;
using Kingmaker.UnitLogic.Levelup.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintRaceVisualPresetReference : BlueprintReference<BlueprintRaceVisualPreset>, IMemoryPackable<BlueprintRaceVisualPresetReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintRaceVisualPresetReferenceFormatter : MemoryPackFormatter<BlueprintRaceVisualPresetReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintRaceVisualPresetReference value)
		{
			BlueprintRaceVisualPresetReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintRaceVisualPresetReference value)
		{
			BlueprintRaceVisualPresetReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintRaceVisualPresetReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintRaceVisualPresetReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintRaceVisualPresetReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintRaceVisualPresetReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintRaceVisualPresetReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintRaceVisualPresetReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintRaceVisualPresetReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintRaceVisualPresetReference), 1, memberCount);
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
		value = new BlueprintRaceVisualPresetReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

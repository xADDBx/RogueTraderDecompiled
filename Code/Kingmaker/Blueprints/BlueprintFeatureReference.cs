using System;
using Kingmaker.UnitLogic.Progression.Features;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintFeatureReference : BlueprintReference<BlueprintFeature>, IMemoryPackable<BlueprintFeatureReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintFeatureReferenceFormatter : MemoryPackFormatter<BlueprintFeatureReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintFeatureReference value)
		{
			BlueprintFeatureReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintFeatureReference value)
		{
			BlueprintFeatureReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintFeatureReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintFeatureReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintFeatureReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintFeatureReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintFeatureReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintFeatureReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintFeatureReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintFeatureReference), 1, memberCount);
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
		value = new BlueprintFeatureReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

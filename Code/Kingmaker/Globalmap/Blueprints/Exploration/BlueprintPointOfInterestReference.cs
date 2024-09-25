using System;
using Kingmaker.Blueprints;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintPointOfInterestReference : BlueprintReference<BlueprintPointOfInterest>, IMemoryPackable<BlueprintPointOfInterestReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintPointOfInterestReferenceFormatter : MemoryPackFormatter<BlueprintPointOfInterestReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintPointOfInterestReference value)
		{
			BlueprintPointOfInterestReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintPointOfInterestReference value)
		{
			BlueprintPointOfInterestReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintPointOfInterestReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintPointOfInterestReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintPointOfInterestReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintPointOfInterestReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintPointOfInterestReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintPointOfInterestReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintPointOfInterestReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintPointOfInterestReference), 1, memberCount);
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
		value = new BlueprintPointOfInterestReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

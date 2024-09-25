using System;
using Kingmaker.Blueprints;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintColonyEventReference : BlueprintReference<BlueprintColonyEvent>, IMemoryPackable<BlueprintColonyEventReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintColonyEventReferenceFormatter : MemoryPackFormatter<BlueprintColonyEventReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintColonyEventReference value)
		{
			BlueprintColonyEventReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintColonyEventReference value)
		{
			BlueprintColonyEventReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintColonyEventReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintColonyEventReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintColonyEventReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintColonyEventReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintColonyEventReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintColonyEventReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintColonyEventReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintColonyEventReference), 1, memberCount);
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
		value = new BlueprintColonyEventReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

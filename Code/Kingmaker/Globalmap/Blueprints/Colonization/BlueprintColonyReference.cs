using System;
using Kingmaker.Blueprints;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintColonyReference : BlueprintReference<BlueprintColony>, IMemoryPackable<BlueprintColonyReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintColonyReferenceFormatter : MemoryPackFormatter<BlueprintColonyReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintColonyReference value)
		{
			BlueprintColonyReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintColonyReference value)
		{
			BlueprintColonyReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintColonyReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintColonyReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintColonyReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintColonyReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintColonyReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintColonyReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintColonyReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintColonyReference), 1, memberCount);
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
		value = new BlueprintColonyReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

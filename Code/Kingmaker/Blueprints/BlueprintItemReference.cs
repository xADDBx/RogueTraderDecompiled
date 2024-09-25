using System;
using Kingmaker.Blueprints.Items;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintItemReference : BlueprintReference<BlueprintItem>, IMemoryPackable<BlueprintItemReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintItemReferenceFormatter : MemoryPackFormatter<BlueprintItemReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintItemReference value)
		{
			BlueprintItemReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintItemReference value)
		{
			BlueprintItemReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintItemReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintItemReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintItemReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintItemReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintItemReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintItemReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintItemReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintItemReference), 1, memberCount);
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
		value = new BlueprintItemReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

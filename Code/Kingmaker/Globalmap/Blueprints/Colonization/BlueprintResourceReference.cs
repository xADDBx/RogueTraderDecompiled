using System;
using Kingmaker.Blueprints;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintResourceReference : BlueprintReference<BlueprintResource>, IMemoryPackable<BlueprintResourceReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintResourceReferenceFormatter : MemoryPackFormatter<BlueprintResourceReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintResourceReference value)
		{
			BlueprintResourceReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintResourceReference value)
		{
			BlueprintResourceReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintResourceReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintResourceReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintResourceReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintResourceReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintResourceReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintResourceReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintResourceReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintResourceReference), 1, memberCount);
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
		value = new BlueprintResourceReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

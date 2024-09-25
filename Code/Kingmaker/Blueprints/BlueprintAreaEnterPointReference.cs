using System;
using Kingmaker.Blueprints.Area;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintAreaEnterPointReference : BlueprintReference<BlueprintAreaEnterPoint>, IMemoryPackable<BlueprintAreaEnterPointReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintAreaEnterPointReferenceFormatter : MemoryPackFormatter<BlueprintAreaEnterPointReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintAreaEnterPointReference value)
		{
			BlueprintAreaEnterPointReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintAreaEnterPointReference value)
		{
			BlueprintAreaEnterPointReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintAreaEnterPointReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintAreaEnterPointReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintAreaEnterPointReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintAreaEnterPointReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintAreaEnterPointReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintAreaEnterPointReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintAreaEnterPointReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintAreaEnterPointReference), 1, memberCount);
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
		value = new BlueprintAreaEnterPointReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

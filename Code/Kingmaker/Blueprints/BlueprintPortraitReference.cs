using System;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintPortraitReference : BlueprintReference<BlueprintPortrait>, IMemoryPackable<BlueprintPortraitReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintPortraitReferenceFormatter : MemoryPackFormatter<BlueprintPortraitReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintPortraitReference value)
		{
			BlueprintPortraitReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintPortraitReference value)
		{
			BlueprintPortraitReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintPortraitReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintPortraitReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintPortraitReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintPortraitReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintPortraitReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintPortraitReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintPortraitReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintPortraitReference), 1, memberCount);
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
		value = new BlueprintPortraitReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

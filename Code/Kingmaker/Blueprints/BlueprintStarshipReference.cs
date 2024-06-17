using System;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintStarshipReference : BlueprintReference<BlueprintStarship>, IMemoryPackable<BlueprintStarshipReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintStarshipReferenceFormatter : MemoryPackFormatter<BlueprintStarshipReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintStarshipReference value)
		{
			BlueprintStarshipReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintStarshipReference value)
		{
			BlueprintStarshipReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintStarshipReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintStarshipReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintStarshipReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintStarshipReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintStarshipReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintStarshipReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintStarshipReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintStarshipReference), 1, memberCount);
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
		value = new BlueprintStarshipReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

using System;
using Kingmaker.UnitLogic.Progression.Paths;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintCareerPathReference : BlueprintReference<BlueprintCareerPath>, IMemoryPackable<BlueprintCareerPathReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintCareerPathReferenceFormatter : MemoryPackFormatter<BlueprintCareerPathReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintCareerPathReference value)
		{
			BlueprintCareerPathReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintCareerPathReference value)
		{
			BlueprintCareerPathReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintCareerPathReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintCareerPathReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintCareerPathReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintCareerPathReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintCareerPathReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintCareerPathReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintCareerPathReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintCareerPathReference), 1, memberCount);
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
		value = new BlueprintCareerPathReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

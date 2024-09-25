using System;
using Kingmaker.Blueprints;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Globalmap.Blueprints.SystemMap;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintStarSystemObjectReference : BlueprintReference<BlueprintStarSystemObject>, IMemoryPackable<BlueprintStarSystemObjectReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintStarSystemObjectReferenceFormatter : MemoryPackFormatter<BlueprintStarSystemObjectReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintStarSystemObjectReference value)
		{
			BlueprintStarSystemObjectReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintStarSystemObjectReference value)
		{
			BlueprintStarSystemObjectReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintStarSystemObjectReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintStarSystemObjectReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintStarSystemObjectReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintStarSystemObjectReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintStarSystemObjectReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintStarSystemObjectReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintStarSystemObjectReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintStarSystemObjectReference), 1, memberCount);
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
		value = new BlueprintStarSystemObjectReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

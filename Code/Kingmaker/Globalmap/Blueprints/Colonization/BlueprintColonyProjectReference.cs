using System;
using Kingmaker.Blueprints;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintColonyProjectReference : BlueprintReference<BlueprintColonyProject>, IMemoryPackable<BlueprintColonyProjectReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintColonyProjectReferenceFormatter : MemoryPackFormatter<BlueprintColonyProjectReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintColonyProjectReference value)
		{
			BlueprintColonyProjectReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintColonyProjectReference value)
		{
			BlueprintColonyProjectReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintColonyProjectReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintColonyProjectReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintColonyProjectReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintColonyProjectReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintColonyProjectReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintColonyProjectReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintColonyProjectReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintColonyProjectReference), 1, memberCount);
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
		value = new BlueprintColonyProjectReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

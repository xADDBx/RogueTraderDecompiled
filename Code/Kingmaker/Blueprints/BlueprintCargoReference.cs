using System;
using Kingmaker.Blueprints.Cargo;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintCargoReference : BlueprintReference<BlueprintCargo>, IMemoryPackable<BlueprintCargoReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintCargoReferenceFormatter : MemoryPackFormatter<BlueprintCargoReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintCargoReference value)
		{
			BlueprintCargoReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintCargoReference value)
		{
			BlueprintCargoReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintCargoReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintCargoReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintCargoReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintCargoReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintCargoReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintCargoReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintCargoReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintCargoReference), 1, memberCount);
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
		value = new BlueprintCargoReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

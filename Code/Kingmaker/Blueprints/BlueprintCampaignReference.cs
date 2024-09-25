using System;
using Kingmaker.Blueprints.Root;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintCampaignReference : BlueprintReference<BlueprintCampaign>, IMemoryPackable<BlueprintCampaignReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintCampaignReferenceFormatter : MemoryPackFormatter<BlueprintCampaignReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintCampaignReference value)
		{
			BlueprintCampaignReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintCampaignReference value)
		{
			BlueprintCampaignReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintCampaignReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintCampaignReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintCampaignReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintCampaignReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintCampaignReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintCampaignReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintCampaignReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintCampaignReference), 1, memberCount);
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
		value = new BlueprintCampaignReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

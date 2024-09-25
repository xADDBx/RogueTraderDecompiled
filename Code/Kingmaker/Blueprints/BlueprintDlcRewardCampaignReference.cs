using System;
using Kingmaker.DLC;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Blueprints;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class BlueprintDlcRewardCampaignReference : BlueprintReference<BlueprintDlcRewardCampaign>, IMemoryPackable<BlueprintDlcRewardCampaignReference>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class BlueprintDlcRewardCampaignReferenceFormatter : MemoryPackFormatter<BlueprintDlcRewardCampaignReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintDlcRewardCampaignReference value)
		{
			BlueprintDlcRewardCampaignReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintDlcRewardCampaignReference value)
		{
			BlueprintDlcRewardCampaignReference.Deserialize(ref reader, ref value);
		}
	}

	static BlueprintDlcRewardCampaignReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintDlcRewardCampaignReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintDlcRewardCampaignReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintDlcRewardCampaignReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintDlcRewardCampaignReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintDlcRewardCampaignReference? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintDlcRewardCampaignReference? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintDlcRewardCampaignReference), 1, memberCount);
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
		value = new BlueprintDlcRewardCampaignReference
		{
			guid = text
		};
		return;
		IL_0068:
		value.guid = text;
	}
}

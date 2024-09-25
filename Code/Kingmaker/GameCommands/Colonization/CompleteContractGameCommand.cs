using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Designers;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands.Colonization;

[MemoryPackable(GenerateType.Object)]
public class CompleteContractGameCommand : GameCommand, IMemoryPackable<CompleteContractGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CompleteContractGameCommandFormatter : MemoryPackFormatter<CompleteContractGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CompleteContractGameCommand value)
		{
			CompleteContractGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CompleteContractGameCommand value)
		{
			CompleteContractGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	public BlueprintQuestReference Quest;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private CompleteContractGameCommand()
	{
	}

	[JsonConstructor]
	public CompleteContractGameCommand(BlueprintQuest quest)
	{
		Quest = quest.ToReference<BlueprintQuestReference>();
	}

	protected override void ExecuteInternal()
	{
		GameHelper.Quests.CompleteObjective(GameHelper.Quests.GetQuest(Quest).Objectives.FirstOrDefault()?.Blueprint);
	}

	static CompleteContractGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CompleteContractGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CompleteContractGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CompleteContractGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CompleteContractGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CompleteContractGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.Quest);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CompleteContractGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintQuestReference value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.Quest;
				reader.ReadPackable(ref value2);
				goto IL_006a;
			}
			value2 = reader.ReadPackable<BlueprintQuestReference>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CompleteContractGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.Quest : null);
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006a;
			}
		}
		value = new CompleteContractGameCommand
		{
			Quest = value2
		};
		return;
		IL_006a:
		value.Quest = value2;
	}
}

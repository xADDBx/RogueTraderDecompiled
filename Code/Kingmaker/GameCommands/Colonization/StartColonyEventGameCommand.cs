using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands.Colonization;

[MemoryPackable(GenerateType.Object)]
public class StartColonyEventGameCommand : GameCommand, IMemoryPackable<StartColonyEventGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class StartColonyEventGameCommandFormatter : MemoryPackFormatter<StartColonyEventGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref StartColonyEventGameCommand value)
		{
			StartColonyEventGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StartColonyEventGameCommand value)
		{
			StartColonyEventGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintColonyReference m_Colony;

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintColonyEventReference m_Event;

	private BlueprintColony Colony => m_Colony?.Get();

	private BlueprintColonyEvent Event => m_Event?.Get();

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private StartColonyEventGameCommand()
	{
	}

	[JsonConstructor]
	public StartColonyEventGameCommand([NotNull] BlueprintColony colony, [NotNull] BlueprintColonyEvent colonyEvent)
	{
		m_Colony = colony.ToReference<BlueprintColonyReference>();
		m_Event = colonyEvent.ToReference<BlueprintColonyEventReference>();
	}

	protected override void ExecuteInternal()
	{
		if (Colony != null && Event != null)
		{
			(Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData data) => data.Colony.Blueprint == Colony)?.Colony)?.StartEvent(m_Event?.Get());
		}
	}

	static StartColonyEventGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StartColonyEventGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new StartColonyEventGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StartColonyEventGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<StartColonyEventGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref StartColonyEventGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Colony);
		writer.WritePackable(in value.m_Event);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref StartColonyEventGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintColonyReference value2;
		BlueprintColonyEventReference value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_Colony;
				value3 = value.m_Event;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				goto IL_009a;
			}
			value2 = reader.ReadPackable<BlueprintColonyReference>();
			value3 = reader.ReadPackable<BlueprintColonyEventReference>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StartColonyEventGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
			}
			else
			{
				value2 = value.m_Colony;
				value3 = value.m_Event;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_009a;
			}
		}
		value = new StartColonyEventGameCommand
		{
			m_Colony = value2,
			m_Event = value3
		};
		return;
		IL_009a:
		value.m_Colony = value2;
		value.m_Event = value3;
	}
}

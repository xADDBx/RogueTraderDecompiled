using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands.Colonization;

[MemoryPackable(GenerateType.Object)]
public class ColonyProjectsUIOpenGameCommand : GameCommand, IMemoryPackable<ColonyProjectsUIOpenGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ColonyProjectsUIOpenGameCommandFormatter : MemoryPackFormatter<ColonyProjectsUIOpenGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ColonyProjectsUIOpenGameCommand value)
		{
			ColonyProjectsUIOpenGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ColonyProjectsUIOpenGameCommand value)
		{
			ColonyProjectsUIOpenGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintColonyReference m_Colony;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private ColonyProjectsUIOpenGameCommand()
	{
	}

	[JsonConstructor]
	public ColonyProjectsUIOpenGameCommand([NotNull] BlueprintColonyReference colony)
	{
		m_Colony = colony;
	}

	protected override void ExecuteInternal()
	{
		Colony colony = Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData data) => data.Colony.Blueprint == m_Colony.Get())?.Colony;
		if (colony != null)
		{
			EventBus.RaiseEvent(delegate(IColonizationProjectsUIHandler h)
			{
				h.HandleColonyProjectsUIOpen(colony);
			});
		}
	}

	static ColonyProjectsUIOpenGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ColonyProjectsUIOpenGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ColonyProjectsUIOpenGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ColonyProjectsUIOpenGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ColonyProjectsUIOpenGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ColonyProjectsUIOpenGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_Colony);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ColonyProjectsUIOpenGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintColonyReference value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_Colony;
				reader.ReadPackable(ref value2);
				goto IL_006a;
			}
			value2 = reader.ReadPackable<BlueprintColonyReference>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ColonyProjectsUIOpenGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Colony : null);
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
		value = new ColonyProjectsUIOpenGameCommand
		{
			m_Colony = value2
		};
		return;
		IL_006a:
		value.m_Colony = value2;
	}
}

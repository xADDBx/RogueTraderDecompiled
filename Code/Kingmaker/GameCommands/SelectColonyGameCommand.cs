using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class SelectColonyGameCommand : GameCommandWithSynchronized, IMemoryPackable<SelectColonyGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SelectColonyGameCommandFormatter : MemoryPackFormatter<SelectColonyGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SelectColonyGameCommand value)
		{
			SelectColonyGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SelectColonyGameCommand value)
		{
			SelectColonyGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly ColonyRef m_ColonyRef;

	[MemoryPackConstructor]
	private SelectColonyGameCommand(ColonyRef m_colonyRef)
	{
		m_ColonyRef = m_colonyRef;
	}

	public SelectColonyGameCommand(Colony colony, bool isSynchronized)
		: this((ColonyRef)colony)
	{
		m_IsSynchronized = isSynchronized;
	}

	protected override void ExecuteInternal()
	{
		if (!m_ColonyRef.TryGet(out var colony))
		{
			PFLog.Net.Error("[SelectColonyGameCommand] Colony not found! " + m_ColonyRef);
			return;
		}
		EventBus.RaiseEvent(delegate(IColonyManagementUIHandler x)
		{
			x.HandleColonyManagementPage(colony);
		});
		ColonyChronicleExtensions.TryStartChronicle(colony);
	}

	static SelectColonyGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SelectColonyGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SelectColonyGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SelectColonyGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SelectColonyGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SelectColonyGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_ColonyRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SelectColonyGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ColonyRef value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<ColonyRef>();
			}
			else
			{
				value2 = value.m_ColonyRef;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SelectColonyGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_ColonyRef : default(ColonyRef));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new SelectColonyGameCommand(value2);
	}
}

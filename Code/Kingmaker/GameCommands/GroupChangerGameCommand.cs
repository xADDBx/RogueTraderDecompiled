using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class GroupChangerGameCommand : GameCommand, IMemoryPackable<GroupChangerGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class GroupChangerGameCommandFormatter : MemoryPackFormatter<GroupChangerGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref GroupChangerGameCommand value)
		{
			GroupChangerGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref GroupChangerGameCommand value)
		{
			GroupChangerGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly UnitReference m_UnitReference;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private GroupChangerGameCommand()
	{
	}

	[MemoryPackConstructor]
	public GroupChangerGameCommand(UnitReference m_unitReference)
	{
		m_UnitReference = m_unitReference;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(IChangeGroupHandler h)
		{
			h.HandleChangeGroup(m_UnitReference.Id);
		});
	}

	static GroupChangerGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<GroupChangerGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new GroupChangerGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GroupChangerGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<GroupChangerGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref GroupChangerGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_UnitReference);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref GroupChangerGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		UnitReference value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<UnitReference>();
			}
			else
			{
				value2 = value.m_UnitReference;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(GroupChangerGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_UnitReference : default(UnitReference));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new GroupChangerGameCommand(value2);
	}
}

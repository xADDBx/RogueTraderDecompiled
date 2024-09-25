using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetPregenGameCommand : GameCommand, IMemoryPackable<CharGenSetPregenGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSetPregenGameCommandFormatter : MemoryPackFormatter<CharGenSetPregenGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSetPregenGameCommand value)
		{
			CharGenSetPregenGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetPregenGameCommand value)
		{
			CharGenSetPregenGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_UnitRef;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CharGenSetPregenGameCommand(EntityRef<BaseUnitEntity> m_unitRef)
	{
		m_UnitRef = m_unitRef;
	}

	public CharGenSetPregenGameCommand([CanBeNull] BaseUnitEntity unit)
		: this((EntityRef<BaseUnitEntity>)unit)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenPregenHandler h)
		{
			h.HandleSetPregen(m_UnitRef.Entity);
		});
	}

	static CharGenSetPregenGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetPregenGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetPregenGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetPregenGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetPregenGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSetPregenGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_UnitRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetPregenGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<BaseUnitEntity> value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			}
			else
			{
				value2 = value.m_UnitRef;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetPregenGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_UnitRef : default(EntityRef<BaseUnitEntity>));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenSetPregenGameCommand(value2);
	}
}

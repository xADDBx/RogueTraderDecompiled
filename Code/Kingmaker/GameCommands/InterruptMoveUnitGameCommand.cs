using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class InterruptMoveUnitGameCommand : GameCommand, IMemoryPackable<InterruptMoveUnitGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class InterruptMoveUnitGameCommandFormatter : MemoryPackFormatter<InterruptMoveUnitGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref InterruptMoveUnitGameCommand value)
		{
			InterruptMoveUnitGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref InterruptMoveUnitGameCommand value)
		{
			InterruptMoveUnitGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly UnitReference m_UnitRef;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private InterruptMoveUnitGameCommand(UnitReference m_unitRef)
	{
		m_UnitRef = m_unitRef;
	}

	public InterruptMoveUnitGameCommand([NotNull] AbstractUnitEntity unit)
		: this(unit.FromAbstractUnitEntity())
	{
	}

	protected override void ExecuteInternal()
	{
		AbstractUnitEntity abstractUnitEntity = m_UnitRef.ToAbstractUnitEntity();
		if (abstractUnitEntity == null)
		{
			PFLog.GameCommands.Error("Unit '{0}' not found!", m_UnitRef.Id);
		}
		else
		{
			abstractUnitEntity.GetOptional<PartUnitCommands>()?.ForceInterruptMove();
		}
	}

	static InterruptMoveUnitGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<InterruptMoveUnitGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new InterruptMoveUnitGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<InterruptMoveUnitGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<InterruptMoveUnitGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref InterruptMoveUnitGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref InterruptMoveUnitGameCommand? value)
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
				value2 = value.m_UnitRef;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(InterruptMoveUnitGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_UnitRef : default(UnitReference));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new InterruptMoveUnitGameCommand(value2);
	}
}

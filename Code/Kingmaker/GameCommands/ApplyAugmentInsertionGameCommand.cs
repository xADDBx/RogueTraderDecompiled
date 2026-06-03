using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class ApplyAugmentInsertionGameCommand : GameCommand, IMemoryPackable<ApplyAugmentInsertionGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ApplyAugmentInsertionGameCommandFormatter : MemoryPackFormatter<ApplyAugmentInsertionGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ApplyAugmentInsertionGameCommand value)
		{
			ApplyAugmentInsertionGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ApplyAugmentInsertionGameCommand value)
		{
			ApplyAugmentInsertionGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<BaseUnitEntity> _unit;

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintAugmentSlotReference _slot;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private ApplyAugmentInsertionGameCommand()
	{
	}

	public ApplyAugmentInsertionGameCommand(BaseUnitEntity unit, BlueprintAugmentSlot slot)
	{
		_unit = unit;
		_slot = slot.ToReference<BlueprintAugmentSlotReference>();
	}

	protected override void ExecuteInternal()
	{
		PartUnitBody partUnitBody = _unit.Entity?.GetOptional<PartUnitBody>();
		if (partUnitBody != null)
		{
			BlueprintAugmentSlot blueprintAugmentSlot = _slot?.Get();
			if (blueprintAugmentSlot != null && partUnitBody.Augments.Slots.TryGetValue(blueprintAugmentSlot, out var value))
			{
				value.ApplyInsertion();
			}
		}
	}

	static ApplyAugmentInsertionGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ApplyAugmentInsertionGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ApplyAugmentInsertionGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ApplyAugmentInsertionGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ApplyAugmentInsertionGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ApplyAugmentInsertionGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value._unit);
		writer.WritePackable(in value._slot);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ApplyAugmentInsertionGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<BaseUnitEntity> value2;
		BlueprintAugmentSlotReference value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value._unit;
				value3 = value._slot;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				goto IL_00a0;
			}
			value2 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			value3 = reader.ReadPackable<BlueprintAugmentSlotReference>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ApplyAugmentInsertionGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<BaseUnitEntity>);
				value3 = null;
			}
			else
			{
				value2 = value._unit;
				value3 = value._slot;
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
				goto IL_00a0;
			}
		}
		value = new ApplyAugmentInsertionGameCommand
		{
			_unit = value2,
			_slot = value3
		};
		return;
		IL_00a0:
		value._unit = value2;
		value._slot = value3;
	}
}

using System;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Code.EntitySystem.Entities;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Interaction;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.UnitLogic.Commands;

[MemoryPackable(GenerateType.Object)]
public sealed class UnitInteractWithObjectParams : UnitCommandParams<UnitInteractWithObject>, IMemoryPackable<UnitInteractWithObjectParams>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class UnitInteractWithObjectParamsFormatter : MemoryPackFormatter<UnitInteractWithObjectParams>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref UnitInteractWithObjectParams value)
		{
			UnitInteractWithObjectParams.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnitInteractWithObjectParams value)
		{
			UnitInteractWithObjectParams.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private ViewBasedPartRef<MapObjectEntity, InteractionPart> m_Interaction;

	[JsonProperty]
	[MemoryPackInclude]
	private ViewBasedPartRef<MapObjectEntity, ViewBasedPart> m_VariantActor;

	[MemoryPackIgnore]
	public InteractionPart Interaction => m_Interaction;

	[MemoryPackIgnore]
	public ViewBasedPart VariantActor => m_VariantActor;

	public override int DefaultApproachRadius
	{
		get
		{
			if (Interaction.Settings.ProximityRadius <= 0)
			{
				return 2;
			}
			return Interaction.Settings.ProximityRadius;
		}
	}

	[JsonConstructor]
	[MemoryPackConstructor]
	private UnitInteractWithObjectParams()
		: base(default(JsonConstructorMark))
	{
	}

	public UnitInteractWithObjectParams([NotNull] InteractionPart interaction, [CanBeNull] IInteractionVariantActor variantActor = null)
		: base((TargetWrapper)interaction.Owner)
	{
		if (interaction.Type != InteractionType.Approach)
		{
			throw new Exception("Map object doesn't support Approach interaction");
		}
		m_Interaction = new ViewBasedPartRef<MapObjectEntity, InteractionPart>(interaction, interaction.GetType());
		if (variantActor == null)
		{
			variantActor = ContextData<InteractionVariantData>.Current?.VariantActor;
		}
		if (variantActor is ViewBasedPart viewBasedPart)
		{
			m_VariantActor = new ViewBasedPartRef<MapObjectEntity, ViewBasedPart>(viewBasedPart, viewBasedPart.GetType());
		}
	}

	public override bool TryMergeInto(AbstractUnitCommand currentCommand)
	{
		return (currentCommand as UnitInteractWithObject)?.Interaction == Interaction;
	}

	static UnitInteractWithObjectParams()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnitInteractWithObjectParams>())
		{
			MemoryPackFormatterProvider.Register(new UnitInteractWithObjectParamsFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnitInteractWithObjectParams[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnitInteractWithObjectParams>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CommandType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<CommandType>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<float?>())
		{
			MemoryPackFormatterProvider.Register(new NullableFormatter<float>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<bool?>())
		{
			MemoryPackFormatterProvider.Register(new NullableFormatter<bool>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<int?>())
		{
			MemoryPackFormatterProvider.Register(new NullableFormatter<int>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<WalkSpeedType?>())
		{
			MemoryPackFormatterProvider.Register(new NullableFormatter<WalkSpeedType>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<WalkSpeedType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<WalkSpeedType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref UnitInteractWithObjectParams? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(16, in value.Type);
		writer.WritePackable(in value.OwnerRef);
		TargetWrapper value2 = value.Target;
		writer.WritePackable(in value2);
		bool value3 = value.FromCutscene;
		bool value4 = value.InterruptAsSoonAsPossible;
		float? value5 = value.OverrideSpeed;
		bool value6 = value.DoNotInterruptAfterFight;
		writer.DangerousWriteUnmanaged(in value3, in value4, in value5, in value6, in value.m_FreeAction, in value.m_NeedLoS, in value.m_ApproachRadius);
		writer.WritePackable(in value.m_ForcedPath);
		writer.DangerousWriteUnmanaged(in value.m_MovementType, in value.m_IsOneFrameCommand, in value.m_SlowMotionRequired);
		writer.WritePackable(in value.m_Interaction);
		writer.WritePackable(in value.m_VariantActor);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UnitInteractWithObjectParams? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		CommandType value2;
		EntityRef<BaseUnitEntity> value3;
		TargetWrapper value4;
		bool value5;
		bool value6;
		float? value7;
		bool value8;
		bool? value9;
		bool? value10;
		int? value11;
		ForcedPath value12;
		WalkSpeedType? value13;
		bool? value14;
		bool? value15;
		ViewBasedPartRef<MapObjectEntity, InteractionPart> value16;
		ViewBasedPartRef<MapObjectEntity, ViewBasedPart> value17;
		if (memberCount == 16)
		{
			if (value != null)
			{
				value2 = value.Type;
				value3 = value.OwnerRef;
				value4 = value.Target;
				value5 = value.FromCutscene;
				value6 = value.InterruptAsSoonAsPossible;
				value7 = value.OverrideSpeed;
				value8 = value.DoNotInterruptAfterFight;
				value9 = value.m_FreeAction;
				value10 = value.m_NeedLoS;
				value11 = value.m_ApproachRadius;
				value12 = value.m_ForcedPath;
				value13 = value.m_MovementType;
				value14 = value.m_IsOneFrameCommand;
				value15 = value.m_SlowMotionRequired;
				value16 = value.m_Interaction;
				value17 = value.m_VariantActor;
				reader.ReadUnmanaged<CommandType>(out value2);
				reader.ReadPackable(ref value3);
				reader.ReadPackable(ref value4);
				reader.ReadUnmanaged<bool>(out value5);
				reader.ReadUnmanaged<bool>(out value6);
				reader.DangerousReadUnmanaged<float?>(out value7);
				reader.ReadUnmanaged<bool>(out value8);
				reader.DangerousReadUnmanaged<bool?>(out value9);
				reader.DangerousReadUnmanaged<bool?>(out value10);
				reader.DangerousReadUnmanaged<int?>(out value11);
				reader.ReadPackable(ref value12);
				reader.DangerousReadUnmanaged<WalkSpeedType?>(out value13);
				reader.DangerousReadUnmanaged<bool?>(out value14);
				reader.DangerousReadUnmanaged<bool?>(out value15);
				reader.ReadPackable(ref value16);
				reader.ReadPackable(ref value17);
				goto IL_0374;
			}
			reader.ReadUnmanaged<CommandType>(out value2);
			value3 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			value4 = reader.ReadPackable<TargetWrapper>();
			reader.DangerousReadUnmanaged<bool, bool, float?, bool, bool?, bool?, int?>(out value5, out value6, out value7, out value8, out value9, out value10, out value11);
			value12 = reader.ReadPackable<ForcedPath>();
			reader.DangerousReadUnmanaged<WalkSpeedType?, bool?, bool?>(out value13, out value14, out value15);
			value16 = reader.ReadPackable<ViewBasedPartRef<MapObjectEntity, InteractionPart>>();
			value17 = reader.ReadPackable<ViewBasedPartRef<MapObjectEntity, ViewBasedPart>>();
		}
		else
		{
			if (memberCount > 16)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnitInteractWithObjectParams), 16, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = CommandType.None;
				value3 = default(EntityRef<BaseUnitEntity>);
				value4 = null;
				value5 = false;
				value6 = false;
				value7 = null;
				value8 = false;
				value9 = null;
				value10 = null;
				value11 = null;
				value12 = null;
				value13 = null;
				value14 = null;
				value15 = null;
				value16 = default(ViewBasedPartRef<MapObjectEntity, InteractionPart>);
				value17 = default(ViewBasedPartRef<MapObjectEntity, ViewBasedPart>);
			}
			else
			{
				value2 = value.Type;
				value3 = value.OwnerRef;
				value4 = value.Target;
				value5 = value.FromCutscene;
				value6 = value.InterruptAsSoonAsPossible;
				value7 = value.OverrideSpeed;
				value8 = value.DoNotInterruptAfterFight;
				value9 = value.m_FreeAction;
				value10 = value.m_NeedLoS;
				value11 = value.m_ApproachRadius;
				value12 = value.m_ForcedPath;
				value13 = value.m_MovementType;
				value14 = value.m_IsOneFrameCommand;
				value15 = value.m_SlowMotionRequired;
				value16 = value.m_Interaction;
				value17 = value.m_VariantActor;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<CommandType>(out value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadPackable(ref value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<bool>(out value5);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<bool>(out value6);
								if (memberCount != 5)
								{
									reader.DangerousReadUnmanaged<float?>(out value7);
									if (memberCount != 6)
									{
										reader.ReadUnmanaged<bool>(out value8);
										if (memberCount != 7)
										{
											reader.DangerousReadUnmanaged<bool?>(out value9);
											if (memberCount != 8)
											{
												reader.DangerousReadUnmanaged<bool?>(out value10);
												if (memberCount != 9)
												{
													reader.DangerousReadUnmanaged<int?>(out value11);
													if (memberCount != 10)
													{
														reader.ReadPackable(ref value12);
														if (memberCount != 11)
														{
															reader.DangerousReadUnmanaged<WalkSpeedType?>(out value13);
															if (memberCount != 12)
															{
																reader.DangerousReadUnmanaged<bool?>(out value14);
																if (memberCount != 13)
																{
																	reader.DangerousReadUnmanaged<bool?>(out value15);
																	if (memberCount != 14)
																	{
																		reader.ReadPackable(ref value16);
																		if (memberCount != 15)
																		{
																			reader.ReadPackable(ref value17);
																			_ = 16;
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0374;
			}
		}
		value = new UnitInteractWithObjectParams
		{
			Type = value2,
			OwnerRef = value3,
			Target = value4,
			FromCutscene = value5,
			InterruptAsSoonAsPossible = value6,
			OverrideSpeed = value7,
			DoNotInterruptAfterFight = value8,
			m_FreeAction = value9,
			m_NeedLoS = value10,
			m_ApproachRadius = value11,
			m_ForcedPath = value12,
			m_MovementType = value13,
			m_IsOneFrameCommand = value14,
			m_SlowMotionRequired = value15,
			m_Interaction = value16,
			m_VariantActor = value17
		};
		return;
		IL_0374:
		value.Type = value2;
		value.OwnerRef = value3;
		value.Target = value4;
		value.FromCutscene = value5;
		value.InterruptAsSoonAsPossible = value6;
		value.OverrideSpeed = value7;
		value.DoNotInterruptAfterFight = value8;
		value.m_FreeAction = value9;
		value.m_NeedLoS = value10;
		value.m_ApproachRadius = value11;
		value.m_ForcedPath = value12;
		value.m_MovementType = value13;
		value.m_IsOneFrameCommand = value14;
		value.m_SlowMotionRequired = value15;
		value.m_Interaction = value16;
		value.m_VariantActor = value17;
	}
}

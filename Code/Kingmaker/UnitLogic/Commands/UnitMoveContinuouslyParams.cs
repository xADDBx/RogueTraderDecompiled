using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

[MemoryPackable(GenerateType.Object)]
public sealed class UnitMoveContinuouslyParams : UnitCommandParams<UnitMoveContinuously>, IMemoryPackable<UnitMoveContinuouslyParams>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class UnitMoveContinuouslyParamsFormatter : MemoryPackFormatter<UnitMoveContinuouslyParams>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref UnitMoveContinuouslyParams value)
		{
			UnitMoveContinuouslyParams.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnitMoveContinuouslyParams value)
		{
			UnitMoveContinuouslyParams.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	public Vector2 Direction;

	[JsonProperty]
	public float Multiplier;

	public bool IsZero
	{
		get
		{
			if (Direction == Vector2.zero)
			{
				return Mathf.Approximately(Multiplier, 0f);
			}
			return false;
		}
	}

	[JsonConstructor]
	[MemoryPackConstructor]
	private UnitMoveContinuouslyParams()
		: base(default(JsonConstructorMark))
	{
	}

	public UnitMoveContinuouslyParams([CanBeNull] TargetWrapper target)
		: base(target)
	{
	}

	public UnitMoveContinuouslyParams(Vector2 direction, float multiplier, [CanBeNull] TargetWrapper target = null)
		: base(target)
	{
		Direction = direction;
		Multiplier = multiplier;
	}

	public override bool TryMergeInto(AbstractUnitCommand currentCommand)
	{
		if (!(currentCommand is UnitMoveContinuously unitMoveContinuously))
		{
			return false;
		}
		unitMoveContinuously.Params.Direction = Direction;
		unitMoveContinuously.Params.Multiplier = Multiplier;
		return true;
	}

	static UnitMoveContinuouslyParams()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnitMoveContinuouslyParams>())
		{
			MemoryPackFormatterProvider.Register(new UnitMoveContinuouslyParamsFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnitMoveContinuouslyParams[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnitMoveContinuouslyParams>());
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
	public static void Serialize(ref MemoryPackWriter writer, ref UnitMoveContinuouslyParams? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(17, in value.Type);
		writer.WritePackable(in value.OwnerRef);
		TargetWrapper value2 = value.Target;
		writer.WritePackable(in value2);
		bool value3 = value.FromCutscene;
		bool value4 = value.InterruptAsSoonAsPossible;
		float? value5 = value.OverrideSpeed;
		bool value6 = value.DoNotInterruptAfterFight;
		writer.DangerousWriteUnmanaged(in value3, in value4, in value5, in value6, in value.m_FreeAction, in value.m_NeedLoS, in value.m_ApproachRadius);
		writer.WritePackable(in value.m_ForcedPath);
		ref WalkSpeedType? movementType = ref value.m_MovementType;
		ref bool? isOneFrameCommand = ref value.m_IsOneFrameCommand;
		ref bool? slowMotionRequired = ref value.m_SlowMotionRequired;
		ref Vector2 direction = ref value.Direction;
		ref float multiplier = ref value.Multiplier;
		value3 = value.IsZero;
		writer.DangerousWriteUnmanaged(in movementType, in isOneFrameCommand, in slowMotionRequired, in direction, in multiplier, in value3);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UnitMoveContinuouslyParams? value)
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
		Vector2 value16;
		float value17;
		if (memberCount == 17)
		{
			bool value18;
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
				value16 = value.Direction;
				value17 = value.Multiplier;
				value18 = value.IsZero;
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
				reader.ReadUnmanaged<Vector2>(out value16);
				reader.ReadUnmanaged<float>(out value17);
				reader.ReadUnmanaged<bool>(out value18);
				goto IL_0396;
			}
			reader.ReadUnmanaged<CommandType>(out value2);
			value3 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			value4 = reader.ReadPackable<TargetWrapper>();
			reader.DangerousReadUnmanaged<bool, bool, float?, bool, bool?, bool?, int?>(out value5, out value6, out value7, out value8, out value9, out value10, out value11);
			value12 = reader.ReadPackable<ForcedPath>();
			reader.DangerousReadUnmanaged<WalkSpeedType?, bool?, bool?, Vector2, float, bool>(out value13, out value14, out value15, out value16, out value17, out value18);
		}
		else
		{
			if (memberCount > 17)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnitMoveContinuouslyParams), 17, memberCount);
				return;
			}
			bool value18;
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
				value16 = default(Vector2);
				value17 = 0f;
				value18 = false;
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
				value16 = value.Direction;
				value17 = value.Multiplier;
				value18 = value.IsZero;
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
																		reader.ReadUnmanaged<Vector2>(out value16);
																		if (memberCount != 15)
																		{
																			reader.ReadUnmanaged<float>(out value17);
																			if (memberCount != 16)
																			{
																				reader.ReadUnmanaged<bool>(out value18);
																				_ = 17;
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
			}
			if (value != null)
			{
				goto IL_0396;
			}
		}
		value = new UnitMoveContinuouslyParams
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
			Direction = value16,
			Multiplier = value17
		};
		return;
		IL_0396:
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
		value.Direction = value16;
		value.Multiplier = value17;
	}
}

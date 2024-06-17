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
		writer.WriteUnmanagedWithObjectHeader(21, in value.Type);
		writer.WritePackable(in value.OwnerRef);
		writer.WriteUnmanaged(in value.m_Handle);
		TargetWrapper value2 = value.Target;
		writer.WritePackable(in value2);
		bool value3 = value.FromCutscene;
		bool value4 = value.CanBeAccelerated;
		bool value5 = value.InterruptAsSoonAsPossible;
		float? value6 = value.OverrideSpeed;
		float? value7 = value.SpeedLimit;
		bool value8 = value.ApplySpeedLimitInCombat;
		bool value9 = value.AiCanInterruptMark;
		bool value10 = value.DoNotInterruptAfterFight;
		writer.DangerousWriteUnmanaged(in value3, in value4, in value5, in value6, in value7, in value8, in value9, in value10, in value.m_FreeAction, in value.m_NeedLoS, in value.m_ApproachRadius);
		writer.WritePackable(in value.m_ForcedPath);
		writer.DangerousWriteUnmanaged(in value.m_MovementType, in value.m_IsOneFrameCommand, in value.m_SlowMotionRequired, in value.Direction, in value.Multiplier);
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
		int value4;
		TargetWrapper value5;
		bool value6;
		bool value7;
		bool value8;
		float? value9;
		float? value10;
		bool value11;
		bool value12;
		bool value13;
		bool? value14;
		bool? value15;
		int? value16;
		ForcedPath value17;
		WalkSpeedType? value18;
		bool? value19;
		bool? value20;
		Vector2 value21;
		float value22;
		if (memberCount == 21)
		{
			if (value != null)
			{
				value2 = value.Type;
				value3 = value.OwnerRef;
				value4 = value.m_Handle;
				value5 = value.Target;
				value6 = value.FromCutscene;
				value7 = value.CanBeAccelerated;
				value8 = value.InterruptAsSoonAsPossible;
				value9 = value.OverrideSpeed;
				value10 = value.SpeedLimit;
				value11 = value.ApplySpeedLimitInCombat;
				value12 = value.AiCanInterruptMark;
				value13 = value.DoNotInterruptAfterFight;
				value14 = value.m_FreeAction;
				value15 = value.m_NeedLoS;
				value16 = value.m_ApproachRadius;
				value17 = value.m_ForcedPath;
				value18 = value.m_MovementType;
				value19 = value.m_IsOneFrameCommand;
				value20 = value.m_SlowMotionRequired;
				value21 = value.Direction;
				value22 = value.Multiplier;
				reader.ReadUnmanaged<CommandType>(out value2);
				reader.ReadPackable(ref value3);
				reader.ReadUnmanaged<int>(out value4);
				reader.ReadPackable(ref value5);
				reader.ReadUnmanaged<bool>(out value6);
				reader.ReadUnmanaged<bool>(out value7);
				reader.ReadUnmanaged<bool>(out value8);
				reader.DangerousReadUnmanaged<float?>(out value9);
				reader.DangerousReadUnmanaged<float?>(out value10);
				reader.ReadUnmanaged<bool>(out value11);
				reader.ReadUnmanaged<bool>(out value12);
				reader.ReadUnmanaged<bool>(out value13);
				reader.DangerousReadUnmanaged<bool?>(out value14);
				reader.DangerousReadUnmanaged<bool?>(out value15);
				reader.DangerousReadUnmanaged<int?>(out value16);
				reader.ReadPackable(ref value17);
				reader.DangerousReadUnmanaged<WalkSpeedType?>(out value18);
				reader.DangerousReadUnmanaged<bool?>(out value19);
				reader.DangerousReadUnmanaged<bool?>(out value20);
				reader.ReadUnmanaged<Vector2>(out value21);
				reader.ReadUnmanaged<float>(out value22);
				goto IL_045e;
			}
			reader.ReadUnmanaged<CommandType>(out value2);
			value3 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			reader.ReadUnmanaged<int>(out value4);
			value5 = reader.ReadPackable<TargetWrapper>();
			reader.DangerousReadUnmanaged<bool, bool, bool, float?, float?, bool, bool, bool, bool?, bool?, int?>(out value6, out value7, out value8, out value9, out value10, out value11, out value12, out value13, out value14, out value15, out value16);
			value17 = reader.ReadPackable<ForcedPath>();
			reader.DangerousReadUnmanaged<WalkSpeedType?, bool?, bool?, Vector2, float>(out value18, out value19, out value20, out value21, out value22);
		}
		else
		{
			if (memberCount > 21)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnitMoveContinuouslyParams), 21, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = CommandType.None;
				value3 = default(EntityRef<BaseUnitEntity>);
				value4 = 0;
				value5 = null;
				value6 = false;
				value7 = false;
				value8 = false;
				value9 = null;
				value10 = null;
				value11 = false;
				value12 = false;
				value13 = false;
				value14 = null;
				value15 = null;
				value16 = null;
				value17 = null;
				value18 = null;
				value19 = null;
				value20 = null;
				value21 = default(Vector2);
				value22 = 0f;
			}
			else
			{
				value2 = value.Type;
				value3 = value.OwnerRef;
				value4 = value.m_Handle;
				value5 = value.Target;
				value6 = value.FromCutscene;
				value7 = value.CanBeAccelerated;
				value8 = value.InterruptAsSoonAsPossible;
				value9 = value.OverrideSpeed;
				value10 = value.SpeedLimit;
				value11 = value.ApplySpeedLimitInCombat;
				value12 = value.AiCanInterruptMark;
				value13 = value.DoNotInterruptAfterFight;
				value14 = value.m_FreeAction;
				value15 = value.m_NeedLoS;
				value16 = value.m_ApproachRadius;
				value17 = value.m_ForcedPath;
				value18 = value.m_MovementType;
				value19 = value.m_IsOneFrameCommand;
				value20 = value.m_SlowMotionRequired;
				value21 = value.Direction;
				value22 = value.Multiplier;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<CommandType>(out value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<int>(out value4);
						if (memberCount != 3)
						{
							reader.ReadPackable(ref value5);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<bool>(out value6);
								if (memberCount != 5)
								{
									reader.ReadUnmanaged<bool>(out value7);
									if (memberCount != 6)
									{
										reader.ReadUnmanaged<bool>(out value8);
										if (memberCount != 7)
										{
											reader.DangerousReadUnmanaged<float?>(out value9);
											if (memberCount != 8)
											{
												reader.DangerousReadUnmanaged<float?>(out value10);
												if (memberCount != 9)
												{
													reader.ReadUnmanaged<bool>(out value11);
													if (memberCount != 10)
													{
														reader.ReadUnmanaged<bool>(out value12);
														if (memberCount != 11)
														{
															reader.ReadUnmanaged<bool>(out value13);
															if (memberCount != 12)
															{
																reader.DangerousReadUnmanaged<bool?>(out value14);
																if (memberCount != 13)
																{
																	reader.DangerousReadUnmanaged<bool?>(out value15);
																	if (memberCount != 14)
																	{
																		reader.DangerousReadUnmanaged<int?>(out value16);
																		if (memberCount != 15)
																		{
																			reader.ReadPackable(ref value17);
																			if (memberCount != 16)
																			{
																				reader.DangerousReadUnmanaged<WalkSpeedType?>(out value18);
																				if (memberCount != 17)
																				{
																					reader.DangerousReadUnmanaged<bool?>(out value19);
																					if (memberCount != 18)
																					{
																						reader.DangerousReadUnmanaged<bool?>(out value20);
																						if (memberCount != 19)
																						{
																							reader.ReadUnmanaged<Vector2>(out value21);
																							if (memberCount != 20)
																							{
																								reader.ReadUnmanaged<float>(out value22);
																								_ = 21;
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
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_045e;
			}
		}
		value = new UnitMoveContinuouslyParams
		{
			Type = value2,
			OwnerRef = value3,
			m_Handle = value4,
			Target = value5,
			FromCutscene = value6,
			CanBeAccelerated = value7,
			InterruptAsSoonAsPossible = value8,
			OverrideSpeed = value9,
			SpeedLimit = value10,
			ApplySpeedLimitInCombat = value11,
			AiCanInterruptMark = value12,
			DoNotInterruptAfterFight = value13,
			m_FreeAction = value14,
			m_NeedLoS = value15,
			m_ApproachRadius = value16,
			m_ForcedPath = value17,
			m_MovementType = value18,
			m_IsOneFrameCommand = value19,
			m_SlowMotionRequired = value20,
			Direction = value21,
			Multiplier = value22
		};
		return;
		IL_045e:
		value.Type = value2;
		value.OwnerRef = value3;
		value.m_Handle = value4;
		value.Target = value5;
		value.FromCutscene = value6;
		value.CanBeAccelerated = value7;
		value.InterruptAsSoonAsPossible = value8;
		value.OverrideSpeed = value9;
		value.SpeedLimit = value10;
		value.ApplySpeedLimitInCombat = value11;
		value.AiCanInterruptMark = value12;
		value.DoNotInterruptAfterFight = value13;
		value.m_FreeAction = value14;
		value.m_NeedLoS = value15;
		value.m_ApproachRadius = value16;
		value.m_ForcedPath = value17;
		value.m_MovementType = value18;
		value.m_IsOneFrameCommand = value19;
		value.m_SlowMotionRequired = value20;
		value.Direction = value21;
		value.Multiplier = value22;
	}
}

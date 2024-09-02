using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.FlagCountable;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

[MemoryPackable(GenerateType.Object)]
public sealed class UnitMoveToProperParams : UnitCommandParams<UnitMoveToProper>, IMemoryPackable<UnitMoveToProperParams>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class UnitMoveToProperParamsFormatter : MemoryPackFormatter<UnitMoveToProperParams>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref UnitMoveToProperParams value)
		{
			UnitMoveToProperParams.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnitMoveToProperParams value)
		{
			UnitMoveToProperParams.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	public readonly CountableFlag DisableAttackOfOpportunity = new CountableFlag();

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public readonly float ActionPointsPerCell;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public readonly int ActionPointsToSpend;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public readonly float[] ApCostPerEveryCell;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public readonly int StraightMoveLength;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public readonly int DiagonalsCount;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool DisableApproachRadius { get; set; }

	public override int DefaultApproachRadius => 10000;

	protected override WalkSpeedType DefaultMovementType => WalkSpeedType.Run;

	[JsonConstructor]
	public UnitMoveToProperParams()
		: base(default(JsonConstructorMark))
	{
	}

	[MemoryPackConstructor]
	public UnitMoveToProperParams(CountableFlag disableAttackOfOpportunity, float actionPointsPerCell, int actionPointsToSpend, float[] apCostPerEveryCell, int straightMoveLength, int diagonalsCount, bool disableApproachRadius)
	{
		DisableAttackOfOpportunity = disableAttackOfOpportunity;
		ActionPointsPerCell = actionPointsPerCell;
		ActionPointsToSpend = actionPointsToSpend;
		ApCostPerEveryCell = apCostPerEveryCell;
		StraightMoveLength = straightMoveLength;
		DiagonalsCount = diagonalsCount;
		DisableApproachRadius = disableApproachRadius;
	}

	public UnitMoveToProperParams([NotNull] ForcedPath path, float actionPointsPerCell)
	{
		List<Vector3> vectorPath = path.vectorPath;
		base._002Ector((TargetWrapper)vectorPath[vectorPath.Count - 1]);
		base.ForcedPath = path;
		ActionPointsPerCell = actionPointsPerCell;
	}

	public UnitMoveToProperParams([NotNull] ForcedPath path, int straightMoveLength, int diagonalsCount, int actionPointsToSpend)
	{
		List<Vector3> vectorPath = path.vectorPath;
		base._002Ector((TargetWrapper)vectorPath[vectorPath.Count - 1]);
		base.ForcedPath = path;
		StraightMoveLength = straightMoveLength;
		DiagonalsCount = diagonalsCount;
		ActionPointsToSpend = actionPointsToSpend;
	}

	public UnitMoveToProperParams([NotNull] ForcedPath path, float actionPointsPerCell, IEnumerable<float> apCostPerPoint)
	{
		List<Vector3> vectorPath = path.vectorPath;
		base._002Ector((TargetWrapper)vectorPath[vectorPath.Count - 1]);
		base.ForcedPath = path;
		ApCostPerEveryCell = apCostPerPoint?.ToArray();
		ActionPointsPerCell = actionPointsPerCell;
	}

	static UnitMoveToProperParams()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnitMoveToProperParams>())
		{
			MemoryPackFormatterProvider.Register(new UnitMoveToProperParamsFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnitMoveToProperParams[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnitMoveToProperParams>());
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
		if (!MemoryPackFormatterProvider.IsRegistered<float[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<float>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref UnitMoveToProperParams? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(21, in value.Type);
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
		writer.WritePackable(in value.DisableAttackOfOpportunity);
		writer.WriteUnmanaged(in value.ActionPointsPerCell, in value.ActionPointsToSpend);
		writer.WriteUnmanagedArray(value.ApCostPerEveryCell);
		ref readonly int straightMoveLength = ref value.StraightMoveLength;
		ref readonly int diagonalsCount = ref value.DiagonalsCount;
		value3 = value.DisableApproachRadius;
		writer.WriteUnmanaged(in straightMoveLength, in diagonalsCount, in value3);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UnitMoveToProperParams? value)
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
		CountableFlag value16;
		float value17;
		int value18;
		float[] value19;
		int value20;
		int value21;
		bool value22;
		if (memberCount == 21)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<CommandType>(out value2);
				value3 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
				value4 = reader.ReadPackable<TargetWrapper>();
				reader.DangerousReadUnmanaged<bool, bool, float?, bool, bool?, bool?, int?>(out value5, out value6, out value7, out value8, out value9, out value10, out value11);
				value12 = reader.ReadPackable<ForcedPath>();
				reader.DangerousReadUnmanaged<WalkSpeedType?, bool?, bool?>(out value13, out value14, out value15);
				value16 = reader.ReadPackable<CountableFlag>();
				reader.ReadUnmanaged<float, int>(out value17, out value18);
				value19 = reader.ReadUnmanagedArray<float>();
				reader.ReadUnmanaged<int, int, bool>(out value20, out value21, out value22);
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
				value16 = value.DisableAttackOfOpportunity;
				value17 = value.ActionPointsPerCell;
				value18 = value.ActionPointsToSpend;
				value19 = value.ApCostPerEveryCell;
				value20 = value.StraightMoveLength;
				value21 = value.DiagonalsCount;
				value22 = value.DisableApproachRadius;
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
				reader.ReadUnmanaged<float>(out value17);
				reader.ReadUnmanaged<int>(out value18);
				reader.ReadUnmanagedArray(ref value19);
				reader.ReadUnmanaged<int>(out value20);
				reader.ReadUnmanaged<int>(out value21);
				reader.ReadUnmanaged<bool>(out value22);
			}
		}
		else
		{
			if (memberCount > 21)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnitMoveToProperParams), 21, memberCount);
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
				value16 = null;
				value17 = 0f;
				value18 = 0;
				value19 = null;
				value20 = 0;
				value21 = 0;
				value22 = false;
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
				value16 = value.DisableAttackOfOpportunity;
				value17 = value.ActionPointsPerCell;
				value18 = value.ActionPointsToSpend;
				value19 = value.ApCostPerEveryCell;
				value20 = value.StraightMoveLength;
				value21 = value.DiagonalsCount;
				value22 = value.DisableApproachRadius;
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
																			reader.ReadUnmanaged<float>(out value17);
																			if (memberCount != 16)
																			{
																				reader.ReadUnmanaged<int>(out value18);
																				if (memberCount != 17)
																				{
																					reader.ReadUnmanagedArray(ref value19);
																					if (memberCount != 18)
																					{
																						reader.ReadUnmanaged<int>(out value20);
																						if (memberCount != 19)
																						{
																							reader.ReadUnmanaged<int>(out value21);
																							if (memberCount != 20)
																							{
																								reader.ReadUnmanaged<bool>(out value22);
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
			_ = value;
		}
		value = new UnitMoveToProperParams(value16, value17, value18, value19, value20, value21, value22)
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
			m_SlowMotionRequired = value15
		};
	}
}

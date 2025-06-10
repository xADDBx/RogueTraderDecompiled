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
public sealed class UnitMoveToParams : UnitCommandParams<UnitMoveTo>, IMemoryPackable<UnitMoveToParams>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class UnitMoveToParamsFormatter : MemoryPackFormatter<UnitMoveToParams>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref UnitMoveToParams value)
		{
			UnitMoveToParams.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnitMoveToParams value)
		{
			UnitMoveToParams.Deserialize(ref reader, ref value);
		}
	}

	public const float DefaultApproachRadiusForAgent = 0.3f;

	[JsonProperty(PropertyName = "ara", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public float ApproachRadiusForAgentASP;

	[JsonProperty(PropertyName = "ori")]
	public float? Orientation { get; set; }

	[JsonProperty(PropertyName = "raw")]
	public bool RunAway { get; set; }

	[JsonProperty(PropertyName = "roa")]
	public bool Roaming { get; set; }

	[JsonProperty(PropertyName = "lf")]
	public bool LeaveFollowers { get; set; }

	[MemoryPackIgnore]
	public float MaxApproachForAgentASP => Mathf.Max(Mathf.Sqrt(2.Cells().Meters), ApproachRadiusForAgentASP);

	public override int DefaultApproachRadius => 10000;

	[JsonConstructor]
	[MemoryPackConstructor]
	private UnitMoveToParams()
		: base(default(JsonConstructorMark))
	{
	}

	public UnitMoveToParams([NotNull] ForcedPath path, [CanBeNull] TargetWrapper target, float approachRadiusForAgent = 0.3f, bool leaveFollowers = false)
		: base(target)
	{
		base.ForcedPath = path;
		ApproachRadiusForAgentASP = approachRadiusForAgent;
		LeaveFollowers = leaveFollowers;
	}

	static UnitMoveToParams()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnitMoveToParams>())
		{
			MemoryPackFormatterProvider.Register(new UnitMoveToParamsFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnitMoveToParams[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnitMoveToParams>());
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
	public static void Serialize(ref MemoryPackWriter writer, ref UnitMoveToParams? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(19, in value.Type);
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
		ref float approachRadiusForAgentASP = ref value.ApproachRadiusForAgentASP;
		value5 = value.Orientation;
		value3 = value.RunAway;
		value4 = value.Roaming;
		value6 = value.LeaveFollowers;
		writer.DangerousWriteUnmanaged(in movementType, in isOneFrameCommand, in slowMotionRequired, in approachRadiusForAgentASP, in value5, in value3, in value4, in value6);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UnitMoveToParams? value)
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
		float value16;
		float? value17;
		bool value18;
		bool value19;
		bool value20;
		if (memberCount == 19)
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
				value16 = value.ApproachRadiusForAgentASP;
				value17 = value.Orientation;
				value18 = value.RunAway;
				value19 = value.Roaming;
				value20 = value.LeaveFollowers;
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
				reader.ReadUnmanaged<float>(out value16);
				reader.DangerousReadUnmanaged<float?>(out value17);
				reader.ReadUnmanaged<bool>(out value18);
				reader.ReadUnmanaged<bool>(out value19);
				reader.ReadUnmanaged<bool>(out value20);
				goto IL_03f4;
			}
			reader.ReadUnmanaged<CommandType>(out value2);
			value3 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			value4 = reader.ReadPackable<TargetWrapper>();
			reader.DangerousReadUnmanaged<bool, bool, float?, bool, bool?, bool?, int?>(out value5, out value6, out value7, out value8, out value9, out value10, out value11);
			value12 = reader.ReadPackable<ForcedPath>();
			reader.DangerousReadUnmanaged<WalkSpeedType?, bool?, bool?, float, float?, bool, bool, bool>(out value13, out value14, out value15, out value16, out value17, out value18, out value19, out value20);
		}
		else
		{
			if (memberCount > 19)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnitMoveToParams), 19, memberCount);
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
				value16 = 0f;
				value17 = null;
				value18 = false;
				value19 = false;
				value20 = false;
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
				value16 = value.ApproachRadiusForAgentASP;
				value17 = value.Orientation;
				value18 = value.RunAway;
				value19 = value.Roaming;
				value20 = value.LeaveFollowers;
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
																		reader.ReadUnmanaged<float>(out value16);
																		if (memberCount != 15)
																		{
																			reader.DangerousReadUnmanaged<float?>(out value17);
																			if (memberCount != 16)
																			{
																				reader.ReadUnmanaged<bool>(out value18);
																				if (memberCount != 17)
																				{
																					reader.ReadUnmanaged<bool>(out value19);
																					if (memberCount != 18)
																					{
																						reader.ReadUnmanaged<bool>(out value20);
																						_ = 19;
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
				goto IL_03f4;
			}
		}
		value = new UnitMoveToParams
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
			ApproachRadiusForAgentASP = value16,
			Orientation = value17,
			RunAway = value18,
			Roaming = value19,
			LeaveFollowers = value20
		};
		return;
		IL_03f4:
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
		value.ApproachRadiusForAgentASP = value16;
		value.Orientation = value17;
		value.RunAway = value18;
		value.Roaming = value19;
		value.LeaveFollowers = value20;
	}
}

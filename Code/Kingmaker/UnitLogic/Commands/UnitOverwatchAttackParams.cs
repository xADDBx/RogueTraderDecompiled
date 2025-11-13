using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.UnitLogic.Commands;

[MemoryPackable(GenerateType.Object)]
public sealed class UnitOverwatchAttackParams : UnitUseAbilityParams, IMemoryPackable<UnitOverwatchAttackParams>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class UnitOverwatchAttackParamsFormatter : MemoryPackFormatter<UnitOverwatchAttackParams>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref UnitOverwatchAttackParams value)
		{
			UnitOverwatchAttackParams.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnitOverwatchAttackParams value)
		{
			UnitOverwatchAttackParams.Deserialize(ref reader, ref value);
		}
	}

	protected override bool DefaultFreeAction => true;

	public override bool DefaultIgnoreCooldown => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private UnitOverwatchAttackParams()
		: base(default(JsonConstructorMark))
	{
	}

	public UnitOverwatchAttackParams([NotNull] AbilityData ability, [NotNull] TargetWrapper target)
		: base(ability, target)
	{
	}

	protected override AbstractUnitCommand CreateCommandInternal()
	{
		return new UnitOverwatchAttack(this);
	}

	static UnitOverwatchAttackParams()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnitOverwatchAttackParams>())
		{
			MemoryPackFormatterProvider.Register(new UnitOverwatchAttackParamsFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnitOverwatchAttackParams[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnitOverwatchAttackParams>());
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
		if (!MemoryPackFormatterProvider.IsRegistered<AttackHitPolicyType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<AttackHitPolicyType>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DamagePolicyType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<DamagePolicyType>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<TargetWrapper>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<TargetWrapper>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref UnitOverwatchAttackParams? value)
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
		ref WalkSpeedType? movementType = ref value.m_MovementType;
		ref bool? isOneFrameCommand = ref value.m_IsOneFrameCommand;
		ref bool? slowMotionRequired = ref value.m_SlowMotionRequired;
		ref bool? ignoreCooldown = ref value.m_IgnoreCooldown;
		value3 = value.DisableLog;
		AttackHitPolicyType value7 = value.HitPolicy;
		DamagePolicyType value8 = value.DamagePolicy;
		value4 = value.KillTarget;
		writer.DangerousWriteUnmanaged(in movementType, in isOneFrameCommand, in slowMotionRequired, in ignoreCooldown, in value3, in value7, in value8, in value4);
		List<TargetWrapper> source = value.AllTargets;
		ListFormatter.SerializePackable(ref writer, ref Unsafe.AsRef(in source));
		value3 = value.DoNotClearMovementPoints;
		writer.WriteUnmanaged(in value3);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UnitOverwatchAttackParams? value)
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
		bool? value16;
		bool value17;
		AttackHitPolicyType value18;
		DamagePolicyType value19;
		bool value20;
		List<TargetWrapper> value21;
		bool value22;
		if (memberCount == 21)
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
				value16 = value.m_IgnoreCooldown;
				value17 = value.DisableLog;
				value18 = value.HitPolicy;
				value19 = value.DamagePolicy;
				value20 = value.KillTarget;
				value21 = value.AllTargets;
				value22 = value.DoNotClearMovementPoints;
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
				reader.DangerousReadUnmanaged<bool?>(out value16);
				reader.ReadUnmanaged<bool>(out value17);
				reader.ReadUnmanaged<AttackHitPolicyType>(out value18);
				reader.ReadUnmanaged<DamagePolicyType>(out value19);
				reader.ReadUnmanaged<bool>(out value20);
				ListFormatter.DeserializePackable(ref reader, ref value21);
				reader.ReadUnmanaged<bool>(out value22);
				goto IL_045a;
			}
			reader.ReadUnmanaged<CommandType>(out value2);
			value3 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			value4 = reader.ReadPackable<TargetWrapper>();
			reader.DangerousReadUnmanaged<bool, bool, float?, bool, bool?, bool?, int?>(out value5, out value6, out value7, out value8, out value9, out value10, out value11);
			value12 = reader.ReadPackable<ForcedPath>();
			reader.DangerousReadUnmanaged<WalkSpeedType?, bool?, bool?, bool?, bool, AttackHitPolicyType, DamagePolicyType, bool>(out value13, out value14, out value15, out value16, out value17, out value18, out value19, out value20);
			value21 = ListFormatter.DeserializePackable<TargetWrapper>(ref reader);
			reader.ReadUnmanaged<bool>(out value22);
		}
		else
		{
			if (memberCount > 21)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnitOverwatchAttackParams), 21, memberCount);
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
				value17 = false;
				value18 = AttackHitPolicyType.Default;
				value19 = DamagePolicyType.Default;
				value20 = false;
				value21 = null;
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
				value16 = value.m_IgnoreCooldown;
				value17 = value.DisableLog;
				value18 = value.HitPolicy;
				value19 = value.DamagePolicy;
				value20 = value.KillTarget;
				value21 = value.AllTargets;
				value22 = value.DoNotClearMovementPoints;
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
																		reader.DangerousReadUnmanaged<bool?>(out value16);
																		if (memberCount != 15)
																		{
																			reader.ReadUnmanaged<bool>(out value17);
																			if (memberCount != 16)
																			{
																				reader.ReadUnmanaged<AttackHitPolicyType>(out value18);
																				if (memberCount != 17)
																				{
																					reader.ReadUnmanaged<DamagePolicyType>(out value19);
																					if (memberCount != 18)
																					{
																						reader.ReadUnmanaged<bool>(out value20);
																						if (memberCount != 19)
																						{
																							ListFormatter.DeserializePackable(ref reader, ref value21);
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
			if (value != null)
			{
				goto IL_045a;
			}
		}
		value = new UnitOverwatchAttackParams
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
			m_IgnoreCooldown = value16,
			DisableLog = value17,
			HitPolicy = value18,
			DamagePolicy = value19,
			KillTarget = value20,
			AllTargets = value21,
			DoNotClearMovementPoints = value22
		};
		return;
		IL_045a:
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
		value.m_IgnoreCooldown = value16;
		value.DisableLog = value17;
		value.HitPolicy = value18;
		value.DamagePolicy = value19;
		value.KillTarget = value20;
		value.AllTargets = value21;
		value.DoNotClearMovementPoints = value22;
	}
}

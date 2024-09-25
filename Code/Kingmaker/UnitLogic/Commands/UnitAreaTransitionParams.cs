using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

[MemoryPackable(GenerateType.Object)]
public sealed class UnitAreaTransitionParams : UnitGroupCommandParams<UnitAreaTransition>, IMemoryPackable<UnitAreaTransitionParams>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class UnitAreaTransitionParamsFormatter : MemoryPackFormatter<UnitAreaTransitionParams>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref UnitAreaTransitionParams value)
		{
			UnitAreaTransitionParams.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnitAreaTransitionParams value)
		{
			UnitAreaTransitionParams.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackIgnore]
	public MechanicEntity TransitionEntity => base.Target?.Entity;

	[MemoryPackIgnore]
	public AreaTransitionPart TransitionPart => base.Target?.Entity?.GetRequired<AreaTransitionPart>();

	public override int DefaultApproachRadius => 2;

	[JsonConstructor]
	private UnitAreaTransitionParams()
		: base(default(JsonConstructorMark))
	{
	}

	[MemoryPackConstructor]
	private UnitAreaTransitionParams(Guid groupGuid, [NotNull] List<EntityRef<BaseUnitEntity>> units)
		: base(groupGuid, units, (TargetWrapper)null)
	{
	}

	public UnitAreaTransitionParams(Guid guid, [NotNull] List<EntityRef<BaseUnitEntity>> units, Vector3 position, [NotNull] AreaTransitionPart transition)
		: base(guid, units, new TargetWrapper(position, null, (MechanicEntity)transition.Owner))
	{
	}

	static UnitAreaTransitionParams()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnitAreaTransitionParams>())
		{
			MemoryPackFormatterProvider.Register(new UnitAreaTransitionParamsFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnitAreaTransitionParams[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnitAreaTransitionParams>());
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
		if (!MemoryPackFormatterProvider.IsRegistered<List<EntityRef<BaseUnitEntity>>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<EntityRef<BaseUnitEntity>>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref UnitAreaTransitionParams? value)
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
		writer.DangerousWriteUnmanaged(in value.m_MovementType, in value.m_IsOneFrameCommand, in value.m_SlowMotionRequired, in value.GroupGuid);
		ListFormatter.SerializePackable(ref writer, ref Unsafe.AsRef(in value.Units));
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UnitAreaTransitionParams? value)
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
		Guid value16;
		List<EntityRef<BaseUnitEntity>> value17;
		if (memberCount == 16)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<CommandType>(out value2);
				value3 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
				value4 = reader.ReadPackable<TargetWrapper>();
				reader.DangerousReadUnmanaged<bool, bool, float?, bool, bool?, bool?, int?>(out value5, out value6, out value7, out value8, out value9, out value10, out value11);
				value12 = reader.ReadPackable<ForcedPath>();
				reader.DangerousReadUnmanaged<WalkSpeedType?, bool?, bool?, Guid>(out value13, out value14, out value15, out value16);
				value17 = ListFormatter.DeserializePackable<EntityRef<BaseUnitEntity>>(ref reader);
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
				value16 = value.GroupGuid;
				value17 = value.Units;
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
				reader.ReadUnmanaged<Guid>(out value16);
				ListFormatter.DeserializePackable(ref reader, ref value17);
			}
		}
		else
		{
			if (memberCount > 16)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnitAreaTransitionParams), 16, memberCount);
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
				value16 = default(Guid);
				value17 = null;
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
				value16 = value.GroupGuid;
				value17 = value.Units;
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
																		reader.ReadUnmanaged<Guid>(out value16);
																		if (memberCount != 15)
																		{
																			ListFormatter.DeserializePackable(ref reader, ref value17);
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
			_ = value;
		}
		value = new UnitAreaTransitionParams(value16, value17)
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

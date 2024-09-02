using System;
using Code.Visual.Animation;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartMovable : EntityPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartMovable>, IEntityPartOwner
	{
		PartMovable Movable { get; }
	}

	public struct PreviousSimulationTickInfo
	{
		public bool HasRotation;

		public bool HasMotion;
	}

	private readonly PerFrameVar<float> m_MemoizedSpeed = new PerFrameVar<float>();

	public bool ForceHasMotion;

	public static readonly Feet NonCombatBaseSpeedFeet = 30.Feet();

	public static readonly float NonCombatBaseSpeedMps = NonCombatBaseSpeedFeet.Meters / 2.5f;

	[JsonProperty(IsReference = false)]
	public Vector3 PreviousPosition { get; set; }

	public float PreviousOrientation { get; set; }

	[JsonProperty]
	public TimeSpan LastMoveTime { get; set; }

	private StatsContainer StatsContainer => base.ConcreteOwner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValueSpeed Speed => StatsContainer.GetStat<ModifiableValueSpeed>(StatType.Speed);

	public bool HasMotionThisSimulationTick => (base.Owner.Position - PreviousPosition).sqrMagnitude > 1E-05f;

	public PreviousSimulationTickInfo PreviousSimulationTick { get; set; }

	public float BaseSpeedMps
	{
		get
		{
			if (BuildModeUtility.IsDevelopment && !CheatsAnimation.SpeedLock)
			{
				return Speed.Mps;
			}
			bool num = base.ConcreteOwner.GetOptional<PartFaction>()?.IsPlayer ?? false;
			bool flag = base.ConcreteOwner.GetOptional<PartUnitCombatState>()?.IsInCombat ?? false;
			if (num && !flag)
			{
				return Mathf.Max(NonCombatBaseSpeedMps, Speed.Mps);
			}
			return Speed.Mps;
		}
	}

	public float CurrentSpeedMps
	{
		get
		{
			if (!m_MemoizedSpeed.UpToDate)
			{
				m_MemoizedSpeed.Value = CalculateCurrentSpeed();
			}
			return m_MemoizedSpeed.Value;
		}
	}

	public float CombatSpeedMps => Math.Max(0.01f, Speed.Mps * CalculateSpeedModifier());

	public float ModifiedSpeedMps => Math.Max(0.01f, BaseSpeedMps * CalculateSpeedModifier());

	public bool SpeedIsAffected => CalculateSpeedModifier() < 1f;

	public float SlowMoSpeedMod { get; set; } = 1f;


	protected override void OnAttach()
	{
		Initialize();
	}

	protected override void OnPrePostLoad()
	{
		Initialize();
	}

	private void Initialize()
	{
		StatsContainer.Register<ModifiableValueSpeed>(StatType.Speed);
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		PreviousPosition = base.Owner.Position;
		if (base.Owner is AbstractUnitEntity abstractUnitEntity)
		{
			PreviousOrientation = abstractUnitEntity.DesiredOrientation;
		}
		ForceHasMotion = true;
	}

	private float CalculateCurrentSpeed()
	{
		AbstractUnitCommand abstractUnitCommand = base.ConcreteOwner.GetOptional<PartUnitCommands>()?.Current;
		if (abstractUnitCommand != null && abstractUnitCommand.OverrideSpeed.HasValue)
		{
			return abstractUnitCommand.OverrideSpeed.Value * SlowMoSpeedMod;
		}
		float num = 0f;
		if (abstractUnitCommand != null)
		{
			AbstractUnitEntity executor = abstractUnitCommand.Executor;
			UnitAnimationManager animationManager = executor.AnimationManager;
			if (animationManager != null && animationManager.NewSpeed >= 0f)
			{
				num = animationManager.NewSpeed * SlowMoSpeedMod;
			}
			else if (animationManager != null || executor is StarshipEntity)
			{
				num = ModifiedSpeedMps;
				switch (abstractUnitCommand.MovementType)
				{
				case WalkSpeedType.Sprint:
					num *= 0.5f;
					break;
				case WalkSpeedType.Run:
					num *= 1.2f;
					break;
				case WalkSpeedType.Crouch:
					num *= 0.5f;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				case WalkSpeedType.Walk:
					break;
				}
			}
			else
			{
				PFLog.Default.ErrorWithReport("Unit has no animation manager");
			}
		}
		else
		{
			num = ModifiedSpeedMps;
		}
		return Math.Max(0.01f, num);
	}

	private float CalculateSpeedModifier()
	{
		float num = 1f;
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		if (loadedAreaState != null && loadedAreaState.Settings.CapitalPartyMode)
		{
			return 1f;
		}
		PartUnitState optional = base.ConcreteOwner.GetOptional<PartUnitState>();
		if (optional != null && optional.HasCondition(UnitCondition.Entangled))
		{
			num *= 0.5f;
		}
		if (Game.Instance.Player.Weather.ActualWeather >= BlueprintRoot.Instance.WeatherSettings.SlowdownBonusBeginsOn)
		{
			num *= 0.5f;
		}
		return num * SlowMoSpeedMod;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Vector3 val2 = PreviousPosition;
		result.Append(ref val2);
		TimeSpan val3 = LastMoveTime;
		result.Append(ref val3);
		return result;
	}
}

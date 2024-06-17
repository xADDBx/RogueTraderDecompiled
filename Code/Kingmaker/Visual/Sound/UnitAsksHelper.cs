using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.GameModes;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using Owlcat.Runtime.Core.Math;

namespace Kingmaker.Visual.Sound;

public static class UnitAsksHelper
{
	public static BlueprintBuff DismemberedUnitBuff => BlueprintRoot.Instance.SystemMechanics.DismemberedUnitBuff;

	public static BlueprintBuff DisintegrateBuff => BlueprintRoot.Instance.SystemMechanics.DisintegrateBuff;

	public static float LowHealthBarkHPPercent => Game.Instance.BlueprintRoot.Sound.LowHealthBarkHPPercent;

	public static float LowShieldBarkPercent => Game.Instance.BlueprintRoot.Sound.LowShieldBarkPercent;

	public static float AggroBarkRadius => Game.Instance.BlueprintRoot.Sound.AggroBarkRadius;

	public static int EnemyMassDeathKillsCount => Game.Instance.BlueprintRoot.Sound.EnemyMassDeathKillsCount;

	public static int TilesToBarkMoveOrderSpaceCombat => Game.Instance.BlueprintRoot.Sound.TilesToBarkMoveOrderSpaceCombat;

	public static Size[] EnemyShipSizesToBarkEnemyDeathSC => Game.Instance.BlueprintRoot.Sound.EnemyShipSizesToBarkEnemyDeathSC;

	public static Size[] EnemyShipSizesToBarkShieldIsDownSC => Game.Instance.BlueprintRoot.Sound.EnemyShipSizesToBarkShieldIsDownSC;

	public static float AggroBarkRadiusScr => AggroBarkRadius * AggroBarkRadius;

	public static bool IsSpaceCombat => Game.Instance.CurrentMode == GameModeType.SpaceCombat;

	public static StarshipEntity PlayerShip => Game.Instance.Player.PlayerShip;

	public static StarshipBarksManager PlayerShipAsks
	{
		get
		{
			if (PlayerShip != null)
			{
				if (!(PlayerShip.View != null))
				{
					return null;
				}
				return PlayerShip.View.Asks as StarshipBarksManager;
			}
			return null;
		}
	}

	public static bool CanSpeakSpaceCombat
	{
		get
		{
			if (IsSpaceCombat)
			{
				return PlayerShipAsks != null;
			}
			return false;
		}
	}

	public static bool IsPlayerShip(this MechanicEntity entity)
	{
		return entity == PlayerShip;
	}

	public static bool Schedule(this BarkWrapper barkWrapper, bool is2D = false, AkCallbackManager.EventCallback callback = null)
	{
		return barkWrapper?.UnitBarksManager.Schedule(barkWrapper, is2D, callback) ?? false;
	}

	public static void ScheduleRandomPersonalizedSpaceCombatBark(Func<UnitBarksManager, BarkWrapper> selector)
	{
		if (CanSpeakSpaceCombat)
		{
			PlayerShipAsks.ScheduleRandomUnitOnPostBark(selector);
		}
	}

	public static BaseUnitEntity GetRandomPartyEntity(Func<BaseUnitEntity, bool> predicate)
	{
		IEnumerable<BaseUnitEntity> enumerable = Game.Instance.State.PlayerState.PartyAndPets.Where(predicate);
		return enumerable.Where(UnitIsCloseToCamera).Random(PFStatefulRandom.Visuals.Sounds) ?? enumerable.Random(PFStatefulRandom.Visuals.Sounds);
	}

	public static BaseUnitEntity GetRandomEntity(Func<BaseUnitEntity, bool> predicate)
	{
		IEnumerable<BaseUnitEntity> enumerable = Game.Instance.State.AllBaseAwakeUnits.Where(predicate);
		return enumerable.Where(UnitIsCloseToCamera).Random(PFStatefulRandom.Visuals.Sounds) ?? enumerable.Random(PFStatefulRandom.Visuals.Sounds);
	}

	public static bool UnitIsCloseToCamera(this BaseUnitEntity entity)
	{
		return VectorMath.SqrDistanceXZ(entity.Position, CameraRig.Instance.transform.position) < AggroBarkRadiusScr;
	}

	public static bool CanSpeakAsks(this MechanicEntity entity)
	{
		if (entity.View == null || entity.View.Asks == null || entity.IsDisposed || entity.IsDisposingNow)
		{
			return false;
		}
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return true;
		}
		if (!baseUnitEntity.LifeState.IsDead)
		{
			return true;
		}
		if (baseUnitEntity.View.DismembermentManager != null && baseUnitEntity.View.DismembermentManager.Dismembered)
		{
			return false;
		}
		if (entity.GetOptional<PartDropLootAndDestroyAfterDelay>() != null)
		{
			return false;
		}
		return baseUnitEntity.Buffs.RawFacts.All((Buff x) => x.Blueprint != DismemberedUnitBuff && x.Blueprint != DisintegrateBuff);
	}
}

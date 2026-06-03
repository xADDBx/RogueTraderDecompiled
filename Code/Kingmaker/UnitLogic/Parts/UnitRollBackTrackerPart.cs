using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[TypeId("0342244201a12ba49bc610318c37c7cb")]
public class UnitRollBackTrackerPart : BaseUnitPart, IHashable
{
	[JsonProperty]
	private Vector3 Position;

	[JsonProperty]
	private int Hp;

	[JsonProperty]
	private int Ap;

	[JsonProperty]
	private List<PartHealth.TemporaryHitPointsData> THp;

	[JsonProperty]
	private float Mp;

	[JsonProperty]
	public PartAbilityCooldowns.CooldownsStateSave CooldownDatas;

	[JsonProperty]
	public bool HasSavedData;

	private int TeleportRadius = 10;

	public void CacheValues()
	{
		Position = base.Owner.Position;
		Hp = base.Owner.HitPointsLeft;
		THp = base.Owner.Health.GetTemporaryHitPointsCopy();
		Ap = base.Owner.CombatState.ActionPointsYellow;
		Mp = base.Owner.CombatState.ActionPointsBlue;
		CooldownDatas = base.Owner.AbilityCooldowns.GetCooldownSaveStateCopy();
		HasSavedData = true;
	}

	public void RollBackAll()
	{
		if (HasSavedData)
		{
			RollBackPosition();
			RollBackPoints();
			RollBackCooldowns();
		}
	}

	public void RollBackCooldowns()
	{
		base.Owner.AbilityCooldowns.RestoreCooldownDataFromState(CooldownDatas, ignoreOncePerCombatRestriction: true);
		CooldownDatas = base.Owner.AbilityCooldowns.GetCooldownSaveStateCopy();
	}

	public void RollBackPoints()
	{
		RollBackHp();
		RollBackTHP();
		RollBackAp();
		RollBackMp();
	}

	public void RollBackHp()
	{
		base.Owner.Health.SetHitPointsLeft(Hp);
	}

	public void RollBackTHP()
	{
		base.Owner.Health.SetTemporaryHits(THp);
		THp = base.Owner.Health.GetTemporaryHitPointsCopy();
	}

	public void RollBackAp()
	{
		base.Owner.CombatState.SetActionPoints(Ap);
	}

	public void RollBackMp()
	{
		base.Owner.CombatState.SetActionPoints(null, Mp);
	}

	public void RollBackPosition()
	{
		base.Owner.Commands.InterruptAll((AbstractUnitCommand _) => true);
		base.Owner.Remove<UnitPartJump>();
		base.Owner.View.MovementAgent.Stop();
		base.Owner.View.MovementAgent.Blocker.Unblock();
		CustomGridNodeBase targetNode = ObstacleAnalyzer.GetNearestNode(Position).node as CustomGridNodeBase;
		if (!GridAreaHelper.TryGetStandableNode(base.Owner as UnitEntity, targetNode, TeleportRadius, out targetNode))
		{
			PFLog.Default.Error($"Failed to get a node to place {base.Owner}.");
			return;
		}
		base.Owner.Position = targetNode.Vector3Position;
		base.Owner.View.MovementAgent.Blocker.BlockAtCurrentPosition();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref Position);
		result.Append(ref Hp);
		result.Append(ref Ap);
		List<PartHealth.TemporaryHitPointsData> tHp = THp;
		if (tHp != null)
		{
			for (int i = 0; i < tHp.Count; i++)
			{
				Hash128 val2 = ClassHasher<PartHealth.TemporaryHitPointsData>.GetHash128(tHp[i]);
				result.Append(ref val2);
			}
		}
		result.Append(ref Mp);
		PartAbilityCooldowns.CooldownsStateSave obj = CooldownDatas;
		Hash128 val3 = StructHasher<PartAbilityCooldowns.CooldownsStateSave>.GetHash128(ref obj);
		result.Append(ref val3);
		result.Append(ref HasSavedData);
		return result;
	}
}

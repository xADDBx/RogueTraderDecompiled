using System.Collections.Generic;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("f0a72cfdf0ef46c41a504aae88b7809c")]
public class EtudeBracketFollowUnit : EtudeBracketTrigger, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public EntityRef<BaseUnitEntity> Leader;

		public readonly List<EntityRef<AbstractUnitEntity>> Followers = new List<EntityRef<AbstractUnitEntity>>();
	}

	[Tooltip("Main character if not specified")]
	[SerializeReference]
	public AbstractUnitEvaluator Leader;

	public bool UseSummonPool;

	[HideIf("UseSummonPool")]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[ShowIf("UseSummonPool")]
	public BlueprintSummonPoolReference SummonPool;

	public bool AlwaysRun;

	public bool CanBeSlowerThanLeader;

	[Tooltip("If set, unit will follow leader while it is playing cutscene. Example: follow while bark with cutscene")]
	public bool FollowWhileCutscene;

	protected override void OnEnter()
	{
		UpdateFollowers();
	}

	protected override void OnResume()
	{
		UpdateFollowers();
	}

	private void UpdateFollowers()
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (Leader != null)
		{
			if (!(Leader.GetValue() is BaseUnitEntity baseUnitEntity))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Etude {this}, {Leader} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
				return;
			}
			componentData.Leader = baseUnitEntity;
		}
		else
		{
			componentData.Leader = Game.Instance.Player.MainCharacterEntity;
		}
		if (componentData.Leader == null)
		{
			PFLog.Default.Error("EtudeBracketFollowUnit.OnEnter: leader is null");
			return;
		}
		foreach (AbstractUnitEntity unit in GetUnits())
		{
			if (unit == null)
			{
				PFLog.Default.Error("EtudeBracketFollowUnit.OnEnter: unit is null");
				continue;
			}
			unit.GetOrCreate<UnitPartFollowUnit>().Init(componentData.Leader, this);
			componentData.Followers.Add(unit);
		}
	}

	protected override void OnExit()
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		foreach (EntityRef<AbstractUnitEntity> follower in componentData.Followers)
		{
			UnitGroup unitGroup = follower.Entity?.GetCombatGroupOptional()?.Group;
			follower.Entity?.Remove<UnitPartFollowUnit>();
			if (unitGroup == null)
			{
				continue;
			}
			bool flag = false;
			foreach (UnitReference unit in unitGroup.Units)
			{
				if (unit.Entity?.ToBaseUnitEntity().GetOptional<UnitPartFollowUnit>() != null)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				unitGroup.IsFollowingUnitInCombat = false;
			}
		}
		componentData.Leader.Entity?.GetOptional<UnitPartFollowedByUnits>()?.Cleanup();
		componentData.Leader = null;
		componentData.Followers.Clear();
	}

	private IEnumerable<AbstractUnitEntity> GetUnits()
	{
		if (UseSummonPool)
		{
			ISummonPool summonPool = Game.Instance.SummonPools.Get(SummonPool.Get());
			if (summonPool == null)
			{
				yield break;
			}
			foreach (AbstractUnitEntity unit in summonPool.Units)
			{
				yield return unit;
			}
		}
		else
		{
			yield return Unit.GetValue();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

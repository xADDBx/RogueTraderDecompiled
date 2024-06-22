using System;
using System.Collections;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs;

[Serializable]
[TypeId("b49bc383429646038f5e6fecbf48f708")]
public class ReplaceUnitTransition : UnitBuffComponentDelegate, IHashable
{
	private enum OriginalUnitPolicy
	{
		KillBrutally,
		Replace
	}

	[SerializeField]
	private BlueprintUnit.Reference m_TargetUnit;

	[SerializeField]
	private OriginalUnitPolicy m_OriginalUnitPolicy;

	public bool DoNotShareFaction;

	public bool DoNotShareCombatGroup;

	[HideIf("DoNotShareCombatGroup")]
	public bool GetCombatGroupIdFromEvaluator;

	[ShowIf("GetCombatGroupIdFromEvaluator")]
	[SerializeReference]
	public StringEvaluator CombatGroupIdEvaluator;

	public bool DoNotShareSummonPools;

	public Polymorph.VisualTransitionSettings VisualSettings;

	public BlueprintUnit TargetUnit => m_TargetUnit;

	protected override void OnActivate()
	{
		BaseUnitEntity owner = base.Owner;
		string uniqueId = Uuid.Instance.CreateString();
		UnitEntity target = Entity.Initialize(new UnitEntity(uniqueId, owner.IsInGame, TargetUnit));
		target.Position = owner.Position;
		target.SetOrientation(owner.Orientation);
		target.AttachView(target.CreateView());
		target.View.transform.position = owner.View.transform.position;
		target.View.transform.rotation = owner.View.transform.rotation;
		base.Owner.GetOrCreate<PartReplaceUnitTransition>().Setup(base.Owner, target);
		target.GetOrCreate<PartReplaceUnitTransition>().Setup(base.Owner, target);
		Game.Instance.EntitySpawner.SpawnEntity(target, owner.HoldingState);
		base.Owner.MovementAgent.Blocker.Unblock();
		if (owner.SpawnFromPsychicPhenomena)
		{
			target.MarkSpawnFromPsychicPhenomena();
		}
		if (owner.IsInCombat)
		{
			target.CombatState.JoinCombat();
			target.Initiative.CopyFrom(owner.Initiative);
			foreach (UnitGroupMemory.UnitInfo enemy in target.CombatGroup.Memory.Enemies)
			{
				Game.Instance.UnitMemoryController.AddToMemory(enemy.Unit, target);
			}
		}
		if (!DoNotShareSummonPools)
		{
			Game.Instance.SummonPools.GetPoolsForUnit(owner).ForEach(delegate(BlueprintSummonPool i)
			{
				Game.Instance.SummonPools.Register(i, target);
			});
		}
		if (!DoNotShareFaction)
		{
			target.Faction.Set(owner.Faction.Blueprint);
		}
		if (!DoNotShareCombatGroup)
		{
			if (GetCombatGroupIdFromEvaluator)
			{
				string value = CombatGroupIdEvaluator.GetValue();
				if (string.IsNullOrEmpty(value))
				{
					PFLog.Default.Error($"String Evaluator of {this} cant find CombatGroupId");
				}
				else
				{
					target.CombatGroup.Id = value;
				}
			}
			else
			{
				target.CombatGroup.Id = owner.CombatGroup.Id;
			}
		}
		switch (m_OriginalUnitPolicy)
		{
		case OriginalUnitPolicy.KillBrutally:
			KillOriginalAndTransitToTarget(this, owner, target);
			break;
		case OriginalUnitPolicy.Replace:
			ReplaceOriginalToTarget(this, owner, target);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		target.MovementAgent.Blocker.BlockAtCurrentPosition();
	}

	private void KillOriginalAndTransitToTarget(ReplaceUnitTransition settings, BaseUnitEntity original, UnitEntity target)
	{
		GameHelper.KillUnit(original, original, 1, UnitDismemberType.InPower);
		IEnumerator routine = Polymorph.Transition(settings.VisualSettings, original.View, target.View);
		MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(routine);
		base.Owner.Facts.Remove(base.Fact);
	}

	private void ReplaceOriginalToTarget(ReplaceUnitTransition settings, BaseUnitEntity original, UnitEntity target)
	{
		StartTransition(settings, original, target);
		original.DetachView();
		Game.Instance.EntityDestroyer.Destroy(original);
		base.Owner.Facts.Remove(base.Fact);
	}

	private static void StartTransition(ReplaceUnitTransition settings, BaseUnitEntity original, UnitEntity target)
	{
		IEnumerator routine = Polymorph.Transition(settings.VisualSettings, original.View, target.View);
		MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(routine);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

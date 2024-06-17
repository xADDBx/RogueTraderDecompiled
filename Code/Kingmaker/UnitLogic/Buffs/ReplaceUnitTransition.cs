using System;
using System.Collections;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
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
		Game.Instance.EntitySpawner.SpawnEntity(target, owner.HoldingState);
		if (owner.SpawnFromPsychicPhenomena)
		{
			target.MarkSpawnFromPsychicPhenomena();
		}
		if (owner.IsInCombat)
		{
			target.CombatState.JoinCombat();
			target.Initiative.CopyFrom(owner.Initiative);
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
			target.CombatGroup.Id = owner.CombatGroup.Id;
		}
		switch (m_OriginalUnitPolicy)
		{
		case OriginalUnitPolicy.KillBrutally:
			GameHelper.KillUnit(owner, owner, 1, UnitDismemberType.InPower);
			break;
		case OriginalUnitPolicy.Replace:
			owner.DetachView();
			Game.Instance.EntityDestroyer.Destroy(owner);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		IEnumerator routine = Polymorph.Transition(VisualSettings, owner.View, target.View);
		MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(routine);
		base.Owner.Facts.Remove(base.Fact);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

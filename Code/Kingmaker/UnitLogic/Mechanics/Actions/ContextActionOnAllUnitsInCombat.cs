using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Covers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("7b1d3a11c0f4426a8584738997ebc207")]
public class ContextActionOnAllUnitsInCombat : ContextAction
{
	public bool OnlyEnemies = true;

	[HideIf("OnlyEnemies")]
	public bool OnlyAllies;

	public ActionList Actions;

	public bool OnlyParty;

	[SerializeField]
	private BlueprintUnitFactReference[] m_FilterNoFacts = new BlueprintUnitFactReference[0];

	[SerializeField]
	private BlueprintUnitFactReference[] m_FilterHaveAnyFact = new BlueprintUnitFactReference[0];

	public bool ActionsOnRandomTarget;

	public bool NotCaster;

	public bool OnlyVisible;

	public bool OnlyNotVisible;

	public bool IncludeDead;

	public ReferenceArrayProxy<BlueprintUnitFact> FilterNoFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] filterNoFacts = m_FilterNoFacts;
			return filterNoFacts;
		}
	}

	public ReferenceArrayProxy<BlueprintUnitFact> FilterHaveAnyFact
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] filterHaveAnyFact = m_FilterHaveAnyFact;
			return filterHaveAnyFact;
		}
	}

	public override string GetCaption()
	{
		return "Run a context action on all units in combat";
	}

	protected override void RunAction()
	{
		MechanicEntity caster = base.Context.MaybeCaster;
		if (caster == null || caster is BaseUnitEntity { IsPreviewUnit: not false })
		{
			return;
		}
		List<BaseUnitEntity> list = ((!OnlyParty) ? Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity p) => !p.Features.IsUntargetable && (IncludeDead || !p.LifeState.IsDead) && p.IsInCombat).ToList() : Game.Instance.State.PlayerState.Party.ToList());
		if (OnlyEnemies)
		{
			list.RemoveAll((BaseUnitEntity p) => !p.CombatGroup.IsEnemy(base.Context.MaybeCaster));
		}
		if (OnlyAllies)
		{
			list.RemoveAll((BaseUnitEntity p) => !p.CombatGroup.IsAlly(base.Context.MaybeCaster));
		}
		if (OnlyVisible)
		{
			list.RemoveAll((BaseUnitEntity p) => LosCalculations.GetWarhammerLos(caster, p).CoverType == LosCalculations.CoverType.Invisible);
		}
		if (OnlyNotVisible)
		{
			list.RemoveAll((BaseUnitEntity p) => LosCalculations.GetWarhammerLos(caster, p).CoverType != LosCalculations.CoverType.Invisible);
		}
		if (NotCaster)
		{
			list.RemoveAll((BaseUnitEntity p) => p == base.Context.MaybeCaster);
		}
		if (list.Empty())
		{
			return;
		}
		foreach (BlueprintUnitFact fact in FilterNoFacts)
		{
			list.RemoveAll((BaseUnitEntity p) => p.Facts.Contains(fact));
		}
		if (FilterHaveAnyFact.Any())
		{
			list = list.Where((BaseUnitEntity unit) => unit.Facts.Contains((EntityFact fact) => FilterHaveAnyFact.Contains(fact.Blueprint))).ToList();
		}
		if (list.Count <= 0)
		{
			return;
		}
		if (ActionsOnRandomTarget)
		{
			BaseUnitEntity entity = list.Random(PFStatefulRandom.Mechanics);
			using (base.Context.GetDataScope(entity.ToITargetWrapper()))
			{
				Actions.Run();
				return;
			}
		}
		foreach (BaseUnitEntity item in list)
		{
			using (base.Context.GetDataScope(item.ToITargetWrapper()))
			{
				Actions.Run();
			}
		}
	}
}

using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("2dcc60a5b076f0047b982e9d4c04b226")]
public class ContextActionOnRandomTargetsAround : ContextAction
{
	public bool OnEnemies = true;

	[HideIf("OnEnemies")]
	public bool OnlyAllies;

	public ActionList Actions;

	public int NumberOfTargets;

	public int TilesRadius;

	public bool LowestWounds;

	public bool AffectDead;

	[SerializeField]
	private BlueprintUnitFactReference m_FilterNoFact;

	[SerializeField]
	private BlueprintUnitFactReference m_FilterHasFact;

	public BlueprintUnitFact FilterNoFact => m_FilterNoFact?.Get();

	public BlueprintUnitFact FilterHasFact => m_FilterHasFact?.Get();

	public override string GetCaption()
	{
		return "Run a context action on random targets around";
	}

	public override void RunAction()
	{
		if ((bool)ContextData<UnitHelper.PreviewUnit>.Current || base.Context.MaybeCaster == null)
		{
			return;
		}
		List<BaseUnitEntity> list = TempList.Get<BaseUnitEntity>();
		foreach (CustomGridNodeBase item in GridAreaHelper.GetNodesSpiralAround(base.Target.NearestNode, base.Target.SizeRect, TilesRadius))
		{
			BaseUnitEntity unit = item.GetUnit();
			if (unit != null && !list.HasItem(unit))
			{
				list.Add(unit);
			}
		}
		if (OnEnemies)
		{
			list.RemoveAll((BaseUnitEntity p) => !p.CombatGroup.IsEnemy(base.Context.MaybeCaster));
		}
		if (OnlyAllies)
		{
			list.RemoveAll((BaseUnitEntity p) => !p.CombatGroup.IsAlly(base.Context.MaybeCaster));
		}
		if (!AffectDead)
		{
			list.RemoveAll((BaseUnitEntity p) => p.IsDead);
		}
		if (list.Empty())
		{
			return;
		}
		if (FilterNoFact != null)
		{
			list.RemoveAll((BaseUnitEntity p) => p.Facts.Contains(FilterNoFact));
		}
		if (FilterHasFact != null)
		{
			list.RemoveAll((BaseUnitEntity p) => !p.Facts.Contains(FilterHasFact));
		}
		if (LowestWounds)
		{
			int lowest = int.MaxValue;
			foreach (BaseUnitEntity item2 in list)
			{
				if (item2.Health.HitPointsLeft < lowest)
				{
					lowest = item2.Health.HitPointsLeft;
				}
			}
			list.RemoveAll((BaseUnitEntity p) => p.Health.HitPointsLeft > lowest);
		}
		int num = NumberOfTargets;
		while (num > 0 && !list.Empty())
		{
			BaseUnitEntity baseUnitEntity = list.Random(PFStatefulRandom.Mechanics);
			using (base.Context.GetDataScope(baseUnitEntity.ToITargetWrapper()))
			{
				Actions.Run();
			}
			list.Remove(baseUnitEntity);
			num--;
		}
	}
}

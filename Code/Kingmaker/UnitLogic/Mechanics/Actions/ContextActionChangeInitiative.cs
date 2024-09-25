using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("12bb59639e1d4c809a464a9882fbcd56")]
public class ContextActionChangeInitiative : ContextAction
{
	[SerializeField]
	private BlueprintBuffReference m_OverflowBuff;

	public BlueprintBuff OverflowBuff => m_OverflowBuff?.Get();

	public override string GetCaption()
	{
		return "Moves the unit up initiative ladder two steps";
	}

	protected override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			return;
		}
		MechanicEntity[] array = Game.Instance.TurnController.TurnOrder.UnitsOrder.ToArray();
		if (!array.Contains(entity) || array.Length <= 2)
		{
			return;
		}
		int num = array.IndexOf(entity);
		BuffDuration duration = new BuffDuration(1.Rounds(), BuffEndCondition.TurnStartOrCombatEnd);
		entity.Initiative.WasPreparedForRound = 0;
		if (num < 2)
		{
			entity.Initiative.Value = array.Last().Initiative.Value - 1f;
			if (num == 0)
			{
				entity.Initiative.ChangePlaces(array.Last().Initiative);
			}
			entity.Buffs.Add(OverflowBuff, base.Context, duration);
		}
		else
		{
			entity.Initiative.ChangePlaces(array[num - 1].Initiative);
			entity.Initiative.ChangePlaces(array[num - 2].Initiative);
		}
	}
}

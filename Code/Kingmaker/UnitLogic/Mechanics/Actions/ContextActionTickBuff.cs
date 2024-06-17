using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics.Components;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("b76edccb17434e3c8140e553dc76c8d8")]
public class ContextActionTickBuff : ContextAction
{
	[SerializeField]
	private BlueprintBuffReference m_TargetBuff;

	public bool OnlyDamage;

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();

	public override string GetCaption()
	{
		return "Perform all actions each round from AddFactContextActions and TurnBasedModeEventsTrigger";
	}

	public override void RunAction()
	{
		foreach (Buff item in base.Target.Entity?.Buffs.Enumerable.Where((Buff p) => p.Blueprint == TargetBuff))
		{
			EntityFact.ComponentsEnumerable<AddFactContextActions> componentsEnumerable = item.SelectComponents<AddFactContextActions>();
			EntityFact.ComponentsEnumerable<TurnBasedModeEventsTrigger> componentsEnumerable2 = item.SelectComponents<TurnBasedModeEventsTrigger>();
			EntityFact.ComponentsEnumerable<DOTLogic> componentsEnumerable3 = item.SelectComponents<DOTLogic>();
			foreach (AddFactContextActions item2 in componentsEnumerable)
			{
				ActionList actions = ((!OnlyDamage) ? item2.NewRound : new ActionList
				{
					Actions = item2.NewRound.Actions.Where((GameAction p) => p is ContextActionDealDamage).ToArray()
				});
				item.RunActionInContext(actions);
				ActionList actions2 = ((!OnlyDamage) ? item2.RoundEnd : new ActionList
				{
					Actions = item2.RoundEnd.Actions.Where((GameAction p) => p is ContextActionDealDamage).ToArray()
				});
				item.RunActionInContext(actions2);
			}
			foreach (TurnBasedModeEventsTrigger item3 in componentsEnumerable2)
			{
				ActionList actions3 = ((!OnlyDamage) ? item3.RoundStartActions : new ActionList
				{
					Actions = item3.RoundStartActions.Actions.Where((GameAction p) => p is ContextActionDealDamage).ToArray()
				});
				item.RunActionInContext(actions3);
				ActionList actions4 = ((!OnlyDamage) ? item3.RoundEndActions : new ActionList
				{
					Actions = item3.RoundEndActions.Actions.Where((GameAction p) => p is ContextActionDealDamage).ToArray()
				});
				item.RunActionInContext(actions4);
			}
			foreach (DOTLogic item4 in componentsEnumerable3)
			{
				DOTLogic.Tick(item, item4, OnlyDamage);
			}
		}
	}
}

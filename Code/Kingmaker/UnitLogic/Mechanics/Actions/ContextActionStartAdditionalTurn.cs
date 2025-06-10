using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("32ab11539189db84aa3d249b00be4d32")]
public class ContextActionStartAdditionalTurn : ContextAction
{
	[SerializeField]
	[Tooltip("Allows caster to interrupt his own turn")]
	private bool m_AllowOnCurrentTurnUnit;

	[SerializeField]
	private ContextValue GrantedMP;

	[SerializeField]
	private ContextValue GrantedAP;

	[SerializeField]
	private bool AsInterruption;

	[SerializeField]
	private RestrictionCalculator AbilityRestrictionForInterrupt;

	[SerializeField]
	private bool InterruptionWithoutUIUpdates;

	[SerializeField]
	private bool LetCurrentUnitFinishAction;

	[SerializeField]
	[Tooltip("If target allready has interruption turn in queue this action doesn't resolve and cancels itself.")]
	private bool ForbidChainInterruption;

	public override string GetCaption()
	{
		if (!AsInterruption)
		{
			return "Target takes an additional turn";
		}
		return "Target interrupts current unit's turn";
	}

	protected override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity == null || !entity.IsInCombat || (entity == Game.Instance.TurnController.CurrentUnit && !m_AllowOnCurrentTurnUnit) || (ForbidChainInterruption && entity.Initiative.InterruptingOrder > 0))
		{
			return;
		}
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		List<CasterExtraTurnBonus> list = ((maybeCaster != null) ? maybeCaster.Facts.GetComponents<CasterExtraTurnBonus>().ToList() : null) ?? new List<CasterExtraTurnBonus>();
		if (entity == base.Context.MaybeCaster)
		{
			list.RemoveAll((CasterExtraTurnBonus p) => p.OnlyIfTargetIsNotOwner);
		}
		int num = GrantedAP.Calculate(base.Context);
		num += ((num > 0) ? list.Sum((CasterExtraTurnBonus p) => p.ActionPointsBonus.Calculate(base.Context)) : 0);
		int num2 = GrantedMP.Calculate(base.Context);
		num2 += ((num2 > 0) ? list.Sum((CasterExtraTurnBonus p) => p.MovementPointsBonus.Calculate(base.Context)) : 0);
		Game.Instance.TurnController.InterruptCurrentTurn(entity, base.Caster, new InterruptionData
		{
			AsExtraTurn = !AsInterruption,
			RestrictionsOnInterrupt = AbilityRestrictionForInterrupt,
			WaitForCommandsToFinish = LetCurrentUnitFinishAction,
			InterruptionWithoutInitiativeAndPanelUpdate = InterruptionWithoutUIUpdates,
			GrantedAP = num,
			GrantedMP = num2
		});
		using (base.Context.GetDataScope(entity.ToITargetWrapper()))
		{
			foreach (CasterExtraTurnBonus item in list)
			{
				item.ActionsOnTarget.Run();
			}
		}
	}
}

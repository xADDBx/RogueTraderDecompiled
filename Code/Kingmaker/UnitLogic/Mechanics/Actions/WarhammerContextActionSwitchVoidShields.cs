using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("bc7154ed0f90e564daa577ebd433f137")]
public class WarhammerContextActionSwitchVoidShields : ContextAction
{
	private enum SwitchActionType
	{
		Activate,
		Deactivate
	}

	[SerializeField]
	private SwitchActionType SwitchAction;

	public override string GetCaption()
	{
		return $"{SwitchAction} shields";
	}

	protected override void RunAction()
	{
		StarshipEntity starshipEntity = (StarshipEntity)base.Target.Entity;
		if (starshipEntity != null)
		{
			switch (SwitchAction)
			{
			case SwitchActionType.Activate:
				starshipEntity.Shields.ActivateShields();
				break;
			case SwitchActionType.Deactivate:
				starshipEntity.Shields.DeactivateShields();
				break;
			}
		}
	}
}

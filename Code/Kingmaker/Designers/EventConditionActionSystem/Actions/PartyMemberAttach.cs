using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("ea5aa7b0edcc4026a49c18b1f065181d")]
public class PartyMemberAttach : GameAction
{
	public BlueprintUnitReference Unit;

	public override string GetCaption()
	{
		return "Attach companion " + Unit?.NameSafe() + " to party";
	}

	protected override void RunAction()
	{
		foreach (BaseUnitEntity allCrossSceneUnit in Game.Instance.Player.AllCrossSceneUnits)
		{
			if (allCrossSceneUnit.Blueprint != Unit.Get())
			{
				continue;
			}
			UnitPartCompanion optional = allCrossSceneUnit.GetOptional<UnitPartCompanion>();
			if (optional != null && optional.State == CompanionState.InPartyDetached)
			{
				allCrossSceneUnit.GetOptional<UnitPartCompanion>().SetState(CompanionState.InParty);
				EventBus.RaiseEvent((IBaseUnitEntity)allCrossSceneUnit, (Action<IPartyHandler>)delegate(IPartyHandler h)
				{
					h.HandleAddCompanion();
				}, isCheckRuntime: true);
			}
		}
	}
}

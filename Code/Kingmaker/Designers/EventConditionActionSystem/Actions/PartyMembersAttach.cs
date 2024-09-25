using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("cdaf4f23bba50a044a00da97d652575b")]
[PlayerUpgraderAllowed(false)]
public class PartyMembersAttach : GameAction
{
	public override string GetCaption()
	{
		return "Combine party";
	}

	protected override void RunAction()
	{
		foreach (BaseUnitEntity allCrossSceneUnit in Game.Instance.Player.AllCrossSceneUnits)
		{
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

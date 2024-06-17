using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Controllers;

public class ShadowSpellController : IController, IGlobalRulebookHandler<RulebookTargetEvent>, IRulebookHandler<RulebookTargetEvent>, ISubscriber, IGlobalRulebookSubscriber
{
	public void OnEventAboutToTrigger(RulebookTargetEvent evt)
	{
		MechanicEntity mechanicEntity = (MechanicEntity)evt.Initiator;
		(mechanicEntity.GetOptional<UnitPartShadowSummon>()?.Context)?.TryAffectByShadow(mechanicEntity, checkChance: false);
	}

	public void OnEventDidTrigger(RulebookTargetEvent evt)
	{
	}
}

using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("adc099b2458f409482d08821e5b33baf")]
[PlayerUpgraderAllowed(false)]
public class ChangeMinersProductivity : GameAction
{
	public int m_ProductivityPercents;

	public override string GetCaption()
	{
		return "Change miner productivity";
	}

	public override void RunAction()
	{
		Game.Instance.ColonizationController.ChangeMinerProductivity(m_ProductivityPercents, null, ColonyStatModifierType.Other);
	}
}

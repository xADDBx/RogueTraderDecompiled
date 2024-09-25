using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/AddFreeRespec")]
[PlayerUpgraderAllowed(false)]
[TypeId("81863632d02440cd8848a5c478b66029")]
public class AddFreeRespecToPlayer : GameAction
{
	public override string GetCaption()
	{
		return "Add free respec for each of the companions.";
	}

	public override string GetDescription()
	{
		return "Добавляет всем текущим компаньонам бесплатный респек.";
	}

	protected override void RunAction()
	{
		foreach (BaseUnitEntity item in (from ch in Game.Instance.Player.AllCrossSceneUnits.Where(delegate(BaseUnitEntity u)
			{
				UnitPartCompanion optional = u.GetOptional<UnitPartCompanion>();
				if (optional == null || optional.State != CompanionState.InParty)
				{
					UnitPartCompanion optional2 = u.GetOptional<UnitPartCompanion>();
					if (optional2 == null)
					{
						return false;
					}
					return optional2.State == CompanionState.Remote;
				}
				return true;
			})
			where ch.Progression != null
			select ch).ToList())
		{
			item.Progression.GiveExtraRespec();
		}
	}
}

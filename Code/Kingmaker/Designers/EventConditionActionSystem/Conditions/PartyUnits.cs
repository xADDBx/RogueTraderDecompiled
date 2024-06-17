using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/PartyUnits")]
[AllowMultipleComponents]
[TypeId("a906a0326978eee4f81b0893a191e7e4")]
public class PartyUnits : Condition
{
	public bool Any;

	public ConditionsChecker Conditions;

	protected override string GetConditionCaption()
	{
		return string.Format("For {0} units in party", Any ? "any" : "all");
	}

	protected override bool CheckCondition()
	{
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			using (ContextData<PartyUnitData>.Request().Setup(partyAndPet))
			{
				if (Any)
				{
					if (Conditions.Check())
					{
						return true;
					}
				}
				else if (!Conditions.Check())
				{
					return false;
				}
			}
		}
		return !Any;
	}
}

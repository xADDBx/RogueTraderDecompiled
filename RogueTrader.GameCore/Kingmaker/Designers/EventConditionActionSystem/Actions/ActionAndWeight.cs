using System;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Serializable]
public class ActionAndWeight
{
	public int Weight;

	public ConditionsChecker Conditions;

	public ActionList Action;
}

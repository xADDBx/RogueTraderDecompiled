using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.QA.Clockwork;

[ComponentName("ClockworkRules/SelectCompanionsCommand")]
[TypeId("2aa0a9ace7513c0458a5942aceeefe69")]
public class SelectCompanionsCommand : ClockworkCommand
{
	public List<BlueprintUnit> companions;

	public void SetUnits(List<UnitReference> unitRefs)
	{
		companions = new List<BlueprintUnit>();
		foreach (UnitReference unitRef in unitRefs)
		{
			companions.Add(unitRef.Entity.ToBaseUnitEntity().Blueprint);
		}
	}

	public override ClockworkRunnerTask GetTask(ClockworkRunner runner)
	{
		List<UnitReference> newCompanions = FindUnitsRefs();
		Game.Instance.Player.ReInitPartyCharacters(newCompanions);
		TaskDelayedCall taskDelayedCall = new TaskDelayedCall(runner, null, "Select companions", 3f);
		taskDelayedCall.SetSourceCommand(this);
		return taskDelayedCall;
	}

	private List<UnitReference> FindUnitsRefs()
	{
		List<UnitReference> list = new List<UnitReference>();
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.State.AllBaseUnits)
		{
			if (companions.Contains(allBaseUnit.Blueprint))
			{
				list.Add(UnitReference.FromIAbstractUnitEntity(allBaseUnit));
				PFLog.Clockwork.Log($"Add {allBaseUnit.Blueprint} to party");
			}
		}
		return list;
	}

	public override string GetCaption()
	{
		return GetStatusString() + "Select party companions";
	}
}

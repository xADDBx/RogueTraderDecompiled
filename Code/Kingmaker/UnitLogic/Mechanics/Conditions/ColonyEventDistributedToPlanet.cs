using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("052035f45ba843d4964321fff1192ad4")]
public class ColonyEventDistributedToPlanet : Condition
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintColonyEventReference m_Event;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintPlanet.Reference m_Planet;

	[SerializeField]
	private bool m_Except;

	public BlueprintColonyEvent Event => m_Event?.Get();

	public BlueprintPlanet Planet => m_Planet?.Get();

	protected override string GetConditionCaption()
	{
		return "";
	}

	protected override bool CheckCondition()
	{
		List<(ColonyEventState, ColoniesState.ColonyData)> colonyEventStates = ColoniesGenerator.GetColonyEventStates(Event);
		if (colonyEventStates.Count == 0)
		{
			return false;
		}
		if (!m_Except)
		{
			return colonyEventStates.Any(((ColonyEventState, ColoniesState.ColonyData) x) => x.Item2.Planet == Planet);
		}
		return colonyEventStates.Any(((ColonyEventState, ColoniesState.ColonyData) x) => x.Item2.Planet != Planet);
	}
}

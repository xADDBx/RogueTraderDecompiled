using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Utility.Attributes;
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
	private BlueprintPlanet.Reference m_Planet;

	[SerializeField]
	private bool m_Except;

	[SerializeField]
	private bool m_AnyPlanet;

	[SerializeField]
	private bool m_CheckStates;

	[SerializeField]
	[EnumFlagsAsButtons]
	private ColonyEventState m_States;

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
		if (m_AnyPlanet)
		{
			return colonyEventStates.Any(IsCorrectState);
		}
		if (!m_Except)
		{
			return colonyEventStates.Any(((ColonyEventState, ColoniesState.ColonyData) x) => OnPlanet(x) && IsCorrectState(x));
		}
		return colonyEventStates.Any(((ColonyEventState, ColoniesState.ColonyData) x) => ExceptPlanet(x) && IsCorrectState(x));
	}

	private bool OnPlanet((ColonyEventState, ColoniesState.ColonyData) x)
	{
		return x.Item2.Planet == Planet;
	}

	private bool ExceptPlanet((ColonyEventState, ColoniesState.ColonyData) x)
	{
		return x.Item2.Planet != Planet;
	}

	private bool IsCorrectState((ColonyEventState, ColoniesState.ColonyData) x)
	{
		if (m_CheckStates)
		{
			return (m_States & x.Item1) != 0;
		}
		return true;
	}
}

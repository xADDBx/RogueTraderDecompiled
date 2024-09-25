using System;
using Kingmaker.AI.Blueprints.Components;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.AI;

public static class AbilityDataEscapeFromThreatExtensions
{
	public static EscapeType GetEscapeType(this AbilityData ability)
	{
		AiEscapeFromThreat component = ability.Blueprint.GetComponent<AiEscapeFromThreat>();
		if (component != null)
		{
			return component.Type;
		}
		throw new ArgumentException($"{ability} doesn't have AiEscapeFromThreat component");
	}
}

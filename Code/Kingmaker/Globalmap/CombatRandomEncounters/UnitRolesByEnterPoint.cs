using System;
using Kingmaker.Blueprints;

namespace Kingmaker.Globalmap.CombatRandomEncounters;

[Serializable]
public class UnitRolesByEnterPoint
{
	public BlueprintAreaEnterPointReference EnterPoint;

	public UnitRole Roles;
}

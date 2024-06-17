using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("a914988c5999d62429333a243c602148")]
public class BuildBalanceRadarChart : BlueprintComponent
{
	[Range(0f, 5f)]
	public int Melee;

	[Range(0f, 5f)]
	public int Ranged;

	[Range(0f, 5f)]
	public int Magic;

	[Range(0f, 5f)]
	public int Defense;

	[Range(0f, 5f)]
	public int Support;

	[Range(0f, 5f)]
	public int Control;
}

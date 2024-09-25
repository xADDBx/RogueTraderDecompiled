using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Visual.Blueprints;

[TypeId("d44062c90fc54e328955a6dae8e36f34")]
public class BlueprintAreaEffectPitVisualSettings : BlueprintScriptableObject
{
	public float DepthMeters = 2.5f;

	public float HoleEdgeMeters = 1.7f;

	public GameObject UnitDisappearFx;

	public GameObject UnitAppearFx;

	public AnimationCurve FallXZCurve;

	public AnimationCurve FallYCurve;

	public AnimationCurve ClimbXZCurve;

	public AnimationCurve ClimbYCurve;
}

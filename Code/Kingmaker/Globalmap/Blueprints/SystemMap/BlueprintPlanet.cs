using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;

namespace Kingmaker.Globalmap.Blueprints.SystemMap;

[TypeId("38a57f27bb1e46e7a1d0200ffbbe1319")]
public class BlueprintPlanet : BlueprintStarSystemObject
{
	public enum PlanetType
	{
		Arid,
		Ash,
		Barren,
		Boreal,
		Burning,
		Continental,
		Frozen,
		Ice,
		GasGiant,
		Lava,
		MoonLike,
		Planetoid,
		Ocean,
		Rocky,
		Sand,
		Savannah,
		Snow,
		Steppes,
		Toxic,
		Tropical,
		Tundra,
		Mined,
		Dead,
		Cracked
	}

	[Serializable]
	public new class Reference : BlueprintReference<BlueprintPlanet>
	{
	}

	public PlanetType Type;

	public LocalizedString TitheGrade = new LocalizedString();

	public override bool ShouldBeHighlighted => true;
}

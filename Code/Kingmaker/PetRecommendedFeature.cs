using System;
using Kingmaker.Blueprints;

namespace Kingmaker;

[Serializable]
public class PetRecommendedFeature
{
	public BlueprintUnitFactReference Feature;

	public bool NotRecommended;
}

using System;
using Kingmaker.Enums;

namespace Kingmaker.Blueprints.Area;

[Serializable]
public class RandomEncounterSettings
{
	public int PerceptionDCModifier;

	public UnitTag[] Tags;
}

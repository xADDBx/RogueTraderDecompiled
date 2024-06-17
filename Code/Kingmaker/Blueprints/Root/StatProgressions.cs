using System;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class StatProgressions
{
	public BlueprintStatProgression BABLow;

	public BlueprintStatProgression BABMedium;

	public BlueprintStatProgression BABHigh;

	public BlueprintStatProgression SavesLow;

	public BlueprintStatProgression SavesHigh;

	public BlueprintStatProgression SavesPrestigeLow;

	public BlueprintStatProgression SavesPrestigeHigh;
}

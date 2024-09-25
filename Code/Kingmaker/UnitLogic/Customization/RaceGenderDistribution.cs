using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Customization;

[TypeId("54880107faad4bf8803ca1c6c6e47369")]
public class RaceGenderDistribution : BlueprintScriptableObject
{
	[NotNull]
	public RaceEntry[] Races = new RaceEntry[0];

	public float MaleBaseWeight = 1f;

	public float FemaleBaseWeight = 1f;

	public List<UnitCustomizationVariation> Generate(int expectedCount)
	{
		Dictionary<UnitCustomizationVariation, double> dictionary = new Dictionary<UnitCustomizationVariation, double>();
		RaceEntry[] races = Races;
		foreach (RaceEntry raceEntry in races)
		{
			dictionary[new UnitCustomizationVariation(raceEntry.Race, Gender.Male)] = 1.0 * (double)raceEntry.BaseWeight * (double)MaleBaseWeight * (double)raceEntry.MaleModifier;
			dictionary[new UnitCustomizationVariation(raceEntry.Race, Gender.Female)] = 1.0 * (double)raceEntry.BaseWeight * (double)FemaleBaseWeight * (double)raceEntry.FemaleModifier;
		}
		double num = dictionary.Values.Sum();
		List<UnitCustomizationVariation> list = new List<UnitCustomizationVariation>();
		foreach (KeyValuePair<UnitCustomizationVariation, double> item in dictionary)
		{
			double num2 = item.Value / num;
			int num3 = (int)Math.Round(num2 * (double)expectedCount);
			if (num2 > 0.0 && num3 <= 0)
			{
				num3 = 1;
			}
			for (int j = 0; j < num3; j++)
			{
				list.Add(new UnitCustomizationVariation(item.Key));
			}
		}
		return list;
	}
}

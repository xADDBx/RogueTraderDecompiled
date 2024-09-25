using System;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class RaceFxScaleSettings
{
	public float Human = 1f;

	public float Spacemarine = 1f;

	public float Eldar = 1f;

	public float GetCoeff(Race? race)
	{
		return race switch
		{
			Race.Human => Human, 
			Race.Spacemarine => Spacemarine, 
			Race.Eldar => Eldar, 
			_ => 1f, 
		};
	}
}

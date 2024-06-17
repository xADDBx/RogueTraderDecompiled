namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public class InclemencyDependentSystem : IInclemencyDependentSystem
{
	public bool InclemencyChanged;

	public InclemencyType Inclemency { get; private set; }

	public float InclemencyChangePercentage { get; private set; }

	public void SetInclemency(InclemencyType inclemency)
	{
		Inclemency = inclemency;
		InclemencyChanged = true;
	}

	public void SetInclemencyChangePercentage(float percentage)
	{
		InclemencyChangePercentage = percentage;
	}
}

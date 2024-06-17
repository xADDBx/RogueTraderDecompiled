namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public interface IInclemencyDependentSystem
{
	void SetInclemency(InclemencyType inclemency);

	void SetInclemencyChangePercentage(float percentage);
}

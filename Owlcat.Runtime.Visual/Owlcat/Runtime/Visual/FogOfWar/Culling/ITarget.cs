namespace Owlcat.Runtime.Visual.FogOfWar.Culling;

public interface ITarget
{
	int RegistryIndex { get; set; }

	string SortOrder { get; }

	TargetProperties Properties { get; }

	bool Revealed { get; set; }

	bool ForceReveal { get; }
}

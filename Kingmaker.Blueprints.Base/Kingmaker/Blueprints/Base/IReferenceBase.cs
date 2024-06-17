namespace Kingmaker.Blueprints.Base;

public interface IReferenceBase
{
	string Guid { get; }

	void ReadGuidFromJson(string value);
}

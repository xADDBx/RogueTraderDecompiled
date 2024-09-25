namespace Kingmaker.EntitySystem.Interfaces;

public interface IAbstractEntityPartComponent
{
	object GetSettings();

	void EnsureEntityPart();
}

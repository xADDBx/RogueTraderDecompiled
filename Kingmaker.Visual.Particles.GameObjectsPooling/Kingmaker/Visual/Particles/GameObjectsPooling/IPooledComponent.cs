namespace Kingmaker.Visual.Particles.GameObjectsPooling;

public interface IPooledComponent
{
	void OnClaim();

	void OnRelease();
}

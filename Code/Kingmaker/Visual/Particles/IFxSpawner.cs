using UnityEngine;

namespace Kingmaker.Visual.Particles;

public interface IFxSpawner
{
	FxSpawnerPriority Priority { get; }

	void SpawnFxOnGameObject(GameObject target);

	void SpawnFxOnPoint(Vector3 point, Quaternion rotation);
}

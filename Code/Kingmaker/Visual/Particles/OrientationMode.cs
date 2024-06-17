using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class OrientationMode : MonoBehaviour, IFxSpawner
{
	public OrientationModeType Mode;

	public FxSpawnerPriority Priority => FxSpawnerPriority.OrientationMode;

	private void OnEnable()
	{
		Do();
	}

	private void Do()
	{
		Mode.Apply(base.gameObject.transform);
	}

	public void SpawnFxOnGameObject(GameObject target)
	{
		if (base.enabled)
		{
			Do();
		}
	}

	public void SpawnFxOnPoint(Vector3 point, Quaternion rotation)
	{
		if (base.enabled)
		{
			Do();
		}
	}
}

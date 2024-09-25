using Kingmaker.Utility.Random;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.Visual;

public class RandomScaleOnStart : MonoBehaviour, IFxSpawner
{
	private bool m_Executed;

	public Vector2 ScaleX = Vector2.one;

	public Vector2 ScaleY = Vector2.one;

	public Vector2 ScaleZ = Vector2.one;

	public FxSpawnerPriority Priority => FxSpawnerPriority.RandomScaleOnStart;

	public void Scale()
	{
		if (!m_Executed)
		{
			Vector3 localScale = new Vector3(PFStatefulRandom.Visual.Range(ScaleX.x, ScaleX.y), PFStatefulRandom.Visual.Range(ScaleY.x, ScaleY.y), PFStatefulRandom.Visual.Range(ScaleZ.x, ScaleZ.y));
			base.transform.localScale = localScale;
			m_Executed = true;
		}
	}

	private void Start()
	{
		Scale();
	}

	public void SpawnFxOnGameObject(GameObject target)
	{
		if (base.enabled)
		{
			Scale();
		}
	}

	public void SpawnFxOnPoint(Vector3 point, Quaternion rotation)
	{
		if (base.enabled)
		{
			Scale();
		}
	}
}

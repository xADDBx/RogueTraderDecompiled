using Kingmaker.Utility.Random;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Visual;

public class RandomRotationOnStart : MonoBehaviour, IFxSpawner
{
	private bool m_Executed;

	public Space Space;

	[MinMaxSlider(-360f, 360f)]
	public Vector2 RotationX;

	[MinMaxSlider(-360f, 360f)]
	public Vector2 RotationY;

	[MinMaxSlider(-360f, 360f)]
	public Vector2 RotationZ;

	public FxSpawnerPriority Priority => FxSpawnerPriority.RandomRotationOnStart;

	public void Rotate()
	{
		if (!m_Executed)
		{
			Quaternion quaternion = Quaternion.Euler(PFStatefulRandom.Visual.Range(RotationX.x, RotationX.y), PFStatefulRandom.Visual.Range(RotationY.x, RotationY.y), PFStatefulRandom.Visual.Range(RotationZ.x, RotationZ.y));
			switch (Space)
			{
			case Space.World:
				base.transform.rotation *= quaternion;
				break;
			case Space.Self:
				base.transform.localRotation *= quaternion;
				break;
			}
			m_Executed = true;
		}
	}

	private void Start()
	{
		Rotate();
	}

	public void SpawnFxOnGameObject(GameObject target)
	{
		if (base.enabled)
		{
			Rotate();
		}
	}

	public void SpawnFxOnPoint(Vector3 point, Quaternion rotation)
	{
		if (base.enabled)
		{
			Rotate();
		}
	}
}

using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects;

public class RadialBlurSetup : MonoBehaviour, IFxSpawner
{
	public RadialBlurSettings Animation;

	public FxSpawnerPriority Priority => FxSpawnerPriority.Initialize;

	public void SpawnFxOnGameObject(GameObject target)
	{
		Animation.TargetObject = base.transform;
		AddAnimationToController();
	}

	public void SpawnFxOnPoint(Vector3 point, Quaternion rotation)
	{
		Animation.TargetObject = base.transform;
		AddAnimationToController();
	}

	private void AddAnimationToController()
	{
		Animation.Reset();
		if (RadialBlurController.Instance != null)
		{
			RadialBlurController.Instance.Animations.Add(Animation);
		}
	}
}

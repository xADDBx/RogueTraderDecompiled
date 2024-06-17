using Kingmaker.View;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class DistortionOffset : MonoBehaviour, IFxSpawner
{
	private GameObject m_TargetGo;

	public FxSpawnerPriority Priority => FxSpawnerPriority.RandomScaleOnStart;

	public void SpawnFxOnGameObject(GameObject target)
	{
		m_TargetGo = target;
	}

	public void SpawnFxOnPoint(Vector3 point, Quaternion rotation)
	{
	}

	private void Update()
	{
		if (m_TargetGo == null)
		{
			return;
		}
		ParticlesSnapController[] componentsInChildren = GetComponentsInChildren<ParticlesSnapController>();
		foreach (ParticlesSnapController particlesSnapController in componentsInChildren)
		{
			ParticleSystemRenderer component = particlesSnapController.GetComponent<ParticleSystemRenderer>();
			if (!(component == null))
			{
				Vector2 vector = CameraRig.Instance.WorldToViewport(m_TargetGo.transform.position);
				Vector2 vector2 = CameraRig.Instance.WorldToViewport(m_TargetGo.transform.position + particlesSnapController.CurrentOffset);
				Material material = component.material;
				material.SetVector(ShaderProps._DistortionOffset, vector - vector2);
				component.material = material;
			}
		}
	}
}

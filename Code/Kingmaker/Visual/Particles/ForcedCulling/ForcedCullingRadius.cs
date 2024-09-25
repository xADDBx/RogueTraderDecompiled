using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.Particles.ForcedCulling;

public class ForcedCullingRadius : MonoBehaviour
{
	public float Radius;

	[SerializeField]
	[FormerlySerializedAs("Static")]
	private bool m_Static = true;

	private int m_CullingSphereIndex = -1;

	private readonly List<ParticleSystem> m_ParticleSystems = new List<ParticleSystem>();

	private readonly List<float> m_ParticleSystemTimes = new List<float>();

	private readonly List<Animator> m_Animators = new List<Animator>();

	private readonly List<Renderer> m_Renderers = new List<Renderer>();

	private readonly List<Light> m_Lights = new List<Light>();

	private readonly List<bool> m_LightsDefaultEnabledStatus = new List<bool>();

	private Transform m_Transform;

	private bool m_IsCulled;

	public bool Static
	{
		get
		{
			return m_Static;
		}
		set
		{
			if (m_Static != value)
			{
				m_Static = value;
				if (base.enabled)
				{
					ForcedCullingService.Instance.Move(this);
				}
			}
		}
	}

	public int SphereIndex => m_CullingSphereIndex;

	private void OnEnable()
	{
		m_Transform = base.transform;
		ForcedCullingService.Instance.Add(this);
	}

	private void OnDisable()
	{
		if (m_IsCulled)
		{
			SetVisible(visible: true);
		}
		ForcedCullingService.Instance.Remove(this);
	}

	public void Init(int sphereIndex)
	{
		m_CullingSphereIndex = sphereIndex;
		GetComponentsInChildren(m_ParticleSystems);
		m_ParticleSystemTimes.Clear();
		foreach (ParticleSystem particleSystem in m_ParticleSystems)
		{
			m_ParticleSystemTimes.Add(particleSystem.time);
		}
		GetComponentsInChildren(m_Animators);
		List<Renderer> value;
		using (CollectionPool<List<Renderer>, Renderer>.Get(out value))
		{
			GetComponentsInChildren(includeInactive: false, value);
			m_Renderers.Clear();
			foreach (Renderer item in value)
			{
				if (item.enabled)
				{
					m_Renderers.Add(item);
				}
			}
		}
		GetComponentsInChildren(m_Lights);
		m_LightsDefaultEnabledStatus.Clear();
		foreach (Light light in m_Lights)
		{
			m_LightsDefaultEnabledStatus.Add(light.enabled);
		}
	}

	public void UpdateBounds(BoundingSphere[] spheres)
	{
		spheres[m_CullingSphereIndex].position = m_Transform.position;
		spheres[m_CullingSphereIndex].radius = Radius;
	}

	public void SetVisible(bool visible)
	{
		for (int i = 0; i < m_ParticleSystems.Count; i++)
		{
			ParticleSystem particleSystem = m_ParticleSystems[i];
			if ((bool)particleSystem)
			{
				if (visible)
				{
					particleSystem.Play();
					particleSystem.time = m_ParticleSystemTimes[i];
				}
				else
				{
					m_ParticleSystemTimes[i] = particleSystem.time;
					particleSystem.Pause();
				}
			}
		}
		foreach (Animator animator in m_Animators)
		{
			if ((bool)animator)
			{
				animator.enabled = visible;
			}
		}
		foreach (Renderer renderer in m_Renderers)
		{
			if ((bool)renderer)
			{
				renderer.enabled = visible;
			}
		}
		for (int j = 0; j < m_Lights.Count; j++)
		{
			Light light = m_Lights[j];
			if ((bool)light)
			{
				light.enabled = visible && m_LightsDefaultEnabledStatus[j];
			}
		}
		m_IsCulled = !visible;
	}

	public void ChangeIndex(int idx, BoundingSphere[] spheres)
	{
		m_CullingSphereIndex = idx;
		UpdateBounds(spheres);
	}

	public void SetLightEnabledByDefault(Light l)
	{
		for (int i = 0; i < m_Lights.Count; i++)
		{
			if (l == m_Lights[i])
			{
				m_LightsDefaultEnabledStatus[i] = l.enabled;
				l.enabled = l.enabled && !m_IsCulled;
				break;
			}
		}
	}
}

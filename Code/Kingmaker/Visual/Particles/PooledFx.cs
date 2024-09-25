using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class PooledFx : PooledGameObject
{
	private static readonly List<IDeactivatableComponent> s_DeactivatableTemp = new List<IDeactivatableComponent>();

	[NonSerialized]
	private bool m_ComponentsCached;

	[NonSerialized]
	private readonly List<ParticleSystem> m_ParticleSystems = new List<ParticleSystem>();

	[NonSerialized]
	private readonly List<ParticleSystem> m_ParticleSubEmitters = new List<ParticleSystem>();

	[NonSerialized]
	private readonly List<SnapControllerBase> m_SnapControllers = new List<SnapControllerBase>();

	[NonSerialized]
	private readonly List<ParticleSystemRenderer> m_ParticleSystemRenderers = new List<ParticleSystemRenderer>();

	[NonSerialized]
	private readonly List<SnapToLocator> m_SnapToLocators = new List<SnapToLocator>();

	[NonSerialized]
	private readonly List<MonoBehaviour> m_DeactivatableComponents = new List<MonoBehaviour>();

	[NonSerialized]
	private int m_LayerAdjustment;

	private static readonly WaitForEndOfFrame WaitForEndOfFrameToken = new WaitForEndOfFrame();

	private void CacheComponents()
	{
		if (!m_ComponentsCached)
		{
			m_ComponentsCached = true;
			CacheComponentsRecursively(base.transform);
		}
	}

	private void CacheComponentsRecursively(Transform t)
	{
		t.GetComponents(s_DeactivatableTemp);
		foreach (IDeactivatableComponent item in s_DeactivatableTemp)
		{
			MonoBehaviour monoBehaviour = item as MonoBehaviour;
			if ((bool)monoBehaviour)
			{
				m_DeactivatableComponents.Add(monoBehaviour);
			}
		}
		SnapToLocator component = t.GetComponent<SnapToLocator>();
		if ((bool)component)
		{
			m_SnapToLocators.Add(component);
		}
		ParticleSystemRenderer component2 = t.GetComponent<ParticleSystemRenderer>();
		if ((bool)component2)
		{
			m_ParticleSystemRenderers.Add(component2);
		}
		SnapControllerBase component3 = t.GetComponent<SnapControllerBase>();
		if ((bool)component3)
		{
			m_SnapControllers.Add(component3);
			return;
		}
		ParticleSystem component4 = t.GetComponent<ParticleSystem>();
		if ((bool)component4)
		{
			m_ParticleSystems.Add(component4);
			if (component4.subEmitters.subEmittersCount > 0)
			{
				for (int i = 0; i < component4.subEmitters.subEmittersCount; i++)
				{
					ParticleSystem subEmitterSystem = component4.subEmitters.GetSubEmitterSystem(i);
					if (!m_ParticleSubEmitters.Contains(subEmitterSystem))
					{
						m_ParticleSubEmitters.Add(subEmitterSystem);
					}
				}
			}
		}
		for (int j = 0; j < t.childCount; j++)
		{
			CacheComponentsRecursively(t.GetChild(j));
		}
	}

	public void AdjustLayerOrder(int delta)
	{
		CacheComponents();
		m_LayerAdjustment += delta;
		for (int i = 0; i < m_ParticleSystemRenderers.Count; i++)
		{
			ParticleSystemRenderer particleSystemRenderer = m_ParticleSystemRenderers[i];
			if ((bool)particleSystemRenderer)
			{
				particleSystemRenderer.sortingOrder += delta;
			}
		}
	}

	private IEnumerator PostponedPlay()
	{
		yield return Application.isBatchMode ? null : WaitForEndOfFrameToken;
		if (!base.IsClaimed)
		{
			yield break;
		}
		for (int i = 0; i < m_ParticleSystems.Count; i++)
		{
			ParticleSystem particleSystem = m_ParticleSystems[i];
			if (!(particleSystem == null) && !m_ParticleSubEmitters.Contains(particleSystem))
			{
				particleSystem.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmittingAndClear);
				particleSystem.Play(withChildren: false);
			}
		}
	}

	public sealed override void OnClaim()
	{
		CacheComponents();
		for (int i = 0; i < m_ParticleSystems.Count; i++)
		{
			ParticleSystem particleSystem = m_ParticleSystems[i];
			if ((bool)particleSystem)
			{
				particleSystem.Simulate(0f, withChildren: false, restart: true);
			}
		}
		Game.Instance.CoroutinesController.Start(PostponedPlay());
		base.OnClaim();
	}

	public sealed override void OnRelease()
	{
		CacheComponents();
		for (int i = 0; i < m_ParticleSystemRenderers.Count; i++)
		{
			ParticleSystemRenderer particleSystemRenderer = m_ParticleSystemRenderers[i];
			if ((bool)particleSystemRenderer)
			{
				particleSystemRenderer.sortingOrder -= m_LayerAdjustment;
			}
			else
			{
				PFLog.Default.Error($"Pooled FX {base.name} has lost a ParticelSystemRenderer #{i}!");
			}
		}
		m_LayerAdjustment = 0;
		for (int j = 0; j < m_ParticleSystems.Count; j++)
		{
			ParticleSystem particleSystem = m_ParticleSystems[j];
			if ((bool)particleSystem)
			{
				particleSystem.Stop(withChildren: false);
			}
			else
			{
				PFLog.Default.Error($"Pooled FX {base.name} has lost a ParticelSystem #{j}!");
			}
		}
		for (int k = 0; k < m_SnapControllers.Count; k++)
		{
			SnapControllerBase snapControllerBase = m_SnapControllers[k];
			if ((bool)snapControllerBase)
			{
				snapControllerBase.Stop();
			}
			else
			{
				PFLog.Default.Error($"Pooled FX {base.name} has lost a SnapController #{k}!");
			}
		}
		for (int l = 0; l < m_SnapToLocators.Count; l++)
		{
			SnapToLocator snapToLocator = m_SnapToLocators[l];
			if ((bool)snapToLocator)
			{
				if (snapToLocator.gameObject != base.gameObject)
				{
					snapToLocator.gameObject.SetActive(value: true);
				}
			}
			else
			{
				PFLog.Default.Error($"Pooled FX {base.name} has lost a SnapToLocator #{l}!");
			}
		}
		for (int m = 0; m < m_DeactivatableComponents.Count; m++)
		{
			MonoBehaviour monoBehaviour = m_DeactivatableComponents[m];
			if ((bool)monoBehaviour)
			{
				monoBehaviour.enabled = true;
				if (monoBehaviour.gameObject != base.gameObject)
				{
					monoBehaviour.gameObject.SetActive(value: true);
				}
			}
			else
			{
				PFLog.Default.Error($"Pooled FX {base.name} has lost a DeactivetableComponent #{m}!");
			}
		}
		base.OnRelease();
	}
}

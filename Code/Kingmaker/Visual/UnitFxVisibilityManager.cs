using System.Collections.Generic;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Trails;
using Owlcat.Runtime.Core.Updatables;
using UnityEngine;

namespace Kingmaker.Visual;

public class UnitFxVisibilityManager : UpdateableBehaviour
{
	private AbstractUnitEntityView m_Unit;

	private bool m_IsVisible = true;

	private readonly List<Renderer> m_Renderers = new List<Renderer>();

	private readonly List<AnimatedLight> m_AnimatedLights = new List<AnimatedLight>();

	private readonly List<Light> m_Lights = new List<Light>();

	private readonly List<CompositeTrailRenderer> m_Trails = new List<CompositeTrailRenderer>();

	public static void Remove(GameObject fx)
	{
		if (!(fx == null))
		{
			UnitFxVisibilityManager component = fx.GetComponent<UnitFxVisibilityManager>();
			if (component != null)
			{
				component.m_Unit = null;
			}
		}
	}

	public void Init(AbstractUnitEntityView unit)
	{
		m_Unit = unit;
		m_Renderers.Clear();
		m_Lights.Clear();
		base.gameObject.GetComponentsInChildren(m_Renderers);
		base.gameObject.GetComponentsInChildren(m_Lights);
		base.gameObject.GetComponentsInChildren(m_Trails);
		base.gameObject.GetComponentsInChildren(m_AnimatedLights);
	}

	public override void DoUpdate()
	{
		if (m_Unit == null || m_Unit.Blueprint.IsCheater)
		{
			return;
		}
		bool flag = m_Unit.IsVisible && (m_Unit.Data == null || m_Unit.Data.IsViewActive);
		if (flag == m_IsVisible)
		{
			return;
		}
		m_IsVisible = flag;
		foreach (Renderer renderer in m_Renderers)
		{
			renderer.enabled = m_IsVisible;
		}
		foreach (Light light in m_Lights)
		{
			light.enabled = m_IsVisible;
		}
		foreach (AnimatedLight animatedLight in m_AnimatedLights)
		{
			animatedLight.enabled = m_IsVisible;
		}
		foreach (CompositeTrailRenderer trail in m_Trails)
		{
			trail.SetEmittersEnabled(m_IsVisible);
		}
	}

	protected override void OnDisabled()
	{
		m_Unit = null;
	}
}

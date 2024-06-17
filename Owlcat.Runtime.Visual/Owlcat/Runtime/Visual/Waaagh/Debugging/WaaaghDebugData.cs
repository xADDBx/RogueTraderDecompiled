using System;
using System.Collections.Generic;
using Owlcat.ShaderLibrary.Visual.Debug;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[ReloadGroup]
public class WaaaghDebugData : ScriptableObject, IDebugData
{
	[Serializable]
	[ReloadGroup]
	public sealed class ShaderResources
	{
		[Reload("Runtime/Waaagh/Debugging/DebugFullscreen.shader", ReloadAttribute.Package.Root)]
		public Shader DebugFullscreenPS;

		[Reload("Runtime/Waaagh/Debugging/ShadowsDebug.shader", ReloadAttribute.Package.Root)]
		public Shader ShadowsDebugPS;

		[Reload("Runtime/Waaagh/Debugging/ShowLightSortingCurve.shader", ReloadAttribute.Package.Root)]
		public Shader ShowLightSortingCurvePS;
	}

	public ShaderResources Shaders;

	[SerializeField]
	private LightingDebug m_LightingDebug = new LightingDebug();

	[SerializeField]
	private RenderingDebug m_RenderingDebug = new RenderingDebug();

	[SerializeField]
	private StencilDebug m_StencilDebug = new StencilDebug();

	[SerializeField]
	private ShadowsDebug m_ShadowsDebug = new ShadowsDebug();

	private DebugDisplaySettingsRendering m_DebugRendering;

	private DebugDisplaySettingsLighting m_DebugLighting;

	private DebugDisplaySettingsShadows m_DebugShadows;

	private IEnumerable<IDebugDisplaySettingsPanelDisposable> m_DisposablePanels;

	public LightingDebug LightingDebug => m_LightingDebug;

	public RenderingDebug RenderingDebug => m_RenderingDebug;

	public StencilDebug StencilDebug => m_StencilDebug;

	public ShadowsDebug ShadowsDebug => m_ShadowsDebug;

	private void Reset()
	{
		m_LightingDebug = new LightingDebug();
		m_RenderingDebug = new RenderingDebug();
		m_StencilDebug = new StencilDebug();
		m_ShadowsDebug = new ShadowsDebug();
	}

	public void RegisterDebug()
	{
		Init();
		DebugManager instance = DebugManager.instance;
		instance.RegisterData(this);
		List<IDebugDisplaySettingsPanelDisposable> list = new List<IDebugDisplaySettingsPanelDisposable>();
		IDebugDisplaySettingsPanelDisposable debugDisplaySettingsPanelDisposable = m_DebugRendering.CreatePanel();
		DebugUI.Widget[] widgets = debugDisplaySettingsPanelDisposable.Widgets;
		DebugUI.Panel panel = instance.GetPanel(debugDisplaySettingsPanelDisposable.PanelName, createIfNull: true);
		panel.children.Add(widgets);
		RegisterRenderGraph(panel);
		list.Add(debugDisplaySettingsPanelDisposable);
		IDebugDisplaySettingsPanelDisposable debugDisplaySettingsPanelDisposable2 = m_DebugLighting.CreatePanel();
		DebugUI.Widget[] widgets2 = debugDisplaySettingsPanelDisposable2.Widgets;
		instance.GetPanel(debugDisplaySettingsPanelDisposable2.PanelName, createIfNull: true).children.Add(widgets2);
		list.Add(debugDisplaySettingsPanelDisposable2);
		IDebugDisplaySettingsPanelDisposable debugDisplaySettingsPanelDisposable3 = m_DebugShadows.CreatePanel();
		DebugUI.Widget[] widgets3 = debugDisplaySettingsPanelDisposable3.Widgets;
		instance.GetPanel(debugDisplaySettingsPanelDisposable3.PanelName, createIfNull: true).children.Add(widgets3);
		list.Add(debugDisplaySettingsPanelDisposable3);
		m_DisposablePanels = list;
	}

	private void Init()
	{
		if (m_DebugRendering == null)
		{
			m_DebugRendering = new DebugDisplaySettingsRendering(this);
		}
		if (m_DebugLighting == null)
		{
			m_DebugLighting = new DebugDisplaySettingsLighting(this);
		}
		if (m_DebugShadows == null)
		{
			m_DebugShadows = new DebugDisplaySettingsShadows(this);
		}
	}

	private void RegisterRenderGraph(DebugUI.Panel panel)
	{
		if (!(panel.displayName == "Rendering"))
		{
			return;
		}
		foreach (RenderGraph registeredRenderGraph in RenderGraph.GetRegisteredRenderGraphs())
		{
			registeredRenderGraph.RegisterDebug(panel);
		}
	}

	public void UnregisterDebug()
	{
		DebugManager instance = DebugManager.instance;
		foreach (IDebugDisplaySettingsPanelDisposable disposablePanel in m_DisposablePanels)
		{
			DebugUI.Widget[] widgets = disposablePanel.Widgets;
			string panelName = disposablePanel.PanelName;
			DebugUI.Panel panel = instance.GetPanel(panelName, createIfNull: true);
			UnregisterRenderGraph(panel);
			ObservableList<DebugUI.Widget> children = panel.children;
			disposablePanel.Dispose();
			children.Remove(widgets);
		}
		m_DisposablePanels = null;
		instance.UnregisterData(this);
	}

	private void UnregisterRenderGraph(DebugUI.Panel panel)
	{
		if (!(panel.displayName == "Rendering"))
		{
			return;
		}
		foreach (RenderGraph registeredRenderGraph in RenderGraph.GetRegisteredRenderGraphs())
		{
			registeredRenderGraph.UnRegisterDebug();
		}
	}

	internal bool DebugNeedsDebugDisplayKeyword()
	{
		if (m_DebugLighting.DebugLightingMode == DebugLightingMode.None && m_DebugRendering.OverdrawMode == DebugOverdrawMode.None && !m_DebugRendering.DebugMipMap)
		{
			return m_DebugRendering.DebugMaterialMode != DebugMaterialMode.None;
		}
		return true;
	}

	public Action GetReset()
	{
		return Reset;
	}
}

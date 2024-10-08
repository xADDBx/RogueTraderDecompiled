using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

public abstract class DebugDisplaySettingsPanel : IDebugDisplaySettingsPanelDisposable, IDebugDisplaySettingsPanel, IDisposable
{
	private readonly List<DebugUI.Widget> m_Widgets = new List<DebugUI.Widget>();

	public abstract string PanelName { get; }

	public DebugUI.Widget[] Widgets => m_Widgets.ToArray();

	protected void AddWidget(DebugUI.Widget widget)
	{
		m_Widgets.Add(widget);
	}

	public void Dispose()
	{
		m_Widgets.Clear();
	}
}

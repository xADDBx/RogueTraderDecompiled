using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Owlcat.Runtime.UI.Controls.Toggles;

[AddComponentMenu("UI/Owlcat/Owlcat Toggle Group", 31)]
[DisallowMultipleComponent]
public class OwlcatToggleGroup : MonoBehaviour
{
	[SerializeField]
	private bool m_AllowSwitchOff;

	[SerializeField]
	private OwlcatToggleGroupNavigation m_ConsoleNavigation;

	public readonly ReactiveProperty<OwlcatToggle> ActiveToggle = new ReactiveProperty<OwlcatToggle>();

	private readonly Dictionary<OwlcatToggle, IDisposable> m_Toggles = new Dictionary<OwlcatToggle, IDisposable>();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private OwlcatToggle FirstActiveToggle => ActiveToggles().FirstOrDefault();

	public bool AllowSwitchOff => m_AllowSwitchOff;

	private void Start()
	{
		EnsureValidState();
	}

	private void OnEnable()
	{
		EnsureValidState();
	}

	private void OnDisable()
	{
		foreach (IDisposable value in m_Toggles.Values)
		{
			value.Dispose();
		}
		m_Toggles.Clear();
		m_NavigationBehaviour?.Clear();
		m_NavigationBehaviour = null;
	}

	private void EnsureValidState()
	{
		if (!m_AllowSwitchOff && !AnyTogglesOn() && m_Toggles.Count != 0)
		{
			m_Toggles.Keys.First().Set(value: true);
		}
		IEnumerable<OwlcatToggle> enumerable = ActiveToggles();
		if (enumerable.Count() > 1)
		{
			OwlcatToggle firstActiveToggle = FirstActiveToggle;
			foreach (OwlcatToggle item in enumerable)
			{
				if (!(item == firstActiveToggle))
				{
					item.Set(value: false);
				}
			}
		}
		UpdateActiveToggle();
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		if (m_NavigationBehaviour != null)
		{
			return m_NavigationBehaviour;
		}
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		m_NavigationBehaviour.ContextName = "OwlcatToggleGroupNav";
		switch (m_ConsoleNavigation)
		{
		case OwlcatToggleGroupNavigation.Horizontal:
			m_NavigationBehaviour.SetEntitiesHorizontal(m_Toggles.Keys.OrderBy((OwlcatToggle x) => x.transform.GetSiblingIndex()).ToList());
			break;
		case OwlcatToggleGroupNavigation.Vertical:
			m_NavigationBehaviour.SetEntitiesVertical(m_Toggles.Keys.OrderBy((OwlcatToggle x) => x.transform.GetSiblingIndex()).ToList());
			break;
		}
		return m_NavigationBehaviour;
	}

	public bool AnyTogglesOn()
	{
		return m_Toggles.Keys.Any((OwlcatToggle x) => x.IsOn.Value);
	}

	public IEnumerable<OwlcatToggle> ActiveToggles()
	{
		return m_Toggles.Keys.Where((OwlcatToggle x) => x.IsOn.Value);
	}

	public void RegisterToggle(OwlcatToggle toggle)
	{
		if (!m_Toggles.ContainsKey(toggle))
		{
			m_Toggles[toggle] = toggle.IsOn.Subscribe(delegate
			{
				HandleToggleChanged(toggle);
			});
		}
	}

	public void UnregisterToggle(OwlcatToggle toggle)
	{
		if (m_Toggles.TryGetValue(toggle, out var value))
		{
			value.Dispose();
			m_Toggles.Remove(toggle);
		}
	}

	private void HandleToggleChanged(OwlcatToggle toggle)
	{
		if (toggle.IsOn.Value)
		{
			HandleToggleOn(toggle);
		}
		UpdateActiveToggle();
	}

	private void HandleToggleOn(OwlcatToggle currentToggle)
	{
		foreach (OwlcatToggle key in m_Toggles.Keys)
		{
			if (!(key == currentToggle))
			{
				key.Set(value: false);
			}
		}
	}

	private void UpdateActiveToggle()
	{
		ActiveToggle.Value = FirstActiveToggle;
	}
}

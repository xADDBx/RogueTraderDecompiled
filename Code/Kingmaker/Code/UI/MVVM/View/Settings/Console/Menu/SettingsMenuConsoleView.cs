using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Settings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.Menu;

public class SettingsMenuConsoleView : ViewBase<SettingsMenuConsoleVM>
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private SettingsMenuEntityConsoleView m_Prefab;

	[Header("Animator")]
	[SerializeField]
	private FadeAnimator m_Animator;

	private GridConsoleNavigationBehaviour m_NavigationBehavior;

	private List<SettingsMenuEntityConsoleView> m_Entities;

	private bool m_IsShowed;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_Animator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		DrawEntities();
		m_NavigationBehavior = new GridConsoleNavigationBehaviour();
		m_Entities = new List<SettingsMenuEntityConsoleView>();
		SettingsMenuEntityConsoleView entity = null;
		foreach (SettingsMenuEntityConsoleView entry in m_WidgetList.Entries)
		{
			m_Entities.Add(entry);
			if (entry.SettingsScreenType == Game.Instance.Player.UISettings.LastSettingsMenuType)
			{
				entity = entry;
			}
		}
		m_NavigationBehavior.SetEntitiesVertical(m_Entities);
		AddDisposable(GamePad.Instance.PushLayer(GetInputLayer()));
		m_NavigationBehavior.FocusOnEntityManual(entity);
		Show();
	}

	private void DrawEntities()
	{
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.MenuEntitiesVMList, m_Prefab));
	}

	private InputLayer GetInputLayer()
	{
		InputLayer inputLayer = m_NavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = "SettingsMenuConsoleViewInput"
		});
		AddDisposable(inputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9));
		return inputLayer;
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
		m_Entities.Clear();
		m_Entities = null;
	}

	private void Show()
	{
		if (!m_IsShowed)
		{
			m_IsShowed = true;
			m_Animator.AppearAnimation();
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Settings);
			});
		}
	}

	public void Hide()
	{
		if (m_IsShowed)
		{
			m_Animator.DisappearAnimation(delegate
			{
				base.gameObject.SetActive(value: false);
				m_IsShowed = false;
			});
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Settings);
			});
		}
	}
}

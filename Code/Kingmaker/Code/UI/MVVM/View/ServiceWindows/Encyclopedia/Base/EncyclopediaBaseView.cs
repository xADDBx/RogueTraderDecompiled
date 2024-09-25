using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.View.Pantograph;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Base;

public class EncyclopediaBaseView : ViewBase<EncyclopediaVM>, IEncyclopediaGlossaryModeHandler, ISubscriber, IInitializable
{
	[SerializeField]
	private PantographView m_PantographView;

	[SerializeField]
	[UsedImplicitly]
	protected EncyclopediaNavigationBaseView m_Navigation;

	[SerializeField]
	[UsedImplicitly]
	protected EncyclopediaPageBaseView m_Page;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	private UIMeinMenuTexts m_UIMeinMenuTexts;

	private bool m_IsShowed;

	public void Initialize()
	{
		m_Page.Initialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(m_PantographView.Show());
		m_UIMeinMenuTexts = UIStrings.Instance.MainMenu;
		m_Title.text = m_UIMeinMenuTexts.Encyclopedia;
		Show();
		m_Navigation.Bind(base.ViewModel.NavigationVM);
		AddDisposable(base.ViewModel.Page.Subscribe(m_Page.Bind));
		AddDisposable(EventBus.Subscribe(this));
	}

	private void Show()
	{
		if (!m_IsShowed)
		{
			m_IsShowed = true;
			UISounds.Instance.Sounds.LocalMap.MapOpen.Play();
			base.gameObject.SetActive(value: true);
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Encyclopedia);
			});
		}
	}

	private void Hide()
	{
		if (m_IsShowed)
		{
			m_IsShowed = false;
			UISounds.Instance.Sounds.LocalMap.MapClose.Play();
			base.gameObject.SetActive(value: false);
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Encyclopedia);
			});
		}
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	public void HandleGlossaryMode(bool state)
	{
		if (!state)
		{
			OnCloseGlossaryMode();
		}
	}

	protected virtual void OnCloseGlossaryMode()
	{
	}
}

using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.PagesMenu;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.SettingsUI;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.PagesMenu;

public class CharInfoPagesMenuPCView : ViewBase<SelectionGroupRadioVM<CharInfoPagesMenuEntityVM>>
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharInfoPagesMenuEntityPCView m_MenuItemViewPrefab;

	[Header("Lens")]
	[SerializeField]
	private Transform m_Lens;

	[SerializeField]
	private float m_LensInitialPositionX = -381f;

	[SerializeField]
	private float m_LensAnimationDuration = 0.55f;

	private readonly float m_LendDistanceThreshold = 0.01f;

	protected override void BindViewImplementation()
	{
		DrawEntities();
		AddDisposable(base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevTab.name, SelectPrev));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextTab.name, SelectNext));
	}

	protected override void DestroyViewImplementation()
	{
		m_WidgetList.Clear();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection.ToArray(), m_MenuItemViewPrefab);
		m_WidgetList.Entries.ForEach(delegate(IWidgetView e)
		{
			(e as CharInfoPagesMenuEntityPCView)?.SetupLens(m_Lens, m_LendDistanceThreshold, m_LensAnimationDuration);
		});
	}

	protected void SelectPrev()
	{
		base.ViewModel.SelectPrevValidEntity();
	}

	protected void SelectNext()
	{
		base.ViewModel.SelectNextValidEntity();
	}
}

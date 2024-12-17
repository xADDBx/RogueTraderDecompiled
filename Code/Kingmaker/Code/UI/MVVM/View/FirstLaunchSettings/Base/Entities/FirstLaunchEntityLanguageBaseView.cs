using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.PC.Entities;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings.Entities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base.Entities;

public abstract class FirstLaunchEntityLanguageBaseView<TFirstLaunchEntityLanguageItemView> : SettingsEntityWithValueView<FirstLaunchEntityLanguageVM> where TFirstLaunchEntityLanguageItemView : FirstLaunchEntityLanguageItemBaseView
{
	[SerializeField]
	private List<TFirstLaunchEntityLanguageItemView> m_ItemViews;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public void SetNavigationBehaviour(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		m_NavigationBehaviour = navigationBehaviour;
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		for (int i = 0; i < base.ViewModel.Items.Count; i++)
		{
			m_ItemViews[i]?.Bind(base.ViewModel.Items[i]);
		}
		for (int j = base.ViewModel.Items.Count; j < m_ItemViews.Count; j++)
		{
			m_ItemViews[j]?.gameObject.Or(null)?.SetActive(value: false);
		}
		BuildNavigation();
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		m_NavigationBehaviour.Clear();
	}

	public override void OnModificationChanged(string reason, bool allowed = true)
	{
	}

	public override bool HandleLeft()
	{
		return false;
	}

	public override bool HandleRight()
	{
		return false;
	}

	private void BuildNavigation()
	{
		for (int i = 0; i < m_ItemViews.Count; i++)
		{
			m_NavigationBehaviour.InsertVertical(i, m_ItemViews[i]);
		}
		foreach (IConsoleEntity entity in m_NavigationBehaviour.Entities.Where((IConsoleEntity e) => e is TFirstLaunchEntityLanguageItemView))
		{
			FirstLaunchEntityLanguageItemPCView firstLaunchEntityLanguageItemPCView = entity as FirstLaunchEntityLanguageItemPCView;
			if (!(firstLaunchEntityLanguageItemPCView == null))
			{
				AddDisposable(ObservableExtensions.Subscribe(firstLaunchEntityLanguageItemPCView.Or(null)?.EntityButton.OnLeftClickAsObservable(), delegate
				{
					m_NavigationBehaviour.FocusOnEntityManual(entity);
				}));
			}
		}
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_ItemViews.FirstOrDefault((TFirstLaunchEntityLanguageItemView item) => item.IsSelected));
		}, 1);
	}
}

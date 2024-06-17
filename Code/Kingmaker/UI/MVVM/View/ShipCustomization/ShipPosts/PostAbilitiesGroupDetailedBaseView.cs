using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.ShipPosts;

public class PostAbilitiesGroupDetailedBaseView : ViewBase<AbilitiesInfoGroupVM>
{
	[SerializeField]
	private PostAbilityDetailedBaseView m_PostAbilityDetailedBaseView;

	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private ScrollRectExtended m_RectExtended;

	private GridConsoleNavigationBehaviour m_Navigation;

	protected override void BindViewImplementation()
	{
		DrawEntities();
		AddDisposable(base.ViewModel.UpdateEventsCommand.Subscribe(DrawEntities));
		AddDisposable(m_Navigation?.DeepestFocusAsObservable.Subscribe(OnFocus));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.AllAbilities.ToArray(), m_PostAbilityDetailedBaseView);
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		m_Navigation?.SetEntitiesVertical(m_WidgetList.Entries.Select((IWidgetView e) => e as PostAbilityDetailedBaseView).ToList());
	}

	private void OnFocus(IConsoleEntity entity)
	{
		PostAbilityDetailedBaseView postAbilityDetailedBaseView = entity as PostAbilityDetailedBaseView;
		if ((bool)postAbilityDetailedBaseView)
		{
			RectTransform targetRect = postAbilityDetailedBaseView.transform as RectTransform;
			m_RectExtended.EnsureVisibleVertical(targetRect);
		}
	}

	public ConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		m_Navigation = new GridConsoleNavigationBehaviour();
		return m_Navigation;
	}
}

using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.Console;

public class PlayerStashConsoleView : PlayerStashView
{
	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_LootView.SlotsGroup.GetNavigation().DeepestFocusAsObservable.Subscribe(ForceScrollToObj));
	}

	public ConsoleNavigationBehaviour GetNavigation()
	{
		return m_LootView.SlotsGroup.GetNavigation();
	}

	private void ForceScrollToObj(IConsoleEntity entity)
	{
		if (entity != null)
		{
			LootSlotConsoleView lootSlotConsoleView = entity as LootSlotConsoleView;
			if ((bool)lootSlotConsoleView)
			{
				RectTransform targetRect = lootSlotConsoleView.transform as RectTransform;
				m_ScrollRect.EnsureVisibleVertical(targetRect);
			}
		}
	}
}

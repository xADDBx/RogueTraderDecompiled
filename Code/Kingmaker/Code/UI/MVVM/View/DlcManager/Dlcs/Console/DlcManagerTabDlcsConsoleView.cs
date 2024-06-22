using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Console;

public class DlcManagerTabDlcsConsoleView : DlcManagerTabDlcsBaseView
{
	[SerializeField]
	private DlcManagerTabDlcsDlcSelectorConsoleView m_DlcSelectorConsoleView;

	[SerializeField]
	private ConsoleHint m_ScrollStoryHint;

	[SerializeField]
	private ConsoleHint m_PurchaseHint;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DlcSelectorConsoleView.Bind(base.ViewModel.SelectionGroup);
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, ConsoleHint purchaseHint)
	{
		if (m_ScrollStoryHint != null)
		{
			AddDisposable(m_ScrollStoryHint.BindCustomAction(3, inputLayer, base.ViewModel.IsEnabled));
		}
		if (m_PurchaseHint != null)
		{
			AddDisposable(m_PurchaseHint.BindCustomAction(10, inputLayer, base.ViewModel.IsEnabled.And(base.ViewModel.DlcIsBought.Not()).And(base.ViewModel.DlcIsAvailableToPurchase).ToReactiveProperty()));
		}
		AddDisposable(purchaseHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.ShowInStore();
		}, 10, base.ViewModel.IsEnabled.And(base.ViewModel.DlcIsBought.Not()).And(base.ViewModel.DlcIsAvailableToPurchase).ToReactiveProperty())));
		purchaseHint.SetLabel(UIStrings.Instance.DlcManager.Purchase);
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		return m_DlcSelectorConsoleView.GetNavigationEntities();
	}

	protected override void UpdateDlcEntitiesImpl()
	{
		base.UpdateDlcEntitiesImpl();
		m_DlcSelectorConsoleView.UpdateDlcEntities();
	}
}

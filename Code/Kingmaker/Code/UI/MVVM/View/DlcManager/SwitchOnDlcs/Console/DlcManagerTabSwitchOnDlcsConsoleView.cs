using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.Console;

public class DlcManagerTabSwitchOnDlcsConsoleView : DlcManagerTabSwitchOnDlcsBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private DlcManagerTabSwitchOnDlcsDlcSelectorConsoleView m_DlcsSelectorConsoleView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DlcsSelectorConsoleView.Bind(base.ViewModel.SelectionGroup);
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_InfoView.ScrollRectExtended.OnSmoothlyScroll(pointerEventData);
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		return m_DlcsSelectorConsoleView.GetNavigationEntities();
	}
}

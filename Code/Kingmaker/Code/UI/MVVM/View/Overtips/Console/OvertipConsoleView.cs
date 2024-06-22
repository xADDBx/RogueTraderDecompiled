using System;
using Kingmaker.Code.UI.Common.PageNavigation;
using Kingmaker.Code.UI.MVVM.View.Surface.InputLayers;
using Kingmaker.UI.Pointer;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Console;

public class OvertipConsoleView : MonoBehaviour, IDisposable
{
	[SerializeField]
	private ConsoleHint m_Hint;

	[SerializeField]
	private TextMeshProUGUI m_HintLabel;

	[SerializeField]
	private PageNavigationConsole m_PageNavigation;

	private CompositeDisposable m_Disposable = new CompositeDisposable();

	public void SetConfirmPosition(float confirmHintAnchoredY)
	{
		if (!(m_HintLabel == null))
		{
			((RectTransform)m_HintLabel.transform).anchoredPosition = new Vector2(0f, confirmHintAnchoredY);
		}
	}

	public void SetPaginatorPosition(float paginatorAnchoredY)
	{
		((RectTransform)m_PageNavigation.transform).anchoredPosition = new Vector2(0f, paginatorAnchoredY);
	}

	public void SetConfirmHint(ReactiveProperty<bool> isActive, string label)
	{
		m_Disposable.Add(m_Hint.BindCustomAction(8, SurfaceMainInputLayer.Instance, isActive.And(ConsoleCursor.Instance.IsNotActiveProperty).ToReactiveProperty()));
		if (m_HintLabel != null)
		{
			m_Hint.SetLabel(label);
			m_HintLabel.autoSizeTextContainer = false;
			m_HintLabel.autoSizeTextContainer = true;
		}
	}

	public void SetPaginator(bool show, bool isChosen, int surroundingsCount = 0, int surroundingIndex = -1)
	{
		if (show && (!ConsoleCursor.Instance.IsActive || Game.Instance.Player.IsInCombat))
		{
			m_PageNavigation.Show(surroundingsCount, null, surroundingIndex);
			if (isChosen)
			{
				m_PageNavigation.AddInput();
			}
			else
			{
				m_PageNavigation.ClearInput();
			}
		}
		else
		{
			m_PageNavigation.Hide();
		}
	}

	public void Dispose()
	{
		m_Disposable.Clear();
		m_PageNavigation.Hide();
	}
}

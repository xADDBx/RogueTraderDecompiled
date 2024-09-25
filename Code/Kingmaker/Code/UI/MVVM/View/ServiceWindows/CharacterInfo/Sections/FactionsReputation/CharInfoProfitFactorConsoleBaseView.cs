using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.FactionsReputation;

public class CharInfoProfitFactorConsoleBaseView : CharInfoProfitFactorItemBaseView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasInputHandler
{
	[Header("Console")]
	[SerializeField]
	private OwlcatMultiSelectable m_FrameButton;

	[SerializeField]
	private ConsoleHint m_ScrollHint;

	private readonly BoolReactiveProperty m_IsFocused = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasScrollBar = new BoolReactiveProperty();

	public void Scroll(InputActionEventData data, float x)
	{
		if (!(m_ColoniesInfoScroll == null))
		{
			PointerEventData data2 = new PointerEventData(EventSystem.current)
			{
				scrollDelta = new Vector2(0f, x * m_ColoniesInfoScroll.scrollSensitivity)
			};
			m_ColoniesInfoScroll.OnSmoothlyScroll(data2);
		}
	}

	public void SetFocus(bool value)
	{
		m_IsFocused.Value = value;
		m_FrameButton.SetFocus(value);
		bool flag = m_ColoniesInfoScroll.content.sizeDelta.y > m_ColoniesInfoScroll.viewport.sizeDelta.y;
		m_HasScrollBar.Value = value && flag;
	}

	public bool IsValid()
	{
		return m_FrameButton.IsValid();
	}

	public bool CanConfirmClick()
	{
		return m_FrameButton.IsValid();
	}

	public void OnConfirmClick()
	{
		TooltipHelper.ShowInfo(Tooltip);
	}

	public string GetConfirmClickHint()
	{
		return UIStrings.Instance.ContextMenu.Information;
	}

	public void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		AddDisposable(inputLayer.AddAxis(Scroll, 3, m_IsFocused));
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
		}, 3, m_HasScrollBar);
		AddDisposable(m_ScrollHint.Bind(inputBindStruct));
		AddDisposable(inputBindStruct);
	}
}

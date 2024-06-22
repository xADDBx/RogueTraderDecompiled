using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog.Console;

public class DialogAnswerConsoleView : DialogAnswerBaseView
{
	[SerializeField]
	private Image m_ConsoleHint;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 20f;

	private readonly ReactiveProperty<bool> m_Focused = new ReactiveProperty<bool>();

	private readonly BoolReactiveProperty m_CanShowTooltip = new BoolReactiveProperty(initialValue: true);

	public bool HasTooltip => base.ViewModel.AnswerTooltip.Value != null;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ConsoleHint.sprite = ConsoleRoot.Instance.Icons.GetIcon(RewiredActionType.Confirm);
		SetTextFontSize(base.ViewModel.FontSizeMultiplier);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_CanShowTooltip.Dispose();
	}

	private void SetTextFontSize(float multiplier)
	{
		m_AnswerText.fontSize = m_DefaultConsoleFontSize * multiplier;
		(from x in m_Focused.Throttle(TimeSpan.FromSeconds(0.20000000298023224))
			where x
			where base.ViewModel != null && base.ViewModel.AnswerTooltip != null
			select x).Subscribe(UpdateHint).AddTo(this);
		m_Focused.Where((bool x) => !x).Subscribe(UpdateHint).AddTo(this);
	}

	protected override void OnFocusStateChanged(FocusState prev, FocusState curr)
	{
		base.OnFocusStateChanged(prev, curr);
		bool flag = curr == FocusState.Foreground;
		m_Focused.Value = flag;
		if (flag)
		{
			UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
		}
	}

	private void UpdateHint(bool visible)
	{
		if (visible && m_CanShowTooltip.Value)
		{
			this.ShowTooltip(base.ViewModel.AnswerTooltip.Value, TooltipConfig);
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
	}

	public override void UpdateTextSize(float multiplier)
	{
		SetTextFontSize(multiplier);
		base.UpdateTextSize(multiplier);
	}

	public void UpdateCanShowTooltip(bool canShow)
	{
		if (m_CanShowTooltip.Value != canShow)
		{
			m_CanShowTooltip.Value = canShow;
			UpdateHint(m_Focused.Value);
		}
	}
}

using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.EtudeCounter.Console;

public class EtudeCounterConsoleView : EtudeCounterView
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private ConsoleHint m_ConsoleHint;

	private bool m_HasEtude;

	private bool m_IsShowing;

	protected override void OnShow()
	{
		base.OnShow();
		m_HasEtude = true;
	}

	protected override void OnHide()
	{
		base.OnHide();
		m_HasEtude = false;
		TryToHide();
	}

	public void AddInput(InputLayer inputLayer)
	{
		AddDisposable(inputLayer.AddButton(delegate
		{
			TryToShow();
		}, 12));
		AddDisposable(inputLayer.AddButton(delegate
		{
			TryToHide();
		}, 12, InputActionEventType.ButtonJustReleased));
		AddDisposable(inputLayer.AddButton(delegate
		{
			TryToHide();
		}, 12, InputActionEventType.ButtonLongPressJustReleased));
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			base.ViewModel.ToggleExtraText();
		}, 10);
		AddDisposable(inputBindStruct);
		m_ConsoleHint.Bind(inputBindStruct);
	}

	private void TryToShow()
	{
		if (m_HasEtude && !m_IsShowing)
		{
			m_IsShowing = true;
			base.gameObject.SetActive(value: true);
			m_FadeAnimator.AppearAnimation();
			UISounds.Instance.Sounds.GreenMessageLine.GreenMessageLineShow.Play();
		}
	}

	private void TryToHide()
	{
		if (m_IsShowing)
		{
			m_IsShowing = false;
			m_FadeAnimator.DisappearAnimation();
			UISounds.Instance.Sounds.GreenMessageLine.GreenMessageLineHide.Play();
		}
	}
}

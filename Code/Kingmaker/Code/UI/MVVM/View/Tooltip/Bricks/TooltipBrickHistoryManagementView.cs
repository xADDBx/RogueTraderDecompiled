using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickHistoryManagementView : TooltipBaseBrickView<TooltipBrickHistoryManagementVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_TitleLabel;

	[SerializeField]
	protected OwlcatMultiButton m_PreviousButton;

	[SerializeField]
	protected OwlcatMultiButton m_NextButton;

	[SerializeField]
	private float m_DefaultFontSize = 23f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 23f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_TitleLabel.text = base.ViewModel.Title;
		AddDisposable(base.ViewModel.PreviousButtonInteractable.Subscribe(delegate(bool value)
		{
			m_PreviousButton.Interactable = value;
		}));
		AddDisposable(base.ViewModel.NextButtonInteractable.Subscribe(delegate(bool value)
		{
			m_NextButton.Interactable = value;
		}));
		AddDisposable(m_PreviousButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			if (base.ViewModel.PreviousButtonInteractable.Value)
			{
				base.ViewModel.OnPreviousButtonClick(null);
			}
		}));
		AddDisposable(m_NextButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			if (base.ViewModel.NextButtonInteractable.Value)
			{
				base.ViewModel.OnNextButtonClick(null);
			}
		}));
		m_TitleLabel.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
	}
}

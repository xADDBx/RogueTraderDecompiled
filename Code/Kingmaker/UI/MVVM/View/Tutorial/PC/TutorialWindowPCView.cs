using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common.DebugInformation;
using Kingmaker.UI.MVVM.VM.Tutorial;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tutorial.PC;

public abstract class TutorialWindowPCView<TViewModel> : TutorialWindowBaseView<TViewModel>, IHasBlueprintInfo where TViewModel : TutorialWindowVM
{
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	protected OwlcatButton m_ConfirmButton;

	[SerializeField]
	protected TextMeshProUGUI m_ConfirmButtonText;

	[SerializeField]
	private float m_TitleDefaultSize = 23f;

	[SerializeField]
	private float m_TriggerDefaultSize = 24f;

	[SerializeField]
	private float m_MainTextsDefaultSize = 21f;

	[SerializeField]
	private float m_ConfirmButtonDefaultSize = 20f;

	private UITutorial m_UITutorial;

	public BlueprintScriptableObject Blueprint => base.ViewModel?.Data?.Blueprint;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(UniRxExtensionMethods.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Hide();
		}));
		AddDisposable(m_DontShowToggle.OnPointerClickAsObservable().Subscribe(delegate
		{
			UISounds.Instance.Sounds.Tutorial.BanTutorialType.Play();
		}));
	}

	protected override void SetTextsSize(float multiplier)
	{
		m_Title.fontSize = m_TitleDefaultSize * multiplier;
		m_TriggerText.fontSize = m_TriggerDefaultSize * multiplier;
		m_TutorialText.fontSize = m_MainTextsDefaultSize * multiplier;
		m_SolutionText.fontSize = m_MainTextsDefaultSize * multiplier;
		m_ConfirmButtonText.fontSize = m_ConfirmButtonDefaultSize * multiplier;
		base.SetTextsSize(multiplier);
	}
}

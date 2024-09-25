using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Formation;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Formation.Base;

public class FormationBaseView : ViewBase<FormationVM>
{
	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_FormationLabel;

	[Header("Selector")]
	[SerializeField]
	[UsedImplicitly]
	private FormationSelectorPCView m_FormationSelectorPCView;

	[Header("Character")]
	[SerializeField]
	[UsedImplicitly]
	protected RectTransform m_CharacterContainer;

	[Header("Animator")]
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private TextMeshProUGUI m_UneditableFormationText;

	public void Initialize()
	{
		m_FadeAnimator.Initialize();
		m_FormationSelectorPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		Initialize();
		m_FadeAnimator.AppearAnimation();
		UISounds.Instance.Sounds.Formation.FormationOpen.Play();
		m_FormationSelectorPCView.Bind(base.ViewModel.FormationSelector);
		AddDisposable(base.ViewModel.SelectedFormationPresetIndex.Subscribe(OnFormationPresetIndexChanged));
		m_FormationLabel.text = UIStrings.Instance.FormationTexts.FormationLabel;
		m_UneditableFormationText.text = UIStrings.Instance.FormationTexts.UneditableFormation;
		Game.Instance.RequestPauseUi(isPaused: true);
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator.DisappearAnimation();
		UISounds.Instance.Sounds.Formation.FormationClose.Play();
		Game.Instance.RequestPauseUi(isPaused: false);
		m_FormationSelectorPCView.Unbind();
	}

	public virtual void OnFormationPresetIndexChanged(int formationPresetIndex)
	{
		OnFormationPresetChanged(formationPresetIndex);
		m_UneditableFormationText.gameObject.SetActive(!base.ViewModel.IsCustomFormation);
	}

	public void OnFormationPresetChanged(int formationPresetIndex)
	{
		float num = 0f;
		foreach (FormationCharacterVM character in base.ViewModel.Characters)
		{
			Vector3 localPosition = character.GetLocalPosition();
			if (localPosition.y < num)
			{
				num = localPosition.y;
			}
		}
		if (num < -185f)
		{
			float num2 = -185f / num;
			m_CharacterContainer.localScale = new Vector3(num2, num2, m_CharacterContainer.localScale.z);
		}
		else
		{
			m_CharacterContainer.localScale = Vector3.one;
		}
	}

	protected void OnSelectFormation()
	{
		base.ViewModel.FormationSelector.SelectNextValidEntity();
		base.ViewModel.FormationSelector.SelectedEntity.Value.SetSelectedFromView(state: true);
	}
}

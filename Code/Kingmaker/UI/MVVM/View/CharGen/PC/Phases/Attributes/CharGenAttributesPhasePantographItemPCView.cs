using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Attributes;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.PC.Phases.Attributes;

public class CharGenAttributesPhasePantographItemPCView : CharGenAttributesPhasePantographItemView
{
	[SerializeField]
	private OwlcatMultiButton m_MinusButton;

	[SerializeField]
	private OwlcatMultiButton m_PlusButton;

	private readonly ReactiveProperty<string> m_PlusButtonHint = new ReactiveProperty<string>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CheckCoopButtons(base.ViewModel.IsMainCharacter.Value);
		AddDisposable(m_MinusButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.RetreatStat();
		}));
		AddDisposable(m_PlusButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.AdvanceStat();
		}));
		AddDisposable(base.ViewModel.CanRetreat.Subscribe(m_MinusButton.SetInteractable));
		AddDisposable(m_PlusButton.SetHint(m_PlusButtonHint));
		AddDisposable(base.ViewModel.CanAdvance.Subscribe(delegate(bool value)
		{
			m_PlusButton.SetInteractable(value);
			m_PlusButtonHint.Value = (value ? string.Empty : ((string)UIStrings.Instance.CharGen.CannotAdvanceStatHint));
		}));
		AddDisposable(base.ViewModel.CheckCoopControls.Subscribe(CheckCoopButtons));
	}

	private void CheckCoopButtons(bool isMainCharacter)
	{
		m_MinusButton.gameObject.SetActive(isMainCharacter);
		m_PlusButton.gameObject.SetActive(isMainCharacter);
	}
}

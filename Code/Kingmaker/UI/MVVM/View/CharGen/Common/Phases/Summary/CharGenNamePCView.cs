using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.CharGen.PC.Phases;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Summary;

public class CharGenNamePCView : CharGenNameBaseView
{
	[SerializeField]
	private OwlcatButton m_SetNameButton;

	[SerializeField]
	private TextMeshProUGUI m_SetNameLabel;

	[SerializeField]
	private OwlcatButton m_SetRandomNameButton;

	public override void Initialize()
	{
		base.Initialize();
		(m_MessageBoxView as CharGenChangeNameMessageBoxPCView)?.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_SetNameLabel.text = UIStrings.Instance.CharGen.EditName;
		AddDisposable(m_SetNameButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ShowChangeNameMessageBox();
		}));
		AddDisposable(m_SetRandomNameButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			GenerateRandomName();
		}));
		AddDisposable(m_SetRandomNameButton.SetHint(UIStrings.Instance.CharGen.SetRandomName));
		CheckCoopButtons(base.ViewModel.IsMainCharacter.Value);
		AddDisposable(base.ViewModel.CheckCoopControls.Subscribe(CheckCoopButtons));
	}

	private void CheckCoopButtons(bool isMainCharacter)
	{
		m_SetNameButton.gameObject.SetActive(isMainCharacter);
		m_SetRandomNameButton.gameObject.SetActive(isMainCharacter);
	}
}

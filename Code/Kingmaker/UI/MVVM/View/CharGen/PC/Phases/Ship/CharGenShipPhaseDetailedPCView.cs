using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Ship;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.PC.Phases.Ship;

public class CharGenShipPhaseDetailedPCView : CharGenShipPhaseDetailedView
{
	[SerializeField]
	private OwlcatButton m_SetNameButton;

	[SerializeField]
	private TextMeshProUGUI m_SetNameButtonLabel;

	[SerializeField]
	private OwlcatButton m_SetRandomNameButton;

	[SerializeField]
	private TextMeshProUGUI m_SetRandomNameButtonLabel;

	[SerializeField]
	private CharGenChangeNameMessageBoxPCView m_MessageBoxPCView;

	public override void Initialize()
	{
		base.Initialize();
		m_SetNameButtonLabel.text = UIStrings.Instance.CharGen.EditNameButton;
		m_SetRandomNameButtonLabel.text = UIStrings.Instance.CharGen.SetRandomNameButton;
		m_MessageBoxPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.MessageBoxVM.Subscribe(m_MessageBoxPCView.Bind));
		AddDisposable(m_SetNameButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ShowChangeNameMessageBox();
		}));
		AddDisposable(m_SetRandomNameButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			GenerateRandomName();
		}));
		AddDisposable(m_SetRandomNameButton.SetHint(UIStrings.Instance.CharGen.SetRandomName));
	}
}

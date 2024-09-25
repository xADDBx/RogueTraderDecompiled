using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.BugReport.PC;

public class BugReportDuplicatesPCView : BugReportDuplicatesBaseView
{
	[Header("PC Buttons")]
	[SerializeField]
	private OwlcatMultiButton m_BackButton;

	protected override void CreateInput()
	{
		base.CreateInput();
		AddDisposable(m_BackButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Close));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close));
	}
}

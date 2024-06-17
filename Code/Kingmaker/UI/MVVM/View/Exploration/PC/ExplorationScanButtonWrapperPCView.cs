using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.PC;

public class ExplorationScanButtonWrapperPCView : ExplorationComponentWrapperBaseView<ExplorationScanButtonWrapperVM>
{
	[SerializeField]
	private OwlcatButton m_ScanButton;

	[SerializeField]
	private TextMeshProUGUI m_ScanButtonLabel;

	public override IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return null;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_ScanButton.OnLeftClickAsObservable().Subscribe(OnScanClick));
		AddDisposable(m_ScanButton.OnConfirmClickAsObservable().Subscribe(OnScanClick));
	}

	public void Initialize()
	{
		m_ScanButtonLabel.text = UIStrings.Instance.ExplorationTexts.ExploBeginScan;
	}

	public void SetButtonInteractable(bool value)
	{
		m_ScanButton.Interactable = value;
	}

	private void OnScanClick()
	{
		base.ViewModel.Interact();
	}
}

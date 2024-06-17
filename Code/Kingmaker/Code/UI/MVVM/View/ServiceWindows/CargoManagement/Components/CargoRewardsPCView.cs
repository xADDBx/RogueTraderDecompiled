using Kingmaker.Blueprints.Root.Strings;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public class CargoRewardsPCView : CargoRewardsBaseView
{
	[Header("PC")]
	[SerializeField]
	private OwlcatButton m_CompleteButton;

	[SerializeField]
	private TextMeshProUGUI m_CompleteButtonLabel;

	protected override void InitializeImpl()
	{
		m_CompleteButtonLabel.text = UIStrings.Instance.CommonTexts.Accept;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_CompleteButton.OnLeftClickAsObservable().Subscribe(base.HandleComplete));
	}
}

using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Mods.Base;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Mods.PC;

public class DlcManagerModEntityPCView : DlcManagerModEntityBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private OwlcatButton m_ModSettingsButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ModSettingsButton.gameObject.SetActive(base.ViewModel.ModSettingsAvailable.Value);
		if (base.ViewModel.ModSettingsAvailable.Value)
		{
			AddDisposable(ObservableExtensions.Subscribe(m_ModSettingsButton.OnLeftClickAsObservable(), delegate
			{
				OpenSettings();
			}));
			AddDisposable(m_ModSettingsButton.SetHint(UIStrings.Instance.DlcManager.ModSettings));
		}
		AddDisposable(m_MultiButton.OnPointerClickAsObservable().Subscribe(delegate
		{
			SwitchValue();
		}));
	}
}

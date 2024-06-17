using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.VisualSettings;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.PC;

public class CharacterVisualSettingsPCView : CharacterVisualSettingsView<CharacterVisualSettingsEntityPCView>
{
	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_Close;

	[SerializeField]
	private OwlcatButton m_Complete;

	[SerializeField]
	private TextMeshProUGUI m_CompleteLabel;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		UISounds.Instance.SetClickAndHoverSound(m_Close, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(m_Close.OnLeftClickAsObservable().Subscribe(delegate
		{
			Close();
		}));
		AddDisposable(m_Complete.OnLeftClickAsObservable().Subscribe(delegate
		{
			Close();
		}));
		m_CompleteLabel.text = UIStrings.Instance.CharGen.Complete;
	}

	public void Close()
	{
		base.ViewModel.Close();
	}
}

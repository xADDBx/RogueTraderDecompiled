using Kingmaker.Localization;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.VisualSettings;

public abstract class CharacterVisualSettingsEntityView : ViewBase<CharacterVisualSettingsEntityVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	protected OwlcatMultiButton m_Button;

	private bool m_IsInit;

	private LocalizedString m_LocalizedLabel;

	public void Initialize(LocalizedString label)
	{
		if (!m_IsInit)
		{
			m_LocalizedLabel = label;
			base.gameObject.SetActive(value: false);
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		m_Label.text = m_LocalizedLabel.Text;
		base.gameObject.SetActive(value: true);
		AddDisposable(base.ViewModel.Locked.Subscribe(delegate(bool value)
		{
			m_Button.Interactable = !value;
		}));
		AddDisposable(base.ViewModel.IsOn.Subscribe(delegate(bool value)
		{
			m_Button.SetActiveLayer((!value) ? 1 : 0);
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}
}

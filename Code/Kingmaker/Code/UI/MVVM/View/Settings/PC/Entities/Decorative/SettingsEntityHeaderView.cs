using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Decorative;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities.Decorative;

public class SettingsEntityHeaderView : VirtualListElementViewBase<SettingsEntityHeaderVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Tittle;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutElementSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutElementSettings;

	protected override void BindViewImplementation()
	{
		m_Tittle.text = base.ViewModel.Tittle;
		AddDisposable(base.ViewModel.LanguageChanged.Subscribe(delegate
		{
			m_Tittle.text = base.ViewModel.Tittle;
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}

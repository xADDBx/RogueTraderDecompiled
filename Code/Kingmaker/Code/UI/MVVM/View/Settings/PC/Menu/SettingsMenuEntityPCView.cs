using Kingmaker.Code.UI.MVVM.VM.Settings.Menu;
using Owlcat.Runtime.UI.SelectionGroup.View;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC.Menu;

public class SettingsMenuEntityPCView : SelectionGroupEntityView<SettingsMenuEntityVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.Title.Subscribe(delegate(string t)
		{
			m_Title.text = t;
		}));
	}
}

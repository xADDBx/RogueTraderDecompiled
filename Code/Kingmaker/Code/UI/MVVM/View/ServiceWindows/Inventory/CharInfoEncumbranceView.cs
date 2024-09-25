using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;

public class CharInfoEncumbranceView : CharInfoComponentView<CharInfoEncumbranceVM>
{
	[SerializeField]
	private EncumbranceView m_EncumbranceView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_EncumbranceView.Bind(base.ViewModel.EncumbranceVm);
		AddDisposable(m_EncumbranceView.SetTooltip(base.ViewModel.Tooltip));
	}
}

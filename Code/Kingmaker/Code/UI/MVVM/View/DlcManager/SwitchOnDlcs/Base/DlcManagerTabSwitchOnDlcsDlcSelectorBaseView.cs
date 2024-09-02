using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.SwitchOnDlcs;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.Base;

public class DlcManagerTabSwitchOnDlcsDlcSelectorBaseView : ViewBase<SelectionGroupRadioVM<DlcManagerSwitchOnDlcEntityVM>>
{
	[SerializeField]
	[UsedImplicitly]
	protected WidgetListMVVM m_WidgetList;

	protected override void BindViewImplementation()
	{
	}

	protected override void DestroyViewImplementation()
	{
	}
}

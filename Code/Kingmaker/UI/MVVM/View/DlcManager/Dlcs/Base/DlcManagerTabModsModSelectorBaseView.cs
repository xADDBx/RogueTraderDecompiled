using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.Mods;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.DlcManager.Dlcs.Base;

public class DlcManagerTabModsModSelectorBaseView : ViewBase<SelectionGroupRadioVM<DlcManagerModEntityVM>>
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

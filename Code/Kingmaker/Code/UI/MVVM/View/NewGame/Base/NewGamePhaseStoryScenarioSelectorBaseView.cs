using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.NewGame.Story;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Base;

public class NewGamePhaseStoryScenarioSelectorBaseView : ViewBase<SelectionGroupRadioVM<NewGamePhaseStoryScenarioEntityVM>>
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

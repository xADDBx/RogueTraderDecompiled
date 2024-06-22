using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Portrait;

public class CharGenPortraitTabView : SelectionGroupEntityView<CharGenPortraitTabVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Label.text = UIUtility.GetCharGenPortraitTabLabel(base.ViewModel.Tab);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharGenPortraitTabVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharGenPortraitTabVM;
	}

	public override void OnChangeSelectedState(bool value)
	{
		base.OnChangeSelectedState(value && base.ViewModel.IsMainCharacter.Value);
		m_Button.CanConfirm = base.ViewModel.IsMainCharacter.Value;
	}
}

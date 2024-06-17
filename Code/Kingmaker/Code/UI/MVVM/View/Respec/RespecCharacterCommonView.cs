using Kingmaker.Code.UI.MVVM.VM.Retrain;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Respec;

public class RespecCharacterCommonView : SelectionGroupEntityView<RespecCharacterVM>, IWidgetView
{
	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private TextMeshProUGUI m_CharacterName;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Image.sprite = base.ViewModel.UnitPortraitPartVM.Portrait.Value;
		m_CharacterName.text = base.ViewModel.CharacterName;
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as RespecCharacterVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is RespecCharacterVM;
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		base.ViewModel.SetSelectedFromView(value);
	}
}

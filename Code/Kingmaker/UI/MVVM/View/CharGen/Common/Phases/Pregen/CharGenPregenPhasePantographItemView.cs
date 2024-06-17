using Kingmaker.UI.MVVM.VM.CharGen.Phases.Pregen;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Pregen;

public class CharGenPregenPhasePantographItemView : SelectionGroupEntityView<CharGenPregenSelectorItemVM>, IWidgetView
{
	[SerializeField]
	private Image m_Portrait;

	[SerializeField]
	private ScrambledTMP m_DisplayName;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DisplayName.SetText(string.Empty, base.ViewModel.CharacterName.Value);
		m_Portrait.sprite = base.ViewModel.Portrait.Value;
		m_Portrait.gameObject.SetActive(base.ViewModel.Portrait.Value != null);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharGenPregenSelectorItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharGenPregenSelectorItemVM;
	}
}

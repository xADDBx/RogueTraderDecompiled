using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.PC.CareerPathProgression;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipSkillsPCView : ShipSkillsBaseView<ShipCareerPathSelectionTabsPCView>
{
	[SerializeField]
	private CareerButtonsBlock m_ButtonsBlock;

	public override void Initialize()
	{
		base.Initialize();
		m_ShipCareerPathSelectionTabsPCView.SetButtonsBlock(m_ButtonsBlock);
		m_ShipCareerPathSelectionTabsPCView.Initialize();
		m_ButtonsBlock.SetActive(state: false);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ShipCareerPathSelectionTabsPCView.Bind(base.ViewModel.ShipProgressionVM?.CareerPathVM);
		m_ButtonsBlock.SetActive(base.ViewModel.ShipProgressionVM?.CareerPathVM.IsInLevelupProcess ?? false);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_ButtonsBlock.SetActive(state: false);
	}
}

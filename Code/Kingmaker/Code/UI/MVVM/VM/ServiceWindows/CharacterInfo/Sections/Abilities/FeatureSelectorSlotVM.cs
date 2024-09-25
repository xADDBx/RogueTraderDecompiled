using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;

public class FeatureSelectorSlotVM : CharInfoFeatureVM
{
	public FeatureSelectorSlotVM(Ability ability, MechanicEntity unit)
		: base(ability, unit)
	{
		m_Tooltip = new ReactiveProperty<TooltipBaseTemplate>();
		m_Tooltip.Value = new TooltipTemplateAbility(ability.Data, isScreenWindowTooltip: true);
	}
}

using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickArmorStatsVM : TooltipBaseBrickVM
{
	public BaseUnitEntity Unit;

	public string ArmorDeflection;

	public string ArmorAbsorption;

	public string Dodge;

	public TooltipBaseTemplate DodgeTooltip;

	public TooltipBrickArmorStatsVM(BaseUnitEntity unit)
	{
		Unit = unit;
		int resultDeflection = Rulebook.Trigger(new RuleCalculateStatsArmor((UnitEntity)Unit)).ResultDeflection;
		int resultAbsorption = Rulebook.Trigger(new RuleCalculateStatsArmor((UnitEntity)Unit)).ResultAbsorption;
		ArmorDeflection = resultDeflection.ToString();
		ArmorAbsorption = $"{resultAbsorption}%";
		RuleCalculateDodgeChance ruleCalculateDodgeChance = Rulebook.Trigger(new RuleCalculateDodgeChance((UnitEntity)Unit));
		Dodge = ruleCalculateDodgeChance.Result.ToString();
		DodgeTooltip = new TooltipTemplateDodge(ruleCalculateDodgeChance);
	}
}

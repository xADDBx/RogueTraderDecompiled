using Kingmaker.Blueprints.Root;
using Kingmaker.Globalmap.Colonization.Requirements;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Requirements;

public class RequirementCargoUI : RequirementUI<RequirementCargo>
{
	public override string Name => base.Requirement.Cargo.Name;

	public override string Description => string.Empty;

	public override Sprite Icon => BlueprintRoot.Instance.UIConfig.UIIcons.CargoTooltipIcons.GetIconByOrigin(base.Requirement.Cargo.OriginType);

	public override string NameForAcronym => null;

	public override Color? IconColor => Color.white;

	public override string CountText => base.Requirement.Count + "%";

	public RequirementCargoUI(RequirementCargo requirement)
		: base(requirement)
	{
	}
}

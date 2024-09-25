using Kingmaker.Globalmap.Colonization.Requirements;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Requirements;

public class RequirementScrapUI : RequirementUI<RequirementScrap>
{
	public override string Name => string.Empty;

	public override string Description => string.Empty;

	public override Sprite Icon => null;

	public override string NameForAcronym => null;

	public override string CountText => base.Requirement.Scrap.ToString();

	public RequirementScrapUI(RequirementScrap requirement)
		: base(requirement)
	{
	}
}

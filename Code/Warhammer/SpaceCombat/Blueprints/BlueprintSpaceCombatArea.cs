using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.GameModes;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("61e2c1e69c5ae5946b86db4b7e880eb9")]
public class BlueprintSpaceCombatArea : BlueprintArea
{
	public int AdditionalExperience;

	public override BlueprintCameraSettings CameraSettings => BlueprintRoot.Instance.CameraRoot.SpaceCombatSettings;

	public override GameModeType AreaStatGameMode => GameModeType.SpaceCombat;

	public override IEnumerable<string> GetActiveSoundBankNames(bool isCurrentPart = false)
	{
		return base.GetActiveSoundBankNames(isCurrentPart).Concat(new string[2] { "SpaceCombat", "AMB_SpaceCombat" });
	}
}

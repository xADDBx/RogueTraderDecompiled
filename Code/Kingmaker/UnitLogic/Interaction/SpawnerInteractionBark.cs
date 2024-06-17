using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using UnityEngine;

namespace Kingmaker.UnitLogic.Interaction;

public class SpawnerInteractionBark : SpawnerInteraction
{
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark, GetNameFromAsset = true)]
	public SharedStringAsset Bark;

	[Tooltip("Show bark on user. By default bark is shown on target unit.")]
	public bool ShowOnUser;

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		if (Bark == null)
		{
			return AbstractUnitCommand.ResultType.Success;
		}
		BarkPlayer.Bark(ShowOnUser ? user : target, Bark.String, -1f, playVoiceOver: false, user);
		return AbstractUnitCommand.ResultType.Success;
	}
}

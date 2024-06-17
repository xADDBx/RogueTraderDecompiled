using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("814e3fb45988498d99851f7e7f0634ee")]
public class TutorialTriggerVendorWindow : TutorialTrigger, IDialogAnswersAddedToPoolHandler, ISubscriber, IHashable
{
	public void HandleDialogAnswersAddedToPool(BlueprintAnswer answer)
	{
		if (answer.OnSelect.Actions.HasItem((GameAction i) => i is StartTrade))
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = GameHelper.GetPlayerCharacter();
			});
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

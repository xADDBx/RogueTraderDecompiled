using System.Collections.Generic;
using System.Linq;
using Kingmaker.BarkBanters;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class DebugInteractionPlayBanterPart : InteractionPart<DebugInteractionPlayBanterSettings>, IHashable
{
	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Action;
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		BlueprintBarkBanter blueprintBarkBanter = base.Settings.BarkBanter.Get();
		if (blueprintBarkBanter != null)
		{
			List<LocalizedString> list = new List<LocalizedString>(16);
			list.AddRange(blueprintBarkBanter.FirstPhrase);
			list.AddRange(blueprintBarkBanter.Responses.Select((BlueprintBarkBanter.BanterResponseEntry r) => r.Response));
			DebugInteractionPlayBanter component = base.Owner.View.gameObject.GetComponent<DebugInteractionPlayBanter>();
			if (component != null)
			{
				component.PlayBarks(list);
			}
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

using System.Collections;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Localization;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[DisallowMultipleComponent]
[KnowledgeDatabaseID("10baf550291b46e295909d2b71204e0f")]
public class DebugInteractionPlayBanter : InteractionComponent<DebugInteractionPlayBanterPart, DebugInteractionPlayBanterSettings>
{
	private Entity entity;

	public void PlayBarks(IEnumerable<LocalizedString> barks)
	{
		StartCoroutine(PlayBarksOneByOne(barks));
	}

	private IEnumerator PlayBarksOneByOne(IEnumerable<LocalizedString> barks)
	{
		if (entity == null)
		{
			entity = base.gameObject.GetComponent<EntityViewBase>()?.Data.ToEntity();
		}
		if (entity == null)
		{
			yield break;
		}
		foreach (LocalizedString bark in barks)
		{
			IBarkHandle barkHandle = BarkPlayer.Bark(entity, bark, -1f, playVoiceOver: true);
			if (barkHandle != null)
			{
				while (barkHandle.IsPlayingBark())
				{
					yield return new WaitForSeconds(0.1f);
				}
				yield return new WaitForSeconds(1f);
			}
		}
	}
}

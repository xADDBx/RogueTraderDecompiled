using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.UnitLogic.Interaction;

[KnowledgeDatabaseID("50ac711133aea7343ae4091a29784c2d")]
public class SpawnerInteractionBark : SpawnerInteraction
{
	[SerializeField]
	private bool UseRandomBark;

	[SerializeField]
	[HideIf("UseRandomBark")]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	private SharedStringAsset? Bark;

	[SerializeField]
	[ShowIf("UseRandomBark")]
	private bool DoNotRepeatLastBark;

	[SerializeField]
	[ShowIf("UseRandomBark")]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	private SharedStringAsset[] RandomBarks = new SharedStringAsset[0];

	[NonSerialized]
	private int _lastRandomBarkIdx = -1;

	[Tooltip("Show bark on user. By default bark is shown on target unit.")]
	public bool ShowOnUser;

	[SerializeField]
	private bool m_PlayVoiceOver = true;

	private SharedStringAsset? GetBark()
	{
		if (UseRandomBark)
		{
			int nextRandomIdx = InteractionHelper.GetNextRandomIdx(RandomBarks.Length, DoNotRepeatLastBark, ref _lastRandomBarkIdx);
			if (nextRandomIdx >= 0)
			{
				return RandomBarks[nextRandomIdx];
			}
			return null;
		}
		return Bark;
	}

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		SharedStringAsset bark = GetBark();
		if (bark == null)
		{
			return AbstractUnitCommand.ResultType.Success;
		}
		BarkPlayer.Bark(ShowOnUser ? user : target, bark.String, -1f, m_PlayVoiceOver, user);
		return AbstractUnitCommand.ResultType.Success;
	}
}

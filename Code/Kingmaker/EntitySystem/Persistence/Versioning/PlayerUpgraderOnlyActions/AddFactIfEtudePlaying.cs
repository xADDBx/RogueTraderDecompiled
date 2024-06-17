using System;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[TypeId("efe5c87cd9de4a219fa05290154c87ad")]
public class AddFactIfEtudePlaying : PlayerUpgraderOnlyAction
{
	private enum TargetType
	{
		MainCharacter,
		AllCompanions
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintEtudeReference m_Etude;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference m_Fact;

	[SerializeField]
	private TargetType m_Target;

	public BlueprintEtude Etude => m_Etude;

	public BlueprintUnitFact Fact => m_Fact;

	public override string GetCaption()
	{
		return $"Add {Fact} if {Etude} playing to {m_Target}";
	}

	protected override void RunActionOverride()
	{
		Etude etude = Game.Instance.Player.EtudesSystem.Etudes.Get(Etude);
		if (etude == null || !etude.IsPlaying)
		{
			return;
		}
		switch (m_Target)
		{
		case TargetType.MainCharacter:
			Game.Instance.Player.MainCharacter.Get<BaseUnitEntity>().AddFact(Fact);
			break;
		case TargetType.AllCompanions:
		{
			foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
			{
				allCharacter.AddFact(Fact);
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}

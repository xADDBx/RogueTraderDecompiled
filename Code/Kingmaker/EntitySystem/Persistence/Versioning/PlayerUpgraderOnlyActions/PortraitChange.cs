using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[TypeId("5484641657df4e3db7df3bddfc068102")]
public class PortraitChange : PlayerUpgraderOnlyAction
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitReference m_Unit;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintPortraitReference m_Portrait;

	public override string GetCaption()
	{
		return "Replace default portrait for unit";
	}

	protected override void RunActionOverride()
	{
		BlueprintScriptableObject blueprint = m_Unit.GetBlueprint();
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			if (m_Unit != null && (blueprint == allCharacter.Blueprint || blueprint == allCharacter.OriginalBlueprint))
			{
				allCharacter.UISettings.SetPortrait(m_Portrait);
			}
		}
	}
}

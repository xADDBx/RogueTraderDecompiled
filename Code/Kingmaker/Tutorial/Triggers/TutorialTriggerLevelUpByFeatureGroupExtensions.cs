using Kingmaker.UnitLogic.Levelup.Selections;

namespace Kingmaker.Tutorial.Triggers;

public static class TutorialTriggerLevelUpByFeatureGroupExtensions
{
	public static TutorialTriggerLevelUpByFeatureGroup.StarshipFeatureGroup ToStarshipFeatureGroup(this FeatureGroup? group)
	{
		return group switch
		{
			FeatureGroup.ShipUltimateAbility => TutorialTriggerLevelUpByFeatureGroup.StarshipFeatureGroup.UltimateAbility, 
			FeatureGroup.ShipActiveAbility => TutorialTriggerLevelUpByFeatureGroup.StarshipFeatureGroup.ActiveAbility, 
			FeatureGroup.AdvancedAbility => TutorialTriggerLevelUpByFeatureGroup.StarshipFeatureGroup.AdvancedAbility, 
			FeatureGroup.ShipUpgrade => TutorialTriggerLevelUpByFeatureGroup.StarshipFeatureGroup.ShipUpgrade, 
			_ => (TutorialTriggerLevelUpByFeatureGroup.StarshipFeatureGroup)0, 
		};
	}
}

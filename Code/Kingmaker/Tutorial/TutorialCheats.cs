using Core.Cheats;
using Kingmaker.Cheats;
using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.Tutorial;

public static class TutorialCheats
{
	[Cheat(Name = "tutorial_start")]
	public static string ShowTutorial(string blueprint)
	{
		BlueprintTutorial blueprint2 = Utilities.GetBlueprint<BlueprintTutorial>(blueprint);
		if (blueprint2 == null)
		{
			return "Can't find blueprint named " + blueprint;
		}
		using (ContextData<TutorialContext>.Request())
		{
			Game.Instance.Player.Tutorial.Trigger(blueprint2, null);
		}
		return "Show tutorial";
	}
}

using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("728a1e0227a34685bc58b7486ecdc249")]
public class ChangeableDynamicIsLoaded : Condition
{
	public SceneReference Scene;

	protected override string GetConditionCaption()
	{
		return $"Dynamic [{Scene.SceneName}] Is Loaded";
	}

	protected override bool CheckCondition()
	{
		if (Game.Instance.State.LoadedAreaState.Area.Blueprint.GetActiveDynamicScenes().Any((SceneReference s) => s.SceneName == Scene.SceneName))
		{
			return true;
		}
		return false;
	}
}

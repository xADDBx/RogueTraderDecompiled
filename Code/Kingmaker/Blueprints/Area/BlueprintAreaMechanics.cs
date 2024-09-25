using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Sound;
using UnityEngine.SceneManagement;

namespace Kingmaker.Blueprints.Area;

[TypeId("c542bb267f6d4651af99d4c5b3a0df9a")]
public class BlueprintAreaMechanics : BlueprintScriptableObject
{
	public BlueprintAreaReference Area;

	public SceneReference Scene;

	public AkBankReference AdditionalDataBank;

	public bool IsSceneLoadedNow()
	{
		return SceneManager.GetSceneByName(Scene.SceneName).isLoaded;
	}
}

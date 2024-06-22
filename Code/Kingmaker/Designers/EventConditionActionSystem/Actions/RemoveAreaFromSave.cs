using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("3dce040d78a64de781b45c12bcc46a68")]
public class RemoveAreaFromSave : GameAction
{
	[SerializeField]
	[FormerlySerializedAs("Area")]
	private BlueprintAreaReference m_Area;

	public BlueprintAreaMechanicsReference SpecificMechanic;

	public BlueprintArea Area => m_Area?.Get();

	public override string GetDescription()
	{
		return "Помечает арию или ее отделюную механику как пройденную. Данные этой арии удаляются из сохранения.\nЕсли помечаете какую-то арию, убедитесь, что игрок не может в нее вернуться после пометки";
	}

	public override string GetCaption()
	{
		return "Mark area finished: " + Area.NameSafe() + " " + SpecificMechanic.Get().NameSafe();
	}

	protected override void RunAction()
	{
		AreaPersistentState areaPersistentState = Game.Instance.State.SavedAreaStates.FirstOrDefault((AreaPersistentState a) => a.Blueprint == Area);
		if (areaPersistentState == null)
		{
			return;
		}
		BlueprintAreaMechanics blueprintAreaMechanics = SpecificMechanic.Get();
		SceneEntitiesState sceneEntitiesState = ((blueprintAreaMechanics != null && blueprintAreaMechanics.Scene.IsDefined) ? areaPersistentState.GetAdditionalSceneStates().FirstOrDefault((SceneEntitiesState s) => s.SceneName == SpecificMechanic.Get().Scene.SceneName) : areaPersistentState.MainState);
		if (sceneEntitiesState != null)
		{
			if (areaPersistentState == Game.Instance.State.LoadedAreaState)
			{
				sceneEntitiesState.SkipSerialize = true;
			}
			else if (sceneEntitiesState != areaPersistentState.MainState)
			{
				areaPersistentState.GetAdditionalSceneStates().Remove(sceneEntitiesState);
			}
			else
			{
				Game.Instance.State.SavedAreaStates.Remove(areaPersistentState);
			}
		}
	}
}

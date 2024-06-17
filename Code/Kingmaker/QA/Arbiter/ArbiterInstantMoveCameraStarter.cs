using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Persistence.Scenes;
using UnityEngine;

namespace Kingmaker.QA.Arbiter;

public class ArbiterInstantMoveCameraStarter
{
	private string GetCurrentAreaName()
	{
		if (Application.isPlaying)
		{
			return Game.Instance?.CurrentlyLoadedArea.NameSafe();
		}
		return string.Empty;
	}

	public void AddArbiterMoveCameraTask(string instruction, int id)
	{
		AreaCheckerComponent areaCheckerComponent = ArbiterInstructionIndex.Instance.GetInstruction(instruction).Test as AreaCheckerComponent;
		ArbiterPoint pointById = areaCheckerComponent.GetPointById(id);
		string text = areaCheckerComponent.Area.NameSafe();
		string text2 = areaCheckerComponent.GetEnterPointById(id).Guid.ToString();
		string text3 = "instant_move_camera " + text + " " + text2 + " " + pointById.Position.x + " " + pointById.Position.y + " " + pointById.Position.z + " " + pointById.Rotation + " " + pointById.Zoom;
		string currentAreaName = GetCurrentAreaName();
		if (!Application.isPlaying)
		{
			MoveInEditor(areaCheckerComponent.Area, pointById.Position.x, pointById.Position.y, pointById.Position.z, pointById.Rotation, pointById.Zoom);
		}
		else if (Arbiter.IsInitialized)
		{
			if (IsOpenedOpenSceneWarning(currentAreaName, text))
			{
				Arbiter.Instance.RunInstruction(text3);
			}
		}
		else if (IsOpenedOpenSceneWarning(currentAreaName, text))
		{
			PlayerPrefs.SetString("ArbiterInstruction", text3);
			PlayerPrefs.Save();
			BundledSceneLoader.LoadScene("Arbiter");
		}
	}

	public void AddSimpleMoveCameraTask(string areaName, string areaEnterPointName, float x, float y, float z, float rotation, float zoom)
	{
		string text = "instant_move_camera " + areaName + " " + areaEnterPointName + " " + x + " " + y + " " + z + " " + rotation + " " + zoom;
		string currentAreaName = GetCurrentAreaName();
		if (!Application.isPlaying)
		{
			BlueprintArea blueprint = Utilities.GetBlueprint<BlueprintArea>(areaName);
			MoveInEditor(blueprint, x, y, z, rotation, zoom);
		}
		else if (Arbiter.IsInitialized)
		{
			if (IsOpenedOpenSceneWarning(currentAreaName, areaName))
			{
				Arbiter.Instance.RunInstruction(text);
			}
		}
		else if (IsOpenedOpenSceneWarning(currentAreaName, areaName))
		{
			PlayerPrefs.SetString("ArbiterInstruction", text);
			PlayerPrefs.Save();
			BundledSceneLoader.LoadScene("Arbiter");
		}
	}

	public void MoveInEditor(BlueprintArea blueprint, float x, float y, float z, float rotation, float zoom)
	{
	}

	private bool IsOpenedOpenSceneWarning(string currentSceneName, string openSceneName)
	{
		try
		{
			return true;
		}
		catch
		{
			return false;
		}
	}
}

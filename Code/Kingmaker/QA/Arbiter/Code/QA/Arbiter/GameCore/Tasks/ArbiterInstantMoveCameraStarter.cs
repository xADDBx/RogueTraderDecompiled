using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Cheats;
using Kingmaker.QA.Arbiter.GameCore.AreaChecker;
using Kingmaker.QA.Arbiter.Service;
using UnityEngine;

namespace Kingmaker.QA.Arbiter.Code.QA.Arbiter.GameCore.Tasks;

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
		AreaCheckerComponent areaCheckerComponent = BlueprintArbiterInstructionIndex.Instance.GetInstruction(instruction).Test as AreaCheckerComponent;
		ArbiterPointElement pointById = areaCheckerComponent.GetPointById(id);
		string text = areaCheckerComponent.Area.NameSafe();
		string text2 = areaCheckerComponent.GetEnterPointById(id).Guid.ToString();
		string instructionName = "instant_move_camera " + text + " " + text2 + " " + pointById.Position.x + " " + pointById.Position.y + " " + pointById.Position.z + " " + pointById.Rotation + " " + pointById.Zoom;
		GetCurrentAreaName();
		if (!Application.isPlaying)
		{
			MoveInEditor(areaCheckerComponent.Area, pointById.Position.x, pointById.Position.y, pointById.Position.z, pointById.Rotation, pointById.Zoom);
		}
		else
		{
			ArbiterService.RunInstruction(instructionName, Array.Empty<object>());
		}
	}

	public void AddSimpleMoveCameraTask(string areaName, string areaEnterPointName, float x, float y, float z, float rotation, float zoom)
	{
		string instructionName = "instant_move_camera " + areaName + " " + areaEnterPointName + " " + x + " " + y + " " + z + " " + rotation + " " + zoom;
		GetCurrentAreaName();
		if (!Application.isPlaying)
		{
			BlueprintArea blueprint = Utilities.GetBlueprint<BlueprintArea>(areaName);
			MoveInEditor(blueprint, x, y, z, rotation, zoom);
		}
		else
		{
			ArbiterService.RunInstruction(instructionName, Array.Empty<object>());
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

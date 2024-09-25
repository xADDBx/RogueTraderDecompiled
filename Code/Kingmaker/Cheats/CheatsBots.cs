using System.IO;
using System.Linq;
using Core.Cheats;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.QA.Arbiter;
using Kingmaker.QA.Clockwork;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Waaagh;
using UnityEngine;

namespace Kingmaker.Cheats;

internal class CheatsBots
{
	[Cheat(Name = "clockwork_start")]
	public static void ConsoleClockworkStart(string scenario)
	{
		if (Clockwork.IsRunning)
		{
			Clockwork.Instance.Stop();
		}
		Clockwork.Instance.Start(scenario);
	}

	[Cheat(Name = "clockwork_stop")]
	public static void ConsoleClockworkStop()
	{
		if (Clockwork.IsRunning)
		{
			Clockwork.Instance.Stop();
		}
	}

	[Cheat(Name = "clockwork_list_scenarios")]
	public static string ConsoleClockworkScenarioList()
	{
		return $"Available {ClockworkScenarioIndex.Instance.Instructions.Count()} Clockwork scenarios:\n\n" + string.Join("\n", ClockworkScenarioIndex.Instance.Instructions.OrderBy((string x) => x));
	}

	[Cheat(Name = "clockwork_status")]
	public static string ConsoleClockworkStatus()
	{
		string @string = PlayerPrefs.GetString("ClockworkScenario", "");
		if (Clockwork.IsRunning || Clockwork.GameIsLoadingWithScenario)
		{
			if (string.IsNullOrEmpty(@string))
			{
				Clockwork instance = Clockwork.Instance;
				return "running " + ((instance == null) ? null : SimpleBlueprintExtendAsObject.Or(instance.Scenario, null)?.ScenarioName);
			}
			return "starting " + @string;
		}
		if (!Clockwork.Enabled)
		{
			return "Clockwork is not ready or disabled.";
		}
		return "stop";
	}

	[Cheat(Name = "arbiter_start")]
	public static void ConsoleArbiterStart(string instruction)
	{
		Arbiter arbiter = Object.FindObjectOfType<Arbiter>();
		if (arbiter == null)
		{
			PlayerPrefs.SetString("ArbiterInstruction", instruction);
			PlayerPrefs.Save();
			BundledSceneLoader.LoadScene("Arbiter");
		}
		else
		{
			arbiter.RunInstruction(instruction);
		}
	}

	[Cheat(Name = "arbiter_stop")]
	public static void ConsoleArbiterStop()
	{
		Arbiter arbiter = Object.FindObjectOfType<Arbiter>();
		if (arbiter != null)
		{
			arbiter.Stop();
		}
	}

	[Cheat(Name = "arbiter_list_instructions")]
	public static string ConsoleArbiterInstructionList()
	{
		return $"Available {ArbiterInstructionIndex.Instance.Instructions.Count()} Arbiter instructions:\n" + string.Join("\n", ArbiterInstructionIndex.Instance.Instructions.OrderBy((string x) => x));
	}

	[Cheat(Name = "arbiter_screenshot")]
	public static void MakeScreenshot(string instructionName, int pointId)
	{
		BlueprintArbiterInstruction instruction = ArbiterInstructionIndex.Instance.GetInstruction(instructionName);
		if (instruction == null)
		{
			PFLog.Arbiter.Error("Instruction " + instructionName + " is not found.");
		}
		else if (instruction.Test is AreaCheckerComponent areaCheckerComponent)
		{
			ArbiterClientIntegration.MoveCameraToPoint(areaCheckerComponent.GetPointById(pointId));
			string samplePath = Path.Combine(ApplicationPaths.persistentDataPath, $"Arbiter/ManualScreenshots/{instructionName}/{pointId}.png");
			string directoryName = Path.GetDirectoryName(samplePath);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			ArbiterClientIntegration.DisableFow();
			CameraRig.Instance.Camera.EnsureComponent<WaaaghCameraScreenshoter>().MakePNG(Arbiter.Root.Resolution.x, Arbiter.Root.Resolution.y, delegate(byte[] buffer)
			{
				File.WriteAllBytes(samplePath, buffer);
				ArbiterClientIntegration.EnableFow();
				PFLog.Arbiter.Log("Screenshot saved at " + samplePath);
			});
		}
		else
		{
			PFLog.Arbiter.Error("Instruction " + instructionName + " has no AreaCheckerComponent");
		}
	}

	[Cheat(Name = "arbiter_camera_teleport", ExecutionPolicy = ExecutionPolicy.All)]
	public static void ArbiterTeleportCamera(string instructionName, int pointId)
	{
		new ArbiterInstantMoveCameraStarter().AddArbiterMoveCameraTask(instructionName, pointId);
	}
}

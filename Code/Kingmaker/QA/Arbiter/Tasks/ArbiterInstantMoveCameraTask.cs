using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.QA.Arbiter.GameCore;
using Kingmaker.QA.Arbiter.GameCore.AreaChecker;
using Kingmaker.QA.Arbiter.Service;
using UnityEngine;

namespace Kingmaker.QA.Arbiter.Tasks;

public class ArbiterInstantMoveCameraTask : ArbiterTask
{
	private readonly string m_BlueprintAreaName;

	private readonly string m_BlueprintAreaEnterPointName;

	private readonly Vector3 m_Position;

	private readonly float m_Rotation;

	private readonly float m_Zoom;

	private readonly bool m_IsArbiter;

	public ArbiterInstantMoveCameraTask(string parametersString)
	{
		m_IsArbiter = parametersString.StartsWith("arbiter");
		parametersString = parametersString.Replace(m_IsArbiter ? "arbiter_instant_move_camera" : "instant_move_camera", string.Empty).Trim();
		List<string> list = parametersString.Split(' ').ToList();
		CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
		cultureInfo.NumberFormat.NumberDecimalSeparator = ",";
		if (!m_IsArbiter)
		{
			try
			{
				m_BlueprintAreaName = list[0];
			}
			catch
			{
				m_BlueprintAreaName = string.Empty;
			}
			try
			{
				m_BlueprintAreaEnterPointName = list[1];
			}
			catch
			{
				m_BlueprintAreaEnterPointName = string.Empty;
			}
			try
			{
				m_Position = new Vector3(float.Parse(list[2], cultureInfo), float.Parse(list[3], cultureInfo), float.Parse(list[4], cultureInfo));
			}
			catch
			{
				m_Position = Vector3.zero;
			}
			try
			{
				m_Rotation = float.Parse(list[5], cultureInfo);
			}
			catch
			{
				m_Rotation = 0f;
			}
			try
			{
				m_Zoom = float.Parse(list[6], cultureInfo);
			}
			catch
			{
				m_Zoom = 0f;
			}
		}
	}

	protected override IEnumerator<ArbiterTask> Routine()
	{
		if (m_IsArbiter)
		{
			yield break;
		}
		BlueprintAreaReference blueprintAreaReference = (BlueprintArbiterInstructionIndex.Instance.GetInstruction(m_BlueprintAreaName)?.Test as AreaCheckerComponent)?.Area;
		BlueprintArea blueprintArea = ((blueprintAreaReference != null) ? ((BlueprintArea)blueprintAreaReference) : Utilities.GetBlueprint<BlueprintArea>(m_BlueprintAreaName));
		if (blueprintArea == null)
		{
			Debug.Log("Can't find BlueprintArea: " + m_BlueprintAreaName);
			yield break;
		}
		BlueprintAreaEnterPoint blueprintAreaEnterPoint = Utilities.GetBlueprint<BlueprintAreaEnterPoint>(m_BlueprintAreaEnterPointName);
		BlueprintAreaPart areaPart = blueprintAreaEnterPoint.AreaPart;
		BlueprintAreaPart blueprintAreaPart = Game.Instance?.CurrentlyLoadedAreaPart;
		if (blueprintAreaPart == null || blueprintAreaPart != areaPart)
		{
			ArbiterService.Logger.Log("Loading " + areaPart?.NameSafe());
			base.Status = "Loading";
			BlueprintAreaPreset defaultPreset = blueprintArea.DefaultPreset;
			yield return new StartNewGameFromPresetTask(this, defaultPreset);
		}
		if (Game.Instance != null)
		{
			ArbiterIntegration.SetGamePause(value: true);
			Game.Instance.Teleport(blueprintAreaEnterPoint);
			yield return null;
			while (LoadingProcess.Instance.IsLoadingInProcess)
			{
				yield return null;
			}
			CheatsTransfer.LocalTeleport(new Vector3(1000f, 1000f, 1000f));
			yield return new WaitForCutsceneTask(this);
			yield return new WaitForDialogTask(this);
			Game.Instance.IsPaused = true;
			ArbiterIntegration.DisableFow();
			yield return new DelayTask(TimeSpan.FromSeconds(3.0), this);
			ArbiterIntegration.SetGamePause(value: true);
			ArbiterIntegration.MoveCameraImmediately(m_Position, m_Rotation, m_Zoom);
		}
	}
}

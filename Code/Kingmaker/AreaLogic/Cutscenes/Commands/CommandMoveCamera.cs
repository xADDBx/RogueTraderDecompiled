using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("4a2a2b09db659af49b23c329e7b1744b")]
public class CommandMoveCamera : CommandBase
{
	private class Data
	{
		public Vector3 OriginalTarget;

		public bool TakingTooLong;

		[CanBeNull]
		public Coroutine ScrollCoroutine;

		public TimeSpan TargetTime;
	}

	[SerializeReference]
	public PositionEvaluator Target;

	public bool Teleport;

	[ConditionalHide("Teleport")]
	public float CameraSpeed;

	[ConditionalHide("Teleport")]
	public bool FixedScrollTime;

	[ConditionalShow("FixedScrollTime")]
	public float CameraMaxSpeed;

	[ConditionalShow("FixedScrollTime")]
	public float CameraMaxTravelTime;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		if (Target != null)
		{
			Game.Instance.CameraController?.Follower?.Release();
			Vector3 vector = CameraRig.Instance.ClampByLevelBounds(Target.GetValue());
			Data commandData = player.GetCommandData<Data>(this);
			commandData.OriginalTarget = vector;
			float targetTime = -1f;
			if (Teleport)
			{
				CameraRig.Instance.ScrollToImmediately(vector);
			}
			else if (FixedScrollTime)
			{
				commandData.ScrollCoroutine = CameraRig.Instance.ScrollToTimed(vector, out targetTime, CameraMaxTravelTime, CameraMaxSpeed, CameraSpeed);
			}
			else if (CameraSpeed > 0f)
			{
				commandData.ScrollCoroutine = CameraRig.Instance.ScrollToTimed(vector, out targetTime, 0f, 0f, CameraSpeed);
			}
			else
			{
				CameraRig.Instance.ScrollTo(vector);
			}
			if (0f < targetTime)
			{
				commandData.TargetTime = Game.Instance.TimeController.GameTime + targetTime.Seconds();
			}
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		base.OnStop(player);
		Data commandData = player.GetCommandData<Data>(this);
		CameraRig.Instance.StopCoroutine(commandData.ScrollCoroutine);
		commandData.ScrollCoroutine = null;
		commandData.TargetTime = TimeSpan.Zero;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		if (!Target)
		{
			return true;
		}
		Data commandData = player.GetCommandData<Data>(this);
		if (!(commandData.TargetTime <= Game.Instance.TimeController.GameTime))
		{
			return commandData.TakingTooLong;
		}
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		if (time > 20.0)
		{
			player.GetCommandData<Data>(this).TakingTooLong = true;
		}
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		if ((bool)Target)
		{
			Vector3 originalTarget = player.GetCommandData<Data>(this).OriginalTarget;
			CameraRig.Instance.ScrollTo(originalTarget);
		}
	}

	public override string GetCaption()
	{
		return "<b>Move camera</b> to " + (Target ? Target.GetCaption() : "???");
	}
}

using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("955b28ccab87414899465e01b0412d33")]
public class CommandControlCamera : CommandBase
{
	public enum TimingModeType
	{
		FixSpeed,
		FixTime,
		Snap
	}

	private class Data
	{
		public bool TakingTooLong;

		public TimeSpan TargetTime;

		[CanBeNull]
		public Coroutine ScrollCoroutine;

		[CanBeNull]
		public Coroutine RotateCoroutine;
	}

	public TimingModeType TimingMode;

	[ShowIf("TimingIsTime")]
	public float FixedTime;

	public bool Move;

	[ShowIf("Move")]
	[SerializeReference]
	public PositionEvaluator MoveTarget;

	[ShowIf("ShowMoveSpeed")]
	public float MoveSpeed;

	[ShowIf("ShowMoveCurve")]
	public AnimationCurve MoveCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public bool Rotate;

	[ShowIf("Rotate")]
	[SerializeReference]
	public FloatEvaluator RotateTarget;

	[ShowIf("ShowRotateSpeed")]
	public float RotateSpeed;

	[ShowIf("ShowRotateCurve")]
	public AnimationCurve RotateCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public bool Zoom;

	[ShowIf("Zoom")]
	[Range(0f, 1f)]
	[Tooltip("0 = далеко\n1 = близко")]
	public float ZoomTarget;

	[ShowIf("ShowZoomSpeed")]
	public float ZoomSpeed;

	[ShowIf("ShowZoomCurve")]
	public AnimationCurve ZoomCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	private bool ShowMoveSpeed
	{
		get
		{
			if (Move)
			{
				return TimingMode == TimingModeType.FixSpeed;
			}
			return false;
		}
	}

	private bool ShowRotateSpeed
	{
		get
		{
			if (Rotate)
			{
				return TimingMode == TimingModeType.FixSpeed;
			}
			return false;
		}
	}

	private bool ShowZoomSpeed
	{
		get
		{
			if (Zoom)
			{
				return TimingMode == TimingModeType.FixSpeed;
			}
			return false;
		}
	}

	private bool ShowMoveCurve
	{
		get
		{
			if (Move)
			{
				if (TimingMode != 0 || !(MoveSpeed > 0f))
				{
					return TimingIsTime;
				}
				return true;
			}
			return false;
		}
	}

	private bool ShowRotateCurve
	{
		get
		{
			if (Rotate)
			{
				if (TimingMode != 0 || !(RotateSpeed > 0f))
				{
					return TimingIsTime;
				}
				return true;
			}
			return false;
		}
	}

	private bool ShowZoomCurve
	{
		get
		{
			if (Zoom)
			{
				if (TimingMode != 0 || !(ZoomSpeed > 0f))
				{
					return TimingIsTime;
				}
				return true;
			}
			return false;
		}
	}

	private bool TimingIsTime => TimingMode == TimingModeType.FixTime;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		TimingModeType timingMode = (skipping ? TimingModeType.Snap : TimingMode);
		StartCameraTransformations(commandData, timingMode);
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		base.OnStop(player);
		Data commandData = player.GetCommandData<Data>(this);
		CameraRig instance = CameraRig.Instance;
		instance.StopCoroutine(commandData.RotateCoroutine);
		instance.StopCoroutine(commandData.ScrollCoroutine);
		commandData.RotateCoroutine = null;
		commandData.ScrollCoroutine = null;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.TakingTooLong)
		{
			return true;
		}
		if (TimeSpan.Zero < commandData.TargetTime)
		{
			return commandData.TargetTime <= Game.Instance.TimeController.GameTime;
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
		Data commandData = player.GetCommandData<Data>(this);
		StartCameraTransformations(commandData, TimingModeType.Snap);
		commandData.RotateCoroutine = null;
		commandData.ScrollCoroutine = null;
	}

	public override string GetCaption()
	{
		if (Move)
		{
			return "<b>Move camera</b> to " + (MoveTarget ? MoveTarget.GetCaption() : "???");
		}
		if (Rotate)
		{
			return "<b>Rotate camera</b> to " + (RotateTarget ? RotateTarget.GetCaption() : "???");
		}
		if (Zoom)
		{
			return "<b>Zoom camera</b> to " + ZoomTarget;
		}
		return "Control camera";
	}

	private void StartCameraTransformations(Data data, TimingModeType timingMode)
	{
		data.TargetTime = TimeSpan.Zero;
		Game.Instance.CameraController?.Follower?.Release();
		if (Move && MoveTarget != null)
		{
			Vector3 position = CameraRig.Instance.ClampByLevelBounds(MoveTarget.GetValue());
			switch (timingMode)
			{
			case TimingModeType.FixSpeed:
				if (MoveSpeed > 0f)
				{
					data.ScrollCoroutine = CameraRig.Instance.ScrollToTimed(position, out var targetTime, 0f, float.MaxValue, MoveSpeed, MoveCurve);
					TimeSpan timeSpan = Game.Instance.TimeController.GameTime + targetTime.Seconds();
					data.TargetTime = ((timeSpan > data.TargetTime) ? timeSpan : data.TargetTime);
					break;
				}
				goto default;
			case TimingModeType.FixTime:
				if (FixedTime > 0f)
				{
					data.ScrollCoroutine = CameraRig.Instance.ScrollToTimed(position, out var targetTime2, FixedTime, float.MaxValue, 0f, MoveCurve);
					TimeSpan timeSpan2 = Game.Instance.TimeController.GameTime + targetTime2.Seconds();
					data.TargetTime = ((timeSpan2 > data.TargetTime) ? timeSpan2 : data.TargetTime);
					break;
				}
				goto default;
			case TimingModeType.Snap:
				if (data.ScrollCoroutine != null)
				{
					CameraRig.Instance.StopCoroutine(data.ScrollCoroutine);
					data.ScrollCoroutine = null;
				}
				CameraRig.Instance.ScrollToImmediately(position);
				break;
			default:
				CameraRig.Instance.ScrollTo(position);
				break;
			}
		}
		if (Rotate && RotateTarget != null)
		{
			float num = RotateTarget.GetValue() + 180f;
			switch (timingMode)
			{
			case TimingModeType.FixSpeed:
				if (RotateSpeed > 0f)
				{
					data.RotateCoroutine = CameraRig.Instance.RotateToTimed(num, out var targetTime4, 0f, RotateSpeed, RotateCurve);
					TimeSpan timeSpan4 = Game.Instance.TimeController.GameTime + targetTime4.Seconds();
					data.TargetTime = ((timeSpan4 > data.TargetTime) ? timeSpan4 : data.TargetTime);
					break;
				}
				goto default;
			case TimingModeType.FixTime:
				if (FixedTime > 0f)
				{
					data.RotateCoroutine = CameraRig.Instance.RotateToTimed(num, out var targetTime3, FixedTime, 0f, RotateCurve);
					TimeSpan timeSpan3 = Game.Instance.TimeController.GameTime + targetTime3.Seconds();
					data.TargetTime = ((timeSpan3 > data.TargetTime) ? timeSpan3 : data.TargetTime);
					break;
				}
				goto default;
			case TimingModeType.Snap:
				CameraRig.Instance.RotateToImmediately(num);
				break;
			default:
				CameraRig.Instance.RotateTo(num);
				break;
			}
		}
		if (!Zoom)
		{
			return;
		}
		float zoomTarget = ZoomTarget;
		switch (timingMode)
		{
		case TimingModeType.FixSpeed:
			if (ZoomSpeed > 0f)
			{
				CameraRig.Instance.CameraZoom.ZoomToTimed(zoomTarget, out var targetTime6, 0f, ZoomSpeed, ZoomCurve);
				TimeSpan timeSpan6 = Game.Instance.TimeController.GameTime + targetTime6.Seconds();
				data.TargetTime = ((timeSpan6 > data.TargetTime) ? timeSpan6 : data.TargetTime);
				return;
			}
			break;
		case TimingModeType.FixTime:
			if (FixedTime > 0f)
			{
				CameraRig.Instance.CameraZoom.ZoomToTimed(zoomTarget, out var targetTime5, FixedTime, 0f, ZoomCurve);
				TimeSpan timeSpan5 = Game.Instance.TimeController.GameTime + targetTime5.Seconds();
				data.TargetTime = ((timeSpan5 > data.TargetTime) ? timeSpan5 : data.TargetTime);
				return;
			}
			break;
		case TimingModeType.Snap:
			CameraRig.Instance.CameraZoom.ZoomToImmediate(zoomTarget);
			return;
		}
		CameraRig.Instance.CameraZoom.CurrentNormalizePosition = zoomTarget;
	}
}

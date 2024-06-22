using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Rest;

public class CameraController : IControllerEnable, IController, IControllerDisable, IControllerTick, IControllerStop
{
	public class CameraUnitFollower
	{
		private MechanicEntity m_Entity;

		private Vector2 m_LastOffset;

		private const float ScreenLockAreaX = 0.3f;

		private const float ScreenLockAreaY = 0.15f;

		private const float OffsetFactor = 3f;

		private const float Smoothness = 2f;

		public bool IsInCombat => Game.Instance.Player.IsInCombat;

		public bool IsInDialog => Game.Instance.CurrentMode == GameModeType.Dialog;

		public bool HasTarget => m_Entity != null;

		public void Follow()
		{
			if (Game.Instance.TurnController.IsSpaceCombat)
			{
				Follow(Game.Instance.Player.PlayerShip);
			}
			else
			{
				Follow(Game.Instance.SelectionCharacter.SelectedUnit.Value ?? Game.Instance.Player.MainCharacterEntity);
			}
		}

		public void Follow(MechanicEntity unit)
		{
			m_Entity = unit;
		}

		public void ReleaseUnit(MechanicEntity unit)
		{
			if (m_Entity == unit)
			{
				m_Entity = null;
			}
		}

		public void Release()
		{
			m_LastOffset = Vector2.zero;
			m_Entity = null;
		}

		public void ScrollTo(Vector3 position)
		{
			if (!(CameraRig.Instance == null))
			{
				Release();
				CameraRig.Instance.ScrollTo(position);
			}
		}

		public void ScrollTo(MechanicEntity unit)
		{
			if (unit != null)
			{
				ScrollTo(unit.Position);
			}
		}

		public Coroutine ScrollToTimed(Vector3 position, float maxTime, float maxSpeed, float speed, AnimationCurve curve = null, bool useUnscaledTime = false, Action callback = null)
		{
			if (CameraRig.Instance == null)
			{
				return null;
			}
			Release();
			return CameraRig.Instance.ScrollToTimed(position, maxTime, maxSpeed, speed, curve, useUnscaledTime, callback);
		}

		public void TryFollow()
		{
			if (CameraRig.Instance == null || m_Entity == null)
			{
				return;
			}
			if (m_Entity.View == null)
			{
				CameraRig.Instance.ScrollTo(m_Entity.Position);
			}
			else if (Game.Instance.IsControllerGamepad && !IsInCombat && !IsInDialog)
			{
				if (CameraRig.Instance.IsScrollingByRoutine)
				{
					return;
				}
				UnitMovementAgentContinuous unitMovementAgentContinuous = (m_Entity.View as UnitEntityView)?.AgentOverride as UnitMovementAgentContinuous;
				bool hasPrediction = Game.Instance.MovePredictionController.HasPrediction;
				if (!(unitMovementAgentContinuous == null) || hasPrediction)
				{
					Vector3 vector = CameraRig.Instance.Camera.WorldToScreenPoint(m_Entity.View.transform.position);
					Vector2 vector2 = new Vector2((vector.x - (float)Screen.width * 0.5f) / (float)Screen.width, (vector.y - (float)Screen.height * 0.5f) / (float)Screen.height);
					if (!(vector2.x * vector2.x / 0.09f + vector2.y * vector2.y / 0.0225f <= 1f) || ((unitMovementAgentContinuous != null) ? unitMovementAgentContinuous.IsReallyMoving : hasPrediction))
					{
						m_LastOffset = ((unitMovementAgentContinuous != null) ? Vector2.Lerp(m_LastOffset, unitMovementAgentContinuous.MoveDirection * 3f, Time.unscaledDeltaTime * 2f) : Vector2.Lerp(m_LastOffset, Vector2.zero, Time.unscaledDeltaTime * 2f));
						Vector3 position = m_Entity.View.ViewTransform.position + m_LastOffset.To3D();
						CameraRig.Instance.ScrollTo(position);
					}
				}
			}
			else
			{
				CameraRig.Instance.ScrollTo(m_Entity.Position);
			}
		}
	}

	private readonly bool m_AllowScroll;

	private readonly bool m_AllowRotate;

	private readonly bool m_AllowZoom;

	private readonly bool m_Clamp;

	public readonly CameraUnitFollower Follower;

	public CameraController(bool allowScroll = true, bool allowZoom = true, bool clamp = true, bool rotate = true)
	{
		m_AllowScroll = allowScroll;
		m_AllowZoom = allowZoom;
		m_Clamp = clamp;
		m_AllowRotate = rotate;
		Follower = new CameraUnitFollower();
	}

	public TickType GetTickType()
	{
		return TickType.EndOfFrame;
	}

	public virtual void Tick()
	{
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.StarSystem || Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.GlobalMap)
		{
			return;
		}
		CameraRig instance = CameraRig.Instance;
		if (!(instance == null) && !instance.FixCamera)
		{
			if (m_AllowScroll)
			{
				Follower.TryFollow();
				instance.TickScroll();
			}
			if (m_AllowRotate)
			{
				instance.TickRotate();
			}
			if (m_AllowZoom)
			{
				instance.CameraZoom.TickZoom();
			}
			instance.TickShake();
		}
	}

	public virtual void OnEnable()
	{
		if (!m_AllowScroll)
		{
			Game.Instance.CursorController.ClearCursor();
		}
		CameraRig.Instance.NoClamp = !m_Clamp;
		if (!m_AllowZoom)
		{
			CameraRig.Instance.ResetCurrentModeSettings();
		}
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.FollowUnit.name, Follower.Follow);
	}

	public void OnDisable()
	{
		Game.Instance.Keyboard.Unbind(UISettingsRoot.Instance.UIKeybindGeneralSettings.FollowUnit.name, Follower.Follow);
	}

	public void OnStop()
	{
		Follower.Release();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.View;
using Kingmaker.View.Spawners;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Kingmaker.AreaLogic.Cutscenes.Commands.Timeline;

[RequireComponent(typeof(PlayableDirector))]
[KnowledgeDatabaseID("f4300d2db0efe6649aff80a306e1305e")]
public class DirectorAdapter : EntityViewBase, IUpdatable, IInterpolatable
{
	[Serializable]
	public class UnitBinding
	{
		private struct DynamicData
		{
			public bool IsDataInitialized;

			private Vector3 m_PreviousPosition;

			private Vector3 m_NextPosition;

			private Quaternion m_PreviousRotation;

			private Quaternion m_NextRotation;

			public void SetPosition(Vector3 position)
			{
				m_PreviousPosition = (IsDataInitialized ? m_NextPosition : position);
				m_NextPosition = position;
			}

			public Vector3 GetPosition(float progress)
			{
				return Vector3.LerpUnclamped(m_PreviousPosition, m_NextPosition, progress);
			}

			public void SetRotation(Quaternion rotation)
			{
				m_PreviousRotation = (IsDataInitialized ? m_NextRotation : rotation);
				m_NextRotation = rotation;
			}

			public Quaternion GetRotation(float progress)
			{
				return Quaternion.LerpUnclamped(m_PreviousRotation, m_NextRotation, progress);
			}

			public void Reset()
			{
				IsDataInitialized = false;
				m_PreviousPosition = default(Vector3);
				m_NextPosition = default(Vector3);
				m_PreviousRotation = default(Quaternion);
				m_NextRotation = default(Quaternion);
			}
		}

		public string Name;

		public UnitViewLink PreviewPrefab;

		public UnitSpawnerBase DefaultSpawner;

		private Animator m_UnitAnimator;

		private GameObject m_LocatorAnimator;

		private EntityRef<AbstractUnitEntity> m_Unit;

		private DynamicData m_DynamicData;

		public Animator UnitAnimator
		{
			get
			{
				if ((bool)m_UnitAnimator)
				{
					return m_UnitAnimator;
				}
				if (Application.isPlaying)
				{
					m_UnitAnimator = ((!DefaultSpawner) ? null : DefaultSpawner.SpawnedUnit?.View.Animator);
				}
				return m_UnitAnimator;
			}
		}

		public GameObject LocatorAnimator
		{
			get
			{
				if ((bool)m_LocatorAnimator)
				{
					return m_LocatorAnimator;
				}
				RecreateLocatorAnimator();
				return m_LocatorAnimator;
			}
		}

		public AbstractUnitEntity BoundUnit
		{
			get
			{
				if (m_Unit != null)
				{
					return m_Unit;
				}
				if ((bool)DefaultSpawner)
				{
					return DefaultSpawner.SpawnedUnit;
				}
				return null;
			}
		}

		[CanBeNull]
		public UnitAnimationManager AnimationManager => BoundUnit?.AnimationManager;

		public void RecreateLocatorAnimator()
		{
			m_LocatorAnimator = new GameObject("[locator]" + Name, typeof(Animator));
			m_LocatorAnimator.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			m_LocatorAnimator.hideFlags = HideFlags.DontSave;
		}

		public void BindUnitFromContext(IEnumerable<CommandPlayTimeline.UnitBindingData> units)
		{
			CommandPlayTimeline.UnitBindingData unitBindingData = units.FirstOrDefault((CommandPlayTimeline.UnitBindingData u) => u.Name == Name);
			AbstractUnitEntity abstractUnitEntity = ((unitBindingData == null) ? null : ElementExtendAsObject.Or(unitBindingData.Unit, null)?.GetValue());
			if (abstractUnitEntity != null)
			{
				m_Unit = abstractUnitEntity;
				m_UnitAnimator = abstractUnitEntity.View.Animator;
			}
		}

		public void FixupUnitPositions()
		{
			if (BoundUnit != null && (bool)LocatorAnimator)
			{
				Vector3 position = LocatorAnimator.transform.position;
				Quaternion rotation = LocatorAnimator.transform.rotation;
				m_DynamicData.SetPosition(position);
				m_DynamicData.SetRotation(Quaternion.Euler(rotation.eulerAngles));
				m_DynamicData.IsDataInitialized = true;
				BoundUnit.Position = position;
				BoundUnit.SetOrientation(rotation.eulerAngles.y);
			}
		}

		public void FixupUnitPositionsInterpolation(float progress)
		{
			if (m_DynamicData.IsDataInitialized)
			{
				BoundUnit.View.ViewTransform.position = m_DynamicData.GetPosition(progress);
				BoundUnit.View.ViewTransform.rotation = m_DynamicData.GetRotation(progress);
			}
		}

		public void Reset()
		{
			m_DynamicData.Reset();
		}
	}

	public enum BoundTrackType
	{
		Animation,
		Position,
		Bark
	}

	[Serializable]
	public class TrackBinding
	{
		public string TrackName;

		public string UnitName;

		public BoundTrackType Type;
	}

	private class TickLogic
	{
		private float m_PreviousTime;

		private float m_CurrentTime;

		private int m_LastUpdateTick;

		[NotNull]
		private readonly DirectorAdapter m_DirectorAdapter;

		[NotNull]
		private PlayableDirector PlayableDirector => m_DirectorAdapter.m_PlayableDirector;

		private static int CurrentUpdateTick => Game.Instance.RealTimeController.CurrentSystemStepIndex;

		public TickLogic([NotNull] DirectorAdapter directorAdapter)
		{
			m_DirectorAdapter = directorAdapter;
		}

		public void OnPlay()
		{
			m_CurrentTime = (float)PlayableDirector.time;
			m_PreviousTime = m_CurrentTime;
		}

		public void OnStop()
		{
		}

		public void Tick(float delta)
		{
			if (PlayableDirector.state == PlayState.Paused)
			{
				Logger.Error("Logic error: expecting director to be playing at this moment");
			}
			m_PreviousTime = m_CurrentTime;
			m_CurrentTime += delta;
			PlayableDirector.time = m_PreviousTime;
			PlayableDirector.Evaluate();
			m_LastUpdateTick = CurrentUpdateTick;
		}

		public void SetTime(float time)
		{
			if (PlayableDirector.state == PlayState.Paused)
			{
				Logger.Error("Logic error: expecting director to be playing at this moment");
			}
			m_PreviousTime = (m_CurrentTime = time);
			PlayableDirector.time = m_PreviousTime;
			PlayableDirector.Evaluate();
		}

		public void Interpolate(float progress)
		{
			if (m_LastUpdateTick == CurrentUpdateTick)
			{
				if (PlayableDirector.state == PlayState.Paused)
				{
					Logger.Error("Logic error: expecting director to be playing at this moment");
				}
				float num = Mathf.LerpUnclamped(m_PreviousTime, m_CurrentTime, progress);
				if (!(Math.Abs((double)num - PlayableDirector.time) < 0.0001))
				{
					PlayableDirector.time = num;
					PlayableDirector.Evaluate();
				}
			}
		}
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Timeline");

	public UnitBinding[] BoundUnits = Array.Empty<UnitBinding>();

	public TrackBinding[] BoundTracks = Array.Empty<TrackBinding>();

	public DirectorCameraLink CameraLink;

	private PlayableDirector m_PlayableDirector;

	[NotNull]
	private TickLogic m_TickLogic;

	public override bool CreatesDataOnLoad => true;

	public void ApplyBindings(List<CommandPlayTimeline.UnitBindingData> units = null)
	{
		PlayableDirector component = GetComponent<PlayableDirector>();
		TimelineAsset timelineAsset = component.playableAsset as TimelineAsset;
		if (!timelineAsset)
		{
			return;
		}
		for (int i = 0; i < BoundTracks.Length; i++)
		{
			TrackBinding trackBinding = BoundTracks[i];
			if (string.IsNullOrEmpty(trackBinding.UnitName) || string.IsNullOrEmpty(trackBinding.TrackName))
			{
				continue;
			}
			UnitBinding unitBinding = BoundUnits.FirstOrDefault((UnitBinding o) => o.Name == trackBinding.UnitName);
			if (unitBinding == null)
			{
				continue;
			}
			if (unitBinding.AnimationManager != null)
			{
				unitBinding.AnimationManager.PlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
			}
			if (units != null)
			{
				unitBinding.BindUnitFromContext(units);
			}
			if (unitBinding.UnitAnimator == null)
			{
				LogChannel.Default.Warning(this, "In director " + base.name + " in " + base.gameObject.scene.name + ": no unit found for binding " + unitBinding.Name);
				continue;
			}
			TrackAsset trackAsset = timelineAsset.GetOutputTracks().FirstOrDefault((TrackAsset t) => t.name == trackBinding.TrackName);
			if (trackAsset == null)
			{
				LogChannel.Default.Warning(this, "In director " + base.name + " in " + base.gameObject.scene.name + ": no track found for binding " + trackBinding.TrackName);
				continue;
			}
			switch (trackBinding.Type)
			{
			case BoundTrackType.Animation:
				component.SetGenericBinding(trackAsset, unitBinding.UnitAnimator);
				break;
			case BoundTrackType.Position:
				component.SetGenericBinding(trackAsset, unitBinding.LocatorAnimator);
				break;
			case BoundTrackType.Bark:
				component.SetGenericBinding(trackAsset, unitBinding.UnitAnimator.gameObject);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		if (CameraLink != null)
		{
			CameraLink.Link();
		}
	}

	public void FixupUnitPositions()
	{
		UnitBinding[] boundUnits = BoundUnits;
		for (int i = 0; i < boundUnits.Length; i++)
		{
			boundUnits[i].FixupUnitPositions();
		}
	}

	public void FixupUnitPositionsInterpolation(float progress)
	{
		UnitBinding[] boundUnits = BoundUnits;
		for (int i = 0; i < boundUnits.Length; i++)
		{
			boundUnits[i].FixupUnitPositionsInterpolation(progress);
		}
	}

	public void SetTime(float time)
	{
		m_TickLogic.SetTime(time);
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new DirectorAdapterEntity(this));
	}

	protected override void Awake()
	{
		base.Awake();
		m_PlayableDirector = GetComponent<PlayableDirector>();
		m_PlayableDirector.timeUpdateMode = DirectorUpdateMode.Manual;
		m_TickLogic = new TickLogic(this);
	}

	protected override void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.ViewTransform.position, "locator");
	}

	void IUpdatable.Tick(float deltaTime)
	{
		m_TickLogic.Tick(deltaTime);
	}

	void IInterpolatable.Tick(float progress)
	{
		m_TickLogic.Interpolate(progress);
		FixupUnitPositionsInterpolation(progress);
	}

	public void Play()
	{
		Game.Instance.DirectorAdapterController.Add(this);
		Game.Instance.InterpolationController.Add(this);
		m_TickLogic.OnPlay();
	}

	public void Stop()
	{
		FixupUnitPositionsInterpolation(1f);
		UnitBinding[] boundUnits = BoundUnits;
		foreach (UnitBinding obj in boundUnits)
		{
			obj.BoundUnit.ControlledByDirector.Release();
			obj.Reset();
		}
		if (CameraLink != null)
		{
			CameraLink.UnLink();
		}
		Game.Instance.DirectorAdapterController.Remove(this);
		Game.Instance.InterpolationController.Remove(this);
		m_TickLogic.OnStop();
	}
}

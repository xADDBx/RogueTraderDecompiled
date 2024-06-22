using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Visual.Particles.Blueprints;
using Kingmaker.Visual.Particles.SnapController;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Visual.Particles;

public abstract class SnapControllerBase : MonoBehaviour
{
	[Serializable]
	public struct OffsetAnimationSettings
	{
		public bool Enabled;

		public AnimationCurve OffsetX;

		public AnimationCurve OffsetY;

		public AnimationCurve OffsetZ;

		public bool UseWorldRotation;

		public string WorldRotationBone;
	}

	private ParticleSystem m_ParticleSystem;

	private ParticleSystemRenderer m_ParticleSystemRenderer;

	private SnapControllerBase m_RuntimeRotatableCopy;

	private PlaybackState m_PlaybackState;

	private ParticleSystemSnapshot m_ParticleSystemSnapshot;

	private bool m_IsVisible;

	private bool m_Enabled;

	private LinkedListNode<SnapControllerBase> m_LinkedListNode;

	[HideInInspector]
	[SerializeField]
	private SnapControllerBase m_RotatableCopy;

	[HideInInspector]
	[SerializeField]
	protected bool m_IsRotatableCopy;

	[SerializeField]
	protected ParticleSnapType SnapType;

	[SerializeField]
	private SnapMapBase Map;

	[SerializeField]
	private bool SupressMapOverride;

	[SerializeField]
	private AnimationCurve CameraOffsetScale = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	[SerializeField]
	private bool ApplySizeScale;

	[SerializeField]
	private bool ApplyLifetimeScale;

	[SerializeField]
	private bool ApplyRateOverTimeScale;

	[SerializeField]
	private bool ApplyBurstScale;

	[SerializeField]
	private bool PersistWhenDisabled;

	[SerializeField]
	protected OffsetAnimationSettings Offset;

	[SerializeField]
	protected List<string> BonesNames;

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintFxLocatorGroup.Reference[] m_LocatorGroups = new BlueprintFxLocatorGroup.Reference[0];

	[SerializeField]
	protected bool IgnoreSpecialBones;

	protected ReferenceArrayProxy<BlueprintFxLocatorGroup> LocatorGroups
	{
		get
		{
			BlueprintReference<BlueprintFxLocatorGroup>[] locatorGroups = m_LocatorGroups;
			return locatorGroups;
		}
	}

	public Vector3 CurrentOffset => m_PlaybackState?.GetCurrentOffset() ?? default(Vector3);

	[UsedImplicitly]
	private void Awake()
	{
		m_ParticleSystem = GetComponent<ParticleSystem>();
		m_ParticleSystemRenderer = GetComponent<ParticleSystemRenderer>();
		if (m_ParticleSystem != null)
		{
			m_ParticleSystemSnapshot = ParticleSystemSnapshot.GetPooled(m_ParticleSystem);
		}
		if (m_RotatableCopy != null)
		{
			m_RotatableCopy.gameObject.SetActive(value: false);
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (m_ParticleSystemSnapshot != null)
		{
			m_ParticleSystemSnapshot.Release();
			m_ParticleSystemSnapshot = null;
		}
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		m_Enabled = true;
	}

	[UsedImplicitly]
	protected virtual void OnDisable()
	{
		m_Enabled = false;
		StopInternal();
	}

	[UsedImplicitly]
	private void OnBecameVisible()
	{
		m_IsVisible = true;
	}

	[UsedImplicitly]
	private void OnBecameInvisible()
	{
		m_IsVisible = false;
	}

	[UsedImplicitly]
	private void OnParticleUpdateJobScheduled()
	{
		m_PlaybackState?.OnParticleUpdateJobScheduled();
	}

	public bool IsPlaying()
	{
		return m_PlaybackState != null;
	}

	public void Play(SnapMapBase snapMapOverride = null)
	{
		if (!m_IsRotatableCopy)
		{
			PlayInternal(snapMapOverride);
		}
	}

	public void Stop()
	{
		if (!m_IsRotatableCopy)
		{
			StopInternal();
		}
	}

	protected virtual void OnStartPlaying(SnapMapBase snapMap)
	{
	}

	private void PlayInternal(SnapMapBase snapMapOverride = null)
	{
		StopInternal();
		if (!m_Enabled)
		{
			PFLog.TechArt.Warning(this, "Unable to play. Snap controller is disabled. GO: " + base.name);
			return;
		}
		if (m_ParticleSystem == null)
		{
			PFLog.TechArt.Warning(this, "Unable to play. Missing particle system. GO: " + base.name);
			return;
		}
		if (m_ParticleSystemRenderer == null)
		{
			PFLog.TechArt.Warning(this, "Unable to play. Missing particle system renderer. GO: " + base.name);
			return;
		}
		SnapMapBase snapMapBase = ResolveSnapMap(snapMapOverride);
		if (snapMapBase == null)
		{
			PFLog.TechArt.Warning(this, "Unable to play. Missing snap map. GO: " + base.name);
			return;
		}
		if (!IsSnapMapEnabled(snapMapBase) && !PersistWhenDisabled)
		{
			PFLog.TechArt.Warning(this, "Unable to play. Snap map is disabled, but 'PersistWhenDisabled' option is not set. GO: " + base.name);
			return;
		}
		HashSet<FxBone> value;
		BoneCollector.Result bones;
		using (CollectionPool<HashSet<FxBone>, FxBone>.Get(out value))
		{
			try
			{
				bones = GetBones(snapMapBase, value);
			}
			finally
			{
			}
			if (value.Count > 0)
			{
				PlayInternal(bones, value, snapMapBase);
			}
			else
			{
				PFLog.TechArt.Warning(this, "Unable to play. Missing fxBones.");
			}
		}
		if (SnapType == ParticleSnapType.Transforms && bones.hasRotatableBones && !m_IsRotatableCopy)
		{
			CreateRuntimeRotatableCopyIfNotExists();
		}
		PlayRotatableCopy(snapMapOverride);
	}

	internal virtual BoneCollector.Result GetBones(SnapMapBase snapMap, HashSet<FxBone> fxBones)
	{
		return new BoneCollector(snapMap, BonesNames, LocatorGroups, ignoreRotatableBones: SnapType == ParticleSnapType.Transforms && !m_IsRotatableCopy, ignoreNonRotatableBones: SnapType == ParticleSnapType.Transforms && m_IsRotatableCopy, ignoreSpecialBones: IgnoreSpecialBones, rotationRootBoneName: Offset.WorldRotationBone).Collect(fxBones);
	}

	private void PlayInternal(BoneCollector.Result boneCollectionResult, HashSet<FxBone> fxBones, SnapMapBase snapMap)
	{
		ParticleSystem.MinMaxCurve minMaxCurveSource = m_ParticleSystem.main.startDelay;
		Transform transform = ((SnapType != 0) ? null : boneCollectionResult.rotationRootBone?.Transform);
		ref OffsetAnimationSettings offset = ref Offset;
		Transform offsetRotationRoot = transform;
		AnimationSampler animationSampler = new AnimationSampler(new OffsetAnimationSampler(m_ParticleSystemSnapshot.startDelay.Evaluate(in minMaxCurveSource, UnityEngine.Random.value), in offset, offsetRotationRoot), new CameraOffsetScaleAnimationSampler(cameraOffsetScaleCurve: CameraOffsetScale, snapMapAdditionalScaleReduced: snapMap.AdditionalScaleReduced, particleSystemStartDelay: m_ParticleSystemSnapshot.startDelay.Evaluate(in minMaxCurveSource, UnityEngine.Random.value)));
		OnStartPlaying(snapMap);
		ISnapBehaviour pooled;
		if (SnapType == ParticleSnapType.Transforms)
		{
			bool isRotatableCopy = m_IsRotatableCopy;
			pooled = TransformSnap.GetPooled(m_ParticleSystem, m_ParticleSystemRenderer, snapMap, m_ParticleSystemSnapshot, fxBones, isRotatableCopy);
		}
		else
		{
			ParticleSystem particleSystem = m_ParticleSystem;
			ParticleSystemSnapshot particleSystemSnapshot = m_ParticleSystemSnapshot;
			pooled = ProceduralMeshSnap.GetPooled(fxBones, ApplySizeScale, ApplyLifetimeScale, ApplyRateOverTimeScale, ApplyBurstScale, particleSystem, snapMap, particleSystemSnapshot);
		}
		m_PlaybackState = PlaybackState.GetPooled(m_ParticleSystem, m_ParticleSystemRenderer, snapMap, animationSampler, PersistWhenDisabled, pooled);
		m_PlaybackState.OnStart();
		if (m_LinkedListNode == null)
		{
			m_LinkedListNode = new LinkedListNode<SnapControllerBase>(this);
		}
		SnapControllerSystem.RegisterControllerForSystemUpdate(m_LinkedListNode);
	}

	private void StopInternal()
	{
		if (m_PlaybackState != null)
		{
			SnapControllerSystem.UnregisterControllerFromSystemUpdate(m_LinkedListNode);
			m_PlaybackState.OnStop();
			m_PlaybackState.Recycle();
			m_PlaybackState = null;
			StopRotatableCopy();
		}
	}

	internal void OnSystemUpdate(CameraData cameraData)
	{
		if (m_PlaybackState != null)
		{
			PlaybackStateUpdateData data = new PlaybackStateUpdateData(m_IsVisible, cameraData);
			if (m_PlaybackState.Update(data) == PlaybackStateUpdateResult.Failure)
			{
				StopInternal();
			}
		}
	}

	private SnapMapBase ResolveSnapMap(SnapMapBase snapMapOverride)
	{
		if (!SupressMapOverride && snapMapOverride != null)
		{
			return snapMapOverride;
		}
		return Map;
	}

	private static bool IsSnapMapEnabled(SnapMapBase snapMap)
	{
		if (!snapMap.gameObject.activeInHierarchy)
		{
			return snapMap.enabled;
		}
		return true;
	}

	private void CreateRuntimeRotatableCopyIfNotExists()
	{
		if (m_RotatableCopy != null || m_RuntimeRotatableCopy != null)
		{
			return;
		}
		try
		{
			m_IsRotatableCopy = true;
			m_RuntimeRotatableCopy = UnityEngine.Object.Instantiate(base.gameObject, base.transform.parent).GetComponent<SnapControllerBase>();
			GameObject obj = m_RuntimeRotatableCopy.gameObject;
			obj.hideFlags = HideFlags.DontSave;
			obj.name = base.gameObject.name + "_RotatableCopy (Runtime)";
			obj.SetActive(value: false);
		}
		finally
		{
			m_IsRotatableCopy = false;
		}
	}

	private void PlayRotatableCopy(SnapMapBase snapMapOverride)
	{
		SnapControllerBase snapControllerBase = ResolveRotatableCopy();
		if (!(snapControllerBase == null))
		{
			snapControllerBase.gameObject.SetActive(value: true);
			snapControllerBase.PlayInternal(snapMapOverride);
		}
	}

	private void StopRotatableCopy()
	{
		SnapControllerBase snapControllerBase = ResolveRotatableCopy();
		if (!(snapControllerBase == null))
		{
			snapControllerBase.StopInternal();
			snapControllerBase.gameObject.SetActive(value: false);
		}
	}

	private SnapControllerBase ResolveRotatableCopy()
	{
		if (m_RuntimeRotatableCopy != null)
		{
			return m_RuntimeRotatableCopy;
		}
		return m_RotatableCopy;
	}
}

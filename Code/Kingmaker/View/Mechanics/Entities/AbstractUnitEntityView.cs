using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Code._TmpTechArt;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.ManualCoroutines;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.Equipment;
using Kingmaker.Visual;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Visual.CharactersRigidbody;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using Kingmaker.Visual.Decals;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Updatables;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Highlighting;
using Owlcat.Runtime.Visual.OccludedObjectHighlighting;
using Pathfinding;
using UnityEngine;
using UnityEngine.AI;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.View.Mechanics.Entities;

[Serializable]
[KnowledgeDatabaseID("140695237c9c40d0b269732622d8f9fc")]
public abstract class AbstractUnitEntityView : MechanicEntityView, IAreaHandler, ISubscriber, IResource, IDetectHover, IGameModeHandler, IEntitySubscriber
{
	public class LateUpdateDriver : RegisteredObjectBase, ILateUpdatable
	{
		private readonly AbstractUnitEntityView m_Unit;

		public LateUpdateDriver(AbstractUnitEntityView unitEntityView)
		{
			m_Unit = unitEntityView;
		}

		void ILateUpdatable.DoLateUpdate()
		{
			m_Unit.DoLateUpdate();
		}
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("UnitEntityView");

	public const string SoftColliderName = "[soft collider]";

	public const string CoreColliderName = "[core collider]";

	[SerializeField]
	private float m_SoftColliderHeight = 1.8f;

	[SerializeField]
	[InspectorReadOnly]
	private CapsuleCollider m_SoftCollider;

	[SerializeField]
	[InspectorReadOnly]
	private MeshCollider m_CoreCollider;

	[SerializeField]
	public SoftColliderPlaceholder SoftColliderPlaceholder;

	public bool ForbidRotation;

	public GameObject OverrideRotatablePart;

	[Space(5f)]
	public GameObject[] Footprints;

	private ParticlesSnapMap m_ParticleSnapMap;

	private UnitMultiHighlight m_Highlighter;

	private Transform m_CenterTorso;

	private UnitAnimationManager m_AnimatorManager;

	private bool m_RenderersAndCollidersAreUpdated;

	private UnitBarksManager m_Asks;

	private StandardMaterialController m_StandardMaterialController;

	private OccludedObjectHighlighter m_OccludedObjectHighlighter;

	private UnitDismembermentManager m_DismembermentManager;

	[UsedImplicitly]
	private LateUpdateDriver m_LateUpdateDriver;

	private readonly List<FxDecal> m_Decals = new List<FxDecal>();

	private readonly CountingGuard m_MouseHighlighted = new CountingGuard();

	private TimeSpan m_StartGetUpTime;

	[InspectorReadOnly]
	private Bounds m_Bounds;

	private string m_Race;

	private BloodyFaceController m_BloodyFaceController;

	[CanBeNull]
	public RigidbodyCreatureController RigidbodyController;

	public bool HasOverriddenRotatablePart => OverrideRotatablePart != null;

	[CanBeNull]
	public Character CharacterAvatar { get; set; }

	public Animator Animator { get; private set; }

	public UnitMovementAgentBase AgentASP { get; private set; }

	public bool IsProne { get; protected set; }

	public bool IsHighlighted { get; private set; }

	public BlueprintUnit Blueprint { get; set; }

	public UnitViewMechadendritesEquipment MechadendritesEquipment { get; protected set; }

	[CanBeNull]
	public CapsuleCollider SoftCollider => m_SoftCollider;

	[CanBeNull]
	public MeshCollider CoreCollider => m_CoreCollider;

	public bool MouseHighlighted
	{
		get
		{
			return m_MouseHighlighted;
		}
		set
		{
			if (SetValueSafe(m_MouseHighlighted, value))
			{
				UpdateHighlight();
				Game.Instance.CursorController.SetUnitCursor(EntityData, value);
				if (!EntityData.Features.IsUntargetable)
				{
					EventBus.RaiseEvent(delegate(IUnitDirectHoverUIHandler h)
					{
						h.HandleHoverChange(this, value);
					});
				}
			}
			if (EntityData.IsDeadAndHasLoot)
			{
				MassLootHelper.HighlightLoot(this, value);
			}
			static bool SetValueSafe(CountingGuard guard, bool b)
			{
				if (guard.Value || b)
				{
					return guard.SetValue(b);
				}
				return false;
			}
		}
	}

	[UsedImplicitly]
	private bool HideColliderFieldsInInspector => GetType() == typeof(AbstractUnitEntityView);

	[CanBeNull]
	public new AbstractUnitEntity EntityData => (AbstractUnitEntity)base.EntityData;

	public new AbstractUnitEntity Data => (AbstractUnitEntity)base.Data;

	public virtual UnitMovementAgentBase MovementAgent => AgentASP;

	public virtual bool KeepCollidersSetupAsIs => false;

	public virtual bool UseHorizontalSoftCollider => false;

	internal virtual float HorizontalSoftColliderRadius => 1f;

	public virtual float Corpulence => 0.3f;

	public virtual bool IsInAoePattern => false;

	public override UnitBarksManager Asks => m_Asks;

	public override ParticlesSnapMap ParticlesSnapMap
	{
		get
		{
			if (!(CharacterAvatar != null))
			{
				return m_ParticleSnapMap;
			}
			return ObjectExtensions.Or(CharacterAvatar.ParticlesSnapMap, null) ?? ObjectExtensions.Or(m_ParticleSnapMap, null);
		}
	}

	public Vector2 CameraOrientedBoundsSize
	{
		get
		{
			if (m_SoftCollider == null)
			{
				return Vector2.zero;
			}
			Bounds bounds = m_SoftCollider.bounds;
			float y = bounds.max.y - base.transform.position.y;
			return new Vector2(bounds.size.x, y);
		}
	}

	public Vector2 CameraOrientedCoreBoundsSize
	{
		get
		{
			if (m_CoreCollider == null)
			{
				return Vector2.zero;
			}
			Bounds bounds = m_CoreCollider.bounds;
			float y = bounds.max.y - base.transform.position.y;
			return new Vector2(bounds.size.x, y);
		}
	}

	public bool IsGetUp
	{
		get
		{
			bool flag = (bool)AnimationManager && AnimationManager.IsStandUp;
			if (TurnController.IsInTurnBasedCombat())
			{
				return flag;
			}
			if (!flag)
			{
				return Game.Instance.TimeController.GameTime - m_StartGetUpTime < 0.5.Seconds();
			}
			return true;
		}
	}

	public float SoftColliderHeight => m_SoftColliderHeight;

	[CanBeNull]
	public UnitDismembermentManager DismembermentManager => m_DismembermentManager;

	[CanBeNull]
	public UnitAnimationManager AnimationManager
	{
		get
		{
			if (!(CharacterAvatar != null))
			{
				return m_AnimatorManager;
			}
			return CharacterAvatar.AnimationManager;
		}
	}

	[NotNull]
	public Transform CenterTorso
	{
		get
		{
			if (!m_CenterTorso)
			{
				return base.transform;
			}
			return m_CenterTorso;
		}
	}

	public bool IsCommandsPreventMovement => EntityData.Commands.PreventMovement;

	public Bounds Bounds => m_Bounds;

	public ViewInterpolationHelper InterpolationHelper { get; private set; }

	public BlockMode BlockMode
	{
		get
		{
			AbstractUnitEntity data = Data;
			if (data == null || !data.Features.CanPassThroughUnits)
			{
				return BlockMode.AllExceptSelector;
			}
			return BlockMode.Ignore;
		}
	}

	public IKController IkController { get; protected set; }

	public bool BlowUpDismember { get; protected set; }

	public bool LimbsApartDismember { get; protected set; }

	public bool LimbsApartDismembermentRestricted { get; set; }

	public virtual List<ItemEntity> Mechadendrites => null;

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.EnsureComponent<Highlighter>();
		m_Highlighter = base.gameObject.EnsureComponent<UnitMultiHighlight>();
		m_OccludedObjectHighlighter = base.gameObject.EnsureComponent<OccludedObjectHighlighter>();
		SetAgentASP();
		SetUnitLayers();
		m_Bounds = GetMaxBounds(base.gameObject, base.ViewTransform.GetComponentsInChildren<Renderer>());
		InterpolationHelper = new ViewInterpolationHelper(this);
	}

	protected override void OnDidAttachToData()
	{
		base.OnDidAttachToData();
		InterpolationHelper.ForceUpdatePosition(Data.Position, Data.Orientation);
		Blueprint = EntityData.Blueprint;
		if ((bool)AgentASP)
		{
			if (EntityData.IsStarship() && !(AgentASP is UnitMovementAgentShip))
			{
				UnityEngine.Object.DestroyImmediate(AgentASP);
				AgentASP = base.gameObject.EnsureComponent<UnitMovementAgentShip>();
			}
			if (!AgentASP.Unit)
			{
				AgentASP.Init(base.gameObject);
				EventBus.Subscribe(AgentASP);
			}
			else
			{
				AgentASP.ResetBlocker();
			}
		}
		UpdateAsks();
		Character componentInChildren = GetComponentInChildren<Character>();
		if (componentInChildren != null)
		{
			componentInChildren.OverrideAnimationSet = EntityData?.GetOptional<UnitPartVisualChange>()?.AnimationSetOverride;
		}
		CharacterAvatar = SetupCharacterAvatar(GetComponentInChildren<Character>());
		if (CharacterAvatar == null || !CharacterAvatar.ParticlesSnapMap)
		{
			m_ParticleSnapMap = GetComponentInChildren<ParticlesSnapMap>();
			if (m_ParticleSnapMap == null)
			{
				PFLog.Default.Error("EntityView " + base.name + " ParticlesSnapMap component is missing!");
			}
		}
		if (ParticlesSnapMap != null && EntityData.Blueprint != null && !ParticlesSnapMap.Initialized)
		{
			ParticlesSnapMap.Init();
			if (EntityData.Blueprint.Race != null)
			{
				ParticlesSnapMap.ParticleSizeScale *= BlueprintRoot.Instance.FxRoot.RaceFxSnapMapScaleSettings.GetCoeff(EntityData.Blueprint.Race.RaceId);
			}
		}
		Animator = GetComponentInChildren<Animator>();
		if ((bool)Animator)
		{
			Animator.EnsureComponent<UnitAnimationCallbackReceiver>();
			if (!CharacterAvatar)
			{
				m_AnimatorManager = Animator.GetComponent<UnitAnimationManager>();
				if ((bool)m_AnimatorManager)
				{
					Animator.runtimeAnimatorController = null;
				}
			}
			Animator.enabled = true;
			Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		}
		DeterminateRace();
		if (AnimationManager != null && EntityData != null)
		{
			OverrideAnimationRaceComponent component = Blueprint.GetComponent<OverrideAnimationRaceComponent>();
			if (component != null)
			{
				AnimationManager.AttachToView(this, component.BlueprintRace.Get());
			}
			else
			{
				PartUnitProgression optional = EntityData.GetOptional<PartUnitProgression>();
				AnimationManager.AttachToView(this, optional?.Race);
			}
			AnimationManager.OnAnimationSetChanged();
			AnimationManager.FireEvents = true;
			EventBus.Subscribe(AnimationManager);
		}
		SpawnFxOnStart component2 = GetComponent<SpawnFxOnStart>();
		if ((bool)component2)
		{
			component2.SpawnFx();
		}
		base.Fader = base.gameObject.EnsureComponent<EntityFader>();
		SetupSoundImportance();
		GetComponentsInChildren<NavmeshCut>().ForEach(Utils.EditorSafeDestroy);
		m_CenterTorso = ObjectExtensions.Or(ParticlesSnapMap, null)?.GetLocatorFirst(FxRoot.Instance.LocatorGroupTorsoCenterFX)?.Transform;
		if (m_CenterTorso == null)
		{
			PFLog.Default.Warning("EntityView " + base.name + " Locator group " + FxRoot.Instance.LocatorGroupTorsoCenterFX.name + " is missing or empty! Using GO transform instead");
		}
		m_DismembermentManager = GetComponent<UnitDismembermentManager>();
		SetupSelectionColliders(forceRecreate: false);
		GetComponentsInChildren(m_Decals);
		SetOccluderColorAndState();
		if ((bool)AstarPath.active)
		{
			ForcePlaceAboveGround();
		}
		if (!EntityData.LifeState.IsConscious)
		{
			IsProne = true;
		}
		if (EntityData.LifeState.IsDead)
		{
			Game.Instance.CoroutinesController.Start(SwitchCoreColliderToDeadState(), this);
		}
		if (Data.LifeState.IsFinallyDead && !Data.GetOptional<UnitPartCompanion>())
		{
			base.Fader.DisableAnimation();
		}
		IkController = GetComponentInChildren<IKController>();
		GetComponentInChildren<HumanoidRagdollManager>()?.InitHumanoidRagdoll();
		GetComponentInChildren<RigidbodyCreatureController>()?.InitRigidbodyCreatureController();
		if (EntityData.LifeState.IsConscious)
		{
			return;
		}
		if ((bool)RigidbodyController)
		{
			PartSavedRagdollState savedRagdoll = EntityData.SavedRagdoll;
			if (savedRagdoll != null && savedRagdoll.Active)
			{
				EntityData.SavedRagdoll.RestoreRagdollState(RigidbodyController);
				return;
			}
		}
		if ((bool)DismembermentManager)
		{
			SavedDismembermentState savedDismemberment = EntityData.SavedDismemberment;
			if (savedDismemberment != null && savedDismemberment.Active)
			{
				EntityData.SavedDismemberment.RestoreDismembermentState(DismembermentManager);
				return;
			}
		}
		if (AnimationManager != null)
		{
			AnimationManager.IsProne = true;
			AnimationManager.IsDead = EntityData.LifeState.IsDead;
			AnimationManager.Tick(RealTimeController.SystemStepDurationSeconds);
			AnimationManager.FastForwardProneAnimation(AnimationManager.IsDead);
			AnimationManager.FastForwardDeathAnimation();
		}
	}

	private Character SetupCharacterAvatar([CanBeNull] Character character)
	{
		if (character == null)
		{
			return null;
		}
		character.OnUpdated += CharacterAvatarUpdated;
		character.OnStart();
		return character;
	}

	private void SetupSoundImportance()
	{
		AkSoundEngine.SetRTPCValue("Importance", (EntityData.Blueprint.VisualSettings.ImportanceOverride > 0) ? EntityData.Blueprint.VisualSettings.ImportanceOverride : ((!EntityData.IsPlayerFaction) ? 1 : 3), base.gameObject);
	}

	protected override void OnWillDetachFromData()
	{
		base.OnWillDetachFromData();
		if (AnimationManager != null)
		{
			AnimationManager.FireEvents = false;
			EventBus.Unsubscribe(AnimationManager);
		}
		if (AgentASP != null)
		{
			EventBus.Unsubscribe(AgentASP);
		}
	}

	protected override void OnVisibilityChanged()
	{
		base.OnVisibilityChanged();
		if (base.IsVisible)
		{
			if (EntityData.LifeState.IsDead)
			{
				EntityData.LifeState.IsDeathRevealed = EntityData.IsInCameraFrustum;
			}
			if ((bool)m_CoreCollider)
			{
				if (!EntityData.LifeState.IsConscious)
				{
					m_CoreCollider.transform.position = CenterTorso.position;
				}
				else
				{
					m_CoreCollider.transform.position = base.ViewTransform.position;
				}
			}
		}
		foreach (FxDecal decal in m_Decals)
		{
			if ((bool)decal)
			{
				decal.enabled = base.IsVisible;
			}
		}
		UpdateHighlight();
	}

	public void SetOccluderColorAndState()
	{
		m_OccludedObjectHighlighter.Color = BlueprintRoot.Instance.FxRoot.OccluderColorDefault;
		bool flag = false;
		if (Data.IsInPlayerParty)
		{
			m_OccludedObjectHighlighter.Color = BlueprintRoot.Instance.FxRoot.OccluderColorAlly;
			flag = true;
		}
		if (Data.IsPlayerEnemy)
		{
			m_OccludedObjectHighlighter.Color = BlueprintRoot.Instance.FxRoot.OccluderColorEnemy;
			flag = EntityData.GetCombatStateOptional()?.IsInCombat ?? false;
		}
		if (!Data.IsPlayerEnemy && !Data.IsInPlayerParty)
		{
			m_OccludedObjectHighlighter.Color = BlueprintRoot.Instance.FxRoot.OccluderColorUnknownCombatant;
			flag = EntityData.GetCombatStateOptional()?.IsInCombat ?? false;
		}
		if (Data.LifeState.State == UnitLifeState.Dead)
		{
			flag = false;
		}
		m_OccludedObjectHighlighter.enabled = flag;
	}

	public void SetupSelectionColliders(bool forceRecreate)
	{
		BlueprintRoot root = BlueprintRootReferenceHelper.GetRoot();
		if (KeepCollidersSetupAsIs)
		{
			return;
		}
		if (!m_SoftCollider || !m_CoreCollider || forceRecreate)
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				if (collider.gameObject.layer == 10 || collider.name == "[soft collider]" || collider.name == "[core collider]")
				{
					Utils.EditorSafeDestroy(collider.gameObject);
				}
			}
			if ((bool)m_SoftCollider)
			{
				Utils.EditorSafeDestroy(m_SoftCollider.gameObject);
			}
			if ((bool)m_CoreCollider)
			{
				Utils.EditorSafeDestroy(m_CoreCollider.gameObject);
			}
			m_SoftCollider = new GameObject("[soft collider]").AddComponent<CapsuleCollider>();
			m_SoftCollider.transform.SetParent(base.ViewTransform, worldPositionStays: false);
			m_CoreCollider = new GameObject("[core collider]").AddComponent<MeshCollider>();
			m_CoreCollider.transform.SetParent(base.ViewTransform, worldPositionStays: false);
			m_CoreCollider.sharedMesh = root.Prefabs.UnitCoreCollider;
		}
		m_SoftCollider.gameObject.tag = "SecondarySelection";
		m_SoftCollider.gameObject.layer = 10;
		m_CoreCollider.gameObject.layer = 10;
		m_SoftCollider.transform.position = CenterTorso.transform.position;
		m_SoftCollider.transform.rotation = CenterTorso.transform.rotation;
		m_SoftCollider.transform.localScale = Vector3.one;
		m_SoftCollider.height = m_SoftColliderHeight;
		if (UseHorizontalSoftCollider)
		{
			m_SoftCollider.transform.Rotate(90f, 0f, 0f, Space.Self);
			m_SoftCollider.radius = HorizontalSoftColliderRadius;
		}
		else
		{
			m_SoftCollider.radius = Corpulence * root.Prefabs.SecondaryColliderWidthCoeff + 0.5f;
		}
		m_SoftCollider.center = Vector3.zero;
		Vector3 localScale = base.ViewTransform.localScale;
		m_CoreCollider.transform.localPosition = Vector3.zero;
		m_CoreCollider.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
		m_CoreCollider.transform.localScale = ((Mathf.Abs(localScale.x * localScale.y * localScale.z) > 0.01f) ? new Vector3((Corpulence + 0.5f) * 2f / localScale.x, (Corpulence + 0.5f) * 2f / localScale.z, root.Prefabs.CoreColliderHeight * 3.57f / localScale.y) : Vector3.one);
	}

	private void CharacterAvatarUpdated(Character avatar)
	{
		MarkRenderersAndCollidersAreUpdated();
	}

	public virtual void MarkRenderersAndCollidersAreUpdated()
	{
		if (m_RenderersAndCollidersAreUpdated)
		{
			return;
		}
		RefreshHighlighters();
		if (m_StandardMaterialController != null || TryGetComponent<StandardMaterialController>(out m_StandardMaterialController))
		{
			m_StandardMaterialController.InvalidateRenderersAndMaterials();
			if (m_BloodyFaceController == null || m_BloodyFaceController.IsDisposed)
			{
				m_BloodyFaceController = new BloodyFaceController(EntityData, m_StandardMaterialController.DissolveController);
			}
			m_BloodyFaceController.InvalidateAnimationState();
			m_BloodyFaceController.UpdateBloodValues(force: true);
		}
		m_RenderersAndCollidersAreUpdated = true;
	}

	protected void RefreshHighlighters()
	{
		if ((bool)m_Highlighter)
		{
			m_Highlighter.Highlighter?.ReinitMaterials();
		}
		if ((bool)m_OccludedObjectHighlighter)
		{
			m_OccludedObjectHighlighter.InvalidateRenderers();
		}
	}

	public void UpdateAsks()
	{
		UnitBarksManager asks = Asks;
		UnitBarksManager unitBarksManager = null;
		try
		{
			m_Asks = null;
			BlueprintUnitAsksList list = EntityData.Asks.List;
			if (list != null)
			{
				UnitAsksComponent component = list.GetComponent<UnitAsksComponent>();
				if (component != null)
				{
					unitBarksManager = (m_Asks = ((EntityData is StarshipEntity unit) ? new StarshipBarksManager(unit, component) : new UnitBarksManager(EntityData, component)));
				}
			}
		}
		finally
		{
			unitBarksManager?.LoadBanks();
			asks?.UnloadBanks();
		}
	}

	public void DeterminateRace()
	{
		if (EntityData != null && EntityData.AnimationManager != null && EntityData.AnimationManager.AnimationSet != null)
		{
			string text = EntityData.AnimationManager.AnimationSet.name;
			if (text.Contains("human", StringComparison.OrdinalIgnoreCase))
			{
				m_Race = "human";
			}
			else if (text.Contains("eldar", StringComparison.OrdinalIgnoreCase))
			{
				m_Race = "eldar";
			}
			else if (text.Contains("spaceMarine", StringComparison.OrdinalIgnoreCase))
			{
				m_Race = "spaceMarine";
			}
			else
			{
				m_Race = "empty";
			}
			string race = m_Race;
			if (!(race == "human") && !(race == "empty"))
			{
				base.gameObject.AddComponent<AddOffset>();
			}
		}
	}

	private Bounds GetMaxBounds(GameObject g, Renderer[] rend)
	{
		Bounds result = new Bounds(g.transform.position, Vector3.zero);
		foreach (Renderer renderer in rend)
		{
			result.Encapsulate(renderer.bounds);
		}
		return result;
	}

	private void SetAgentASP()
	{
		base.gameObject.EnsureComponent<UnitMovementAgent>();
		NavMeshAgent component = GetComponent<NavMeshAgent>();
		if ((bool)component)
		{
			PFLog.Default.Warning("NavMesh agent should not be used anymore", this);
			UnityEngine.Object.Destroy(component);
		}
		UnitMovementAgent[] components = GetComponents<UnitMovementAgent>();
		if (components.Length > 1)
		{
			PFLog.Default.Warning("More than one UnitMovementAgent on one unit", this);
			for (int i = 1; i < components.Length; i++)
			{
				UnityEngine.Object.DestroyImmediate(components[i]);
			}
		}
		AgentASP = GetComponent<UnitMovementAgent>();
	}

	private void SetUnitLayers()
	{
		base.gameObject.layer = LayerMask.NameToLayer("Unit");
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer obj in componentsInChildren)
		{
			obj.gameObject.layer = LayerMask.NameToLayer("Unit");
			obj.renderingLayerMask = 2u;
		}
		PFLog.TechArt.Log("Rendering Layer Mask changing for each renderer. Contour light for all characters");
	}

	public void ForcePlaceAboveGround()
	{
		if (!(EntityData.GetStateOptional()?.ControlledByDirector) && Game.Instance.CurrentlyLoadedArea != null && Game.Instance.CurrentlyLoadedArea.IsNavmeshArea && !(Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.StarSystem))
		{
			EntityData.Position = ObstacleAnalyzer.GetNearestNode(EntityData.Position, null, ObstacleAnalyzer.UnwalkableXZConstraint).position;
			base.ViewTransform.position = GetViewPositionOnGround(EntityData.Position);
		}
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		ForcePeacefulLook(peaceful: false);
		ResetHighlight();
	}

	public void HandleHoverChange(bool isHover)
	{
		MouseHighlighted = isHover;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.Default)
		{
			ResetHighlight();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public IEntity GetSubscribingEntity()
	{
		return EntityData;
	}

	public virtual void ForcePeacefulLook(bool peaceful)
	{
		if (CharacterAvatar != null)
		{
			CharacterAvatar.PeacefulMode = peaceful;
		}
	}

	private void ResetHighlight()
	{
		ResetMouseHighlighted();
	}

	public void ResetMouseHighlighted()
	{
		MassLootHelper.Clear();
		if ((bool)m_MouseHighlighted)
		{
			EventBus.RaiseEvent(delegate(IUnitDirectHoverUIHandler h)
			{
				h.HandleHoverChange(this, isHover: false);
			});
		}
		m_MouseHighlighted.Reset();
		UpdateHighlight();
		if (!TurnController.IsInTurnBasedCombat())
		{
			Game.Instance.CursorController.SetUnitCursor(EntityData, isHighlighted: false);
		}
	}

	public void UpdateHighlight(bool raiseEvent = true)
	{
		if (EntityData == null)
		{
			return;
		}
		IsHighlighted = false;
		bool isDead = EntityData.LifeState.IsDead;
		bool flag = TurnController.IsInTurnBasedCombat();
		if (!isDead || (EntityData.IsDeadAndHasLoot && !flag) || (Game.Instance.Player.UISettings.ShowInspect && !flag) || Game.Instance.SelectedAbilityHandler?.Ability?.Blueprint.GetComponent<ICanTargetDeadUnits>() != null)
		{
			InteractionHighlightController interactionHighlightController = Game.Instance.InteractionHighlightController;
			IAbstractUnitEntity entity = Game.Instance.Player.MainCharacter.Entity;
			IsHighlighted = interactionHighlightController != null && (interactionHighlightController.IsHighlighting || (bool)m_MouseHighlighted || IsInAoePattern) && EntityData.IsVisibleForPlayer && (EntityData.IsPlayerFaction || EntityData.IsPlayerEnemy || isDead || (EntityData.IsExtra && EntityData.SelectClickInteraction((BaseUnitEntity)entity) != null));
		}
		bool flag2 = Game.Instance.Player.PartyAndPets.Contains(EntityData);
		Color baseColor = Color.clear;
		if (IsHighlighted)
		{
			if (EntityData.LifeState.IsDead)
			{
				baseColor = (EntityData.LootViewed ? Game.Instance.BlueprintRoot.UIConfig.VisitedLootColor : Game.Instance.BlueprintRoot.UIConfig.StandartUnitLootColor);
			}
			else if (flag2)
			{
				baseColor = Game.Instance.BlueprintRoot.UIConfig.AllyHighlightColor;
			}
			else
			{
				IAbstractUnitEntity entity2 = Game.Instance.Player.MainCharacter.Entity;
				bool num = EntityData.IsEnemy(entity2);
				bool flag3 = EntityData.GetFactionOptional()?.Neutral ?? false;
				baseColor = (num ? Game.Instance.BlueprintRoot.UIConfig.EnemyHighlightColor : ((!flag3) ? Game.Instance.BlueprintRoot.UIConfig.NaturalHighlightColor : Game.Instance.BlueprintRoot.UIConfig.NeutralHighlightColor));
			}
		}
		m_Highlighter.BaseColor = baseColor;
		if (raiseEvent)
		{
			EventBus.RaiseEvent(delegate(IUnitHighlightUIHandler h)
			{
				h.HandleHighlightChange(this);
			});
		}
	}

	public virtual Vector3 GetViewPositionOnGround(Vector3 mechanicsPosition)
	{
		using (ProfileScope.NewScope("GetViewPositionOnGround"))
		{
			Vector3 baseViewPositionOnGround = GetBaseViewPositionOnGround(mechanicsPosition);
			return baseViewPositionOnGround + (CalcVerticalShift(baseViewPositionOnGround, MovementAgent.Corpulence * 0.85f) + EntityData.FlyHeight) * Vector3.up;
		}
		static float CalcVerticalShift(Vector3 pos, float corpulence)
		{
			for (float num = 1.5f; num <= 101.5f; num += 5f)
			{
				if (Cast(pos, num, num, corpulence, out var hitOffset2))
				{
					return hitOffset2;
				}
			}
			return 0f;
		}
		static bool Cast(Vector3 point, float offsetUp, float offsetDown, float corpulence, out float hitOffset)
		{
			if (!UnitMovementAgentBase.FallbackToRayCast)
			{
				return SphereCast(point, offsetUp, offsetDown, corpulence, out hitOffset);
			}
			return RayCast(point, offsetUp, offsetDown, out hitOffset);
		}
		static bool RayCast(Vector3 pivot, float offsetUp, float offsetDown, out float hitOffset)
		{
			if (Physics.Raycast(pivot + offsetUp * Vector3.up, Vector3.down, out var hitInfo, offsetDown + offsetUp, 2359553))
			{
				hitOffset = offsetUp - hitInfo.distance;
				return hitInfo.distance != 0f;
			}
			hitOffset = 0f;
			return false;
		}
		static bool SphereCast(Vector3 pivot, float offsetUp, float offsetDown, float corpulence, out float hitOffset)
		{
			if (Physics.SphereCast(pivot + offsetUp * Vector3.up, corpulence, Vector3.down, out var hitInfo2, offsetDown + offsetUp, 2359553))
			{
				hitOffset = offsetUp - hitInfo2.distance - corpulence;
				return hitInfo2.distance != 0f;
			}
			hitOffset = 0f;
			return false;
		}
	}

	private Vector3 GetBaseViewPositionOnGround(Vector3 mechanicsPosition)
	{
		bool flag = Data?.IsDead ?? false;
		if (flag && RigidbodyController != null && RigidbodyController.IsRagdollPositionsRestored)
		{
			return mechanicsPosition;
		}
		if ((bool)CenterTorso && (flag || (IsProne && RigidbodyController != null && RigidbodyController.RagdollWorking && (AnimationManager == null || AnimationManager.CurrentAction == null || !(AnimationManager.CurrentAction.Action is UnitAnimationActionProne unitAnimationActionProne) || !unitAnimationActionProne.AllowFallingBelowGround))))
		{
			return base.ViewTransform.position;
		}
		return mechanicsPosition + SizePathfindingHelper.GetSizePositionOffset(Data);
	}

	public void EnterProneState()
	{
		if (!IsProne)
		{
			IsProne = true;
			StopMoving();
			if ((bool)AgentASP)
			{
				ObstaclesHelper.RemoveFromGroup(AgentASP);
			}
			EntityData.Wake(6f);
			OnEnterProneState();
		}
	}

	public void LeaveProneState()
	{
		if (IsProne)
		{
			m_StartGetUpTime = Game.Instance.TimeController.GameTime;
			m_CoreCollider.transform.position = base.ViewTransform.position;
			IsProne = false;
			if ((bool)AgentASP)
			{
				ObstaclesHelper.ConnectToGroups(AgentASP);
			}
			OnExitProneState();
		}
	}

	protected virtual void OnEnterProneState()
	{
	}

	protected virtual void OnExitProneState()
	{
	}

	public void StopMoving()
	{
		if ((bool)AgentASP)
		{
			AgentASP.Stop();
		}
	}

	public override void UpdateViewActive()
	{
		base.UpdateViewActive();
		if ((bool)CharacterAvatar)
		{
			CharacterAvatar.PreventUpdate = !Data.IsViewActive;
		}
	}

	public virtual void HandleDeath()
	{
		SpawnFxOnStart component = GetComponent<SpawnFxOnStart>();
		if ((bool)component)
		{
			component.HandleUnitDeath();
		}
		if (base.IsVisible)
		{
			EntityData.LifeState.IsDeathRevealed = EntityData.IsInCameraFrustum;
			Game.Instance.CoroutinesController.Start(PlayDeathEffect(), this);
		}
		if (Data.LifeState.IsFinallyDead && !Data.GetOptional<UnitPartCompanion>())
		{
			base.Fader.DisableAnimation();
		}
		Game.Instance.CoroutinesController.Start(SwitchCoreColliderToDeadState(), this);
		SetOccluderColorAndState();
	}

	private DeathFxFromEnergyEntry LastDamageFxOptions()
	{
		DamageData damageData = EntityData?.Health.LastHandledDamage?.Damage;
		if (damageData == null)
		{
			return DeathFxFromEnergyEntry.Default;
		}
		return BlueprintRoot.Instance.FxRoot.DeathFxOptionForEnergyDamage(damageData.Type);
	}

	private IEnumerator PlayDeathEffect()
	{
		if (LastDamageFxOptions().PlayBloodPuddle)
		{
			ShieldHitEntry shieldHitEntry = BlueprintRoot.Instance.HitSystemRoot.ShieldHitEntrys.FirstOrDefault((ShieldHitEntry x) => x.Type == EntityData.ShieldType);
			GameObject effect = Blueprint.VisualSettings.GetBloodPuddle(EntityData.SurfaceType);
			if (shieldHitEntry != null && !shieldHitEntry.ShowEntityBlood)
			{
				effect = null;
			}
			bool flag = true;
			if (DismembermentHandler.ShouldDismember(Data))
			{
				UnitDismemberType dismemberType = DismembermentHandler.GetDismemberType(Data);
				if (dismemberType == UnitDismemberType.LimbsApart)
				{
					LimbsApartDismember = true;
				}
				else
				{
					GameObject dismember = Blueprint.VisualSettings.GetDismember(EntityData.SurfaceType, dismemberType);
					if (dismember != null)
					{
						effect = dismember;
						flag = false;
						AkSoundEngine.StopAll(base.gameObject);
						BlowUpDismember = true;
					}
				}
			}
			if ((bool)effect)
			{
				if (flag)
				{
					if (AnimationManager != null)
					{
						yield return null;
						while (AnimationManager.IsGoingProne)
						{
							yield return null;
						}
					}
					else if (RigidbodyController != null)
					{
						yield return YieldInstructions.WaitForSecondsGameTime(RigidbodyController.RagdollTime);
					}
				}
				GameObject obj = FxHelper.SpawnFxOnEntity(effect, this);
				UnitFxVisibilityManager.Remove(obj);
				FxHelper.RegisterBlood(obj);
			}
		}
		if (!EntityData.IsPlayerFaction && !EntityData.Features.SuppressedDecomposition && ((AnimationManager?.GetAction(UnitAnimationType.Prone) as UnitAnimationActionProne)?.AllowFallingBelowGround ?? false))
		{
			do
			{
				yield return null;
			}
			while (AnimationManager.IsGoingProne);
			if (EntityData is BaseUnitEntity baseUnitEntity)
			{
				baseUnitEntity.Inventory.DropLootToGround();
			}
			Game.Instance.EntityDestroyer.Destroy(EntityData);
		}
	}

	private IEnumerator SwitchCoreColliderToDeadState()
	{
		TimeSpan finishTime = Game.Instance.TimeController.GameTime + TimeSpan.FromSeconds(10.0);
		while (Game.Instance.TimeController.GameTime < finishTime && EntityData != null && EntityData.LifeState.IsDead)
		{
			CoreCollider.transform.position = CenterTorso.position;
			yield return null;
		}
	}

	protected override void OnDestroy()
	{
		m_Asks?.UnloadBanks();
		m_Asks = null;
		if (m_LateUpdateDriver != null)
		{
			Logger.Error("m_LateUpdateDriver is not null on destroy! Shouldn't happen!");
			m_LateUpdateDriver.Disable();
			m_LateUpdateDriver = null;
		}
		m_BloodyFaceController?.Dispose();
		HandleOnDestroy();
		base.OnDestroy();
	}

	protected virtual void HandleOnDestroy()
	{
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_LateUpdateDriver = new LateUpdateDriver(this);
		m_LateUpdateDriver.Enable();
	}

	protected override void OnDisable()
	{
		if (m_LateUpdateDriver != null)
		{
			m_LateUpdateDriver.Disable();
			m_LateUpdateDriver = null;
		}
		base.OnDisable();
	}

	public void DoLateUpdate()
	{
		if (m_RenderersAndCollidersAreUpdated && base.gameObject.activeSelf)
		{
			UpdateCachedRenderersAndColliders();
			m_RenderersAndCollidersAreUpdated = false;
		}
		OnDoLateUpdate();
		if (!(m_SoftCollider != null))
		{
			return;
		}
		if (SoftColliderPlaceholder == null || !SoftColliderPlaceholder.gameObject.activeInHierarchy)
		{
			SoftColliderPlaceholder = GetComponentInChildren<SoftColliderPlaceholder>(includeInactive: false);
		}
		Transform transform = null;
		if (SoftColliderPlaceholder != null)
		{
			transform = SoftColliderPlaceholder.transform;
		}
		if (transform == null)
		{
			transform = (KeepCollidersSetupAsIs ? null : CenterTorso);
		}
		if (transform != null)
		{
			m_SoftCollider.transform.position = transform.transform.position;
			m_SoftCollider.transform.rotation = transform.transform.rotation;
			if ((bool)SoftColliderPlaceholder && SoftColliderPlaceholder.overrideColliderParameters)
			{
				m_SoftCollider.height = SoftColliderPlaceholder.ColliderHeight;
				m_SoftCollider.radius = SoftColliderPlaceholder.ColliderRadius;
			}
			if (UseHorizontalSoftCollider && SoftColliderPlaceholder == null)
			{
				m_SoftCollider.transform.Rotate(90f, 0f, 0f, Space.Self);
			}
		}
	}

	protected virtual void OnDoLateUpdate()
	{
	}

	public bool IsMoving()
	{
		if ((bool)AgentASP)
		{
			return AgentASP.WantsToMove;
		}
		return false;
	}

	public void MoveTo(ForcedPath path, Vector3 destination, float approachRadius)
	{
		if ((bool)AgentASP)
		{
			if (Game.Instance.CurrentlyLoadedArea.IsNavmeshArea && (bool)AstarPath.active && AstarPath.active.graphs.Length != 0)
			{
				AgentASP.FollowPath(path, destination, approachRadius);
				return;
			}
			AgentASP.ForcePath(ForcedPath.Construct(new List<Vector3> { EntityData.Position, destination }));
		}
	}

	internal virtual void OnMovementStarted(Vector3 pathDestination, bool preview = false)
	{
	}

	internal virtual void OnMovementInterrupted(Vector3 destination)
	{
		EntityData.Commands.CurrentMoveTo?.Interrupt();
	}

	internal virtual void OnMovementComplete()
	{
	}

	internal virtual void OnMovementWaypointUpdate(int index)
	{
	}

	internal void OnPathNotFound()
	{
		StopMoving();
	}

	public virtual void HandleDamage()
	{
		if (m_BloodyFaceController != null && !m_BloodyFaceController.IsDisposed)
		{
			m_BloodyFaceController.UpdateBloodValues();
		}
	}

	public virtual float GetSpeedAnimationCoeff(WalkSpeedType type, bool inCombat)
	{
		return 1f;
	}
}

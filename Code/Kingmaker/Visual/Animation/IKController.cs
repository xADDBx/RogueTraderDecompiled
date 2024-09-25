using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Kingmaker.View.Equipment;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.Mounts;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.ProfilingCounters;
using Owlcat.Runtime.Core.Utility;
using RootMotion.FinalIK;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public class IKController : MonoBehaviour
{
	private class Hand
	{
		public readonly IKEffector IkEffector;

		public bool IkEnabled;

		public bool RotationEnabled;

		public Hand(IKEffector ikEffector)
		{
			IkEffector = ikEffector;
		}
	}

	public LimbIK LeftArmLimbIk;

	public LimbIK RightArmLimbIk;

	public GrounderBipedIK GrounderIK;

	private Animator m_CharacterAnimator;

	public Character CharacterSystem;

	public UnitEntityView CharacterUnitEntity;

	public bool IsDollRoom;

	public Vector3 ShieldTargetPosition;

	public Vector3 ShieldTargetRotation;

	private TimeSpan m_LastArrowLaunchedToShieldTime;

	private Hand m_RightHand;

	private Hand m_LeftHand;

	private bool m_EnableIK = true;

	private GameObject m_TorchIkTargetRight;

	private GameObject m_TorchIkTargetLeft;

	private GameObject m_ShieldIkTarget;

	private UnitAnimationManager m_AnimationManager;

	private bool m_MountConfigApplied;

	private MountOffsets mount;

	public FullBodyBipedIK BipedIk { get; private set; }

	public GrounderFBBIK GrounderIk { get; private set; }

	public bool EnableIK
	{
		get
		{
			return m_EnableIK;
		}
		set
		{
			m_EnableIK = value;
		}
	}

	private void OnEnable()
	{
		if ((bool)CharacterSystem)
		{
			CharacterSystem.OnUpdated += SetupIkSystem;
		}
	}

	private void OnDisable()
	{
		if ((bool)CharacterSystem)
		{
			CharacterSystem.OnUpdated -= SetupIkSystem;
		}
	}

	private void Start()
	{
		if ((bool)CharacterSystem)
		{
			SetupIkSystem(CharacterSystem);
		}
		else
		{
			base.enabled = false;
		}
	}

	private void SetupIkSystem(Character character)
	{
		if (!CharacterSystem)
		{
			CharacterSystem = GetComponent<Character>();
			if (!CharacterSystem)
			{
				PFLog.Default.Error("No character system in IK controller");
				return;
			}
		}
		SetupFbbik();
		SetupGrounder();
	}

	private void SetupFbbik()
	{
		if ((bool)BipedIk)
		{
			return;
		}
		m_TorchIkTargetRight = new GameObject("IK_Target_Torch_Right");
		m_TorchIkTargetRight.transform.parent = CharacterSystem.transform.FindChildRecursive("Spine_2");
		m_TorchIkTargetRight.transform.localRotation = Quaternion.Euler(-27.885f, 142.499f, -85.813f);
		m_TorchIkTargetRight.transform.localPosition = new Vector3(-0.238f, -0.48f, 0.223f);
		m_TorchIkTargetLeft = new GameObject("IK_Target_Torch_Left");
		m_TorchIkTargetLeft.transform.parent = CharacterSystem.transform.FindChildRecursive("Spine_2");
		m_TorchIkTargetLeft.transform.localRotation = Quaternion.Euler(0f, -180f, -90f);
		m_TorchIkTargetLeft.transform.localPosition = new Vector3(-0.242f, -0.397f, -0.22f);
		m_ShieldIkTarget = new GameObject("IK_Target_Shield");
		m_ShieldIkTarget.transform.parent = CharacterSystem.transform.FindChildRecursive("Spine_2");
		ShieldTargetPosition = new Vector3(-0.331f, -0.358f, -0.125f);
		m_ShieldIkTarget.transform.localPosition = ShieldTargetPosition;
		ShieldTargetRotation = new Vector3(172.1f, 10.8f, 113.8f);
		m_ShieldIkTarget.transform.localRotation = Quaternion.Euler(ShieldTargetRotation.x, ShieldTargetRotation.y, ShieldTargetRotation.z);
		m_CharacterAnimator = CharacterSystem.GetComponentInChildren(typeof(Animator)) as Animator;
		if (m_CharacterAnimator != null && (bool)m_CharacterAnimator.GetComponent<UnitAnimationManager>())
		{
			m_AnimationManager = m_CharacterAnimator.GetComponent<UnitAnimationManager>();
		}
		if (!m_CharacterAnimator)
		{
			PFLog.Default.Error("No character animator");
			return;
		}
		if (m_CharacterAnimator != null)
		{
			BipedIk = m_CharacterAnimator.gameObject.AddComponent<FullBodyBipedIK>();
		}
		BipedIk.references.root = CharacterSystem.transform.FindChildRecursive("UMA_Male_Rig");
		BipedIk.references.pelvis = CharacterSystem.transform.FindChildRecursive("Pelvis");
		BipedIk.references.leftThigh = CharacterSystem.transform.FindChildRecursive("L_Up_leg");
		BipedIk.references.rightThigh = CharacterSystem.transform.FindChildRecursive("R_Up_leg");
		BipedIk.references.leftCalf = CharacterSystem.transform.FindChildRecursive("L_leg");
		BipedIk.references.rightCalf = CharacterSystem.transform.FindChildRecursive("R_leg");
		BipedIk.references.leftFoot = CharacterSystem.transform.FindChildRecursive("L_foot");
		BipedIk.references.rightFoot = CharacterSystem.transform.FindChildRecursive("R_foot");
		BipedIk.references.leftUpperArm = CharacterSystem.transform.FindChildRecursive("L_Up_arm");
		BipedIk.references.rightUpperArm = CharacterSystem.transform.FindChildRecursive("R_Up_arm");
		BipedIk.references.leftForearm = CharacterSystem.transform.FindChildRecursive("L_ForeArm");
		BipedIk.references.rightForearm = CharacterSystem.transform.FindChildRecursive("R_ForeArm");
		BipedIk.references.leftHand = CharacterSystem.transform.FindChildRecursive("L_WeaponBone");
		BipedIk.references.rightHand = CharacterSystem.transform.FindChildRecursive("R_WeaponBone");
		BipedIk.references.head = CharacterSystem.transform.FindChildRecursive("Head");
		BipedIk.solver.rootNode = CharacterSystem.transform.FindChildRecursive("Pelvis");
		BipedIk.references.spine = new Transform[3];
		BipedIk.references.spine[0] = CharacterSystem.transform.FindChildRecursive("Spine_1");
		BipedIk.references.spine[1] = CharacterSystem.transform.FindChildRecursive("Spine_2");
		BipedIk.references.spine[2] = CharacterSystem.transform.FindChildRecursive("Spine_3");
		BipedIk.SetReferences(BipedIk.references, BipedIk.solver.rootNode);
		BipedIk.solver.SetToReferences(BipedIk.references, BipedIk.solver.rootNode);
		BipedIk.solver.iterations = 2;
		BipedIk.solver.rightHandEffector.target = m_TorchIkTargetRight.transform;
		BipedIk.solver.rightHandEffector.positionWeight = 0f;
		BipedIk.solver.rightHandEffector.rotationWeight = 0f;
		m_RightHand = new Hand(BipedIk.solver.rightHandEffector);
		BipedIk.solver.leftHandEffector.target = m_TorchIkTargetLeft.transform;
		BipedIk.solver.leftHandEffector.positionWeight = 0f;
		BipedIk.solver.leftHandEffector.rotationWeight = 0f;
		m_LeftHand = new Hand(BipedIk.solver.leftHandEffector);
		BipedIk.solver.iterations = 0;
	}

	private void SetupGrounder()
	{
		if (!GrounderIk && (bool)BipedIk)
		{
			GrounderIk = BipedIk.gameObject.AddComponent<GrounderFBBIK>();
			GrounderIk.ik = BipedIk;
			GrounderIk.solver.layers = 2359553;
			GrounderIk.solver.quality = Grounding.Quality.Fastest;
		}
	}

	private void CheckState()
	{
		if (!CharacterSystem)
		{
			PFLog.Default.Error("No CharacterSystem in " + base.gameObject.name);
			return;
		}
		if (!CharacterUnitEntity)
		{
			CharacterUnitEntity = CharacterSystem.GetComponent<UnitEntityView>();
		}
		if (!CharacterUnitEntity || CharacterUnitEntity.EntityData == null || CharacterUnitEntity.HandsEquipment == null || !BipedIk)
		{
			return;
		}
		UnitViewHandsEquipment handsEquipment = CharacterUnitEntity.HandsEquipment;
		m_EnableIK = m_EnableIK && CharacterUnitEntity.EntityData.LifeState.IsConscious && !CharacterUnitEntity.EntityData.State.IsProne && !CharacterUnitEntity.EntityData.LifeState.IsDead;
		if (m_EnableIK)
		{
			if ((bool)BipedIk)
			{
				BipedIk.enabled = true;
			}
			if ((bool)GrounderIk)
			{
				GrounderIk.enabled = !IsDollRoom;
			}
			if (Game.Instance.IsPaused && !IsDollRoom)
			{
				GrounderIk.enabled = false;
			}
			if (!(CharacterUnitEntity.EntityData?.View == null) && !CharacterUnitEntity.EntityData.Body.IsPolymorphed)
			{
				CheckStylesForIk(handsEquipment.ActiveMainHandWeaponStyle, handsEquipment.ActiveOffHandWeaponStyle, CharacterUnitEntity);
			}
		}
		else
		{
			if ((bool)BipedIk)
			{
				BipedIk.enabled = false;
			}
			if ((bool)GrounderIk)
			{
				GrounderIk.enabled = false;
			}
		}
	}

	private void SetIkLeftHand()
	{
		EquipmentOffsets equipmentOffsets = null;
		if (BipedIk != null && BipedIk.references.rightHand != null && BipedIk.references.rightHand.GetComponentInChildren<EquipmentOffsets>() != null)
		{
			equipmentOffsets = BipedIk.references.rightHand.GetComponentInChildren<EquipmentOffsets>();
		}
		if (equipmentOffsets != null && equipmentOffsets.IkTargetLeftHand != null)
		{
			m_LeftHand.IkEffector.target = equipmentOffsets.IkTargetLeftHand;
			m_LeftHand.IkEnabled = true;
			m_LeftHand.RotationEnabled = false;
		}
	}

	private void CheckStylesForIk(WeaponAnimationStyle primaryAnimStyle, WeaponAnimationStyle secondaryAnimStyle, UnitEntityView unitEntityView)
	{
		if (!unitEntityView)
		{
			return;
		}
		if (m_AnimationManager.CanRunIdleAction() && !m_AnimationManager.HasActedAnimation && CharacterSystem.IsInDollRoom && primaryAnimStyle.IsTwoHanded())
		{
			SetIkLeftHand();
		}
		else if (unitEntityView.HandsEquipment != null && unitEntityView.HandsEquipment.InCombat)
		{
			if (secondaryAnimStyle == WeaponAnimationStyle.Shield && Game.Instance.TimeController.GameTime - m_LastArrowLaunchedToShieldTime < 1.Seconds())
			{
				m_LeftHand.IkEffector.target = m_ShieldIkTarget.transform;
				m_LeftHand.IkEnabled = true;
				m_LeftHand.RotationEnabled = true;
			}
			else
			{
				m_LeftHand.IkEnabled = false;
			}
		}
		else
		{
			m_RightHand.IkEnabled = false;
			m_LeftHand.IkEnabled = false;
		}
	}

	private static void UpdateHand(Hand hand)
	{
		IKEffector ikEffector = hand.IkEffector;
		if (hand.IkEnabled)
		{
			if (ikEffector.positionWeight <= 1f)
			{
				ikEffector.positionWeight += 0.05f;
			}
			if (hand.RotationEnabled && ikEffector.rotationWeight <= 1f)
			{
				ikEffector.rotationWeight += 0.05f;
			}
			return;
		}
		if (hand.RotationEnabled && ikEffector.rotationWeight >= 0f)
		{
			ikEffector.rotationWeight -= 0.05f;
		}
		if (ikEffector.positionWeight >= 0f)
		{
			ikEffector.positionWeight -= 0.05f;
			return;
		}
		ikEffector.target = null;
		ikEffector.positionWeight = 0f;
		ikEffector.rotationWeight = 0f;
	}

	public void OffHandIKs()
	{
		m_RightHand.IkEnabled = false;
		m_LeftHand.IkEnabled = false;
		m_RightHand.IkEffector.positionWeight = 0f;
		m_RightHand.IkEffector.rotationWeight = 0f;
		m_LeftHand.IkEffector.positionWeight = 0f;
		m_LeftHand.IkEffector.rotationWeight = 0f;
	}

	public void ArrowLaunchedToShield()
	{
		m_LastArrowLaunchedToShieldTime = Game.Instance.TimeController.GameTime;
	}

	public void SetupIkForMountedCombat()
	{
		if ((bool)mount.PelvisIkTarget && (bool)mount.Root)
		{
			BipedIk.solver.headMapping.maintainRotationWeight = 1f;
			BipedIk.references.root.transform.localPosition = mount.Root.localPosition;
			BipedIk.references.pelvis.position = mount.PelvisIkTarget.position;
			BipedIk.references.pelvis.rotation = mount.PelvisIkTarget.rotation;
			BipedIk.solver.bodyEffector.target = mount.PelvisIkTarget;
			BipedIk.solver.bodyEffector.positionWeight = 0.6f;
			BipedIk.solver.bodyEffector.rotationWeight = 1f;
		}
		if ((bool)mount.LeftFootIkTarget)
		{
			BipedIk.solver.leftFootEffector.target = mount.LeftFootIkTarget;
			BipedIk.solver.leftFootEffector.positionWeight = 1f;
			BipedIk.solver.leftFootEffector.rotationWeight = 1f;
		}
		if ((bool)mount.LeftKneeIkTarget)
		{
			BipedIk.solver.leftLegChain.bendConstraint.bendGoal = mount.LeftKneeIkTarget;
			BipedIk.solver.leftLegChain.bendConstraint.weight = 1f;
		}
		if ((bool)mount.RightFootIkTarget)
		{
			BipedIk.solver.rightFootEffector.target = mount.RightFootIkTarget;
			BipedIk.solver.rightFootEffector.positionWeight = 1f;
			BipedIk.solver.rightFootEffector.rotationWeight = 1f;
		}
		if ((bool)mount.RightKneeIkTarget)
		{
			BipedIk.solver.rightLegChain.bendConstraint.bendGoal = mount.RightKneeIkTarget;
			BipedIk.solver.rightLegChain.bendConstraint.weight = 1f;
		}
		if ((bool)mount.Hands)
		{
			if (!CharacterUnitEntity.EntityData.IsInCombat)
			{
				BipedIk.solver.leftHandEffector.target = mount.Hands;
				BipedIk.solver.leftHandEffector.positionWeight = 1f;
				BipedIk.solver.leftArmMapping.weight = 0.7f;
				BipedIk.solver.rightHandEffector.target = mount.Hands;
				BipedIk.solver.rightHandEffector.positionWeight = 1f;
				BipedIk.solver.rightArmMapping.weight = 0.7f;
			}
			else
			{
				BipedIk.solver.leftHandEffector.target = null;
				BipedIk.solver.leftHandEffector.positionWeight = 0f;
				BipedIk.solver.leftArmMapping.weight = 0.3f;
				BipedIk.solver.rightHandEffector.target = null;
				BipedIk.solver.rightHandEffector.positionWeight = 0f;
				BipedIk.solver.rightArmMapping.weight = 0.3f;
			}
		}
		GrounderIk.enabled = false;
	}

	public void MountedCombatIk(BaseUnitEntity saddledUnit)
	{
		if (saddledUnit.View.GetComponent<MountOffsets>() == null)
		{
			UberDebug.Log("NOT on mount " + saddledUnit.View.name);
			return;
		}
		mount = saddledUnit.View.GetComponent<MountOffsets>();
		Size size = saddledUnit.State.Size;
		mount.SetSizeConfig(size);
		RaceMountOffsetsConfig.MountOffsetData mountOffsets = mount.GetMountOffsets(CharacterUnitEntity.EntityData.Blueprint.Race);
		if (mountOffsets == null)
		{
			UberDebug.LogError("Can't find MountOffsetConfig for " + mount.name + ". Owner = " + saddledUnit.View.name);
			return;
		}
		if (!m_MountConfigApplied)
		{
			m_MountConfigApplied = mount.ApplyConfigToMount(mount, mountOffsets);
		}
		if (m_MountConfigApplied)
		{
			SetupIkForMountedCombat();
		}
	}

	private bool FirstNotNullRendererIsVisible()
	{
		if (CharacterUnitEntity.Renderers.Any((Renderer x) => x != null))
		{
			return !CharacterUnitEntity.Renderers.FirstOrDefault((Renderer x) => x != null).isVisible;
		}
		return false;
	}

	private void Update()
	{
		using (Counters.IK?.Measure())
		{
			if (BipedIk == null)
			{
				return;
			}
			if (CharacterUnitEntity.Renderers.Count <= 0 || FirstNotNullRendererIsVisible())
			{
				BipedIk.enabled = false;
				GrounderIk.enabled = false;
				return;
			}
			BaseUnitEntity baseUnitEntity = CharacterUnitEntity.Or(null)?.EntityData?.GetSaddledUnit();
			if (!IsDollRoom && baseUnitEntity?.View != null)
			{
				BipedIk.enabled = true;
				BipedIk.solver.iterations = 1;
				MountedCombatIk(baseUnitEntity);
				return;
			}
			BipedIk.references.root.transform.localPosition = Vector3.zero;
			BipedIk.references.root.transform.localScale = Vector3.one;
			BipedIk.solver.iterations = 0;
			BipedIk.solver.bodyEffector.positionWeight = 0f;
			BipedIk.solver.bodyEffector.rotationWeight = 0f;
			BipedIk.solver.leftFootEffector.positionWeight = 0f;
			BipedIk.solver.leftFootEffector.rotationWeight = 0f;
			BipedIk.solver.rightFootEffector.positionWeight = 0f;
			BipedIk.solver.rightFootEffector.rotationWeight = 0f;
			BipedIk.solver.leftLegChain.bendConstraint.weight = 0f;
			BipedIk.solver.rightLegChain.bendConstraint.weight = 0f;
			m_MountConfigApplied = false;
			CheckState();
			if (m_RightHand != null)
			{
				UpdateHand(m_RightHand);
			}
			if (m_LeftHand != null)
			{
				UpdateHand(m_LeftHand);
			}
		}
	}

	private void OnDrawGizmos()
	{
	}
}

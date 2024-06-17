using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Visual.CharactersRigidbody;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public class AnimSnapToCloth : MonoBehaviour
{
	[Serializable]
	public class Joint
	{
		public GameObject SnappedToClothJoint;

		public int PositionNumberPoint;

		public int AimNumberPoint;

		public GameObject SkinnedJoint;

		public GameObject AnimatedJoint;

		public GameObject EmptyObjectForClothChange;

		public Vector3 ClothRotateAdd;
	}

	public GameObject GameObjectWithCloth;

	public Animator AnimatorForSpecialClips;

	public float TimeBlendIntoAnimation = 1f;

	public float TimeBlendFromAnimation = 1f;

	public string TriggerToIdle = "TRIGGER_IDLE";

	public Joint[] Joints;

	[HideInInspector]
	public AnimationManager AnimManager;

	[HideInInspector]
	public GameObject RootCharacter;

	[HideInInspector]
	public bool GotInfo;

	private Cloth m_cloth;

	private Vector3 m_PositionRoot;

	private bool m_AnimPlaying;

	private float m_IdleWeight;

	private float m_BlendIntoIdle;

	private float m_BlendFromIdle;

	private float m_StartTimeIntoIdle;

	private float m_StartTimeFromIdle;

	private bool m_BlendIs;

	private bool m_GotInfo;

	private string m_CurrentClip;

	private bool m_RagdollStart;

	public void InitializeAnimSnapToCloth()
	{
		FindAnimManagerInFathersAndRoot();
		GotInfo = true;
	}

	private void Start()
	{
		m_cloth = GameObjectWithCloth.GetComponent<Cloth>();
		if (m_cloth == null)
		{
			PFLog.Default.Error("GameObject with cloth does not have a cloth");
		}
		m_AnimPlaying = true;
		m_IdleWeight = 1f;
		m_StartTimeIntoIdle = -1f;
		m_StartTimeFromIdle = -1f;
		m_BlendIntoIdle = TimeBlendIntoAnimation;
		m_BlendFromIdle = TimeBlendFromAnimation;
	}

	private void OnValidate()
	{
		Start();
	}

	private void Update()
	{
		if (!GotInfo && !m_GotInfo && (double)Time.time >= 0.1)
		{
			FindAnimManagerInFathersAndRoot();
			m_GotInfo = true;
		}
		if (AnimManager == null || AnimManager.ActiveActions.Count == 0 || m_cloth == null || RootCharacter == null)
		{
			return;
		}
		CalculateIdleWeight();
		Joint[] joints = Joints;
		foreach (Joint joint in joints)
		{
			if (joint.PositionNumberPoint <= m_cloth.vertices.Length)
			{
				joint.SnappedToClothJoint.transform.position = m_cloth.transform.TransformPoint(m_cloth.vertices[joint.PositionNumberPoint]);
			}
			if (joint.AimNumberPoint <= m_cloth.vertices.Length)
			{
				joint.SnappedToClothJoint.transform.LookAt(m_cloth.transform.TransformPoint(m_cloth.vertices[joint.AimNumberPoint]));
			}
			joint.EmptyObjectForClothChange.transform.position = joint.SnappedToClothJoint.transform.position;
			joint.EmptyObjectForClothChange.transform.rotation = joint.SnappedToClothJoint.transform.rotation;
		}
		joints = Joints;
		foreach (Joint joint2 in joints)
		{
			joint2.EmptyObjectForClothChange.transform.localRotation = Quaternion.Euler(joint2.EmptyObjectForClothChange.transform.localRotation.eulerAngles + joint2.ClothRotateAdd);
		}
		joints = Joints;
		foreach (Joint joint3 in joints)
		{
			joint3.SkinnedJoint.transform.position = Vector3.Lerp(joint3.EmptyObjectForClothChange.transform.position, joint3.AnimatedJoint.transform.position, m_IdleWeight);
			joint3.SkinnedJoint.transform.rotation = Quaternion.Slerp(joint3.EmptyObjectForClothChange.transform.rotation, joint3.AnimatedJoint.transform.rotation, m_IdleWeight);
		}
	}

	private void CalculateIdleWeight()
	{
		bool flag;
		if (!m_RagdollStart)
		{
			RigidbodyCreatureController componentInParent = GetComponentInParent<RigidbodyCreatureController>();
			if (componentInParent != null && componentInParent.RagdollWorking)
			{
				m_RagdollStart = true;
				flag = true;
				TriggerState("TRIGGER_death_death", 1f);
			}
			else
			{
				flag = IsIdleOrSpecialStatePlaying();
			}
		}
		else
		{
			flag = true;
		}
		if (m_AnimPlaying == flag)
		{
			if (!m_BlendIs)
			{
				return;
			}
		}
		else
		{
			m_BlendIs = false;
		}
		m_AnimPlaying = flag;
		if (m_AnimPlaying)
		{
			float num = ((!(m_BlendFromIdle < TimeBlendFromAnimation)) ? 0f : m_IdleWeight);
			if (!m_BlendIs)
			{
				m_StartTimeIntoIdle = Time.time;
				m_BlendIs = true;
			}
			m_BlendIntoIdle = Time.time - m_StartTimeIntoIdle;
			if (m_BlendIntoIdle >= TimeBlendIntoAnimation)
			{
				m_BlendIs = false;
			}
			m_IdleWeight = num + m_BlendIntoIdle / TimeBlendIntoAnimation * (1f - num);
		}
		else if (!m_AnimPlaying)
		{
			float num2 = ((!(m_BlendIntoIdle < TimeBlendIntoAnimation)) ? 1f : m_IdleWeight);
			if (!m_BlendIs)
			{
				m_StartTimeFromIdle = Time.time;
				m_BlendIs = true;
			}
			m_BlendFromIdle = Time.time - m_StartTimeFromIdle;
			if (m_BlendFromIdle >= TimeBlendFromAnimation)
			{
				m_BlendIs = false;
			}
			m_IdleWeight = num2 - m_BlendFromIdle / TimeBlendFromAnimation;
		}
	}

	private void FindAnimManagerInFathersAndRoot()
	{
		if (AnimManager != null && RootCharacter != null)
		{
			return;
		}
		GameObject gameObject = base.gameObject;
		while (!(gameObject.transform.parent?.gameObject == null))
		{
			gameObject = gameObject.transform.parent.gameObject;
			if (!(gameObject.GetComponent<AnimationManager>() == null))
			{
				AnimManager = gameObject.GetComponent<AnimationManager>();
				RootCharacter = gameObject.transform.parent.gameObject ?? gameObject;
				break;
			}
		}
	}

	private void TriggerState(string trigger, float speed)
	{
		if (m_CurrentClip != trigger)
		{
			m_CurrentClip = trigger;
			AnimatorForSpecialClips.SetTrigger(trigger);
			AnimatorForSpecialClips.speed = speed;
		}
	}

	private bool IsSpecialStatePlaying()
	{
		AnimSnapToClothAnimationSettings tailAnimationSettings = BlueprintRoot.Instance.CharGenRoot.TailAnimationSettings;
		if (tailAnimationSettings == null)
		{
			return false;
		}
		for (int num = AnimManager.ActiveActions.Count - 1; num > 0; num--)
		{
			AnimSnapToClothAnimationSettings.ClipsPlaySpecialAnim[] clipsWhichPlaySpecialAnimation = tailAnimationSettings.ClipsWhichPlaySpecialAnimation;
			foreach (AnimSnapToClothAnimationSettings.ClipsPlaySpecialAnim clipsPlaySpecialAnim in clipsWhichPlaySpecialAnimation)
			{
				if (clipsPlaySpecialAnim.Clip == AnimManager.ActiveActions[num].ActiveAnimation.GetActiveClip())
				{
					TriggerState(clipsPlaySpecialAnim.Trigger, clipsPlaySpecialAnim.Speed);
					return true;
				}
			}
			AnimSnapToClothAnimationSettings.ClipsPlaySimpleAnim[] clipsWhichPlaySimpleAnimationLikeIdle = tailAnimationSettings.ClipsWhichPlaySimpleAnimationLikeIdle;
			foreach (AnimSnapToClothAnimationSettings.ClipsPlaySimpleAnim clipsPlaySimpleAnim in clipsWhichPlaySimpleAnimationLikeIdle)
			{
				if (clipsPlaySimpleAnim.Clip == AnimManager.ActiveActions[num].ActiveAnimation.GetActiveClip())
				{
					if (m_CurrentClip != TriggerToIdle)
					{
						m_CurrentClip = TriggerToIdle;
						AnimatorForSpecialClips.SetTrigger(TriggerToIdle);
					}
					AnimatorForSpecialClips.speed = clipsPlaySimpleAnim.Speed;
					return true;
				}
			}
		}
		return false;
	}

	private bool IsIdleOrSpecialStatePlaying()
	{
		if (AnimManager.ActiveActions[0].ActiveAnimation.GetWeight() != 1f)
		{
			return IsSpecialStatePlaying();
		}
		if (m_PositionRoot == RootCharacter.transform.position)
		{
			if (m_CurrentClip != TriggerToIdle)
			{
				m_CurrentClip = TriggerToIdle;
				AnimatorForSpecialClips.SetTrigger(TriggerToIdle);
			}
			AnimatorForSpecialClips.speed = 1f;
			return true;
		}
		m_PositionRoot = RootCharacter.transform.position;
		return false;
	}
}

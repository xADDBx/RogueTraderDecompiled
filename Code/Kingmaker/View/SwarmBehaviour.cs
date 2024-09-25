using System;
using System.Collections;
using System.Reflection;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.CharactersRigidbody;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.View;

public class SwarmBehaviour : MonoBehaviour
{
	[CanBeNull]
	private static readonly MethodInfo s_RebindMethod;

	private static Transform s_DeadBodiesRoot;

	public GameObject[] SwarmObjects;

	public bool OrientWhenMoving;

	public float AttackAnimationTime = 1f;

	public SwarmDeathType DeathType;

	public SwarmElementDirection ElementDirection;

	public float RagdollTime = 3f;

	public Vector3 KickUpVector = Vector3.zero;

	public Vector3 Torque = Vector3.zero;

	public float ImpulseValue;

	public float RandomImpulseMin;

	public float RandomImpulseMax;

	[CanBeNull]
	private Animator m_Animator;

	[CanBeNull]
	private Animator[] m_Animators;

	[CanBeNull]
	private UnitEntityView m_Unit;

	private int m_AliveCount;

	private bool m_WasOriented;

	private RigidbodyCreatureController.ImpulseData m_LastImpulse;

	private static Transform DeadBodiesRoot
	{
		get
		{
			if (s_DeadBodiesRoot == null)
			{
				GameObject gameObject = new GameObject("[Swarm Bodies]");
				Game.Instance.DynamicRoot?.Add(gameObject.transform);
				s_DeadBodiesRoot = gameObject.transform;
			}
			return s_DeadBodiesRoot;
		}
	}

	static SwarmBehaviour()
	{
		s_RebindMethod = typeof(Animator).GetMethod("Rebind", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(bool) }, null);
	}

	private void Awake()
	{
		SwarmObjects.Shuffle(PFStatefulRandom.View);
		m_Animator = GetComponentInChildren<Animator>();
		m_Animators = GetComponentsInChildren<Animator>();
		m_Unit = GetComponentInParent<UnitEntityView>();
		m_AliveCount = SwarmObjects.Length;
	}

	private void LateUpdate()
	{
		if (!(m_Unit == null))
		{
			if (OrientWhenMoving && m_Unit.MovementAgent.IsReallyMoving && m_Unit.EntityData.Movable.PreviousSimulationTick.HasMotion)
			{
				Quaternion rot = Quaternion.Euler(0f, m_Unit.EntityData.Orientation, 0f);
				ApplyRotation(rot, local: false);
				m_WasOriented = true;
			}
			else if (m_WasOriented)
			{
				ApplyRotation(Quaternion.identity, local: true);
				m_WasOriented = false;
			}
		}
	}

	private void ApplyRotation(Quaternion rot, bool local)
	{
		for (int i = 0; i < SwarmObjects.Length && i < m_AliveCount; i++)
		{
			GameObject gameObject = SwarmObjects[i];
			if (gameObject == null)
			{
				continue;
			}
			Rigidbody componentInChildren = gameObject.GetComponentInChildren<Rigidbody>(includeInactive: true);
			if (!(componentInChildren == null))
			{
				if (local)
				{
					componentInChildren.transform.localRotation = rot;
				}
				else
				{
					componentInChildren.transform.rotation = rot;
				}
			}
		}
	}

	public void UpdateHealth(float percent)
	{
		int num = (int)(percent * (float)SwarmObjects.Length);
		if (num < 0)
		{
			num = 0;
		}
		if (num == 0 && percent > 0f)
		{
			num = 1;
		}
		for (int i = num; i < m_AliveCount; i++)
		{
			PlayDeath(SwarmObjects[i]);
		}
		for (int j = m_AliveCount; j < num; j++)
		{
			PlayReanimation(SwarmObjects[j]);
		}
		if (DeathType != 0 && num < m_AliveCount)
		{
			if (m_Unit != null)
			{
				m_Unit.MarkRenderersAndCollidersAreUpdated();
			}
			if (m_Animator != null && s_RebindMethod != null)
			{
				s_RebindMethod.Invoke(m_Animator, new object[1] { false });
			}
		}
		if (DeathType == SwarmDeathType.Disable || m_AliveCount > num)
		{
			m_AliveCount = num;
		}
	}

	private void PlayDeath(GameObject go)
	{
		Vector3 vector = Vector3.forward;
		switch (DeathType)
		{
		case SwarmDeathType.Disable:
			go.SetActive(value: false);
			break;
		case SwarmDeathType.Ragdoll:
		{
			go.transform.parent = DeadBodiesRoot;
			Rigidbody[] componentsInChildren = go.GetComponentsInChildren<Rigidbody>(includeInactive: true);
			Rigidbody[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: true);
			}
			if (componentsInChildren[0] != null)
			{
				switch (ElementDirection)
				{
				case SwarmElementDirection.Forward:
					vector = componentsInChildren[0].transform.forward;
					break;
				case SwarmElementDirection.Backward:
					vector = -componentsInChildren[0].transform.forward;
					break;
				case SwarmElementDirection.Right:
					vector = componentsInChildren[0].transform.right;
					break;
				case SwarmElementDirection.Left:
					vector = -componentsInChildren[0].transform.right;
					break;
				case SwarmElementDirection.Up:
					vector = componentsInChildren[0].transform.up;
					break;
				case SwarmElementDirection.Down:
					vector = -componentsInChildren[0].transform.up;
					break;
				}
				if (m_LastImpulse != null && Game.Instance.TimeController.RealTime - m_LastImpulse.Time < 0.2f.Seconds())
				{
					float num = ImpulseValue + PFStatefulRandom.Swarm.Range(RandomImpulseMin, RandomImpulseMax) * (1f + m_LastImpulse.MagnitudeModifier);
					RigidbodyCreatureController.ApplyImpulseToRagdoll(componentsInChildren[0], num * m_LastImpulse.Direction, KickUpVector + vector, zeroVerticalVector: true, Torque);
					StartCoroutine(StopRigidbodyCoroutine(componentsInChildren, RagdollTime));
				}
			}
			break;
		}
		}
		UpdateSnapBone(go, enable: false);
	}

	private static IEnumerator StopRigidbodyCoroutine(Rigidbody[] rbs, float ragdollTime)
	{
		yield return new WaitForSeconds(ragdollTime);
		foreach (Rigidbody rigidbody in rbs)
		{
			if (rigidbody != null)
			{
				rigidbody.gameObject.SetActive(value: false);
			}
		}
	}

	private void PlayReanimation(GameObject go)
	{
		if (DeathType != SwarmDeathType.Ragdoll)
		{
			if (DeathType == SwarmDeathType.Disable)
			{
				go.SetActive(value: true);
			}
			UpdateSnapBone(go, enable: true);
		}
	}

	private void UpdateSnapBone(GameObject go, bool enable)
	{
		SnapMapBase componentInChildren = GetComponentInChildren<SnapMapBase>();
		if (componentInChildren == null)
		{
			return;
		}
		foreach (FxBone bone in componentInChildren.Bones)
		{
			if (IsSameSwarmObject(bone.Transform, go.transform))
			{
				if (enable)
				{
					bone.Flags &= ~FxBoneFlags.Disabled;
				}
				else
				{
					bone.Flags |= FxBoneFlags.Disabled;
				}
			}
		}
	}

	private bool IsSameSwarmObject(Transform t1, Transform t2)
	{
		Transform transform = base.transform;
		Transform transform2 = t1;
		while (transform2 != null && transform2 != transform)
		{
			if (transform2 == t2)
			{
				return true;
			}
			transform2 = transform2.parent;
		}
		transform2 = t2;
		while (transform2 != null && transform2 != transform)
		{
			if (transform2 == t1)
			{
				return true;
			}
			transform2 = transform2.parent;
		}
		return false;
	}

	public void PlayAttack([NotNull] MechanicsContext context, [NotNull] ActionList attackActions)
	{
		StartCoroutine(PlayAttackCoroutine(context, attackActions));
	}

	private IEnumerator PlayAttackCoroutine([NotNull] MechanicsContext context, [NotNull] ActionList attackActions)
	{
		PlayAttackAnimation();
		yield return new WaitForSeconds(AttackAnimationTime);
		using (context.GetDataScope())
		{
			attackActions.Run();
		}
	}

	private void PlayAttackAnimation()
	{
		if (m_Animators != null)
		{
			Animator[] animators = m_Animators;
			for (int i = 0; i < animators.Length; i++)
			{
				animators[i].SetTrigger("Attack");
			}
		}
	}

	public void ApplyImpulse(Vector3 direction, float addMagnitude)
	{
		m_LastImpulse = m_LastImpulse ?? new RigidbodyCreatureController.ImpulseData();
		m_LastImpulse.Direction = direction;
		m_LastImpulse.MagnitudeModifier = addMagnitude;
		m_LastImpulse.Time = Game.Instance.TimeController.RealTime;
	}
}

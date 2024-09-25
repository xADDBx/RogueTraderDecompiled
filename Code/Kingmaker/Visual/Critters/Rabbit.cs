using System.Collections;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Visual.Critters;

[RequireComponent(typeof(CritterMoveAgent))]
public class Rabbit : MonoBehaviour
{
	private CritterMoveAgent m_MoveAgent;

	private Animator m_Animator;

	public float RoamRadius = 10f;

	public bool Fearless;

	public float MinIdleTime = 4f;

	public float MaxIdleTime = 8f;

	public float RoamChance = 0.5f;

	public float IdleCrossfade = 0.25f;

	private Vector3 m_StartPos;

	private int m_ThinkFrame;

	private float m_NextIdleTime;

	private static int s_Count;

	private bool m_Frightened;

	private bool m_PriorityPath;

	private bool isSleeping;

	private bool wasSleeping;

	private Coroutine routine;

	private Transform cameraRig;

	private static StatefulRandom Random => PFStatefulRandom.NonDeterministic;

	public void Start()
	{
		m_StartPos = base.transform.position;
		m_MoveAgent = GetComponent<CritterMoveAgent>();
		m_MoveAgent.Init();
		m_Animator = GetComponentInChildren<Animator>();
		if (!m_Animator.runtimeAnimatorController)
		{
			PFLog.Default.Error(this, "No controller on " + this);
			Object.Destroy(base.gameObject);
			return;
		}
		s_Count++;
		m_ThinkFrame = s_Count % 5;
		routine = StartCoroutine(Think());
		cameraRig = CameraRig.Instance.transform;
	}

	private IEnumerator Think()
	{
		while ((bool)this)
		{
			if (Time.frameCount % 5 != m_ThinkFrame)
			{
				yield return null;
				continue;
			}
			if (m_MoveAgent.WantsToMove)
			{
				if (!m_Frightened)
				{
					MaybeRunAway();
				}
				yield return null;
				continue;
			}
			if (m_Frightened)
			{
				m_Frightened = false;
				m_MoveAgent.MaxSpeed /= 2f;
			}
			if (m_PriorityPath)
			{
				m_Animator.CrossFadeInFixedTime("Idle" + Random.Range(1, 3), IdleCrossfade);
				m_PriorityPath = false;
				yield return new WaitForSeconds(Random.Range(4f, 10f));
			}
			MaybeRunAway();
			if (Time.time < m_NextIdleTime || m_MoveAgent.WantsToMove)
			{
				yield return null;
			}
			else if (Random.value < RoamChance)
			{
				Vector2 v = Random.insideUnitCircle.normalized * Random.Range(0f, RoamRadius);
				Vector3 vector = m_StartPos + v.To3D();
				vector = base.transform.position + Vector3.ClampMagnitude(vector - base.transform.position, Random.Range(2f, Mathf.Max(3f, RoamRadius)));
				vector = ObstacleAnalyzer.GetNearestNode(vector).position;
				if (Vector3.Distance(m_StartPos, vector) > 1f)
				{
					m_MoveAgent.PathTo(vector);
				}
			}
			else
			{
				m_Animator.CrossFadeInFixedTime("Idle" + Random.Range(1, 3), IdleCrossfade);
				m_NextIdleTime = Time.time + Random.Range(MinIdleTime, MaxIdleTime);
			}
		}
	}

	private void MaybeRunAway()
	{
		if (Fearless || m_PriorityPath)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		Vector3 position = base.transform.position;
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			if (allUnit.LifeState.IsConscious && VectorMath.SqrDistanceXZ(allUnit.Position, position) < 16f)
			{
				zero += position - allUnit.Position;
			}
		}
		if (zero == Vector3.zero)
		{
			return;
		}
		zero.Normalize();
		zero = Quaternion.Euler(0f, Random.Range(-45f, 45f), 0f) * zero;
		Vector3 position2 = ObstacleAnalyzer.GetNearestNode(position + zero * RoamRadius).position;
		if (!(VectorMath.SqrDistanceXZ(position2, position) < 1f))
		{
			m_MoveAgent.PathTo(position2);
			if (!m_Frightened)
			{
				m_MoveAgent.MaxSpeed *= 2f;
				m_Frightened = true;
			}
		}
	}

	private void Update()
	{
		if (Time.frameCount % 6 == 0)
		{
			if (Vector3.Distance(cameraRig.position, base.transform.position) >= 25f)
			{
				wasSleeping = isSleeping;
				isSleeping = true;
			}
			else
			{
				wasSleeping = isSleeping;
				isSleeping = false;
			}
			if (isSleeping && !wasSleeping)
			{
				StopCoroutine(routine);
				wasSleeping = true;
			}
			else if (!isSleeping && wasSleeping)
			{
				routine = StartCoroutine(Think());
				wasSleeping = false;
			}
		}
		if (!isSleeping && !(AstarData.active == null))
		{
			m_MoveAgent.TickMovement(Time.deltaTime);
			m_Animator.SetFloat("Speed", m_MoveAgent.Speed);
		}
	}

	public void OnDrawGizmosSelected()
	{
		DebugDraw.DrawCircle(Application.isPlaying ? m_StartPos : base.transform.position, Vector3.up, RoamRadius, new Color(0.62f, 0.81f, 0.73f), DebugDraw.DepthTestType.Alpha);
	}

	public void SetPriorityPath(Vector3 targetPoint)
	{
		if (m_Frightened)
		{
			m_Frightened = false;
			m_MoveAgent.MaxSpeed /= 2f;
		}
		m_MoveAgent.PathTo(targetPoint);
		m_PriorityPath = true;
	}
}

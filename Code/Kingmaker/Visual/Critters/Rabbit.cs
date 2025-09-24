using System.Collections;
using Kingmaker.Controllers;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.ManualCoroutines;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Visual.Critters;

[RequireComponent(typeof(CritterMoveAgent))]
public class Rabbit : MonoBehaviour, IUpdatable, IInterpolatable
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

	private const int ThinkFrameCount = 3;

	private int m_ThinkFrame;

	private float m_NextIdleTime;

	private static int s_Count;

	private bool m_Frightened;

	private bool m_PriorityPath;

	private bool isSleeping;

	private bool wasSleeping;

	private CoroutineHandler routine;

	private Transform cameraRig;

	private bool m_Initialized;

	private Vector3 m_PrevPosition;

	private Vector3 m_CurrPosition;

	private Vector2 m_PrevForward;

	private Vector2 m_CurrForward;

	private static StatefulRandom Random => PFStatefulRandom.NonDeterministic;

	private static int CurrentTick => Game.Instance.RealTimeController.CurrentNetworkTick;

	private static float CurrentTime => (float)Game.Instance.TimeController.GameTime.TotalSeconds;

	private void OnEnable()
	{
		Game.Instance.CustomUpdateController.Add(this);
		Game.Instance.InterpolationController.Add(this);
		Init(force: false);
	}

	private void OnDisable()
	{
		Game.Instance.CustomUpdateController.Remove(this);
		Game.Instance.InterpolationController.Remove(this);
	}

	public void Init(bool force)
	{
		if (!force && !m_Initialized)
		{
			m_Initialized = true;
			m_PrevPosition = (m_CurrPosition = base.transform.position);
			m_PrevForward = (m_CurrForward = base.transform.forward.To2D().normalized);
			m_StartPos = m_PrevPosition;
			m_MoveAgent = GetComponent<CritterMoveAgent>();
			m_MoveAgent.Init(m_CurrPosition, m_CurrForward);
			m_Animator = GetComponentInChildren<Animator>();
			if (!m_Animator.runtimeAnimatorController)
			{
				PFLog.Default.Error(this, "No controller on " + this);
				Object.Destroy(base.gameObject);
				return;
			}
			s_Count++;
			m_ThinkFrame = s_Count % 3;
			routine = Game.Instance.CoroutinesController.Start(Think());
			cameraRig = CameraRig.Instance.transform;
		}
	}

	private IEnumerator Think()
	{
		while ((bool)this)
		{
			if (CurrentTick % 3 != m_ThinkFrame)
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
				yield return YieldInstructions.WaitForSecondsGameTime(Random.Range(4f, 10f));
			}
			MaybeRunAway();
			if (CurrentTime < m_NextIdleTime || m_MoveAgent.WantsToMove)
			{
				yield return null;
			}
			else if (Random.value < RoamChance)
			{
				Vector2 v = Random.insideUnitCircle.normalized * Random.Range(0f, RoamRadius);
				Vector3 vector = m_StartPos + v.To3D();
				vector = m_CurrPosition + Vector3.ClampMagnitude(vector - m_CurrPosition, Random.Range(2f, Mathf.Max(3f, RoamRadius)));
				vector = ObstacleAnalyzer.GetNearestNode(vector).position;
				if (Vector3.Distance(m_StartPos, vector) > 1f)
				{
					m_MoveAgent.PathTo(m_CurrPosition, vector);
				}
			}
			else
			{
				m_Animator.CrossFadeInFixedTime("Idle" + Random.Range(1, 3), IdleCrossfade);
				m_NextIdleTime = CurrentTime + Random.Range(MinIdleTime, MaxIdleTime);
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
		Vector3 currPosition = m_CurrPosition;
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			if (allUnit.LifeState.IsConscious && VectorMath.SqrDistanceXZ(allUnit.Position, currPosition) < 16f)
			{
				zero += currPosition - allUnit.Position;
			}
		}
		if (zero == Vector3.zero)
		{
			return;
		}
		zero.Normalize();
		zero = Quaternion.Euler(0f, Random.Range(-45f, 45f), 0f) * zero;
		Vector3 position = ObstacleAnalyzer.GetNearestNode(currPosition + zero * RoamRadius).position;
		if (!(VectorMath.SqrDistanceXZ(position, currPosition) < 1f))
		{
			m_MoveAgent.PathTo(m_CurrPosition, position);
			if (!m_Frightened)
			{
				m_MoveAgent.MaxSpeed *= 2f;
				m_Frightened = true;
			}
		}
	}

	void IUpdatable.Tick(float delta)
	{
		if (CurrentTick % 4 == 0)
		{
			if (cameraRig == null || Vector3.Distance(cameraRig.position, m_CurrPosition) >= 25f)
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
				Game.Instance.CoroutinesController.Stop(ref routine);
				wasSleeping = true;
			}
			else if (!isSleeping && wasSleeping)
			{
				routine = Game.Instance.CoroutinesController.Start(Think());
				wasSleeping = false;
			}
		}
		if (!isSleeping && !(AstarData.active == null))
		{
			m_MoveAgent.TickMovement(delta);
			m_Animator.SetFloat("Speed", m_MoveAgent.Speed);
			m_PrevPosition = m_CurrPosition;
			m_CurrPosition = m_MoveAgent.Position;
			m_PrevForward = m_CurrForward;
			m_CurrForward = m_MoveAgent.Forward;
		}
	}

	void IInterpolatable.Tick(float progress)
	{
		Vector3 vector = Vector3.LerpUnclamped(m_PrevPosition, m_CurrPosition, progress);
		Vector2 v = Vector2.LerpUnclamped(m_PrevForward, m_CurrForward, progress);
		base.transform.position = vector;
		base.transform.LookAt(vector + v.To3D());
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
		m_MoveAgent.PathTo(m_CurrPosition, targetPoint);
		m_PriorityPath = true;
	}
}

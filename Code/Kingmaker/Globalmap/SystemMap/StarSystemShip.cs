using System.Collections.Generic;
using Kingmaker.View;
using Kingmaker.View.GlobalMap;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

[RequireComponent(typeof(UnitMovementAgentStarSystem))]
public class StarSystemShip : MonoBehaviour
{
	[SerializeField]
	private Transform mechanicsTransform;

	[SerializeField]
	private StarSystemStarshipView view;

	[SerializeField]
	private float m_MaxSpeedOverride;

	[SerializeField]
	private float m_ApproachStarSystemObjectRadius;

	public Vector3 Position
	{
		get
		{
			return mechanicsTransform.position;
		}
		set
		{
			if (!(mechanicsTransform.position == value))
			{
				mechanicsTransform.position = value;
			}
		}
	}

	public Quaternion Rotation
	{
		get
		{
			return mechanicsTransform.rotation;
		}
		set
		{
			if (!(mechanicsTransform.rotation == value))
			{
				mechanicsTransform.rotation = value;
			}
		}
	}

	public UnitMovementAgentStarSystem Agent { get; set; }

	public Vector3 ViewPosition => view.Position;

	public StarSystemObjectView StarSystemObjectLandOn { get; set; }

	private void Awake()
	{
		Agent = GetComponent<UnitMovementAgentStarSystem>();
		Agent.Init(base.gameObject, this);
		Agent.MaxSpeedOverride = m_MaxSpeedOverride;
	}

	public void ApplyRotation()
	{
		Vector3 forward = Agent.MoveDirection.To3D();
		if (forward.sqrMagnitude > 0.01f)
		{
			Rotation = Quaternion.Euler(0f, Quaternion.LookRotation(forward).eulerAngles.y, 0f);
		}
	}

	public void DrawPath(List<Vector3> path)
	{
		view.DrawPath(path);
	}

	public void UndrawPath()
	{
		view.UndrawPath();
	}

	public void CheckLanding()
	{
		Collider[] array = Physics.OverlapSphere(Position, m_ApproachStarSystemObjectRadius + ShipPathHelper.Delta);
		for (int i = 0; i < array.Length; i++)
		{
			StarSystemObjectView component = array[i].gameObject.GetComponent<StarSystemObjectView>();
			if ((bool)component && component.CheckLanding())
			{
				StarSystemObjectLandOn = component;
				break;
			}
		}
	}
}

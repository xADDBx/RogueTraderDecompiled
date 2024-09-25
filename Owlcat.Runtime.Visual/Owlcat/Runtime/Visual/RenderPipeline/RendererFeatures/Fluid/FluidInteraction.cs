using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Fluid;

public class FluidInteraction : MonoBehaviour
{
	private Vector3 m_PrevPos;

	private Vector2 m_MovementForce;

	public float MovementForceScale = 1f;

	public float Size = 1f;

	[Range(0f, 1f)]
	public float LerpFromSideDirToRadialDir = 1f;

	[Range(0f, 1f)]
	public float LerpToMovementDir = 1f;

	public static HashSet<FluidInteraction> All { get; private set; }

	static FluidInteraction()
	{
		All = new HashSet<FluidInteraction>();
	}

	public Vector2 GetMovementForce()
	{
		return m_MovementForce;
	}

	private void OnEnable()
	{
		All.Add(this);
		m_PrevPos = base.transform.position;
	}

	private void OnDisable()
	{
		All.Remove(this);
	}

	private void Update()
	{
		m_MovementForce = (base.transform.position - m_PrevPos).To2D() * MovementForceScale;
		m_PrevPos = base.transform.position;
	}
}

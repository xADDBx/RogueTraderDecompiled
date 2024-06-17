using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public class Gravity : MonoBehaviour
{
	private GravityForce m_Gravity;

	public float3 GravityVector = new float3(0f, -9.8f, 0f);

	private void OnEnable()
	{
		if (m_Gravity == null)
		{
			m_Gravity = new GravityForce();
		}
		PBD.RegisterForce(m_Gravity);
	}

	private void OnDisable()
	{
		PBD.UnregisterForce(m_Gravity);
	}

	private void Update()
	{
		m_Gravity.GravityVector = GravityVector;
	}
}

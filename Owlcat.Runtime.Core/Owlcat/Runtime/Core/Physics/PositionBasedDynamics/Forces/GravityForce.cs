using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;

public class GravityForce : IForce
{
	public static int _Gravity = Shader.PropertyToID("_Gravity");

	public float3 GravityVector;

	public Body Body { get; private set; }

	public string ComputeShaderKernel => "Gravity";

	public GravityForce(Body body, float3 gravity)
	{
		Body = body;
		GravityVector = gravity;
	}

	public GravityForce(float3 gravity)
		: this(null, gravity)
	{
	}

	public GravityForce(Body body)
		: this(body, new float3(0f, -9.81f, 0f))
	{
	}

	public GravityForce()
		: this(null, new float3(0f, -9.81f, 0f))
	{
	}

	public void SetupShader(ComputeShader shader, CommandBuffer cmd)
	{
		cmd.SetComputeVectorParam(shader, _Gravity, (Vector3)GravityVector);
	}

	public bool IsActive()
	{
		return math.length(GravityVector) > 0f;
	}

	public void SetupSimulationJob(ref SimulationJob job)
	{
		if (IsActive())
		{
			job.Gravity += GravityVector;
		}
	}
}

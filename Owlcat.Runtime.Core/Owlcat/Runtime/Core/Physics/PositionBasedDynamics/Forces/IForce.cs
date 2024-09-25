using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;

public interface IForce
{
	Body Body { get; }

	string ComputeShaderKernel { get; }

	bool IsActive();

	void SetupShader(ComputeShader shader, CommandBuffer cmd);

	void SetupSimulationJob(ref SimulationJob job);
}

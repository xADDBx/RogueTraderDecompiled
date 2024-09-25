using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;

public class WindForce : IForce
{
	private static int _StrengthNoiseWeight = Shader.PropertyToID("_StrengthNoiseWeight");

	private static int _StrengthNoiseContrast = Shader.PropertyToID("_StrengthNoiseContrast");

	private static int _WindVector = Shader.PropertyToID("_WindVector");

	private static int _GlobalWindEnabled = Shader.PropertyToID("_GlobalWindEnabled");

	private static int _CompressedStrengthOctaves = Shader.PropertyToID("_CompressedStrengthOctaves");

	private static int _CompressedShiftOctaves = Shader.PropertyToID("_CompressedShiftOctaves");

	public float2 WindVector;

	public float StrengthNoiseWeight;

	public float StrengthNoiseContrast;

	public NativeArray<float4> CompressedStrengthOctaves;

	public NativeArray<float4> CompressedShiftOctaves;

	private Vector4[] m_CompressedStrengthOctaves;

	private Vector4[] m_CompressedShiftOctaves;

	public Body Body { get; private set; }

	public string ComputeShaderKernel => "WindPerlinNoise";

	public bool IsActive()
	{
		return math.length(WindVector) > 0f;
	}

	public void SetupShader(ComputeShader shader, CommandBuffer cmd)
	{
		if (IsActive())
		{
			cmd.SetComputeFloatParam(shader, _GlobalWindEnabled, 1f);
			cmd.SetComputeFloatParam(shader, _StrengthNoiseWeight, StrengthNoiseWeight);
			cmd.SetComputeFloatParam(shader, _StrengthNoiseContrast, StrengthNoiseContrast);
			cmd.SetComputeVectorParam(shader, _WindVector, (Vector2)WindVector);
			if (m_CompressedStrengthOctaves == null || m_CompressedStrengthOctaves.Length != CompressedStrengthOctaves.Length)
			{
				m_CompressedStrengthOctaves = new Vector4[CompressedStrengthOctaves.Length];
			}
			for (int i = 0; i < m_CompressedStrengthOctaves.Length; i++)
			{
				m_CompressedStrengthOctaves[i] = CompressedStrengthOctaves[i];
			}
			cmd.SetComputeVectorArrayParam(shader, _CompressedStrengthOctaves, m_CompressedStrengthOctaves);
			if (m_CompressedShiftOctaves == null || m_CompressedShiftOctaves.Length != CompressedShiftOctaves.Length)
			{
				m_CompressedShiftOctaves = new Vector4[CompressedShiftOctaves.Length];
			}
			for (int j = 0; j < m_CompressedShiftOctaves.Length; j++)
			{
				m_CompressedShiftOctaves[j] = CompressedShiftOctaves[j];
			}
			cmd.SetComputeVectorArrayParam(shader, _CompressedShiftOctaves, m_CompressedShiftOctaves);
		}
	}

	internal void DisableGlobalWind()
	{
		Shader.SetGlobalFloat(_GlobalWindEnabled, 0f);
	}

	public void SetupGlobalShaderParameters()
	{
		Shader.SetGlobalFloat(_GlobalWindEnabled, 1f);
		Shader.SetGlobalFloat(_StrengthNoiseWeight, StrengthNoiseWeight);
		Shader.SetGlobalFloat(_StrengthNoiseContrast, StrengthNoiseContrast);
		Shader.SetGlobalVector(_WindVector, (Vector2)WindVector);
		if (m_CompressedStrengthOctaves == null || m_CompressedStrengthOctaves.Length != CompressedStrengthOctaves.Length)
		{
			m_CompressedStrengthOctaves = new Vector4[CompressedStrengthOctaves.Length];
		}
		for (int i = 0; i < m_CompressedStrengthOctaves.Length; i++)
		{
			m_CompressedStrengthOctaves[i] = CompressedStrengthOctaves[i];
		}
		Shader.SetGlobalVectorArray(_CompressedStrengthOctaves, m_CompressedStrengthOctaves);
		if (m_CompressedShiftOctaves == null || m_CompressedShiftOctaves.Length != CompressedShiftOctaves.Length)
		{
			m_CompressedShiftOctaves = new Vector4[CompressedShiftOctaves.Length];
		}
		for (int j = 0; j < m_CompressedShiftOctaves.Length; j++)
		{
			m_CompressedShiftOctaves[j] = CompressedShiftOctaves[j];
		}
		Shader.SetGlobalVectorArray(_CompressedShiftOctaves, m_CompressedShiftOctaves);
	}

	public void SetupSimulationJob(ref SimulationJob job)
	{
		if (IsActive())
		{
			job.StrengthNoiseWeight = StrengthNoiseWeight;
			job.StrengthNoiseContrast = StrengthNoiseContrast;
			job.WindVector = WindVector;
			job.CompressedStrengthOctaves = CompressedStrengthOctaves;
			job.CompressedShiftOctaves = CompressedShiftOctaves;
		}
	}
}

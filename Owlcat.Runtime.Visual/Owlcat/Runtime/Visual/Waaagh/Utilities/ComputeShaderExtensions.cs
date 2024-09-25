using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Utilities;

public static class ComputeShaderExtensions
{
	public static ComputeShaderKernelDescriptor GetKernelDescriptor(this ComputeShader shader, string kernelName)
	{
		ComputeShaderKernelDescriptor result = default(ComputeShaderKernelDescriptor);
		result.Index = shader.FindKernel(kernelName);
		if (result.Index > -1)
		{
			shader.GetKernelThreadGroupSizes(result.Index, out result.ThreadGroupSize.x, out result.ThreadGroupSize.y, out result.ThreadGroupSize.z);
		}
		return result;
	}
}

using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Data;

[Serializable]
public class StencilStateData
{
	public bool OverrideStencilState;

	public int StencilReference;

	public CompareFunction StencilCompareFunction = CompareFunction.Always;

	public StencilOp PassOperation;

	public StencilOp FailOperation;

	public StencilOp ZFailOperation;
}

using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class VFXPreparePassData : PassDataBase
{
	public Camera Camera;

	public CullingResults CullingResults;
}

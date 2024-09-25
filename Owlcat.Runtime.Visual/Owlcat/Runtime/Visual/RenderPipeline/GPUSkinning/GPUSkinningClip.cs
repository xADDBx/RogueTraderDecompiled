using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.GPUSkinning;

public class GPUSkinningClip : ScriptableObject
{
	public AnimationClip Clip;

	public GameObject AnimatorPrefab;

	public int BakeFPS = 15;

	public GPUSkinningClipData Data;
}

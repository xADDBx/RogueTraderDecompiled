using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUSkinning;

[CreateAssetMenu(fileName = "GPUSkinningAnimation", menuName = "GPU Animation/GPU Skinning Animation")]
public class GPUSkinningAnimation : ScriptableObject
{
	[SerializeField]
	[HideInInspector]
	private string m_PrefabId;

	[Range(1f, 120f)]
	public int BakeFPS = 15;

	public int BonesCount;

	public List<GPUAnimationClip> AnimationClips = new List<GPUAnimationClip>();

	public Texture2D AnimationTexture;

	public string PrefabId
	{
		get
		{
			return m_PrefabId;
		}
		set
		{
			m_PrefabId = value;
		}
	}
}

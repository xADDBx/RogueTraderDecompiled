using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.Decals;

[ExecuteInEditMode]
public class FullScreenDecal : MonoBehaviour
{
	[SerializeField]
	private Material m_Material;

	public Material Material
	{
		get
		{
			return m_Material;
		}
		set
		{
			m_Material = value;
		}
	}

	public static HashSet<FullScreenDecal> All { get; private set; } = new HashSet<FullScreenDecal>();


	private void OnEnable()
	{
		_ = m_Material == null;
		All.Add(this);
	}

	private void OnDisable()
	{
		All.Remove(this);
	}
}

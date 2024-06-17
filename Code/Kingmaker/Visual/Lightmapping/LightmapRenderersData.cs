using System.Collections.Generic;
using Kingmaker.Utility.GuidUtility;
using UnityEngine;

namespace Kingmaker.Visual.Lightmapping;

[ExecuteInEditMode]
public class LightmapRenderersData : MonoBehaviour
{
	public static readonly List<LightmapRenderersData> Instances = new List<LightmapRenderersData>();

	[SerializeField]
	[HideInInspector]
	private string m_Id;

	[SerializeField]
	[HideInInspector]
	public Renderer[] Renderers;

	public string Id
	{
		get
		{
			if (!string.IsNullOrEmpty(m_Id))
			{
				return m_Id;
			}
			return m_Id = Uuid.Instance.CreateString();
		}
	}

	private void OnEnable()
	{
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}
}

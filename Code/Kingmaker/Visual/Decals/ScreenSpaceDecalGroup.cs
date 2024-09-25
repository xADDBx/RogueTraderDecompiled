using System;
using UnityEngine;

namespace Kingmaker.Visual.Decals;

public class ScreenSpaceDecalGroup : MonoBehaviour
{
	protected const string c_NameColorProperty = "_Color";

	[NonSerialized]
	protected ScreenSpaceDecal[] m_Decals;

	[SerializeField]
	protected Color m_GroupColor = Color.white;

	public Color GroupColor
	{
		get
		{
			return m_GroupColor;
		}
		set
		{
			m_GroupColor = value;
			ApplyMultiplication();
		}
	}

	public float GroupAlpha
	{
		get
		{
			return m_GroupColor.a;
		}
		set
		{
			m_GroupColor.a = value;
			ApplyMultiplication();
		}
	}

	private void OnTransformChildrenChanged()
	{
		m_Decals = GetComponentsInChildren<ScreenSpaceDecal>(includeInactive: true);
	}

	private void OnEnable()
	{
		m_Decals = GetComponentsInChildren<ScreenSpaceDecal>(includeInactive: true);
	}

	public void ApplyMultiplication()
	{
		if (m_Decals == null)
		{
			return;
		}
		int num = m_Decals.Length;
		for (int i = 0; i < num; i++)
		{
			ScreenSpaceDecal screenSpaceDecal = m_Decals[i];
			if (!(screenSpaceDecal == null) && !(screenSpaceDecal.SharedMaterial == null))
			{
				screenSpaceDecal.MaterialProperties.SetColor("_Color", screenSpaceDecal.SharedMaterial.GetColor("_Color") * m_GroupColor);
			}
		}
	}
}

using System.Collections.Generic;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UI.Common;

[ExecuteInEditMode]
public class CameraFollowTasksSceneHelper : MonoBehaviour
{
	private static readonly List<CameraFollowTasksSceneHelper> Instances = new List<CameraFollowTasksSceneHelper>(2);

	[SerializeField]
	private RectTransform m_SafeRect;

	[SerializeField]
	private bool m_AutoUpdateInEditor;

	[SerializeField]
	private bool m_UseOffsets;

	[ShowIf("m_UseOffsets")]
	[SerializeField]
	private RectOffset m_Offsets;

	[HideIf("m_UseOffsets")]
	[SerializeField]
	private Rect m_Rect;

	private RectTransform m_ParentRect;

	private Vector2 m_SavedParentSize;

	public static CameraFollowTasksSceneHelper Instance => Instances.LastOrDefault();

	private RectTransform ParentRect
	{
		get
		{
			if (m_ParentRect == null)
			{
				m_ParentRect = m_SafeRect.parent as RectTransform;
			}
			return m_ParentRect;
		}
	}

	public Rect SafeRect { get; private set; }

	private void OnEnable()
	{
		Instances.AddUnique(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}

	private void Update()
	{
		if (m_SavedParentSize != ParentRect.rect.size)
		{
			m_SavedParentSize = ParentRect.rect.size;
			UpdateSafeRect();
		}
	}

	private void UpdateSafeRect()
	{
		SafeRect = GetSafeRect();
		m_SafeRect.anchoredPosition = new Vector2(SafeRect.x * m_SavedParentSize.x, (0f - SafeRect.y) * m_SavedParentSize.y);
		m_SafeRect.anchorMin = Vector2.up;
		m_SafeRect.anchorMax = Vector2.up;
		m_SafeRect.pivot = Vector2.up;
		m_SafeRect.sizeDelta = new Vector2(SafeRect.width * m_SavedParentSize.x, SafeRect.height * m_SavedParentSize.y);
	}

	private Rect GetSafeRect()
	{
		if (!m_UseOffsets)
		{
			return m_Rect;
		}
		Vector2 position = new Vector2((float)m_Offsets.left / m_SavedParentSize.x, (float)m_Offsets.top / m_SavedParentSize.y);
		Vector2 size = new Vector2((m_SavedParentSize.x - (float)m_Offsets.left - (float)m_Offsets.right) / m_SavedParentSize.x, (m_SavedParentSize.y - (float)m_Offsets.top - (float)m_Offsets.bottom) / m_SavedParentSize.y);
		return new Rect(position, size);
	}
}

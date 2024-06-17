using System;
using UniRx;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;

[Serializable]
public class VirtualListLayoutElementSettings
{
	public enum LayoutOverrideType
	{
		None,
		Custom,
		UnityLayout
	}

	[SerializeField]
	private LayoutOverrideType m_OverrideType;

	[SerializeField]
	private VirtualListLayoutPadding m_Padding;

	[SerializeField]
	private bool m_OverrideWidth;

	[SerializeField]
	private float m_Width;

	[SerializeField]
	private bool m_OverrideHeight;

	[SerializeField]
	private float m_Height;

	private BoolReactiveProperty m_IsDirty = new BoolReactiveProperty();

	public LayoutOverrideType OverrideType => m_OverrideType;

	public static VirtualListLayoutElementSettings None { get; } = new VirtualListLayoutElementSettings
	{
		m_OverrideType = LayoutOverrideType.None
	};


	public bool OverrideWidth
	{
		get
		{
			return m_OverrideWidth;
		}
		set
		{
			if (value != m_OverrideWidth)
			{
				m_OverrideWidth = value;
				m_IsDirty.Value = true;
			}
		}
	}

	public float Width
	{
		get
		{
			return m_Width;
		}
		set
		{
			if (value != m_Width)
			{
				m_Width = value;
				m_IsDirty.Value = true;
			}
		}
	}

	public bool OverrideHeight
	{
		get
		{
			return m_OverrideHeight;
		}
		set
		{
			if (value != m_OverrideHeight)
			{
				m_OverrideHeight = value;
				m_IsDirty.Value = true;
			}
		}
	}

	public float Height
	{
		get
		{
			return m_Height;
		}
		set
		{
			if (value != m_Height)
			{
				m_Height = value;
				m_IsDirty.Value = true;
			}
		}
	}

	public VirtualListLayoutPadding Padding
	{
		get
		{
			return m_Padding;
		}
		set
		{
			m_Padding = value;
			m_IsDirty.Value = false;
		}
	}

	public IReadOnlyReactiveProperty<bool> IsDirty => m_IsDirty;

	public void MarkUpdated()
	{
		m_IsDirty.Value = false;
	}

	public void CopyFrom(VirtualListLayoutElementSettings original)
	{
		m_OverrideType = original.OverrideType;
		Padding = original.Padding;
		m_OverrideWidth = original.m_OverrideWidth;
		m_Width = original.m_Width;
		m_OverrideHeight = original.m_OverrideHeight;
		m_Height = original.m_Height;
		m_IsDirty.Value = original.m_IsDirty.Value;
	}

	public void ResetToDefault()
	{
		if (this != None)
		{
			Padding = VirtualListLayoutPadding.Zero;
			m_OverrideWidth = false;
			m_Width = 0f;
			m_OverrideHeight = false;
			m_Height = 0f;
			m_IsDirty.Value = false;
		}
	}

	public void SetDirty()
	{
		m_IsDirty.Value = true;
	}
}

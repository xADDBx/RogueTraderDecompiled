using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

public class DebugDisplaySettingsShadows : IDebugDisplaySettingsData
{
	private static class Strings
	{
		public static DebugUI.Widget.NameAndTooltip AtlasOccupancy = new DebugUI.Widget.NameAndTooltip
		{
			name = "Atlas Occupancy",
			tooltip = "Shows debug layer for shadow atlas occupancy."
		};

		public static DebugUI.Widget.NameAndTooltip ViewAtlas = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show Shadow Buffer",
			tooltip = "Shows debug layer for shadow buffer."
		};

		public static DebugUI.Widget.NameAndTooltip BufferScale = new DebugUI.Widget.NameAndTooltip
		{
			name = "Buffer Scale",
			tooltip = "Debug buffer scale."
		};

		public static DebugUI.Widget.NameAndTooltip ColorMultiplier = new DebugUI.Widget.NameAndTooltip
		{
			name = "Color Multiplier",
			tooltip = "Debug color multiplier."
		};

		public static DebugUI.Widget.NameAndTooltip OccupiedColor = new DebugUI.Widget.NameAndTooltip
		{
			name = "Atlas Occupied Nodes",
			tooltip = "The color of occupied nodes in debug layer"
		};

		public static DebugUI.Widget.NameAndTooltip PartiallyOccupiedColor = new DebugUI.Widget.NameAndTooltip
		{
			name = "Atlas Partially Occupied Nodes",
			tooltip = "The color of partially occupied nodes in debug layer"
		};

		public static DebugUI.Widget.NameAndTooltip OccupiedInHierarchyColor = new DebugUI.Widget.NameAndTooltip
		{
			name = "Atlas OccupiedInHierarchy Nodes",
			tooltip = "The color of occupied in hierarchy nodes in debug layer"
		};
	}

	private static class WidgetFactory
	{
		internal static DebugUI.Widget CreateDebugAtlasOccupancy(DebugDisplaySettingsShadows data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.AtlasOccupancy,
				autoEnum = typeof(DebugShadowBufferType),
				getter = () => (int)data.DebugAtlasOccupancy,
				setter = delegate
				{
				},
				getIndex = () => (int)data.DebugAtlasOccupancy,
				setIndex = delegate(int value)
				{
					data.DebugAtlasOccupancy = (DebugShadowBufferType)value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugShadowBufferType(DebugDisplaySettingsShadows data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.ViewAtlas,
				autoEnum = typeof(DebugShadowBufferType),
				getter = () => (int)data.DebugViewAtlas,
				setter = delegate
				{
				},
				getIndex = () => (int)data.DebugViewAtlas,
				setIndex = delegate(int value)
				{
					data.DebugViewAtlas = (DebugShadowBufferType)value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugBufferScale(DebugDisplaySettingsShadows data)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.BufferScale,
				getter = () => data.DebugBufferScale,
				setter = delegate(float value)
				{
					data.DebugBufferScale = value;
				},
				min = () => 0.1f,
				max = () => 1f
			};
		}

		internal static DebugUI.Widget CreateDebugColorMultiplier(DebugDisplaySettingsShadows data)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.ColorMultiplier,
				getter = () => data.DebugColorMultiplier,
				setter = delegate(float value)
				{
					data.DebugColorMultiplier = value;
				},
				min = () => 1f,
				max = () => 100f
			};
		}

		internal static DebugUI.Widget CreateOccupiedColor(DebugDisplaySettingsShadows data)
		{
			return new DebugUI.ColorField
			{
				nameAndTooltip = Strings.OccupiedColor,
				getter = () => data.DebugOccupiedColor,
				setter = delegate(Color value)
				{
					data.DebugOccupiedColor = value;
				},
				hdr = false,
				showAlpha = false,
				showPicker = true
			};
		}

		internal static DebugUI.Widget CreatePartiallyOccupiedColor(DebugDisplaySettingsShadows data)
		{
			return new DebugUI.ColorField
			{
				nameAndTooltip = Strings.PartiallyOccupiedColor,
				getter = () => data.DebugPartiallyOccupiedColor,
				setter = delegate(Color value)
				{
					data.DebugPartiallyOccupiedColor = value;
				},
				hdr = false,
				showAlpha = false,
				showPicker = true
			};
		}

		internal static DebugUI.Widget CreateOccupiedInHierarchyColor(DebugDisplaySettingsShadows data)
		{
			return new DebugUI.ColorField
			{
				nameAndTooltip = Strings.OccupiedInHierarchyColor,
				getter = () => data.DebugOccupiedInHierarchy,
				setter = delegate(Color value)
				{
					data.DebugOccupiedInHierarchy = value;
				},
				hdr = false,
				showAlpha = false,
				showPicker = true
			};
		}
	}

	private class SettingsPanel : DebugDisplaySettingsPanel
	{
		public override string PanelName => "Shadows";

		public SettingsPanel(DebugDisplaySettingsShadows data)
		{
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Shadows Debug Modes",
				isHeader = true,
				opened = true,
				children = 
				{
					WidgetFactory.CreateDebugAtlasOccupancy(data),
					WidgetFactory.CreateOccupiedColor(data),
					WidgetFactory.CreatePartiallyOccupiedColor(data),
					WidgetFactory.CreateOccupiedInHierarchyColor(data),
					WidgetFactory.CreateDebugShadowBufferType(data),
					WidgetFactory.CreateDebugBufferScale(data),
					WidgetFactory.CreateDebugColorMultiplier(data)
				}
			});
		}
	}

	private WaaaghDebugData m_DebugData;

	public DebugShadowBufferType DebugAtlasOccupancy
	{
		get
		{
			return m_DebugData.ShadowsDebug.AtlasOccupancy;
		}
		set
		{
			m_DebugData.ShadowsDebug.AtlasOccupancy = value;
		}
	}

	public DebugShadowBufferType DebugViewAtlas
	{
		get
		{
			return m_DebugData.ShadowsDebug.ViewAtlas;
		}
		set
		{
			m_DebugData.ShadowsDebug.ViewAtlas = value;
		}
	}

	public float DebugBufferScale
	{
		get
		{
			return m_DebugData.ShadowsDebug.DebugScale;
		}
		set
		{
			m_DebugData.ShadowsDebug.DebugScale = value;
		}
	}

	public float DebugColorMultiplier
	{
		get
		{
			return m_DebugData.ShadowsDebug.DebugColorMultiplier;
		}
		set
		{
			m_DebugData.ShadowsDebug.DebugColorMultiplier = value;
		}
	}

	public Color DebugOccupiedColor
	{
		get
		{
			return m_DebugData.ShadowsDebug.AtlasNodesOccupied;
		}
		set
		{
			m_DebugData.ShadowsDebug.AtlasNodesOccupied = value;
		}
	}

	public Color DebugPartiallyOccupiedColor
	{
		get
		{
			return m_DebugData.ShadowsDebug.AtlasNodesPartiallyOccupied;
		}
		set
		{
			m_DebugData.ShadowsDebug.AtlasNodesPartiallyOccupied = value;
		}
	}

	public Color DebugOccupiedInHierarchy
	{
		get
		{
			return m_DebugData.ShadowsDebug.AtlasNodesOccupiedInHierarchy;
		}
		set
		{
			m_DebugData.ShadowsDebug.AtlasNodesOccupiedInHierarchy = value;
		}
	}

	public DebugDisplaySettingsShadows(WaaaghDebugData debugData)
	{
		m_DebugData = debugData;
	}

	public IDebugDisplaySettingsPanelDisposable CreatePanel()
	{
		return new SettingsPanel(this);
	}
}

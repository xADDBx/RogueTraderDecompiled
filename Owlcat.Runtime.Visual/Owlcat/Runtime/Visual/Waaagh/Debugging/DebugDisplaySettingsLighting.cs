using Owlcat.ShaderLibrary.Visual.Debug;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

public class DebugDisplaySettingsLighting : IDebugDisplaySettingsData
{
	private static class Strings
	{
		public static readonly DebugUI.Widget.NameAndTooltip LightingDebugMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Lighting Debug Mode",
			tooltip = "Use the drop-down to select which lighting and shadow debug information to overlay on the screen."
		};

		public static readonly DebugUI.Widget.NameAndTooltip ClustersDebugMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Clusters Debug Mode",
			tooltip = "Use the drop-down to select which clusters debug information to overlay on the screen."
		};

		public static readonly DebugUI.Widget.NameAndTooltip LightingFeatures = new DebugUI.Widget.NameAndTooltip
		{
			name = "Lighting Features",
			tooltip = "Filter and debug selected lighting features in the system."
		};

		public static readonly DebugUI.Widget.NameAndTooltip ShowLightSortingCurve = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show Light Sorting Curve",
			tooltip = "Use it to see the lights sorting curve."
		};

		public static readonly DebugUI.Widget.NameAndTooltip LightSortingCurveColorStart = new DebugUI.Widget.NameAndTooltip
		{
			name = "Curve Color Start",
			tooltip = "Curve Color."
		};

		public static readonly DebugUI.Widget.NameAndTooltip LightSortingCurveColorEnd = new DebugUI.Widget.NameAndTooltip
		{
			name = "Curve Color End",
			tooltip = "Curve Color."
		};
	}

	public static class WidgetFactory
	{
		internal static DebugUI.Widget CreateLightingDebugMode(DebugDisplaySettingsLighting data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.LightingDebugMode,
				autoEnum = typeof(DebugLightingMode),
				getter = () => (int)data.DebugLightingMode,
				setter = delegate
				{
				},
				getIndex = () => (int)data.DebugLightingMode,
				setIndex = delegate(int value)
				{
					data.DebugLightingMode = (DebugLightingMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateClustersDebugMode(DebugDisplaySettingsLighting data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.ClustersDebugMode,
				autoEnum = typeof(DebugClustersMode),
				getter = () => (int)data.DebugClustersMode,
				setter = delegate
				{
				},
				getIndex = () => (int)data.DebugClustersMode,
				setIndex = delegate(int value)
				{
					data.DebugClustersMode = (DebugClustersMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateShowLightSortingCurve(DebugDisplaySettingsLighting data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.ShowLightSortingCurve,
				getter = () => data.ShowLightSortingCurve,
				setter = delegate(bool value)
				{
					data.ShowLightSortingCurve = value;
				}
			};
		}

		internal static DebugUI.Widget CreateLightSortingCurveColorStart(DebugDisplaySettingsLighting data)
		{
			return new DebugUI.ColorField
			{
				nameAndTooltip = Strings.LightSortingCurveColorStart,
				getter = () => data.LightSortingCurveColorStart,
				setter = delegate(Color value)
				{
					data.LightSortingCurveColorStart = value;
				},
				showAlpha = false
			};
		}

		internal static DebugUI.Widget CreateLightSortingCurveColorEnd(DebugDisplaySettingsLighting data)
		{
			return new DebugUI.ColorField
			{
				nameAndTooltip = Strings.LightSortingCurveColorEnd,
				getter = () => data.LightSortingCurveColorEnd,
				setter = delegate(Color value)
				{
					data.LightSortingCurveColorEnd = value;
				},
				showAlpha = false
			};
		}
	}

	private class SettingsPanel : DebugDisplaySettingsPanel
	{
		public override string PanelName => "Lighting";

		public SettingsPanel(DebugDisplaySettingsLighting data)
		{
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Lighting Debug Modes",
				isHeader = true,
				opened = true,
				children = 
				{
					WidgetFactory.CreateClustersDebugMode(data),
					WidgetFactory.CreateLightingDebugMode(data),
					WidgetFactory.CreateShowLightSortingCurve(data),
					WidgetFactory.CreateLightSortingCurveColorStart(data),
					WidgetFactory.CreateLightSortingCurveColorEnd(data)
				}
			});
		}
	}

	private WaaaghDebugData m_DebugData;

	public DebugLightingMode DebugLightingMode
	{
		get
		{
			return m_DebugData.LightingDebug.DebugLightingMode;
		}
		internal set
		{
			m_DebugData.LightingDebug.DebugLightingMode = value;
		}
	}

	public DebugClustersMode DebugClustersMode
	{
		get
		{
			return m_DebugData.LightingDebug.DebugClustersMode;
		}
		internal set
		{
			m_DebugData.LightingDebug.DebugClustersMode = value;
		}
	}

	public bool ShowLightSortingCurve
	{
		get
		{
			return m_DebugData.LightingDebug.ShowLightSortingCurve;
		}
		internal set
		{
			m_DebugData.LightingDebug.ShowLightSortingCurve = value;
		}
	}

	public Color LightSortingCurveColorStart
	{
		get
		{
			return m_DebugData.LightingDebug.LightSortingCurveColorStart;
		}
		internal set
		{
			m_DebugData.LightingDebug.LightSortingCurveColorStart = value;
		}
	}

	public Color LightSortingCurveColorEnd
	{
		get
		{
			return m_DebugData.LightingDebug.LightSortingCurveColorEnd;
		}
		internal set
		{
			m_DebugData.LightingDebug.LightSortingCurveColorEnd = value;
		}
	}

	public DebugDisplaySettingsLighting(WaaaghDebugData data)
	{
		m_DebugData = data;
	}

	public IDebugDisplaySettingsPanelDisposable CreatePanel()
	{
		return new SettingsPanel(this);
	}
}

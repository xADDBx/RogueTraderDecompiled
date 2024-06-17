using System;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.ShaderLibrary.Visual.Debug;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

internal class DebugDisplaySettingsRendering : IDebugDisplaySettingsData
{
	private static class Strings
	{
		public static readonly DebugUI.Widget.NameAndTooltip MaterialDebugMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Material Debug Mode",
			tooltip = "Use the drop-down to select which material information to overlay on the screen."
		};

		public static readonly DebugUI.Widget.NameAndTooltip OverdrawMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Overdraw Mode",
			tooltip = "Debug anywhere pixels are overdrawn on top of each other."
		};

		public static DebugUI.Widget.NameAndTooltip DebugMipMap = new DebugUI.Widget.NameAndTooltip
		{
			name = "Debug MipMap",
			tooltip = "MipMap ratio rendering."
		};

		public static DebugUI.Widget.NameAndTooltip DebugStencilMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Debug Stencil Mode",
			tooltip = "Visualize Stencil Reference Value"
		};

		public static DebugUI.Widget.NameAndTooltip DebugStencilFlags = new DebugUI.Widget.NameAndTooltip
		{
			name = "Debug Stencil Flags",
			tooltip = "Predefined Stencil Reference Values"
		};

		public static DebugUI.Widget.NameAndTooltip DebugStencilRef = new DebugUI.Widget.NameAndTooltip
		{
			name = "Debug Stencil Ref",
			tooltip = "Stencil Ref"
		};
	}

	public static class WidgetFactory
	{
		internal static DebugUI.Widget CreateBuffersDebugMode(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.MaterialDebugMode,
				autoEnum = typeof(DebugMaterialMode),
				getter = () => (int)data.DebugMaterialMode,
				setter = delegate
				{
				},
				getIndex = () => (int)data.DebugMaterialMode,
				setIndex = delegate(int value)
				{
					data.DebugMaterialMode = (DebugMaterialMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateOverdrawMode(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.OverdrawMode,
				autoEnum = typeof(DebugOverdrawMode),
				getter = () => (int)data.OverdrawMode,
				setter = delegate
				{
				},
				getIndex = () => (int)data.OverdrawMode,
				setIndex = delegate(int value)
				{
					data.OverdrawMode = (DebugOverdrawMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugMipMap(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DebugMipMap,
				getter = () => data.DebugMipMap,
				setter = delegate(bool value)
				{
					data.DebugMipMap = value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugStencilMode(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.DebugStencilMode,
				autoEnum = typeof(StencilDebugType),
				getter = () => (int)data.DebugStencilType,
				setter = delegate
				{
				},
				getIndex = () => (int)data.DebugStencilType,
				setIndex = delegate(int value)
				{
					data.DebugStencilType = (StencilDebugType)value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugStencilFlags(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.BitField
			{
				nameAndTooltip = Strings.DebugStencilFlags,
				enumType = typeof(StencilRef),
				getter = () => data.DebugStencilFlags,
				setter = delegate(Enum value)
				{
					data.DebugStencilFlags = (StencilRef)(object)value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugStencilRef(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.IntField
			{
				nameAndTooltip = Strings.DebugStencilRef,
				getter = () => data.DebugStencilRef,
				setter = delegate(int value)
				{
					data.DebugStencilRef = value;
				},
				min = () => 0,
				max = () => 255
			};
		}
	}

	private class SettingsPanel : DebugDisplaySettingsPanel
	{
		public override string PanelName => "Rendering";

		public SettingsPanel(DebugDisplaySettingsRendering data)
		{
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Buffers Debug Modes",
				isHeader = true,
				opened = true,
				children = 
				{
					WidgetFactory.CreateBuffersDebugMode(data),
					WidgetFactory.CreateOverdrawMode(data),
					WidgetFactory.CreateDebugMipMap(data)
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Stencil Debug",
				isHeader = true,
				opened = true,
				children = 
				{
					WidgetFactory.CreateDebugStencilMode(data),
					WidgetFactory.CreateDebugStencilFlags(data),
					WidgetFactory.CreateDebugStencilRef(data)
				}
			});
		}
	}

	private WaaaghDebugData m_DebugData;

	public DebugMaterialMode DebugMaterialMode
	{
		get
		{
			return m_DebugData.RenderingDebug.DebugMaterialMode;
		}
		internal set
		{
			m_DebugData.RenderingDebug.DebugMaterialMode = value;
		}
	}

	public DebugOverdrawMode OverdrawMode
	{
		get
		{
			return m_DebugData.RenderingDebug.OverdrawMode;
		}
		internal set
		{
			m_DebugData.RenderingDebug.OverdrawMode = value;
		}
	}

	public bool DebugMipMap
	{
		get
		{
			return m_DebugData.RenderingDebug.DebugMipMap;
		}
		internal set
		{
			m_DebugData.RenderingDebug.DebugMipMap = value;
		}
	}

	public StencilDebugType DebugStencilType
	{
		get
		{
			return m_DebugData.StencilDebug.StencilDebugType;
		}
		internal set
		{
			m_DebugData.StencilDebug.StencilDebugType = value;
		}
	}

	public StencilRef DebugStencilFlags
	{
		get
		{
			return m_DebugData.StencilDebug.Flags;
		}
		internal set
		{
			m_DebugData.StencilDebug.Flags = value;
		}
	}

	public int DebugStencilRef
	{
		get
		{
			return m_DebugData.StencilDebug.Ref;
		}
		internal set
		{
			m_DebugData.StencilDebug.Ref = value;
		}
	}

	public DebugDisplaySettingsRendering(WaaaghDebugData data)
	{
		m_DebugData = data;
	}

	public IDebugDisplaySettingsPanelDisposable CreatePanel()
	{
		return new SettingsPanel(this);
	}
}

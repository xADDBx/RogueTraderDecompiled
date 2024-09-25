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

		public static string QuadOverdrawHelpMessage = "\r\nHighlights gpu quads running multiple fragment shaders\r\n\r\nThis is mainly caused by small or thin triangles.\r\nAnother reason is suboptimal object sorting.\r\nUse LODs to reduce the amount of overdraw when objects are far away.\r\n\r\nThis tool is useful for checking isolated objects and finding areas of the mesh that have too high triangle density.\r\n\r\nMax Quad Cost - Number of fragment shader executions\r\n\r\nObject Filter - Type of objects to draw\r\n\r\nDepth Test\r\n    - Disabled (recommended)\r\n      Xray-like behaviour. \r\n      Useful for inspecting isolated objects\r\n      Not represents actual behaviour in runtime\r\n    - Enabled\r\n      Useful for reducing noise while inspecting scene\r\n      Closest to actual runtime behaviour. \r\n      Cost depends on current camera view and object sorting settings. \r\n    - Pre Pass\r\n      Useful for reducing noise while inspecting surfaces\r\n      Not represents actual behaviour in runtime\r\n";
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
			return new DebugUI.VBox
			{
				children = 
				{
					(DebugUI.Widget)new DebugUI.EnumField
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
					},
					(DebugUI.Widget)new DebugUI.Container
					{
						isHiddenCallback = () => data.OverdrawMode != DebugOverdrawMode.QuadOverdraw,
						children = 
						{
							(DebugUI.Widget)new DebugUI.IntField
							{
								displayName = "Max Quad Cost",
								tooltip = null,
								getter = () => data.QuadOverdrawSettings.MaxQuadCost,
								setter = delegate(int value)
								{
									data.QuadOverdrawSettings.MaxQuadCost = value;
								},
								min = () => 0,
								max = () => 100
							},
							(DebugUI.Widget)new DebugUI.EnumField
							{
								displayName = "Depth Test",
								autoEnum = typeof(QuadOverdrawDepthTestMode),
								getter = () => (int)data.QuadOverdrawSettings.DepthTestMode,
								setter = delegate
								{
								},
								getIndex = () => (int)data.QuadOverdrawSettings.DepthTestMode,
								setIndex = delegate(int value)
								{
									data.QuadOverdrawSettings.DepthTestMode = (QuadOverdrawDepthTestMode)value;
								}
							},
							(DebugUI.Widget)new DebugUI.BoolField
							{
								displayName = "Depth Helper Plane Enabled",
								tooltip = null,
								isHiddenCallback = () => data.QuadOverdrawSettings.DepthTestMode != QuadOverdrawDepthTestMode.Enabled,
								getter = () => data.QuadOverdrawSettings.DepthHelperPlaneEneabled,
								setter = delegate(bool value)
								{
									data.QuadOverdrawSettings.DepthHelperPlaneEneabled = value;
								}
							},
							(DebugUI.Widget)new DebugUI.FloatField
							{
								displayName = "Depth Helper Plane Level",
								tooltip = null,
								isHiddenCallback = () => data.QuadOverdrawSettings.DepthTestMode != QuadOverdrawDepthTestMode.Enabled,
								getter = () => data.QuadOverdrawSettings.DepthHelperPlaneLevel,
								setter = delegate(float value)
								{
									data.QuadOverdrawSettings.DepthHelperPlaneLevel = value;
								}
							},
							(DebugUI.Widget)new DebugUI.EnumField
							{
								displayName = "Object Filter",
								autoEnum = typeof(QuadOverdrawObjectFilter),
								getter = () => (int)data.QuadOverdrawSettings.ObjectFilter,
								setter = delegate
								{
								},
								getIndex = () => (int)data.QuadOverdrawSettings.ObjectFilter,
								setIndex = delegate(int value)
								{
									data.QuadOverdrawSettings.ObjectFilter = (QuadOverdrawObjectFilter)value;
								}
							},
							(DebugUI.Widget)new DebugUI.MessageBox
							{
								displayName = Strings.QuadOverdrawHelpMessage,
								style = DebugUI.MessageBox.Style.Info
							}
						}
					}
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

	public QuadOverdrawSettings QuadOverdrawSettings
	{
		get
		{
			return m_DebugData.RenderingDebug.QuadOverdrawSettings;
		}
		internal set
		{
			m_DebugData.RenderingDebug.QuadOverdrawSettings = value;
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

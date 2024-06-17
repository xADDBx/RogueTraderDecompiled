using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Settings.Graphics;

[CreateAssetMenu(menuName = "Settings/Graphics presets list")]
public class GraphicsPresetsList : ScriptableObject
{
	[Serializable]
	public struct UserControlledValues
	{
		public bool VSyncMode;

		public bool FrameRateLimitEnabled;

		public bool FrameRateLimit;

		public bool ShadowsQuality;

		public bool TexturesQuality;

		public bool DepthOfField;

		public bool Bloom;

		public bool SSAOQuality;

		public bool SSRQuality;

		public bool AntialiasingMode;

		public bool AntialiasingQuality;

		public bool FootprintsMode;

		public bool FsrMode;

		public bool FsrSharpness;

		public bool VolumetricLightingQuality;

		public bool ParticleSystemsLightingEnabled;

		public bool ParticleSystemsShadowsEnabled;

		public bool FilmGrainEnabled;

		public bool UIFrequentTimerInterval;

		public bool UIInfrequentTimerInterval;

		public bool CrowdQuality;
	}

	[SerializeField]
	private GraphicsPresetAsset[] m_GraphicsPresets;

	[SerializeField]
	private GraphicsPresetAsset m_ConsoleGraphicsPreset;

	[SerializeField]
	private GraphicsPresetAsset m_SteamDeckGraphicsPreset;

	[SerializeField]
	[InfoBox("If you change the 'Console User Controlled Values' value, then change the 'Settings Platform' value in the corresponding UISetting in 'Assets/Mechanics/Blueprints/Root/Settings/Settings/GraphicsSettings'")]
	private UserControlledValues m_ConsoleUserControlledValues;

	public IReadOnlyList<GraphicsPresetAsset> GraphicsPresets => m_GraphicsPresets;

	public GraphicsPresetAsset ConsoleGraphicsPreset => m_ConsoleGraphicsPreset;

	public GraphicsPresetAsset SteamDeckGraphicsPreset => m_SteamDeckGraphicsPreset;

	public UserControlledValues ConsoleUserControlledValues => m_ConsoleUserControlledValues;
}

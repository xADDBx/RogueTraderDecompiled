using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Sound;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Sound;
using MemoryPack;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Blueprints.Area;

[TypeId("60192bc8449d2d944a99c6c58f48814b")]
[MemoryPackable(GenerateType.NoGenerate)]
public class BlueprintAreaPart : BlueprintMechanicEntityFact
{
	[Serializable]
	public struct SceneOverride
	{
		[SerializeField]
		private BlueprintEtudeReference m_EtudeToCheck;

		[SerializeField]
		private SceneReference m_StaticScene;

		[SerializeField]
		private SceneReference m_StaticSceneConsole;

		[SerializeField]
		private SceneReference m_LightScene;

		[SerializeField]
		private SceneReference m_LightSceneConsole;

		public BlueprintEtude Etude => m_EtudeToCheck.Get();

		public SceneReference StaticScene => m_StaticScene;

		public SceneReference StaticSceneConsole => m_StaticSceneConsole;

		public SceneReference LightScene => m_LightScene;

		public SceneReference LightSceneConsole => m_LightSceneConsole;

		public SceneReference GetStaticScene(bool console)
		{
			if (console)
			{
				SceneReference staticSceneConsole = m_StaticSceneConsole;
				if (staticSceneConsole != null && staticSceneConsole.IsDefined)
				{
					return m_StaticSceneConsole;
				}
			}
			return m_StaticScene;
		}

		public SceneReference GetLightScene(bool console)
		{
			if (console)
			{
				SceneReference lightSceneConsole = m_LightSceneConsole;
				if (lightSceneConsole != null && lightSceneConsole.IsDefined)
				{
					return m_LightSceneConsole;
				}
			}
			return m_LightScene;
		}
	}

	public enum LocalMapRotationDegree
	{
		Degree0 = 0,
		Degree90 = 90,
		Degree180 = 180,
		Degree270 = 270
	}

	[SerializeField]
	private SceneReference m_DynamicScene;

	[SerializeField]
	private SceneReference m_StaticScene;

	[SerializeField]
	private SceneReference m_StaticSceneConsoleOverride;

	[SerializeField]
	private SceneReference m_LightScene;

	[SerializeField]
	private SceneReference m_LightSceneConsoleOverride;

	[SerializeField]
	private SceneOverride[] m_SceneOverrides = Array.Empty<SceneOverride>();

	[SerializeField]
	[ShowCreator]
	private AreaPartBounds m_Bounds;

	[NotNull]
	[SerializeField]
	private SceneReference[] m_AudioTimeOfDayVariants = Array.Empty<SceneReference>();

	[NotNull]
	[SerializeField]
	[AkBankReference]
	private string[] m_SoundBankNames = Array.Empty<string>();

	[SerializeField]
	[ShowIf("IsPart")]
	[InfoBox("Turn on to load banks only on enter this area part")]
	private bool m_ManageBanksSeparately;

	[SerializeField]
	[InfoBox("Delay before area banks unload. Needed to fade all sounds properly")]
	private float m_UnloadBanksDelay = 2f;

	public AkStateReference MusicSetting;

	[SerializeField]
	private IndoorType m_IndoorType;

	[SerializeField]
	private WeatherProfileExtended m_WeatherProfile;

	[SerializeField]
	private InclemencyType m_WeatherInclemencyMin;

	[SerializeField]
	private InclemencyType m_WeatherInclemencyMax = InclemencyType.Storm;

	public bool IsSingleLightScene;

	[Tooltip("Set camera rotation to default value on area enter")]
	public bool CameraRotationOnEnter;

	[ShowIf("CameraRotationOnEnter")]
	public float DefaultCameraRotation;

	public bool VailAffectsTheWeather = true;

	public int StartVailValueForLocation;

	public LocalMapRotationDegree LocalMapRotationDeg;

	private bool IsPart => !(this is BlueprintArea);

	public AreaPartBounds Bounds
	{
		get
		{
			return m_Bounds;
		}
		set
		{
			m_Bounds = value;
		}
	}

	public virtual IEnumerable<SceneReference> AudioScenes => m_AudioTimeOfDayVariants;

	public bool HasLight => LightScene.IsDefined;

	public InclemencyType WeatherInclemencyMin => m_WeatherInclemencyMin;

	public InclemencyType WeatherInclemencyMax => m_WeatherInclemencyMax;

	public WeatherProfileExtended WeatherProfile
	{
		get
		{
			if ((m_WeatherProfile == null || m_WeatherProfile.IsEmpty()) && VailAffectsTheWeather && BlueprintWarhammerRoot.Instance.WarpWeatherRoot?.WeatherProfile != null)
			{
				return BlueprintWarhammerRoot.Instance.WarpWeatherRoot.WeatherProfile;
			}
			return m_WeatherProfile;
		}
		set
		{
			m_WeatherProfile = value;
		}
	}

	public SceneReference DynamicScene
	{
		get
		{
			return m_DynamicScene;
		}
		set
		{
			m_DynamicScene = value;
		}
	}

	public SceneReference StaticScene
	{
		get
		{
			bool isConsolePlatform = Application.isConsolePlatform;
			SceneReference staticScene = (Application.isPlaying ? m_SceneOverrides.FirstOrDefault((SceneOverride v) => Game.Instance.Player.EtudesSystem.Etudes.Get(v.Etude)?.IsPlaying ?? false) : default(SceneOverride)).GetStaticScene(isConsolePlatform);
			if (staticScene != null && staticScene.IsDefined)
			{
				return staticScene;
			}
			if (isConsolePlatform)
			{
				SceneReference staticSceneConsoleOverride = m_StaticSceneConsoleOverride;
				if (staticSceneConsoleOverride != null && staticSceneConsoleOverride.IsDefined)
				{
					return m_StaticSceneConsoleOverride;
				}
			}
			return m_StaticScene;
		}
		set
		{
			m_StaticScene = value;
		}
	}

	public SceneReference MainStaticScene => m_StaticScene;

	public SceneReference MainStaticSceneConsole => m_StaticSceneConsoleOverride;

	public SceneReference LightScene
	{
		get
		{
			bool isConsolePlatform = Application.isConsolePlatform;
			SceneReference lightScene = (Application.isPlaying ? m_SceneOverrides.FirstOrDefault((SceneOverride v) => Game.Instance.Player.EtudesSystem.Etudes.Get(v.Etude)?.IsPlaying ?? false) : default(SceneOverride)).GetLightScene(isConsolePlatform);
			if (lightScene != null && lightScene.IsDefined)
			{
				return lightScene;
			}
			if (isConsolePlatform)
			{
				SceneReference lightSceneConsoleOverride = m_LightSceneConsoleOverride;
				if (lightSceneConsoleOverride != null && lightSceneConsoleOverride.IsDefined)
				{
					return m_LightSceneConsoleOverride;
				}
			}
			return m_LightScene;
		}
		set
		{
			m_LightScene = value;
		}
	}

	public SceneReference MainLightScene => m_LightScene;

	public SceneReference MainLightSceneConsole => m_LightSceneConsoleOverride;

	public bool ManageBanksSeparately
	{
		get
		{
			if (IsPart)
			{
				return m_ManageBanksSeparately;
			}
			return false;
		}
	}

	public float UnloadBanksDelay => m_UnloadBanksDelay;

	public IEnumerable<SceneReference> GetAllScenes(bool console)
	{
		return from v in GetAllStaticScenes().Concat(GetAllLightScenes()).Concat(AudioScenes).Append(m_DynamicScene)
			where v?.IsDefined ?? false
			select v;
	}

	public IEnumerable<SceneReference> GetAllScenes()
	{
		return from v in GetAllStaticScenes().Concat(GetAllLightScenes()).Concat(AudioScenes).Append(m_DynamicScene)
			where v?.IsDefined ?? false
			select v;
	}

	public IEnumerable<SceneReference> GetAllStaticScenes(bool console)
	{
		SceneOverride[] sceneOverrides = m_SceneOverrides;
		foreach (SceneOverride sceneOverride in sceneOverrides)
		{
			SceneReference staticScene = sceneOverride.GetStaticScene(console);
			if (staticScene != null && staticScene.IsDefined)
			{
				yield return staticScene;
			}
		}
		SceneReference sceneReference;
		if (console)
		{
			SceneReference staticSceneConsoleOverride = m_StaticSceneConsoleOverride;
			if (staticSceneConsoleOverride != null && staticSceneConsoleOverride.IsDefined)
			{
				sceneReference = m_StaticSceneConsoleOverride;
				goto IL_00d1;
			}
		}
		sceneReference = m_StaticScene;
		goto IL_00d1;
		IL_00d1:
		yield return sceneReference;
	}

	public IEnumerable<SceneReference> GetAllStaticScenes()
	{
		SceneOverride[] sceneOverrides = m_SceneOverrides;
		for (int i = 0; i < sceneOverrides.Length; i++)
		{
			SceneOverride sceneOverride = sceneOverrides[i];
			SceneReference staticScene = sceneOverride.StaticScene;
			if (staticScene != null && staticScene.IsDefined)
			{
				yield return staticScene;
			}
			staticScene = sceneOverride.StaticSceneConsole;
			if (staticScene != null && staticScene.IsDefined)
			{
				yield return staticScene;
			}
		}
		SceneReference staticSceneConsoleOverride = m_StaticSceneConsoleOverride;
		if (staticSceneConsoleOverride != null && staticSceneConsoleOverride.IsDefined)
		{
			yield return m_StaticSceneConsoleOverride;
		}
		yield return m_StaticScene;
	}

	public IEnumerable<SceneReference> GetAllLightScenes(bool console)
	{
		SceneOverride[] sceneOverrides = m_SceneOverrides;
		foreach (SceneOverride sceneOverride in sceneOverrides)
		{
			SceneReference lightScene = sceneOverride.GetLightScene(console);
			if (lightScene != null && lightScene.IsDefined)
			{
				yield return lightScene;
			}
		}
		SceneReference sceneReference;
		if (console)
		{
			SceneReference lightSceneConsoleOverride = m_LightSceneConsoleOverride;
			if (lightSceneConsoleOverride != null && lightSceneConsoleOverride.IsDefined)
			{
				sceneReference = m_LightSceneConsoleOverride;
				goto IL_00d1;
			}
		}
		sceneReference = m_LightScene;
		goto IL_00d1;
		IL_00d1:
		yield return sceneReference;
	}

	public IEnumerable<SceneReference> GetAllLightScenes()
	{
		SceneOverride[] sceneOverrides = m_SceneOverrides;
		for (int i = 0; i < sceneOverrides.Length; i++)
		{
			SceneOverride sceneOverride = sceneOverrides[i];
			SceneReference lightScene = sceneOverride.LightScene;
			if (lightScene != null && lightScene.IsDefined)
			{
				yield return lightScene;
			}
			lightScene = sceneOverride.LightSceneConsole;
			if (lightScene != null && lightScene.IsDefined)
			{
				yield return lightScene;
			}
		}
		SceneReference lightSceneConsoleOverride = m_LightSceneConsoleOverride;
		if (lightSceneConsoleOverride != null && lightSceneConsoleOverride.IsDefined)
		{
			yield return m_LightSceneConsoleOverride;
		}
		yield return m_LightScene;
	}

	public SceneReference GetLightScene()
	{
		if (LightScene.IsDefined)
		{
			return LightScene;
		}
		PFLog.Default.Error(this, $"Has no light settings in area part '{this}'");
		return null;
	}

	public SceneReference GetAudioScene(TimeOfDay timeOfDay)
	{
		switch (m_AudioTimeOfDayVariants.Length)
		{
		case 1:
			return m_AudioTimeOfDayVariants[0];
		case 4:
			return m_AudioTimeOfDayVariants[(int)timeOfDay];
		default:
			if (m_AudioTimeOfDayVariants.Length != 0)
			{
				PFLog.Default.Error(this, $"Invalid audio settings in area part '{this}'");
			}
			return null;
		}
	}

	public virtual IEnumerable<SceneReference> GetStaticAndActiveDynamicScenes()
	{
		if (m_DynamicScene != null)
		{
			yield return m_DynamicScene;
		}
		yield return StaticScene;
	}

	public virtual IEnumerable<string> GetActiveSoundBankNames(bool isCurrentPart = false)
	{
		if (isCurrentPart || !ManageBanksSeparately)
		{
			string[] soundBankNames = m_SoundBankNames;
			for (int i = 0; i < soundBankNames.Length; i++)
			{
				yield return soundBankNames[i];
			}
		}
	}

	public void SetAudioScenes(SceneReference[] audioScenes)
	{
		m_AudioTimeOfDayVariants = audioScenes;
	}

	protected override Type GetFactType()
	{
		return typeof(Kingmaker.AreaLogic.Area);
	}

	public TimeOfDay GetTimeOfDay()
	{
		return m_IndoorType switch
		{
			IndoorType.None => Game.Instance.TimeOfDay, 
			IndoorType.IndoorLikeMorning => TimeOfDay.Morning, 
			IndoorType.IndoorLikeDay => TimeOfDay.Day, 
			IndoorType.IndoorLikeEvening => TimeOfDay.Evening, 
			IndoorType.IndoorLikeNight => TimeOfDay.Night, 
			_ => Game.Instance.TimeOfDay, 
		};
	}
}

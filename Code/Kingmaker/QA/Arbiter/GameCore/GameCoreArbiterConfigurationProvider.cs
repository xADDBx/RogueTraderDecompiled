using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA.Arbiter.Service;
using Kingmaker.QA.Profiling;
using Kingmaker.Settings;
using Kingmaker.Utility.Performance;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.Traps;
using Owlcat.Runtime.Core.ProfilingCounters;
using UnityEngine;

namespace Kingmaker.QA.Arbiter.GameCore;

public class GameCoreArbiterConfigurationProvider : IArbiterServiceConfigurationProvider
{
	public sealed class GameCoreUITexturesMeasure : IMeasure
	{
		public string Name => "UITextures";

		public float Value
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public void FillValues(Dictionary<string, float> visit)
		{
			foreach (KeyValuePair<string, float> areaLoadMeasurement in GetAreaLoadMeasurements())
			{
				visit.Add(areaLoadMeasurement.Key, areaLoadMeasurement.Value);
			}
		}

		public static Dictionary<string, float> GetAreaLoadMeasurements()
		{
			Dictionary<string, float> dictionary = new Dictionary<string, float>();
			try
			{
				CheatsDebug.UITexturesCountForArbiter uITexturesCountForArbiter = CheatsDebug.GetUITexturesCountForArbiter();
				long num = uITexturesCountForArbiter.UI_Atlases.Size / 1048576;
				long num2 = uITexturesCountForArbiter.UI_NoAtlases.Size / 1048576;
				dictionary.Add("UITexturesAtlas", num);
				dictionary.Add("UITexturesNonAtlas", num2);
				ArbiterService.Logger.Log("Graphics settings: " + $"\n\tQuality preset: {SettingsRoot.Graphics.GraphicsQuality.GetValue()}" + $"\n\tGamma Correction: {SettingsRoot.Display.GammaCorrection.GetValue()}" + $"\n\tBrightness: {SettingsRoot.Display.Brightness.GetValue()}" + $"\n\tContrast: {SettingsRoot.Display.Contrast.GetValue()}" + $"\n\tDeuteranopia: {SettingsRoot.Accessiability.Deuteranopia.GetValue()}" + $"\n\tProtanopia: {SettingsRoot.Accessiability.Protanopia.GetValue()}" + $"\n\tTritanopia: {SettingsRoot.Accessiability.Tritanopia.GetValue()}");
			}
			catch (Exception ex)
			{
				ArbiterService.Logger.Exception(ex);
				dictionary.Add("UITexturesAtlas", -1f);
				dictionary.Add("UITexturesNonAtlas", -1f);
			}
			return dictionary;
		}
	}

	public ArbiterServiceConfiguration GetConfiguration()
	{
		IArbiterEnvironment arbiterEnvironment2;
		if (!Application.isEditor)
		{
			IArbiterEnvironment arbiterEnvironment = new GameCoreArbiterEnvironment();
			arbiterEnvironment2 = arbiterEnvironment;
		}
		else
		{
			IArbiterEnvironment arbiterEnvironment = new EditorArbiterEnvironment();
			arbiterEnvironment2 = arbiterEnvironment;
		}
		IArbiterEnvironment environment = arbiterEnvironment2;
		ArbiterTaskFactoryConfiguration taskFactoryConfiguration = new GameCoreArbiterTaskFactoryConfiguration();
		IList<IMeasure> gameCoreMeasures = new List<IMeasure>
		{
			new Measure("AwakeUnits", () => Game.Instance.State.AllAwakeUnits.Count),
			new Measure("TotalUnits", () => Game.Instance.State.AllUnits.All.Count),
			new Measure("UniqueUnitPrefabs", () => Game.Instance.State.AllUnits.All.Select((AbstractUnitEntity u) => u.ViewSettings.PrefabGuid).Distinct().Count()),
			new Measure("Corpses", () => Game.Instance.State.AllUnits.All.Count((AbstractUnitEntity u) => u.LifeState.IsDead)),
			new Measure("MapObjects", () => Game.Instance.State.MapObjects.Count()),
			new Measure("InteractiveObjects", () => Game.Instance.State.MapObjects.Count((MapObjectEntity m) => m.View.GetComponent<IInteractionComponent>() != null)),
			new Measure("Traps", () => Game.Instance.State.MapObjects.OfType<TrapObjectData>().Count()),
			new Measure("Cutscenes", () => Game.Instance.State.Cutscenes.Count()),
			new GameCoreUITexturesMeasure()
		};
		Kingmaker.QA.Profiling.Counters.All.ForEach(delegate(Counter x)
		{
			gameCoreMeasures.Add(new Measure(x.Name, () => (float)x.GetMedian()));
			gameCoreMeasures.Add(new Measure("Limit." + x.Name + ".Failed", () => (float)x.WarningLevel));
		});
		ObjectLimits.Entries.ToList().ForEach(delegate(ObjectLimits.Entry x)
		{
			string text = x.Name.Replace(" ", "");
			gameCoreMeasures.Add(new Measure(text, () => x.Getter()));
			gameCoreMeasures.Add(new Measure("Limit." + text + ".Warning", () => x.Threshold));
			gameCoreMeasures.Add(new Measure("Limit." + text + ".Failed", () => (float)x.Threshold * 1.1f));
		});
		return new ArbiterServiceConfiguration
		{
			Environment = environment,
			EventHandler = new GameCoreArbiterEventHandler(),
			TaskFactoryConfiguration = taskFactoryConfiguration,
			Measurements = gameCoreMeasures,
			InstructionIndex = new BlueprintArbiterInstructionIndex(),
			LocalMapRenderer = new GameCoreArbiterLocalMapRenderer(),
			SceneBoundary = new GameCoreSceneBoundary(),
			ClientIntegration = new GameCoreArbiterIntegration()
		};
	}
}

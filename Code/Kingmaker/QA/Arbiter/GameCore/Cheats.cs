using Core.Cheats;
using Kingmaker.QA.Arbiter.Service;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;

namespace Kingmaker.QA.Arbiter.GameCore;

public class Cheats
{
	[Cheat(Name = "arbiter_spt", ExecutionPolicy = ExecutionPolicy.PlayMode, Description = "Run Arbiter Static Performance Test. <step> - jump interval for camera ([10]), <type> type of algorithm ([0],1,2), <vsync> - ([-1]/0/1)")]
	public static void ArbiterRunStaticPerformanceTest(float step = 10f, int type = 0, int iVSync = -1)
	{
		bool? flag = ((iVSync == -1) ? null : new bool?(iVSync != 0));
		ArbiterService.Initialize<GameCoreArbiterConfigurationProvider>();
		ArbiterService.RunInstruction("StaticPerformanceTest", new object[3] { step, type, flag });
	}

	[Cheat(Name = "weather_set", Description = "Сменить погоду")]
	public static InclemencyType SetWeather(InclemencyType type = InclemencyType.Clear)
	{
		return ArbiterIntegration.SetWeather(type);
	}

	[Cheat(Name = "clouds_disable", Description = "Отключить облака")]
	public static void DisableClouds()
	{
		ArbiterIntegration.DisableClouds();
	}

	[Cheat(Name = "clouds_enable", Description = "Включить облака")]
	public static void EnableClouds()
	{
		ArbiterIntegration.EnableClouds();
	}

	[Cheat(Name = "fog_disable", Description = "Отключить туманных объемы")]
	public static void DisableFog(bool disable = true)
	{
		ArbiterIntegration.DisableFog(disable);
	}

	[Cheat(Name = "fog_enable", Description = "Включить туманных объемы")]
	public static void EnableFog(bool disable = true)
	{
		ArbiterIntegration.EnableFog(disable);
	}

	[Cheat(Name = "wind_disable", Description = "Отключить ветер")]
	public static void DisableWind(bool disable = true)
	{
		ArbiterIntegration.DisableWind();
	}

	[Cheat(Name = "wind_enable", Description = "Включить ветер")]
	public static void EnableWind()
	{
		ArbiterIntegration.EnableWind();
	}

	[Cheat(Name = "fow_disable", Description = "Отключить туман войны")]
	public static void DisableFow()
	{
		ArbiterIntegration.DisableFow();
	}

	[Cheat(Name = "fow_enable", Description = "Включить туман войны")]
	public static void EnableFow()
	{
		ArbiterIntegration.EnableFow();
	}

	[Cheat(Name = "fx_disable", Description = "Отключить fx")]
	public static void DisableFx()
	{
		ArbiterIntegration.DisableFx();
	}

	[Cheat(Name = "fx_enable", Description = "Включить fx")]
	public static void EnableFx()
	{
		ArbiterIntegration.EnableFx();
	}

	[Cheat(Name = "units_hide", Description = "Скрыть юниты")]
	public static void HideUnits()
	{
		ArbiterIntegration.HideUnits();
	}

	[Cheat(Name = "units_show", Description = "Показать юниты")]
	public static void ShowUnits()
	{
		ArbiterIntegration.ShowUnits();
	}

	[Cheat(Name = "ui_hide", Description = "Скрыть UI")]
	public static void HideUi()
	{
		ArbiterIntegration.HideUi();
	}

	[Cheat(Name = "ui_show", Description = "Показать UI")]
	public static void ShowUi()
	{
		ArbiterIntegration.ShowUi();
	}
}

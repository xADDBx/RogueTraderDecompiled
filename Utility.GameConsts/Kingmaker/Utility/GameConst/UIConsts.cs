using UnityEngine;

namespace Kingmaker.Utility.GameConst;

public static class UIConsts
{
	public const string DefaultFormat = "{0}";

	public const string CharSheetFormat = "<color=#888888FF>{0}</color> {1}";

	public const string CapReachedFormat = "<color=#888888FF>{0}</color> <color=#FFBF00FF>{1}</color>";

	public const string BuffedFormat = "<color=#888888FF>{0}</color> <color=#42EB35FF>{1}</color>";

	public const string DebuffedFormat = "<color=#888888FF>{0}</color> <color=#B40431FF>{1}</color>";

	public const string BuffedStatFormat = "<color=#42EB35FF>{0}</color>";

	public const string DebuffedStatFormat = "<color=#B40431FF>{0}</color>";

	public const int GroupMaxCountWithPets = 12;

	public const int GroupMaxCount = 6;

	public const int GroupMaxNavigatorsCount = 1;

	public const int MinStashSlotCount = 120;

	public const int MinStashSlotCountInLoot = 81;

	public const int SlotsInRowInLoot = 9;

	public const int SlotsInRow = 6;

	public const int CargoSlotsInRow = 6;

	public const int CargoConsoleSlotsInRow = 6;

	public const int CargoMinSlots = 12;

	public const int CargoConsoleMinSlots = 20;

	public const float FadeTime = 0.2f;

	public const float DoubleFadeTime = 0.4f;

	public const float QadroFadeTime = 0.8f;

	public const float UnoFadeTime = 0.1f;

	public const float PingTime = 7.5f;

	public const int MinSettingsItemsCount = 10;

	public const int MinSettingsGroupsCount = 1;

	public static int MinLootSlotsWithShowedStash = 64;

	public static int MinLootSlotsInSingleObj = 21;

	public static int MinConsoleLootSlotsInSingleObj = 9;

	public static int MinLootSlotsInTwoObj = 18;

	public static int MinLootSlotsInThreeObj = 16;

	public static int MinLootSlotsInRow = 3;

	public static int MinConsoleLootSlotsInRow = 5;

	public static int LootSlotsInRowConsole = 5;

	public static int MinLootSlotsWithShowedStashConsole = 30;

	public static int MinLootSlotsInSingleObjConsole = 10;

	public static float HPUninjured = 1f;

	public static float HPBarelyInjured = 0.8f;

	public static float HPInjured = 0.5f;

	public static float HPBadlyInjured = 0.1f;

	public static float HPNearDeath = 0f;

	public static int HoursInDay = 24;

	public static float BarkMoveTime = 0.3f;

	public static float StoryAnimationTime = 0.15f;

	public static string SuffixOn = "On";

	public static string SuffixOff = "Off";

	public static float QuestNotificationTime = 6f;

	public const int ActionSlotsCountMainBar = 10;

	public const int ActionSlotsCount = 100;

	public static readonly Vector2 LocalMapMaxSize = new Vector2(1760f, 830f);

	public static string PCDemoVoicesENGBank = "PC_DemoVoices_ENG";

	public static string SplashScreens = "SplashScreen";

	public static float RevealObjectHighlightTime = 2f;

	public static float SurfaceZoomOnTriggerClamp = 0.5f;

	public static float SurfaceRotateOnTriggerClamp = 0.5f;

	public static float SurfaceZoomSpeed = 0.05f;
}

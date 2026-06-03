using System;

namespace Kingmaker;

public struct ShowBossHPBarUIStruct
{
	public string Id;

	public string BossName;

	public Func<int> CurrentHPGetter;

	public Func<int> MaxHPGetter;
}

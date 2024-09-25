using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.GameModes;

public class GameModeType
{
	private static readonly List<GameModeType> List = new List<GameModeType>();

	public static readonly GameModeType None = new GameModeType("None");

	public static readonly GameModeType Default = new GameModeType("Default");

	public static readonly GameModeType GlobalMap = new GameModeType("GlobalMap");

	public static readonly GameModeType Dialog = new GameModeType("Dialog");

	public static readonly GameModeType Pause = new GameModeType("Pause");

	public static readonly GameModeType Cutscene = new GameModeType("Cutscene");

	public static readonly GameModeType GameOver = new GameModeType("GameOver");

	public static readonly GameModeType BugReport = new GameModeType("BugReport");

	public static readonly GameModeType CutsceneGlobalMap = new GameModeType("CutsceneGlobalMap");

	public static readonly GameModeType SpaceCombat = new GameModeType("SpaceCombat");

	public static readonly GameModeType StarSystem = new GameModeType("StarSystem");

	public readonly int Index;

	public readonly string Name;

	public static int Count { get; private set; }

	public static ReadonlyList<GameModeType> All => List;

	public GameModeType(string name)
	{
		Index = Count++;
		Name = name;
		List.Add(this);
	}

	public override bool Equals(object obj)
	{
		return this == obj;
	}

	public override int GetHashCode()
	{
		return Index;
	}

	public override string ToString()
	{
		return Name;
	}

	public static implicit operator int(GameModeType type)
	{
		return type.Index;
	}

	public static bool operator ==(GameModeType t1, GameModeType t2)
	{
		return (object)t1 == t2;
	}

	public static bool operator !=(GameModeType t1, GameModeType t2)
	{
		return (object)t1 != t2;
	}
}

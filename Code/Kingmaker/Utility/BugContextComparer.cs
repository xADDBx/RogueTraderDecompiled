using System;
using System.Collections.Generic;

namespace Kingmaker.Utility;

public class BugContextComparer : IComparer<BugContext>
{
	private static Dictionary<BugContext.ContextType, int> typePriority = new Dictionary<BugContext.ContextType, int>
	{
		{
			BugContext.ContextType.Desync,
			0
		},
		{
			BugContext.ContextType.Coop,
			5
		},
		{
			BugContext.ContextType.Unit,
			10
		},
		{
			BugContext.ContextType.Item,
			10
		},
		{
			BugContext.ContextType.Spell,
			10
		},
		{
			BugContext.ContextType.SpellSpace,
			10
		},
		{
			BugContext.ContextType.GroupChanger,
			10
		},
		{
			BugContext.ContextType.TransitionMap,
			10
		},
		{
			BugContext.ContextType.Dialog,
			20
		},
		{
			BugContext.ContextType.Colonization,
			25
		},
		{
			BugContext.ContextType.Encounter,
			30
		},
		{
			BugContext.ContextType.Interface,
			40
		},
		{
			BugContext.ContextType.Exploration,
			50
		},
		{
			BugContext.ContextType.GlobalMap,
			50
		},
		{
			BugContext.ContextType.SurfaceCombat,
			50
		},
		{
			BugContext.ContextType.SpaceCombat,
			50
		},
		{
			BugContext.ContextType.Area,
			70
		},
		{
			BugContext.ContextType.Debug,
			200
		}
	};

	private static Dictionary<string, int> uiFeaturePriority = new Dictionary<string, int> { { "", 200 } };

	private const int defaultPriority = 100;

	private const int minPriority = 200;

	public int Compare(BugContext alice, BugContext bob)
	{
		int num = CompareTypes(alice, bob);
		if (num != 0)
		{
			return num;
		}
		if (alice.Type == BugContext.ContextType.Interface)
		{
			return CompareUiFeatures(alice, bob);
		}
		return 0;
	}

	private int CompareTypes(BugContext alice, BugContext bob)
	{
		return CompareByPresetPriorityMap(alice, bob, GetTypePriority);
	}

	private int CompareUiFeatures(BugContext alice, BugContext bob)
	{
		return CompareByPresetPriorityMap(alice, bob, GetUiFeaturePriority);
	}

	private int CompareByPresetPriorityMap(BugContext alice, BugContext bob, Func<BugContext, int> getPriority)
	{
		int num = GetExclusivePriority(alice) ?? getPriority(alice);
		int num2 = GetExclusivePriority(bob) ?? getPriority(bob);
		if (num > num2)
		{
			return 1;
		}
		if (num < num2)
		{
			return -1;
		}
		return 0;
	}

	private int? GetExclusivePriority(BugContext context)
	{
		if (context.Type == BugContext.ContextType.Interface && context.UiFeature == "SystemMap")
		{
			return 60;
		}
		if (context.Type == BugContext.ContextType.Interface && context.OtherUiFeature == "Bark")
		{
			return 200;
		}
		return null;
	}

	private static int GetTypePriority(BugContext bc)
	{
		if (typePriority.TryGetValue(bc.Type, out var value))
		{
			return value;
		}
		return 100;
	}

	private static int GetUiFeaturePriority(BugContext bc)
	{
		if (bc.UiFeature != null && uiFeaturePriority.TryGetValue(bc.UiFeature, out var value))
		{
			return value;
		}
		return 100;
	}
}

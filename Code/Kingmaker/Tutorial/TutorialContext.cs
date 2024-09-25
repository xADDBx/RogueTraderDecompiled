using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Tutorial;

public class TutorialContext : ContextData<TutorialContext>
{
	private readonly Dictionary<string, TutorialContextItem> m_Items = new Dictionary<string, TutorialContextItem>();

	[CanBeNull]
	public BaseUnitEntity RevealUnitInfo { get; set; }

	public TutorialContextItem this[TutorialContextKey key]
	{
		get
		{
			if (key == TutorialContextKey.Invalid)
			{
				PFLog.Default.ErrorWithReport("TutorialContextItem[].get: invalid key");
				return default(TutorialContextItem);
			}
			return m_Items.Get(key.ToString());
		}
		set
		{
			if (key == TutorialContextKey.Invalid)
			{
				PFLog.Default.ErrorWithReport("TutorialContextItem[].set: invalid key");
				return;
			}
			string text = key.ToString();
			if (m_Items.ContainsKey(text))
			{
				PFLog.Default.ErrorWithReport("TutorialContextItem[].set: overwriting value is forbidden (key: " + text + ")");
				return;
			}
			m_Items[text] = value;
			TutorialContextKey paired = key.GetPaired();
			if (paired != 0)
			{
				m_Items[paired.ToString()] = value;
			}
		}
	}

	public BaseUnitEntity SourceUnit
	{
		get
		{
			return this[TutorialContextKey.SourceUnit].Unit;
		}
		set
		{
			this[TutorialContextKey.SourceUnit] = value;
		}
	}

	public BaseUnitEntity TargetUnit
	{
		get
		{
			return this[TutorialContextKey.TargetUnit].Unit;
		}
		set
		{
			this[TutorialContextKey.TargetUnit] = value;
		}
	}

	public ItemEntity SourceItem
	{
		get
		{
			return this[TutorialContextKey.SourceItem].Item;
		}
		set
		{
			this[TutorialContextKey.SourceItem] = value;
		}
	}

	public EntityFact SourceFact
	{
		get
		{
			return this[TutorialContextKey.SourceFact].Fact;
		}
		set
		{
			this[TutorialContextKey.SourceFact] = value;
		}
	}

	public AbilityData SourceAbility
	{
		get
		{
			return this[TutorialContextKey.SourceAbility].Ability;
		}
		set
		{
			this[TutorialContextKey.SourceAbility] = value;
		}
	}

	public BaseUnitEntity SolutionUnit
	{
		get
		{
			return this[TutorialContextKey.SolutionUnit].Unit;
		}
		set
		{
			this[TutorialContextKey.SolutionUnit] = value;
		}
	}

	public ItemEntity SolutionItem
	{
		get
		{
			return this[TutorialContextKey.SolutionItem].Item;
		}
		set
		{
			this[TutorialContextKey.SolutionItem] = value;
		}
	}

	public AbilityData SolutionAbility
	{
		get
		{
			return this[TutorialContextKey.SolutionAbility].Ability;
		}
		set
		{
			this[TutorialContextKey.SolutionAbility] = value;
		}
	}

	public EntityFact SolutionFact
	{
		get
		{
			return this[TutorialContextKey.SolutionFact].Fact;
		}
		set
		{
			this[TutorialContextKey.SolutionFact] = value;
		}
	}

	public TutorialContextItem? Get(string key)
	{
		if (!m_Items.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	protected override void Reset()
	{
		m_Items.Clear();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic.Levelup.Obsolete;

public class FeatureSelectionState : IEquatable<FeatureSelectionState>
{
	public readonly FeatureSource Source;

	[NotNull]
	public readonly IFeatureSelection Selection;

	public readonly int Index;

	public readonly int Level;

	[CanBeNull]
	private List<FeatureSelectionViewState> m_ViewState;

	[CanBeNull]
	public FeatureSelectionState Parent { get; }

	public BlueprintFeature SourceFeature => Source.Blueprint as BlueprintFeature;

	[CanBeNull]
	public IFeatureSelectionItem SelectedItem { get; private set; }

	[CanBeNull]
	public FeatureSelectionState Next { get; private set; }

	public bool Selected => SelectedItem != null;

	public bool IgnorePrerequisites
	{
		get
		{
			FeatureSelectionState featureSelectionState = this;
			while (featureSelectionState != null)
			{
				if (featureSelectionState.Selection.IsIgnorePrerequisites())
				{
					return true;
				}
				featureSelectionState = featureSelectionState.Parent;
			}
			return false;
		}
	}

	public FeatureSelectionState([CanBeNull] FeatureSelectionState parent, FeatureSource source, [NotNull] IFeatureSelection selection, int index, int selectionLevel)
	{
		Parent = parent;
		Source = source;
		Selection = selection;
		Index = index;
		Level = selectionLevel;
	}

	private List<FeatureSelectionViewState> SetupViewState()
	{
		if (Game.Instance.LevelUpController == null)
		{
			return null;
		}
		LevelUpState state = Game.Instance.LevelUpController.State;
		List<FeatureSelectionViewState> list = new List<FeatureSelectionViewState>();
		foreach (IFeatureSelectionItem item2 in Selection.GetItems(state))
		{
			FeatureSelectionViewState item = new FeatureSelectionViewState(this, Selection, item2);
			list.Add(item);
		}
		return list;
	}

	public FeatureSelectionViewState GetViewState(IFeatureSelectionItem item)
	{
		if (m_ViewState == null)
		{
			m_ViewState = SetupViewState();
			if (m_ViewState == null)
			{
				return new FeatureSelectionViewState(null, null, null);
			}
		}
		FeatureSelectionViewState featureSelectionViewState = m_ViewState.FirstOrDefault((FeatureSelectionViewState state) => state.Feature == item.Feature && state.Param == item.Param);
		if (featureSelectionViewState == null)
		{
			return new FeatureSelectionViewState(null, null, null);
		}
		int index = m_ViewState.IndexOf(featureSelectionViewState);
		if (!Selected)
		{
			m_ViewState[index] = new FeatureSelectionViewState(this, Selection, item);
		}
		return m_ViewState[index];
	}

	public void Select(IFeatureSelectionItem item, FeatureSelectionState nextSelection)
	{
		SelectedItem = item;
		Next = nextSelection;
	}

	public static bool operator ==(FeatureSelectionState d1, FeatureSelectionState d2)
	{
		if ((object)d1 == null && (object)d2 == null)
		{
			return true;
		}
		if ((object)d1 == null || (object)d2 == null)
		{
			return false;
		}
		return d1.Equals(d2);
	}

	public static bool operator !=(FeatureSelectionState d1, FeatureSelectionState d2)
	{
		return !(d1 == d2);
	}

	public bool Equals(FeatureSelectionState other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (Selection.Equals(other.Selection) && Index == other.Index && Level == other.Level)
		{
			return object.Equals(SelectedItem, other.SelectedItem);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((FeatureSelectionState)obj);
	}

	public override int GetHashCode()
	{
		return (((((Selection.GetHashCode() * 397) ^ Index) * 397) ^ Level) * 397) ^ ((SelectedItem != null) ? SelectedItem.GetHashCode() : 0);
	}

	public bool IsSelectedInChildren(IFeatureSelectionItem item)
	{
		FeatureSelectionState featureSelectionState = this;
		for (int i = 0; i < 100; i++)
		{
			if (featureSelectionState == null)
			{
				break;
			}
			if (featureSelectionState.SelectedItem == item)
			{
				return true;
			}
			featureSelectionState = featureSelectionState.Next;
		}
		return false;
	}

	public bool CanSelectAnything(LevelUpState state)
	{
		if (Selection.IsObligatory())
		{
			return true;
		}
		if (Selection.IsSelectionProhibited(state.PreviewUnit))
		{
			return false;
		}
		foreach (IFeatureSelectionItem item in Selection.GetItems(state))
		{
			if (item != SelectedItem && Selection.CanSelect(state, this, item))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSame(FeatureSelectionState other)
	{
		if (!Equals(other) && (Selection != other.Selection || Index != other.Index))
		{
			return false;
		}
		if (other.Parent == null && Parent == null)
		{
			return true;
		}
		if ((other.Parent == null) ^ (Parent == null))
		{
			return false;
		}
		return other.Parent.IsSame(Parent);
	}

	public bool CanDropLevelupPlan(BlueprintCharacterClass characterClass)
	{
		return true;
	}
}

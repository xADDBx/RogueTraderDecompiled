using System.Linq;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic.Levelup.Obsolete;

public class FeatureSelectionViewState
{
	public enum SelectState
	{
		Forbidden,
		AlreadyHas,
		CanSelect
	}

	public int RecomendationPriority;

	public BlueprintFeature Feature;

	public FeatureParam Param;

	public SelectState CanSelectState { get; private set; }

	public bool CanSelect => CanSelectState == SelectState.CanSelect;

	public FeatureSelectionViewState(FeatureSelectionState selectionState, IFeatureSelection selection, IFeatureSelectionItem item)
	{
		if (!(selectionState == null) && selection != null && item != null)
		{
			Feature = item.Feature;
			Param = item.Param;
			RecomendationPriority = 0;
			RefreshCanSelectState(selectionState, selection, item);
		}
	}

	public void RefreshCanSelectState(FeatureSelectionState selectionState, IFeatureSelection selection, IFeatureSelectionItem item)
	{
		bool flag = selection.CanSelect(Game.Instance.LevelUpController.State, selectionState, item);
		bool flag2 = false;
		flag2 = ((!(item.Param == null)) ? (!flag && Game.Instance.LevelUpController.Preview.Progression.Features.Enumerable.Any((Feature f) => f.Blueprint != null && f.Blueprint == item.Feature && f.Param == item.Param)) : (!flag && Game.Instance.LevelUpController.Preview.Progression.Features.Enumerable.Any((Feature f) => f.Blueprint != null && f.Blueprint == item.Feature)));
		CanSelectState = (flag ? SelectState.CanSelect : (flag2 ? SelectState.AlreadyHas : SelectState.Forbidden));
	}
}

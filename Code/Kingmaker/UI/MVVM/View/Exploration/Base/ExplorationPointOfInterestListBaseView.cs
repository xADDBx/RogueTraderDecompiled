using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ExplorationPointOfInterestListBaseView<TExplorationPointOfInterestView, TExplorationResourcePointView> : ExplorationComponentBaseView<ExplorationPointOfInterestListVM> where TExplorationPointOfInterestView : ExplorationPointOfInterestBaseView where TExplorationResourcePointView : ExplorationResourcePointBaseView
{
	[Serializable]
	public struct PointsVariant
	{
		public TExplorationPointOfInterestView[] PointsOfInterest;

		public TExplorationResourcePointView[] ResourcePoints;
	}

	[SerializeField]
	private List<PointsVariant> m_ExplorationPointsVariants;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.UpdatePointsOfInterestCommand.Subscribe(UpdatePointsOfInterest));
		AddDisposable(base.ViewModel.UpdateResourcesCommand.Subscribe(UpdatePlanetResourcePoints));
		AddDisposable(base.ViewModel.ClearAllPoints.Subscribe(ClearAllPoints));
	}

	protected override void DestroyViewImplementation()
	{
		ClearAllPoints();
	}

	public List<IFloatConsoleNavigationEntity> GetNavigationEntities()
	{
		List<IFloatConsoleNavigationEntity> list = new List<IFloatConsoleNavigationEntity>();
		list.AddRange(m_ExplorationPointsVariants[base.ViewModel.PlanetVariant].PointsOfInterest);
		list.AddRange(m_ExplorationPointsVariants[base.ViewModel.PlanetVariant].ResourcePoints);
		return list;
	}

	private void UpdatePointsOfInterest()
	{
		for (int i = 0; i < base.ViewModel.PointsOfInterestVMs.Count; i++)
		{
			TExplorationPointOfInterestView val = m_ExplorationPointsVariants[base.ViewModel.PlanetVariant].PointsOfInterest[i];
			val.Initialize();
			val.Bind(base.ViewModel.PointsOfInterestVMs[i]);
			val.gameObject.SetActive(value: true);
		}
	}

	private void UpdatePlanetResourcePoints()
	{
		for (int i = 0; i < base.ViewModel.ResourcesVMs.Count; i++)
		{
			TExplorationResourcePointView val = m_ExplorationPointsVariants[base.ViewModel.PlanetVariant].ResourcePoints[i];
			val.Bind(base.ViewModel.ResourcesVMs[i]);
			val.gameObject.SetActive(value: true);
		}
	}

	private void ClearAllPoints()
	{
		foreach (PointsVariant explorationPointsVariant in m_ExplorationPointsVariants)
		{
			TExplorationPointOfInterestView[] pointsOfInterest = explorationPointsVariant.PointsOfInterest;
			foreach (TExplorationPointOfInterestView val in pointsOfInterest)
			{
				val.Unbind();
				val.gameObject.SetActive(value: false);
			}
			TExplorationResourcePointView[] resourcePoints = explorationPointsVariant.ResourcePoints;
			foreach (TExplorationResourcePointView val2 in resourcePoints)
			{
				val2.Unbind();
				val2.gameObject.SetActive(value: false);
			}
		}
	}
}

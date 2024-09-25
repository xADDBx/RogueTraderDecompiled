using System;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartDestructionStagesManager : MechanicEntityPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartDestructionStagesManager>, IEntityPartOwner
	{
		PartDestructionStagesManager DestructionStages { get; }
	}

	private IDestructionStagesManager[] m_ViewManagers = new IDestructionStagesManager[0];

	private DestructionStage m_CurrentStage;

	public PartHealth Health => base.Owner.GetRequired<PartHealth>();

	public DestructionStage Stage => CalculateDestructionStage();

	protected override void OnViewDidAttach()
	{
		Update(onLoad: true);
	}

	public void Update()
	{
		Update(onLoad: false);
	}

	public void UpdateOnIsInGameTrue()
	{
		Update(onLoad: true);
	}

	private void Update(bool onLoad)
	{
		if (base.Owner.View == null)
		{
			return;
		}
		if (onLoad)
		{
			m_ViewManagers = base.Owner.View.GetComponentsInChildren<IDestructionStagesManager>();
		}
		if (!onLoad && m_CurrentStage == Stage)
		{
			return;
		}
		m_CurrentStage = Stage;
		foreach (IDestructionStagesManager item in base.Owner.GetAll<IDestructionStagesManager>())
		{
			try
			{
				item.ChangeStage(Stage, onLoad);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex, "Exception occured in " + item.name);
			}
		}
		IDestructionStagesManager[] viewManagers = m_ViewManagers;
		foreach (IDestructionStagesManager destructionStagesManager in viewManagers)
		{
			try
			{
				destructionStagesManager.ChangeStage(Stage, onLoad);
			}
			catch (Exception ex2)
			{
				PFLog.Default.Exception(ex2, "Exception occured in " + destructionStagesManager.name);
			}
		}
	}

	private DestructionStage CalculateDestructionStage()
	{
		int num = Health.HitPoints;
		int hitPointsLeft = Health.HitPointsLeft;
		if (hitPointsLeft <= 0)
		{
			return DestructionStage.Destroyed;
		}
		if ((float)hitPointsLeft / (float)num <= 0.5f)
		{
			return DestructionStage.Damaged;
		}
		return DestructionStage.Whole;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

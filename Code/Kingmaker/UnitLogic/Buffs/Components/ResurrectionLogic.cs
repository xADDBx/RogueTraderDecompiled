using System;
using System.Collections;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[TypeId("6d63baaf147647f488d3d749eeddaeca")]
public class ResurrectionLogic : UnitBuffComponentDelegate, IHashable
{
	public GameObject FirstFx;

	public float FirstFxDelay;

	public GameObject SecondFx;

	public float SecondFxDelay;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Features.IsUntargetable.Retain();
		base.Owner.PreventDirectControl.Retain();
		base.Owner.Features.CantAct.Retain();
		base.Owner.Features.CantMove.Retain();
		if (base.Owner.View.AnimationManager != null)
		{
			base.Owner.View.AnimationManager.Disabled = true;
		}
		base.Owner.View.StartCoroutine(ResurrectionCoroutine(base.Runtime));
	}

	protected override void OnDeactivate()
	{
		base.Owner.Features.IsUntargetable.Release();
		base.Owner.PreventDirectControl.Release();
		base.Owner.Features.CantAct.Release();
		base.Owner.Features.CantMove.Release();
		base.Owner.View.HandsEquipment.HandleEquipmentSetChanged();
	}

	private IEnumerator ResurrectionCoroutine(ComponentRuntime runtime)
	{
		UnitEntityView unitView = base.Owner.View;
		TimeSpan startTime = Game.Instance.TimeController.GameTime;
		bool firstFxStarted = false;
		bool secondFxStarted = false;
		Vector3 torsoPosition = base.Owner.View.CenterTorso.transform.position;
		while (!secondFxStarted)
		{
			TimeSpan gameTime = Game.Instance.TimeController.GameTime;
			if (!firstFxStarted && gameTime - startTime >= FirstFxDelay.Seconds())
			{
				FxHelper.SpawnFxOnEntity(FirstFx, unitView);
				firstFxStarted = true;
			}
			if (gameTime - startTime >= SecondFxDelay.Seconds())
			{
				MechanicEntity maybeCaster = runtime.Fact.MaybeContext.MaybeCaster;
				BaseUnitEntity owner = runtime.Owner;
				PartVision partVision = maybeCaster?.GetVisionOptional();
				if (partVision != null && !partVision.HasLOS(owner))
				{
					owner.Position = maybeCaster.Position + maybeCaster.View.ViewTransform.forward.normalized * 2f;
				}
				else
				{
					Vector3 pos = GeometryUtils.ProjectToGround(torsoPosition);
					pos = ObstacleAnalyzer.GetNearestNode(pos).position;
					owner.Position = pos;
				}
				if (unitView.AnimationManager != null)
				{
					unitView.AnimationManager.Disabled = false;
				}
				FxHelper.SpawnFxOnEntity(SecondFx, unitView);
				secondFxStarted = true;
			}
			yield return null;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

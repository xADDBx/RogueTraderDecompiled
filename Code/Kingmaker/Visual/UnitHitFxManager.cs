using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual;

public class UnitHitFxManager : MonoBehaviour
{
	private class ReachFXMover : MonoBehaviour
	{
		public Transform From;

		public Transform To;

		public float Start;

		public float Finish;

		private void Update()
		{
			if ((bool)From && (bool)To && Finish > Start)
			{
				float num = (Time.time - Start) / (Finish - Start);
				base.transform.position = Vector3.Lerp(From.position, To.position, num);
				base.transform.LookAt(To.position);
				if (num < 1f)
				{
					return;
				}
			}
			FxHelper.Destroy(base.gameObject);
		}
	}

	private UnitEntityView m_View;

	private bool m_HasDodgeAction;

	public void HandleMeleeAttackHit(BaseUnitEntity attacker, AttackResult attackResult, bool crit, ItemEntityWeapon attackWeapon)
	{
		Vector3 normalized = (attacker.View.CenterTorso.position - m_View.CenterTorso.position).normalized;
		if (attackResult.IsHit())
		{
			if ((double)Vector3.Dot(normalized, m_View.ViewTransform.forward) > 0.3 && m_View.AnimationManager != null)
			{
				m_View.AnimationManager.Execute(UnitAnimationType.Hit);
			}
			float num = Vector3.Distance(attacker.Position, m_View.EntityData.Position) - m_View.Corpulence - attacker.Corpulence;
			float num2 = attacker.Corpulence * BlueprintRoot.Instance.SystemMechanics.ReachFXBaseRange;
			if (num >= num2)
			{
				SpawnReachHitFX(attacker);
			}
		}
		else
		{
			if (attackResult == AttackResult.Dodge && !CanDodge())
			{
				attackResult = AttackResult.Miss;
			}
			if (attackResult == AttackResult.Dodge)
			{
				m_View.AnimationManager?.ExecuteIfIdle(UnitAnimationType.Dodge);
			}
		}
	}

	private void SpawnReachHitFX(BaseUnitEntity attacker)
	{
		FxHelper.SpawnFxOnEntity(BlueprintRoot.Instance.SystemMechanics.ReachFXTargetPrefab, m_View);
		GameObject gameObject = FxHelper.SpawnFxOnEntity(BlueprintRoot.Instance.SystemMechanics.ReachFXMovingPrefab, attacker.View);
		if ((bool)gameObject)
		{
			string reachFXLocatorName = BlueprintRoot.Instance.SystemMechanics.ReachFXLocatorName;
			Transform transform = attacker.View.ParticlesSnapMap[reachFXLocatorName]?.Transform;
			Transform transform2 = m_View.ParticlesSnapMap[reachFXLocatorName]?.Transform;
			if ((bool)transform && (bool)transform2)
			{
				ReachFXMover reachFXMover = gameObject.EnsureComponent<ReachFXMover>();
				reachFXMover.From = transform;
				reachFXMover.To = transform2;
				reachFXMover.Start = Time.time;
				reachFXMover.Finish = Time.time + BlueprintRoot.Instance.SystemMechanics.ReachFXFlightTime;
			}
			else
			{
				FxHelper.Destroy(gameObject);
			}
		}
	}

	private bool CanDodge()
	{
		if (m_HasDodgeAction)
		{
			return m_View.AnimationManager.CanRunIdleAction();
		}
		return false;
	}

	public void SetView(UnitEntityView unitEntityView)
	{
		m_View = unitEntityView;
		m_HasDodgeAction = m_View.AnimationManager != null && m_View.AnimationManager.AnimationSet != null && (bool)m_View.AnimationManager.AnimationSet.GetAction(UnitAnimationType.Dodge);
	}
}

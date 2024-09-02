using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.GameCommands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[KnowledgeDatabaseID("b88723ff06f484141a79ce5ca2b23424")]
public class FogOfWarRevealerTrigger : EntityViewBase, IUpdatable
{
	[HashNoGenerate]
	private class EntityData : Entity
	{
		[JsonProperty]
		private bool m_RevealComplete;

		public new FogOfWarRevealerTrigger View => (FogOfWarRevealerTrigger)base.View;

		public EntityData(JsonConstructorMark _)
			: base(_)
		{
		}

		public EntityData(EntityViewBase view)
			: base(view.UniqueId, view.IsInGameBySettings)
		{
		}

		public EntityData(string uniqueId, bool isInGame)
			: base(uniqueId, isInGame)
		{
		}

		protected override IEntityViewBase CreateViewForData()
		{
			return null;
		}

		protected override void OnPreSave()
		{
			m_RevealComplete = View.m_RevealComplete;
		}

		protected override void OnViewDidAttach()
		{
			base.OnViewDidAttach();
			View.m_RevealComplete = m_RevealComplete;
			if (m_RevealComplete)
			{
				View.gameObject.SetActive(value: false);
			}
		}
	}

	[SerializeField]
	private bool ProceduralAnimate = true;

	[SerializeField]
	private bool Animated;

	[SerializeField]
	private List<Animator> Animators;

	[SerializeField]
	private string TriggerName = "fow";

	[SerializeField]
	private string AgroLayer = "Selectable";

	[SerializeField]
	private List<FogOfWarRevealerSettings> revealers = new List<FogOfWarRevealerSettings>();

	[SerializeField]
	private float ProceduralAnimationScaleSpeed = 0.05f;

	[SerializeField]
	private float ProceduralAnimationScaleLimit = 50f;

	[SerializeField]
	private float ProceduralAnimationScaleStep = 20f;

	[SerializeField]
	private bool KeepRevealerAliveAfterAnimation;

	private Rigidbody m_TriggerRigidBody;

	private List<Collider> m_TriggerColliders = new List<Collider>();

	private bool m_ShowLinkedRevealers;

	private bool m_RevealStarted;

	private bool m_RevealComplete;

	private bool m_SubscribedForUpdates;

	public static readonly Dictionary<string, FogOfWarRevealerTrigger> AllTriggers = new Dictionary<string, FogOfWarRevealerTrigger>();

	private Vector3 LocalScaleControl = Vector3.zero;

	protected override void OnEnable()
	{
		AllTriggers[UniqueId] = this;
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		AllTriggers.Remove(UniqueId);
		base.OnDisable();
	}

	private void Start()
	{
		m_TriggerRigidBody = GetComponentInChildren<Rigidbody>();
		m_TriggerColliders = GetComponentsInChildren<Collider>().ToList();
		if (!m_TriggerRigidBody)
		{
			base.enabled = false;
			UberDebug.LogError("Fow Revealer Animation Trigger Error : No Rigidbody on trigger : " + base.name);
		}
		base.gameObject.layer = LayerMask.NameToLayer("FXRaycast");
		if (m_TriggerColliders.Count < 1)
		{
			base.enabled = false;
			UberDebug.LogError("Fow Revealer Animation Trigger Error : No trigger colliders on trigger : " + base.name);
		}
		m_TriggerRigidBody.isKinematic = true;
		foreach (Collider triggerCollider in m_TriggerColliders)
		{
			triggerCollider.isTrigger = true;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UnsubscribeFromUpdates();
	}

	private void OnTriggerEnter(Collider trespasser)
	{
		if (trespasser.gameObject.layer == LayerMask.NameToLayer(AgroLayer) && !(trespasser.transform.parent == null) && !(trespasser.transform.parent.GetComponent<UnitEntityView>() == null) && trespasser.transform.parent.GetComponent<UnitEntityView>().EntityData != null && trespasser.transform.parent.GetComponent<UnitEntityView>().EntityData.GetCompanionOptional() != null && trespasser.transform.parent.GetComponent<UnitEntityView>().EntityData.GetCompanionOptional().State == CompanionState.InParty)
		{
			Game.Instance.GameCommandQueue.AddCommand(new FogOfWarRevealerTriggerGameCommand(UniqueId));
		}
	}

	public void Reveal()
	{
		if (m_RevealStarted || m_RevealComplete)
		{
			return;
		}
		if (revealers.Count <= 0)
		{
			UberDebug.LogError("No revealers in " + base.name);
			return;
		}
		if (AgroLayer == "")
		{
			UberDebug.LogError("Empty AgroLayer in " + base.name);
			return;
		}
		if (Animated)
		{
			if (Animators.Count <= 0)
			{
				UberDebug.LogError("No animators for trigger in " + base.name);
				return;
			}
			if (TriggerName == "")
			{
				UberDebug.LogError("Empty animation trigger name in " + base.name);
				return;
			}
		}
		if (Animated)
		{
			foreach (Animator animator in Animators)
			{
				animator.SetTrigger(TriggerName);
			}
			base.enabled = false;
		}
		else
		{
			if (!ProceduralAnimate)
			{
				return;
			}
			foreach (FogOfWarRevealerSettings revealer in revealers)
			{
				revealer.transform.localScale = Vector3.zero;
			}
			SubscribeForUpdates();
			m_RevealStarted = true;
		}
	}

	private void AnimateRevealer(float deltaTime)
	{
		if (LocalScaleControl.x >= ProceduralAnimationScaleLimit)
		{
			if (!KeepRevealerAliveAfterAnimation)
			{
				foreach (FogOfWarRevealerSettings revealer in revealers)
				{
					FogOfWarControllerData.RemoveRevealer(revealer.transform);
				}
			}
			if ((bool)m_TriggerRigidBody)
			{
				m_TriggerRigidBody.Sleep();
			}
			foreach (Collider triggerCollider in m_TriggerColliders)
			{
				if ((bool)triggerCollider)
				{
					triggerCollider.enabled = false;
				}
			}
			OnComplete();
			return;
		}
		foreach (FogOfWarRevealerSettings revealer2 in revealers)
		{
			if (!revealer2.RevealManual)
			{
				FogOfWarControllerData.AddRevealer(revealer2.transform);
				revealer2.RevealManual = true;
			}
			if (!revealer2.enabled)
			{
				revealer2.enabled = true;
			}
			Vector3 vector = ProceduralAnimationScaleStep * deltaTime * Vector3.one;
			Vector3 vector2 = revealer2.transform.localScale + vector;
			revealer2.transform.localScale = vector2;
			LocalScaleControl = vector2;
		}
	}

	void IUpdatable.Tick(float delta)
	{
		try
		{
			float gameDeltaTime = Game.Instance.TimeController.GameDeltaTime;
			gameDeltaTime = 1f / 60f * gameDeltaTime / Mathf.Max(1f / 60f, ProceduralAnimationScaleSpeed);
			AnimateRevealer(gameDeltaTime);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			OnComplete();
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnComplete()
	{
		base.enabled = false;
		m_RevealComplete = true;
		UnsubscribeFromUpdates();
	}

	public override Entity CreateEntityData(bool load)
	{
		return new EntityData(this);
	}

	private void SubscribeForUpdates()
	{
		if (!m_SubscribedForUpdates)
		{
			Game.Instance.FogOfWarRevealerTriggerController.Add(this);
			m_SubscribedForUpdates = true;
		}
	}

	private void UnsubscribeFromUpdates()
	{
		if (m_SubscribedForUpdates)
		{
			Game.Instance.FogOfWarRevealerTriggerController.Remove(this);
			m_SubscribedForUpdates = false;
		}
	}

	protected override void OnDrawGizmos()
	{
		if (!m_ShowLinkedRevealers)
		{
			return;
		}
		foreach (FogOfWarRevealerSettings revealer in revealers)
		{
			Debug.DrawLine(revealer.transform.position, base.ViewTransform.position, Color.cyan);
		}
	}
}

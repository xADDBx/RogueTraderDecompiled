using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Sound.Base;
using Kingmaker.Visual.Particles;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Highlighting;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class DestructionPart : ViewBasedPart<DestructionSettings>, IHashable
{
	[JsonProperty]
	private bool m_AlreadyDestroyed;

	public bool AlreadyDestroyed => m_AlreadyDestroyed;

	public new MapObjectView View => (MapObjectView)base.View;

	public new MapObjectEntity Owner => (MapObjectEntity)base.Owner;

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		UpdateViews();
		if ((bool)base.Settings.DestructionPlayer)
		{
			if (AlreadyDestroyed)
			{
				base.Settings.DestructionPlayer.RewindToEnd();
			}
			base.Settings.DestructionPlayer.Pause = true;
		}
	}

	private void UpdateViews()
	{
		SetActiveViews(base.Settings.DestroyedViews, AlreadyDestroyed);
		SetActiveViews(base.Settings.NormalViews, !AlreadyDestroyed);
		View.GetComponent<Highlighter>()?.ReinitMaterials();
	}

	private void SetActiveViews(List<GameObject> views, bool active)
	{
		foreach (GameObject view in views)
		{
			view.SetActive(active);
		}
	}

	public void ForceDestroy()
	{
		m_AlreadyDestroyed = true;
	}

	public void Destroy(BaseUnitEntity user)
	{
		if (AlreadyDestroyed)
		{
			PFLog.Default.Error("{0} can't destroy {1}", user, this);
		}
		else if (GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(user, StatType.SkillAthletics, base.Settings.DC)
		{
			Voice = RulePerformSkillCheck.VoicingType.All
		}).ResultIsSuccess)
		{
			SoundEventsManager.PostEvent(base.Settings.SuccessSound, View.gameObject);
			m_AlreadyDestroyed = true;
			UpdateViews();
			if ((bool)base.Settings.DestructionPlayer)
			{
				base.Settings.DestructionPlayer.RewindToStart();
				base.Settings.DestructionPlayer.Pause = false;
			}
			SpawnDestructionFx();
			using (ContextData<MechanicEntityData>.Request().Setup(Owner))
			{
				base.Settings.OnDestructionActions.Get()?.Actions.Run();
			}
			EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IDestructionHandler>)delegate(IDestructionHandler h)
			{
				h.HandleDestructionSuccess(View);
			}, isCheckRuntime: true);
		}
		else
		{
			SoundEventsManager.PostEvent(base.Settings.FailedSound, View.gameObject);
			EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IDestructionHandler>)delegate(IDestructionHandler h)
			{
				h.HandleDestructionFail(View);
			}, isCheckRuntime: true);
		}
	}

	public void SpawnDestructionFx()
	{
		PrefabLink destructionFx = BlueprintRoot.Instance.Interaction.DestructionFx;
		if ((object)destructionFx != null && destructionFx.Exists())
		{
			GameObject gameObject = FxHelper.SpawnFxOnGameObject(destructionFx.Load(), View.gameObject);
			Bounds viewBounds = GetViewBounds();
			gameObject.transform.localScale = viewBounds.size.magnitude * Vector3.one;
			gameObject.transform.position = viewBounds.center;
		}
	}

	private Bounds GetViewBounds()
	{
		Bounds result = new Bounds(View.gameObject.transform.position, Vector3.zero);
		bool flag = false;
		IEnumerable<Renderer> enumerable;
		if (View.Renderers.Count <= 0)
		{
			IEnumerable<Renderer> componentsInChildren = View.GetComponentsInChildren<Renderer>();
			enumerable = componentsInChildren;
		}
		else
		{
			IEnumerable<Renderer> componentsInChildren = View.Renderers;
			enumerable = componentsInChildren;
		}
		foreach (Renderer item in enumerable)
		{
			if ((bool)item)
			{
				if (!flag)
				{
					result = new Bounds(item.bounds.center, item.bounds.size);
					flag = true;
				}
				else
				{
					result.Encapsulate(item.bounds);
				}
			}
		}
		return result;
	}

	public BaseUnitEntity SelectUnit(List<BaseUnitEntity> units)
	{
		BaseUnitEntity baseUnitEntity = null;
		int num = int.MinValue;
		foreach (BaseUnitEntity unit in units)
		{
			if (units.Count <= 1 || !unit.IsPet)
			{
				int num2 = unit.Stats.GetStat(StatType.SkillAthletics);
				if (unit.State.CanAct && unit.State.CanMove && (baseUnitEntity == null || num2 > num))
				{
					baseUnitEntity = unit;
					num = num2;
				}
			}
		}
		return baseUnitEntity;
	}

	public void PlayStartSound(BaseUnitEntity executor)
	{
		SoundEventsManager.PostEvent(base.Settings.DestructionStartSound, View.Or(null)?.gameObject);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_AlreadyDestroyed);
		return result;
	}
}

using System;
using System.Collections.Generic;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.Settings;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Tutorial;

public class TutorialSystem : Entity, IHashable
{
	[JsonProperty]
	private TimeSpan m_CooldownBeforeTime;

	[JsonProperty]
	private int m_ShowIndex;

	[CanBeNull]
	private TutorialData m_ShowData;

	[JsonProperty]
	private List<TutorialTag> m_BannedTags = new List<TutorialTag>();

	[CanBeNull]
	private TutorialData m_CandidateForShow;

	private float? m_Countdown;

	private readonly HashSet<BlueprintTutorial> m_TriedToTriggerThisFrame = new HashSet<BlueprintTutorial>();

	public bool HasCooldown => m_CooldownBeforeTime > Game.Instance.TimeController.RealTime;

	public int LastCooldownTutorialPriority { get; private set; }

	public bool HasCandidateForShow => m_CandidateForShow != null;

	public bool HasShownData => m_ShowData != null;

	public TutorialData ShowingData
	{
		get
		{
			return m_ShowData;
		}
		set
		{
			m_ShowData = value;
		}
	}

	public Tutorial Ensure(BlueprintTutorial blueprint)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			return Facts.Get<Tutorial>(blueprint) ?? Facts.Add(new Tutorial(blueprint));
		}
	}

	public bool IsTagBanned(TutorialTag tag)
	{
		return !SettingsRoot.Game.Tutorial.ShouldShowTag(tag);
	}

	protected TutorialSystem(JsonConstructorMark _)
		: base(_)
	{
	}

	public TutorialSystem()
		: base("tutorial_system", isInGame: true)
	{
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	public void Ban(BlueprintTutorial blueprintTutorial)
	{
		Tutorial tutorial = Ensure(blueprintTutorial);
		tutorial.Banned = true;
		tutorial.UpdateIsEnabled();
	}

	public void BanTag(TutorialTag tag)
	{
		if (!IsTagBanned(tag))
		{
			m_BannedTags.Add(tag);
			SettingsRoot.Game.Tutorial.SetValueAndConfirmForTag(tag, value: false);
			SettingsController.Instance.SaveAll();
			UpdateEnabledTutorials();
		}
	}

	public void UpdateEnabledTutorials()
	{
		foreach (Tutorial item in Facts.GetAll<Tutorial>())
		{
			item.UpdateIsEnabled();
		}
	}

	public bool OnTryToTrigger(Tutorial tutorial, TutorialContext context)
	{
		if (!tutorial.IsEnabled)
		{
			if (tutorial.IsLimitReached)
			{
				EventBus.RaiseEvent(delegate(ITutorialTriggerFailedHandler h)
				{
					h.HandleLimitReached(tutorial, context);
				});
			}
			if (tutorial.Owner.IsTagBanned(tutorial.Blueprint.Tag))
			{
				EventBus.RaiseEvent(delegate(ITutorialTriggerFailedHandler h)
				{
					h.HandleTagBanned(tutorial, context);
				});
			}
			return false;
		}
		if (!m_TriedToTriggerThisFrame.Contains(tutorial.Blueprint))
		{
			tutorial.TriggeredTimes++;
		}
		m_TriedToTriggerThisFrame.Add(tutorial.Blueprint);
		if (!tutorial.Blueprint.IgnoreCooldown && HasCooldown)
		{
			if (LastCooldownTutorialPriority > tutorial.Blueprint.Priority)
			{
				EventBus.RaiseEvent(delegate(ITutorialTriggerFailedHandler h)
				{
					h.HandleHigherPriorityCooldown(tutorial, context);
				});
			}
			else
			{
				EventBus.RaiseEvent(delegate(ITutorialTriggerFailedHandler h)
				{
					h.HandleLowerOrEqualPriorityCooldown(tutorial, context);
				});
			}
			return false;
		}
		if (tutorial.Blueprint.Frequency > 1 && tutorial.LastShowIndex > 0 && Math.Abs(m_ShowIndex - tutorial.LastShowIndex) < tutorial.Blueprint.Frequency)
		{
			EventBus.RaiseEvent(delegate(ITutorialTriggerFailedHandler h)
			{
				h.HandleFrequencyReached(tutorial, context);
			});
			return false;
		}
		if (m_CandidateForShow != null && m_CandidateForShow.Blueprint.Priority >= tutorial.Blueprint.Priority)
		{
			EventBus.RaiseEvent(delegate(ITutorialTriggerFailedHandler h)
			{
				h.HandleLowerOrEqualPriorityCooldown(tutorial, context);
			});
			return false;
		}
		if (m_CandidateForShow?.Trigger != null)
		{
			return false;
		}
		return true;
	}

	public void Trigger(BlueprintTutorial tutorial, [CanBeNull] TutorialTrigger trigger)
	{
		TutorialContext current = ContextData<TutorialContext>.Current;
		if (current == null)
		{
			PFLog.Default.ErrorWithReport("BlueprintTutorial.Trigger: context is missing");
			return;
		}
		bool flag = false;
		if (trigger != null)
		{
			foreach (TutorialSolver component in tutorial.GetComponents<TutorialSolver>())
			{
				try
				{
					flag = component.Solve(current);
				}
				catch (Exception exception)
				{
					PFLog.Default.ExceptionWithReport(exception, null);
				}
				if (flag)
				{
					break;
				}
			}
		}
		if (trigger != null && trigger.RevealTargetUnitInfo && current.RevealUnitInfo.FromBaseUnitEntity() == null)
		{
			current.RevealUnitInfo = current.TargetUnit;
		}
		TutorialData tutorialData = new TutorialData(tutorial, trigger, current.RevealUnitInfo, flag)
		{
			SolutionItem = current.SolutionItem,
			SolutionAbility = current.SolutionAbility,
			SolutionUnit = (current.SolutionUnit ?? (current.SolutionAbility?.Caster as BaseUnitEntity) ?? (current.SolutionItem?.Owner as BaseUnitEntity)),
			SourceUnit = current.SourceUnit
		};
		tutorialData.AddPage(tutorial);
		foreach (ITutorialPage component2 in tutorial.GetComponents<ITutorialPage>())
		{
			tutorialData.AddPage(component2);
		}
		m_CandidateForShow = tutorialData;
		m_Countdown = ((trigger != null && !tutorial.IgnoreCooldown) ? new float?(BlueprintRoot.Instance.SystemMechanics.TutorialDelaySeconds) : null);
		if (trigger != null && LoadingProcess.Instance.IsLoadingInProcess)
		{
			m_Countdown = m_Countdown.GetValueOrDefault() + BlueprintRoot.Instance.SystemMechanics.TutorialDelaySecondsAfterLoading;
		}
	}

	public void Tick()
	{
		m_TriedToTriggerThisFrame.Clear();
		if (m_CandidateForShow == null)
		{
			return;
		}
		if (m_Countdown.HasValue)
		{
			m_Countdown -= Game.Instance.TimeController.DeltaTime;
			if (m_Countdown > 0f)
			{
				return;
			}
		}
		if (CutsceneLock.Active)
		{
			return;
		}
		if (m_CandidateForShow.Blueprint.SetCooldown)
		{
			m_CooldownBeforeTime = Game.Instance.TimeController.RealTime + BlueprintRoot.Instance.SystemMechanics.TutorialCooldownSeconds.Seconds();
			LastCooldownTutorialPriority = m_CandidateForShow.Blueprint.Priority;
		}
		if (m_CandidateForShow.RevealUnitInfo.Entity.FromBaseUnitEntity() != null)
		{
			Game.Instance.Player.InspectUnitsManager.ForceRevealUnitInfo(m_CandidateForShow.RevealUnitInfo.Entity);
		}
		try
		{
			Show(m_CandidateForShow);
		}
		finally
		{
			m_CandidateForShow = null;
			m_Countdown = null;
		}
	}

	private void Show(TutorialData data)
	{
		EventBus.RaiseEvent(delegate(INewTutorialUIHandler h)
		{
			h.ShowTutorial(data);
		});
		Tutorial tutorial;
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			tutorial = Ensure(data.Blueprint);
		}
		tutorial.ShowedTimes++;
		tutorial.LastShowIndex = ++m_ShowIndex;
		tutorial.UpdateIsEnabled();
	}

	[Cheat(Name = "tutorial_unban")]
	public static void UnBanAll()
	{
		SettingsRoot.Game.Tutorial.SetValueAndConfirmForAll(value: true);
		SettingsController.Instance.SaveAll();
		foreach (Tutorial item in Game.Instance.Player.Tutorial.Facts.GetAll<Tutorial>())
		{
			item.Banned = false;
		}
		Game.Instance.Player.Tutorial.UpdateEnabledTutorials();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_CooldownBeforeTime);
		result.Append(ref m_ShowIndex);
		List<TutorialTag> bannedTags = m_BannedTags;
		if (bannedTags != null)
		{
			for (int i = 0; i < bannedTags.Count; i++)
			{
				TutorialTag obj = bannedTags[i];
				Hash128 val2 = UnmanagedHasher<TutorialTag>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		return result;
	}
}

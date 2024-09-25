using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.EntitySystem;
using Kingmaker.QA;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

public class EtudesTree : EntityFactsProcessor<Etude>
{
	private readonly List<Etude> m_Roots = new List<Etude>();

	private readonly HashSet<Etude> m_StoppingSet = new HashSet<Etude>();

	private readonly HashSet<Etude> m_StartingSet = new HashSet<Etude>();

	private bool m_NeedCleanupCompleted;

	private EtudesSystem EtudesSystem => (EtudesSystem)base.Manager.Owner;

	protected override Etude PrepareFactForAttach(Etude fact)
	{
		fact.SuppressActivationOnAttach = true;
		return fact;
	}

	protected override Etude PrepareFactForDetach(Etude fact)
	{
		return fact;
	}

	protected override void OnFactDidAttach(Etude fact)
	{
		LinkEtudeToTree(fact);
	}

	private void LinkEtudeToTree(Etude fact)
	{
		(fact.Parent?.Children ?? m_Roots).Add(fact);
	}

	protected override void OnFactWillDetach(Etude fact)
	{
		foreach (Etude item in fact.Children.ToTempList())
		{
			Remove(item);
		}
		(fact.Parent?.Children ?? m_Roots).Remove(fact);
	}

	protected override void OnFactDidDetached(Etude fact)
	{
	}

	public void RestoreTreeStructure()
	{
		foreach (Etude rawFact in base.RawFacts)
		{
			LinkEtudeToTree(rawFact);
		}
	}

	public void FixupEtudesTree(EtudesSystem system)
	{
		m_Roots.ToTempList().ForEach(delegate(Etude root)
		{
			FixupEtudeStartsWith(system, root);
		});
	}

	private void FixupEtudeStartsWith(EtudesSystem system, Etude etude)
	{
		if (!etude.IsPlaying)
		{
			return;
		}
		foreach (BlueprintEtudeReference item in etude.Blueprint.StartsWith)
		{
			BlueprintEtude blueprint = item.Get();
			if (blueprint == null)
			{
				PFLog.Etudes.Error("Can't find Blueprint with guid " + item.Guid);
			}
			else if (!system.EtudeIsCompleted(blueprint) && !base.RawFacts.HasItem((Etude x) => x.Blueprint == blueprint))
			{
				system.StartEtude(blueprint, "etudes fixup StartWith");
			}
		}
		foreach (Etude item2 in etude.Children.ToTempList())
		{
			FixupEtudeStartsWith(system, item2);
		}
	}

	public void CheckEtudeHierarchyForErrors()
	{
		StringBuilder stringBuilder = new StringBuilder();
		HashSet<BlueprintEtude> hashSet = BlueprintRoot.Instance.UtilityRoot.SkipCheckingEtudeHierarchy.Select((BlueprintEtudeReference e) => e.Get()).ToHashSet();
		foreach (Etude rawFact in base.RawFacts)
		{
			if (hashSet.Contains(rawFact.Blueprint))
			{
				PFLog.Etudes.Log($"Skip checking etude hierarchy {rawFact}");
				continue;
			}
			if (rawFact.Blueprint == null)
			{
				PFLog.Etudes.Error($"Can't find Blueprint for etude {rawFact}");
				continue;
			}
			string etudeHierarchyError = GetEtudeHierarchyError(rawFact);
			stringBuilder.Append(etudeHierarchyError);
		}
		if (stringBuilder.Length > 0)
		{
			PFLog.Default.ErrorWithReport(stringBuilder.ToString());
		}
	}

	private string GetEtudeHierarchyError(Etude etude)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(CheckEtudeParent(etude));
		stringBuilder.Append(CheckEtudeParentRunning(etude));
		stringBuilder.Append(CheckEtudeChildren(etude));
		return stringBuilder.ToString();
	}

	private string CheckEtudeParent(Etude etude)
	{
		if (etude.Parent?.Blueprint != etude.Blueprint.Parent?.Get())
		{
			return $"[Etude Hierarchy] {etude} Parent mismatch! Blueprint: {etude.Blueprint.Parent?.Get()}, State: {etude.Parent?.Blueprint} \n";
		}
		return string.Empty;
	}

	private string CheckEtudeParentRunning(Etude etude)
	{
		if (etude.Blueprint.StartsParent && !Game.Instance.Player.EtudesSystem.EtudeIsNotStarted(etude.Blueprint) && Game.Instance.Player.EtudesSystem.EtudeIsNotStarted(etude.Parent?.Blueprint))
		{
			return $"[Etude Hierarchy] {etude} should start parent, but {etude.Parent?.Blueprint} is not started! \n";
		}
		return string.Empty;
	}

	private string CheckEtudeChildren(Etude etude)
	{
		StringBuilder stringBuilder = new StringBuilder();
		HashSet<BlueprintEtude> hashSet = (from e in etude.Blueprint.StartsWith
			where e.Get() != null
			select e.Get()).ToHashSet();
		HashSet<BlueprintEtude> hashSet2 = etude.Children.Select((Etude e) => e.Blueprint).ToHashSet();
		hashSet.IntersectWith(hashSet2);
		hashSet2.SymmetricExceptWith(hashSet);
		foreach (Etude child in etude.Children)
		{
			if (child.Parent == etude)
			{
				hashSet2.Remove(child.Blueprint);
			}
		}
		if (hashSet2.Count > 0)
		{
			string arg = hashSet2.Aggregate(string.Empty, (string result, BlueprintEtude etude) => result + etude.name + ", ");
			stringBuilder.Append($"[Etude Hierarchy] {etude} Children mismatch!, State has etudes: {arg}but BP hasn't!\n");
		}
		return stringBuilder.ToString();
	}

	public void MaybeDeactivateCompletedEtudes()
	{
		m_NeedCleanupCompleted = false;
		foreach (Etude root in m_Roots)
		{
			MaybeDeactivateCompleted(root);
		}
		if (!m_NeedCleanupCompleted)
		{
			return;
		}
		foreach (Etude item in base.RawFacts.ToTempList())
		{
			if (item.IsCompleted)
			{
				Remove(item);
				EtudesSystem.InternalMarkCompleted(item.Blueprint);
			}
		}
	}

	private void MaybeDeactivateCompleted(Etude etude)
	{
		if (etude.IsCompleted)
		{
			return;
		}
		bool flag = etude.CompletionInProgress;
		etude.ComplitionBlockers.Clear();
		foreach (Etude item in etude.Children.ToTempList())
		{
			MaybeDeactivateCompleted(item);
			if (!item.IsCompleted)
			{
				etude.ComplitionBlockers.Add(item);
			}
			flag = item.IsCompleted && flag;
		}
		bool flag2 = !etude.IsPlaying || etude.Blueprint.CompletionCondition.Check();
		if (!(flag && flag2))
		{
			return;
		}
		etude.CallComponents(delegate(IEtudeCompleteTrigger t)
		{
			t.OnComplete();
		});
		if (etude.IsActive)
		{
			etude.Deactivate();
		}
		etude.FinishCompletion();
		m_NeedCleanupCompleted = true;
		if (etude.Parent != null && etude.Parent.CompletionInProgress)
		{
			return;
		}
		PFLog.Etudes.Log("Finally completed etude: " + etude.Blueprint.name);
		foreach (BlueprintEtudeReference item2 in etude.Blueprint.StartsOnComplete)
		{
			if (!item2.IsEmpty())
			{
				EtudesSystem.StartEtude(item2.Get(), "startsOnComplete from " + etude.Name + " " + etude.UniqueId);
			}
		}
	}

	public void SelectPlayingEtudes()
	{
		m_StoppingSet.Clear();
		m_StartingSet.Clear();
		foreach (Etude root in m_Roots)
		{
			ProcessEtudeActivation(root);
		}
		m_StartingSet.RemoveWhere((Etude e) => !IsPlayingOrStarting(e.Parent));
		if (FilterOnActors())
		{
			m_StartingSet.RemoveWhere((Etude e) => !IsPlayingOrStarting(e.Parent));
		}
		if (FilterOnSynchronization())
		{
			m_StartingSet.RemoveWhere((Etude e) => !IsPlayingOrStarting(e.Parent));
		}
		foreach (Etude item in m_StoppingSet)
		{
			Stop(item, isRoot: true);
		}
		foreach (Etude item2 in m_StartingSet)
		{
			item2.Activate();
		}
	}

	private bool IsPlayingOrStarting(Etude etude)
	{
		while (etude != null)
		{
			if ((!etude.IsPlaying && !m_StartingSet.Contains(etude)) || m_StoppingSet.Contains(etude))
			{
				return false;
			}
			etude = etude.Parent;
		}
		return true;
	}

	private void ProcessEtudeActivation(Etude etude)
	{
		if (etude.IsCompleted)
		{
			return;
		}
		bool flag = EtudeCanPlay(etude);
		if (etude.IsPlaying)
		{
			if (!flag)
			{
				m_StoppingSet.Add(etude);
			}
		}
		else if (flag)
		{
			m_StartingSet.Add(etude);
		}
		if (!flag)
		{
			return;
		}
		foreach (Etude child in etude.Children)
		{
			ProcessEtudeActivation(child);
		}
	}

	private bool FilterOnActors()
	{
		bool result = false;
		List<Etude> list = (from e in m_StartingSet
			where e.Blueprint.HasActors
			orderby e.Blueprint.Priority descending
			select e).ToTempList();
		for (int i = 0; i < list.Count; i++)
		{
			Etude etude = list[i];
			if ((etude.Parent != null && !IsPlayingOrStarting(etude.Parent)) || CheckBlockConflicts(etude, list, i))
			{
				m_StartingSet.Remove(etude);
				result = true;
			}
			else
			{
				BreakConflictingEtude(etude);
			}
		}
		return result;
	}

	private bool CheckBlockConflicts(Etude etude, List<Etude> startingByPrio, int index)
	{
		foreach (BlueprintEtudeConflictingGroupReference actorReference in etude.Blueprint.ConflictingGroups)
		{
			if (actorReference.IsEmpty())
			{
				continue;
			}
			BlueprintEtude task = Game.Instance.Player.EtudesSystem.GetConflictingGroupTask(actorReference.Get());
			if (!task || task.Priority < etude.Blueprint.Priority || m_StoppingSet.Any((Etude t) => IsSameOrParentOf(t.Blueprint, task)))
			{
				for (int i = 0; i < index; i++)
				{
					Etude etude2 = startingByPrio[i];
					if (m_StartingSet.Contains(etude2) && etude2.Blueprint.ConflictingGroups.Any((BlueprintEtudeConflictingGroupReference r) => r.Guid == actorReference.Guid))
					{
						task = startingByPrio[i].Blueprint;
						break;
					}
				}
			}
			if ((bool)task && task.Priority >= etude.Blueprint.Priority)
			{
				return true;
			}
		}
		return false;
	}

	private void BreakConflictingEtude(Etude etude)
	{
		foreach (BlueprintEtudeConflictingGroupReference conflictingGroup in etude.Blueprint.ConflictingGroups)
		{
			if (!conflictingGroup.IsEmpty())
			{
				BlueprintEtude conflictingGroupTask = Game.Instance.Player.EtudesSystem.GetConflictingGroupTask(conflictingGroup.Get());
				if ((bool)conflictingGroupTask)
				{
					PFLog.Etudes.Log("Etude " + conflictingGroupTask.name + " interrupted: conflict with " + etude.Blueprint.name + " on " + conflictingGroup.Get().name);
					m_StoppingSet.Add(Get(conflictingGroupTask));
				}
			}
		}
	}

	private void RemoveChildTreeFromStarting(Etude parent)
	{
		foreach (Etude child in parent.Children)
		{
			m_StartingSet.Remove(child);
			RemoveChildTreeFromStarting(child);
		}
	}

	private bool FilterOnSynchronization()
	{
		bool result = false;
		foreach (Etude rawFact in base.RawFacts)
		{
			if (!rawFact.Blueprint.IsSynchronized || !IsPlayingOrStarting(rawFact) || rawFact.Blueprint.HasActors)
			{
				continue;
			}
			foreach (BlueprintEtudeReference item in rawFact.Blueprint.Synchronized)
			{
				if (item.IsEmpty() || item.Get().IsSynchronized)
				{
					continue;
				}
				Etude etude = Get(item.Get());
				if (etude == null || !IsPlayingOrStarting(etude))
				{
					if (rawFact.IsPlaying)
					{
						PFLog.Etudes.Log($"Stopping etude {rawFact.Blueprint} because sync etude {item.Get()} is not playing");
						m_StoppingSet.Add(rawFact);
					}
					else
					{
						m_StartingSet.Remove(rawFact);
					}
					result = true;
				}
			}
		}
		return result;
	}

	private bool IsSameOrParentOf(BlueprintEtude e1, BlueprintEtude e2)
	{
		if (e1 != e2)
		{
			if (!e1.Parent.IsEmpty())
			{
				return IsSameOrParentOf(e1.Parent.Get(), e2);
			}
			return false;
		}
		return true;
	}

	private bool EtudeCanPlay(Etude etude)
	{
		if (etude.Blueprint.ActivationCondition.Check())
		{
			if (etude.Blueprint.HasLinkedAreaPart)
			{
				return etude.Blueprint.IsLinkedAreaPart(EtudesSystem.LoadEtudesForAreaPart);
			}
			return true;
		}
		return false;
	}

	private void Stop(Etude etude, bool isRoot)
	{
		foreach (Etude child in etude.Children)
		{
			Stop(child, isRoot: false);
		}
		if (!isRoot)
		{
			m_StartingSet.Remove(etude);
		}
		if (etude.IsPlaying)
		{
			etude.Deactivate();
		}
	}

	public void AddToPlaying(Etude etude)
	{
		foreach (BlueprintEtudeConflictingGroupReference conflictingGroup in etude.Blueprint.ConflictingGroups)
		{
			((EtudesSystem)base.Manager.Owner).SetConflictingGroupTask(conflictingGroup.Get(), etude);
		}
	}

	public void RemoveFromPlaying(Etude etude)
	{
		foreach (BlueprintEtudeConflictingGroupReference conflictingGroup in etude.Blueprint.ConflictingGroups)
		{
			((EtudesSystem)base.Manager.Owner).SetConflictingGroupTask(conflictingGroup.Get(), null);
		}
	}
}

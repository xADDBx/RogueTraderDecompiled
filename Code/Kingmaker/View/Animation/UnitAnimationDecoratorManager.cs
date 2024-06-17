using System;
using System.Collections.Generic;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View.Animation;

public class UnitAnimationDecoratorManager
{
	public class DecoratorCacheEntry
	{
		public Tuple<UnitAnimationDecoratorObject, AnimationClip> Id;

		public float Lifetime;

		public List<GameObject> Decorators = new List<GameObject>();
	}

	private AnimationManager m_AnimationManager;

	private Dictionary<Tuple<UnitAnimationDecoratorObject, AnimationClip>, DecoratorCacheEntry> m_Decorators = new Dictionary<Tuple<UnitAnimationDecoratorObject, AnimationClip>, DecoratorCacheEntry>();

	private List<DecoratorCacheEntry> m_CurrentDecoratorsList = new List<DecoratorCacheEntry>();

	public UnitAnimationDecoratorManager(UnitAnimationManager animationManager)
	{
		m_AnimationManager = animationManager;
	}

	public void Update(float dt)
	{
		if (m_CurrentDecoratorsList.Count <= 0)
		{
			return;
		}
		AnimationClip animationClip = m_AnimationManager.CurrentAction?.ActiveAnimation?.GetPlayableClip();
		for (int num = m_CurrentDecoratorsList.Count - 1; num >= 0; num--)
		{
			DecoratorCacheEntry decoratorCacheEntry = m_CurrentDecoratorsList[num];
			decoratorCacheEntry.Lifetime -= dt;
			if (decoratorCacheEntry.Lifetime <= 0f || animationClip == null || animationClip != decoratorCacheEntry.Id.Item2)
			{
				foreach (GameObject decorator in decoratorCacheEntry.Decorators)
				{
					decorator?.SetActive(value: false);
				}
				m_CurrentDecoratorsList.RemoveAt(num);
			}
		}
	}

	public void ShowDecorator(UnitAnimationDecoratorObject decorator, AbstractUnitEntityView unitEntity = null)
	{
		AnimationClip animationClip = m_AnimationManager.CurrentAction?.ActiveAnimation?.GetPlayableClip();
		PFLog.Default.Log("Clip in list " + animationClip);
		if (animationClip == null)
		{
			return;
		}
		Tuple<UnitAnimationDecoratorObject, AnimationClip> tuple = Tuple.Create(decorator, animationClip);
		if (!m_Decorators.TryGetValue(tuple, out var decoratorEntry))
		{
			decoratorEntry = new DecoratorCacheEntry
			{
				Id = tuple
			};
			DecoratorEntry[] entries = decorator.Entries;
			foreach (DecoratorEntry decoratorEntry2 in entries)
			{
				if (!string.IsNullOrEmpty(decoratorEntry2.BoneName) && decoratorEntry2.Prefab != null)
				{
					Transform transform = m_AnimationManager.transform.FindChildRecursive(decoratorEntry2.BoneName);
					if (transform != null)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(decoratorEntry2.Prefab);
						gameObject.transform.parent = transform;
						gameObject.transform.localScale = decoratorEntry2.Scale;
						gameObject.transform.localPosition = decoratorEntry2.Position;
						gameObject.transform.localRotation = Quaternion.Euler(decoratorEntry2.Rotation);
						decoratorEntry.Decorators.Add(gameObject);
						unitEntity?.MarkRenderersAndCollidersAreUpdated();
					}
				}
			}
			m_Decorators.Add(tuple, decoratorEntry);
		}
		if (!m_CurrentDecoratorsList.Exists((DecoratorCacheEntry x) => x == decoratorEntry))
		{
			m_CurrentDecoratorsList.Add(decoratorEntry);
		}
		decoratorEntry.Lifetime = decorator.Duration;
		foreach (GameObject decorator2 in decoratorEntry.Decorators)
		{
			decorator2.SetActive(value: true);
		}
	}
}

using DG.Tweening;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Pathfinding;
using UnityEngine;
using UnityHeapCrawler;

namespace Kingmaker.Utility;

public class GameHeapSnapshot
{
	public static void HeapSnapshot()
	{
		HeapSnapshotCollector heapSnapshotCollector = new HeapSnapshotCollector().AddRoot(Game.Instance, "Game.Instance").AddRoot(EventBus.GlobalSubscribers, "EventBus.GlobalSubscribers").AddRoot(EventBus.EntitySubscribers, "EventBus.UnitSubscribers")
			.AddRoot(RulebookEventBus.GlobalRulebookSubscribers, "RulebookEventBus.GlobalRulebookSubscribers")
			.AddRoot(RulebookEventBus.InitiatorRulebookSubscribers, "RulebookEventBus.InitiatorRulebookSubscribers")
			.AddRoot(RulebookEventBus.TargetRulebookSubscribers, "RulebookEventBus.TargetRulebookSubscribers")
			.AddForbiddenTypes(typeof(ResourcesLibrary))
			.AddForbiddenTypes("TweenManager")
			.AddStaticTypes(GenericStaticTypesHolder.Types.ToArray())
			.AddStaticTypes(PoolTypesHolder.Types.ToArray())
			.AddTrackedTypes(typeof(Mesh))
			.AddTrackedTypes(typeof(EntityViewBase))
			.AddTrackedTypes(typeof(Entity))
			.AddTrackedTypes(typeof(EntityFact))
			.AddTrackedTypes(typeof(Texture), typeof(Sprite))
			.AddTrackedTypes(typeof(Object))
			.AddRootTypes(typeof(IController));
		heapSnapshotCollector.SizeMode = SizeMode.Total;
		heapSnapshotCollector.UserRootsSettings.MinItemSize = 0;
		heapSnapshotCollector.UserRootsSettings.MaxChildren = 100;
		CrawlSettings crawlSettings = heapSnapshotCollector.AddUnityRootsGroup<BlueprintScriptableObject>("blueprints", "Blueprints", CrawlOrder.ScriptableObjects);
		crawlSettings.MinItemSize = 0;
		crawlSettings.MaxChildren = 100;
		CrawlSettings crawlSettings2 = heapSnapshotCollector.AddRootsGroup("preloaded-resources", "Preloaded resources", CrawlOrder.ScriptableObjects, ResourcesLibrary.LoadedResources);
		crawlSettings2.IncludeAllUnityTypes = true;
		crawlSettings2.MinItemSize = 0;
		crawlSettings2.MaxChildren = 100;
		CrawlSettings crawlSettings3 = heapSnapshotCollector.AddRootsGroup("tweens", "Tweens", CrawlOrder.ScriptableObjects, DOTween.PlayingTweens(), DOTween.PausedTweens());
		crawlSettings3.IncludeAllUnityTypes = true;
		crawlSettings3.MinItemSize = 0;
		crawlSettings3.MaxChildren = 100;
		heapSnapshotCollector.HierarchySettings.PrintOnlyGameObjects = false;
		heapSnapshotCollector.HierarchySettings.MinItemSize = 0;
		heapSnapshotCollector.HierarchySettings.MaxChildren = 100;
		heapSnapshotCollector.PrefabsSettings.PrintOnlyGameObjects = false;
		heapSnapshotCollector.PrefabsSettings.MinItemSize = 0;
		heapSnapshotCollector.PrefabsSettings.MaxChildren = 100;
		heapSnapshotCollector.UnityObjectsSettings.PrintOnlyGameObjects = false;
		heapSnapshotCollector.UnityObjectsSettings.MinItemSize = 0;
		heapSnapshotCollector.UnityObjectsSettings.MaxChildren = 100;
		heapSnapshotCollector.Start();
	}

	public static void StartSnapshot()
	{
		if (CommandLineArguments.Parse().Contains("start-snapshot"))
		{
			HeapSnapshot();
		}
	}

	public static void MainMenuSnapshot()
	{
		if (CommandLineArguments.Parse().Contains("menu-snapshot"))
		{
			HeapSnapshot();
		}
	}
}

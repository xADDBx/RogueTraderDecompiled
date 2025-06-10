using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("d958e19d8e614d5085e2fbd286bbe07c")]
public class EtudeBracketForbidOpenSelectedWindows : EtudeBracketTrigger, IHashable
{
	[Flags]
	public enum SelectedWindowsTypes
	{
		Inventory = 1,
		CharacterInfo = 2
	}

	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public SelectedWindowsTypes SelectedWindows;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref SelectedWindows);
			return result;
		}
	}

	[EnumFlagsAsButtons]
	public SelectedWindowsTypes SelectedWindows;

	protected override void OnEnter()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		savableData.SelectedWindows = SelectedWindows;
		bool flag = false;
		if (HasFlag(savableData.SelectedWindows, SelectedWindowsTypes.Inventory))
		{
			Game.Instance.Player.InventoryWindowBlocked.Retain();
			flag = true;
		}
		if (HasFlag(savableData.SelectedWindows, SelectedWindowsTypes.CharacterInfo))
		{
			Game.Instance.Player.CharacterInfoWindowBlocked.Retain();
			flag = true;
		}
		if (flag)
		{
			EventBus.RaiseEvent(delegate(ICanAccessSelectedWindowsHandler h)
			{
				h.HandleSelectedWindowsBlocked();
			});
		}
	}

	protected override void OnExit()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		bool flag = false;
		if (HasFlag(savableData.SelectedWindows, SelectedWindowsTypes.Inventory))
		{
			Game.Instance.Player.InventoryWindowBlocked.Release();
			flag = true;
		}
		if (HasFlag(savableData.SelectedWindows, SelectedWindowsTypes.CharacterInfo))
		{
			Game.Instance.Player.CharacterInfoWindowBlocked.Release();
			flag = true;
		}
		if (flag)
		{
			EventBus.RaiseEvent(delegate(ICanAccessSelectedWindowsHandler h)
			{
				h.HandleSelectedWindowsBlocked();
			});
		}
	}

	private static bool HasFlag(SelectedWindowsTypes selectedWindows, SelectedWindowsTypes windowType)
	{
		return (selectedWindows & windowType) != 0;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

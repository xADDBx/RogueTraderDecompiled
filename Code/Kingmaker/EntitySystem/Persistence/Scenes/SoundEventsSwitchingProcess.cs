using System.Collections;
using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.Sound;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Persistence.Scenes;

public class SoundEventsSwitchingProcess
{
	private List<SoundEventsEmitter.Info> m_ActivateList;

	private List<SoundEventsEmitter.Info> m_DeactivateList;

	public void BeforeAreaPartChange(BlueprintAreaPart newAreaPart)
	{
		if ((bool)newAreaPart && newAreaPart.ManageBanksSeparately)
		{
			SoundBanksManager.LoadBanks(newAreaPart.GetActiveSoundBankNames(isCurrentPart: true));
		}
		CollectSoundEventEmittersToDeactivate(newAreaPart);
		EmitDeactivate();
	}

	public IEnumerator AfterAreaPartChange(BlueprintAreaPart newAreaPart)
	{
		CollectSoundEventEmittersToActivate(newAreaPart);
		IEnumerator banksLoading = LoadNewBanks();
		while (banksLoading.MoveNext())
		{
			yield return null;
		}
		EmitActivate();
		SoundBanksManager.UnloadNotUsed();
	}

	public void CollectSoundEventEmittersToDeactivate(BlueprintAreaPart newAreaPart)
	{
		m_DeactivateList = ListPool<SoundEventsEmitter.Info>.Claim();
		foreach (SoundEventsEmitter.Info item in ObjectRegistry<SoundEventsEmitter.Info>.Instance)
		{
			if (item.AreaPart != newAreaPart && item.Scene.name != newAreaPart.StaticScene.SceneName)
			{
				m_DeactivateList.Add(item);
			}
		}
	}

	public void CollectSoundEventEmittersToActivate(BlueprintAreaPart newAreaPart)
	{
		m_ActivateList = ListPool<SoundEventsEmitter.Info>.Claim();
		foreach (SoundEventsEmitter.Info item in ObjectRegistry<SoundEventsEmitter.Info>.Instance)
		{
			if (item.AreaPart == newAreaPart || item.Scene.name == newAreaPart.StaticScene.SceneName)
			{
				m_ActivateList.Add(item);
			}
		}
	}

	public void EmitDeactivate()
	{
		foreach (SoundEventsEmitter.Info deactivate in m_DeactivateList)
		{
			deactivate.EmitDeactivated();
		}
	}

	public IEnumerator LoadNewBanks()
	{
		bool flag = true;
		List<SoundBanksManager.BankHandle> bankHandles = ListPool<SoundBanksManager.BankHandle>.Claim();
		foreach (SoundEventsEmitter.Info activate in m_ActivateList)
		{
			SoundBanksManager.BankHandle bankHandle = activate.RequestBank();
			if (bankHandle != null && !bankHandle.Loaded)
			{
				flag = false;
				bankHandles.Add(bankHandle);
			}
		}
		while (!flag)
		{
			yield return null;
			flag = true;
			foreach (SoundBanksManager.BankHandle item in bankHandles)
			{
				if (!item.Loaded && item.Loading)
				{
					flag = false;
					break;
				}
			}
		}
		ListPool<SoundBanksManager.BankHandle>.Release(bankHandles);
	}

	public void EmitActivate()
	{
		foreach (SoundEventsEmitter.Info activate in m_ActivateList)
		{
			activate.EmitActivated();
		}
		foreach (SoundEventsEmitter.Info deactivate in m_DeactivateList)
		{
			deactivate.UnloadBank();
		}
		ListPool<SoundEventsEmitter.Info>.Release(m_ActivateList);
		ListPool<SoundEventsEmitter.Info>.Release(m_DeactivateList);
	}
}

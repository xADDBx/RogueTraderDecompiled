using System.Collections.Generic;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.DollRoom;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.Code.UI.DollRoom;

public class CharGenDollRoom : CharacterDollRoom, ILevelUpDollHandler, ISubscriber
{
	[SerializeField]
	private AnimationClipWrapper m_RightHandedAnimationWrapper;

	private Character m_MaleDefaultAvatar;

	private Character m_FemaleDefaultAvatar;

	private bool m_LeftHanded;

	private DollState m_DollState;

	private bool m_IsDirty;

	private Coroutine m_AddEquipmentEntitiesCoroutine;

	public override void Show()
	{
		base.Show();
		SetupDollPostProcessAndAnimation(isCharGen: true);
	}

	protected override void Cleanup()
	{
		base.Cleanup();
		m_DollState = null;
		if (m_AddEquipmentEntitiesCoroutine != null)
		{
			MonoSingleton<CoroutineRunner>.Instance.StopCoroutine(m_AddEquipmentEntitiesCoroutine);
			m_AddEquipmentEntitiesCoroutine = null;
		}
	}

	private void LateUpdate()
	{
		if (m_IsDirty)
		{
			UpdateDoll(m_DollState);
			m_IsDirty = false;
		}
		Services.GetInstance<CharacterAtlasService>().Update();
	}

	protected override void UpdateInternal()
	{
		if (!(m_Avatar == null) && !(m_Avatar.AnimationManager == null))
		{
			m_Avatar.AnimationManager.CustomUpdate(RealTimeController.SystemStepDurationSeconds);
		}
	}

	public void HandleDollStateUpdated(DollState dollState)
	{
		m_DollState = dollState;
		m_IsDirty = true;
	}

	public void BindDollState(DollState dollState)
	{
		if (m_DollState != dollState)
		{
			Cleanup();
			m_DollState = dollState;
			m_IsDirty = true;
		}
	}

	private void UpdateDoll(DollState dollState)
	{
		int num = ((dollState.Gender != 0) ? 1 : 0);
		BlueprintCharGenRoot instance = BlueprintCharGenRoot.Instance;
		if (num == 0)
		{
			Object.Destroy(m_MaleDefaultAvatar.Or(null)?.gameObject);
			m_MaleDefaultAvatar = CreateAvatar(instance.MaleDoll, "CharGen Male");
			SetupAnimationManager(m_MaleDefaultAvatar.AnimationManager);
			m_MaleDefaultAvatar.AnimationManager.IsInCombat = false;
		}
		else
		{
			Object.Destroy(m_FemaleDefaultAvatar.Or(null)?.gameObject);
			m_FemaleDefaultAvatar = CreateAvatar(instance.FemaleDoll, "CharGen Female");
			SetupAnimationManager(m_FemaleDefaultAvatar.AnimationManager);
			m_FemaleDefaultAvatar.AnimationManager.IsInCombat = false;
		}
		Character avatar = ((dollState.Gender == Gender.Male) ? m_MaleDefaultAvatar : m_FemaleDefaultAvatar);
		SetAvatar(avatar, activateAvatar: false);
		Skeleton skeleton = dollState.GetSkeleton();
		if (skeleton != null && avatar.Skeleton != skeleton)
		{
			avatar.Skeleton = skeleton;
		}
		avatar.RemoveAllEquipmentEntities();
		List<EquipmentEntityLink> list = dollState.CollectEntities();
		EquipmentEntityLink[] collection = ((dollState.Gender == Gender.Female) ? instance.FemaleClothes : instance.MaleClothes);
		list.AddRange(collection);
		avatar.AddEquipmentEntities(list);
		OnEEsAdded();
		void OnEEsAdded()
		{
			dollState.ApplyRamps(avatar);
			avatar.UpdateHelmetVisibility(dollState.ShowHelm);
			avatar.UpdateBackpackVisibility(dollState.ShowBackpack);
			avatar.UpdateClothVisibility(dollState.ShowCloth);
			AnimationClipWrapper rightHandedAnimationWrapper = m_RightHandedAnimationWrapper;
			if (rightHandedAnimationWrapper != null)
			{
				UnitAnimationActionClip unitAnimationActionClip = UnitAnimationActionClip.Create(rightHandedAnimationWrapper, "UpdateDoll");
				unitAnimationActionClip.TransitionIn = 0f;
				unitAnimationActionClip.TransitionOut = 0f;
				AnimationActionHandle handle = avatar.AnimationManager.CreateHandle(unitAnimationActionClip);
				avatar.AnimationManager.Execute(handle);
			}
			avatar.gameObject.SetActive(value: true);
		}
	}
}

using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Capital;

[ExecuteInEditMode]
public class CapitalCompanionSpawnPlace : MonoBehaviour, IDialogReference
{
	public static readonly List<CapitalCompanionSpawnPlace> Instances = new List<CapitalCompanionSpawnPlace>();

	[SerializeField]
	[FormerlySerializedAs("CompanionBlueprint")]
	[InfoBox(Text = "Не используйте CapitalCompanionSpawnPlace, вместо них лучше ставить CompanionSpawner-ы.")]
	private BlueprintUnitReference m_CompanionBlueprint;

	[SerializeField]
	[FormerlySerializedAs("OverrideDialog")]
	private BlueprintDialogReference m_OverrideDialog;

	public SharedStringAsset OverrideBark;

	public BlueprintUnit CompanionBlueprint => m_CompanionBlueprint?.Get();

	public BlueprintDialog OverrideDialog => m_OverrideDialog?.Get();

	private void OnEnable()
	{
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}

	public DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		if (dialog != OverrideDialog)
		{
			return DialogReferenceType.None;
		}
		return DialogReferenceType.Start;
	}
}

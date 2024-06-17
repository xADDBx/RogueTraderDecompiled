using UnityEngine;

namespace Kingmaker.Blueprints.JsonSystem.EditorDatabase;

public class BlueprintComponentEditorWrapper : ScriptableWrapperBase<BlueprintComponent>
{
	[SerializeReference]
	public BlueprintComponent Component;

	[SerializeField]
	[HideInInspector]
	private string m_AssetId;

	[SerializeField]
	[HideInInspector]
	private string m_ComponentName;

	public override BlueprintComponent WrappedInstance
	{
		get
		{
			return Component;
		}
		set
		{
			Component = value;
		}
	}

	public static BlueprintComponentEditorWrapper Wrap(BlueprintComponent bp)
	{
		BlueprintComponentEditorWrapper blueprintComponentEditorWrapper = ScriptableObject.CreateInstance<BlueprintComponentEditorWrapper>();
		blueprintComponentEditorWrapper.Component = bp;
		blueprintComponentEditorWrapper.m_AssetId = bp?.OwnerBlueprint?.AssetGuid ?? "";
		blueprintComponentEditorWrapper.m_ComponentName = bp?.name ?? "";
		blueprintComponentEditorWrapper.hideFlags = HideFlags.DontSave;
		blueprintComponentEditorWrapper.name = "[CW]" + bp.name;
		blueprintComponentEditorWrapper.SyncPropertiesWithProto();
		return blueprintComponentEditorWrapper;
	}

	public override BlueprintComponent GetCanonicalInstance()
	{
		return null;
	}

	protected override ScriptableWrapperBase<BlueprintComponent> GetWrapper(BlueprintComponent t)
	{
		return Wrap(t);
	}

	public override string GetOwnerBlueprintId()
	{
		return m_AssetId;
	}

	protected override string GetRootProperty()
	{
		return "Component";
	}
}

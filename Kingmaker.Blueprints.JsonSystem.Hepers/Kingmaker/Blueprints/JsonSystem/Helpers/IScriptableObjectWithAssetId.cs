namespace Kingmaker.Blueprints.JsonSystem.Helpers;

public interface IScriptableObjectWithAssetId
{
	string name { get; set; }

	string AssetGuid { get; set; }
}

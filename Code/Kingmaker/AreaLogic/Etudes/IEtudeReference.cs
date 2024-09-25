namespace Kingmaker.AreaLogic.Etudes;

public interface IEtudeReference
{
	EtudeReferenceType GetUsagesFor(BlueprintEtude etude);
}

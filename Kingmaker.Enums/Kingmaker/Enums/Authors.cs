using System.ComponentModel;
using UnityEngine;

namespace Kingmaker.Enums;

public enum Authors
{
	[Description("al.gusev")]
	[DescriptionEmail("<al.gusev@owlcat.games>")]
	Unknown = -1,
	[InspectorName("LD/EugeneIvanov")]
	[Description("ivanov")]
	[DescriptionEmail("<ivanov@owlcat.games>")]
	EugeneIvanov = 1,
	[InspectorName("LD/AndreySaenko")]
	[Description("saenko")]
	[DescriptionEmail("<saenko@owlcat.games>")]
	AndreySaenko = 2,
	[InspectorName("LD/VadimBgatzev (Inactive)")]
	[Description("bgatzev")]
	[DescriptionEmail("<bgatzev@owlcat.games>")]
	VadimBgatzev = 3,
	[InspectorName("LD/AlexeyPolezhaev")]
	[Description("polezhaev")]
	[DescriptionEmail("<polezhaev@owlcat.games>")]
	AlexeyPolezhaev = 5,
	[InspectorName("LD/VyacheslavZolotovsky")]
	[Description("zolotovsky")]
	[DescriptionEmail("<zolotovsky@owlcat.games>")]
	VyacheslavZolotovsky = 6,
	[InspectorName("LD/VladimirZuev")]
	[Description("zuev")]
	[DescriptionEmail("<zuev@owlcat.games>")]
	VladimirZuev = 7,
	[InspectorName("LD/AlexeySilaev")]
	[Description("silaev")]
	[DescriptionEmail("<silaev@owlcat.games>")]
	AlexeySilaev = 8,
	[InspectorName("LD/KseniyaUrchenko")]
	[Description("urchenko")]
	[DescriptionEmail("<urchenko@owlcat.games>")]
	KseniyaUrchenko = 9,
	[InspectorName("LD/ArtemSchellenberg")]
	[Description("schellenberg")]
	[DescriptionEmail("<schellenberg@owlcat.games>")]
	ArtemSchellenberg = 10,
	[InspectorName("LD/AlisaSinelnikova (Inactive)")]
	[Description("sinelnikova")]
	[DescriptionEmail("<sinelnikova@owlcat.games>")]
	AlisaSinelnikova = 11,
	[InspectorName("LD/EvgeniyIvanovGD")]
	[Description("e.ivanov")]
	[DescriptionEmail("<e.ivanov@owlcat.games>")]
	EvgeniyIvanovGD = 12,
	[InspectorName("LD/AntonKharybin")]
	[Description("kharybin")]
	[DescriptionEmail("<kharybin@owlcat.games>")]
	AntonKharybin = 13,
	[InspectorName("GD/AlexanderGusev")]
	[Description("a.gusev")]
	[DescriptionEmail("<a.gusev@owlcat.games>")]
	AlexanderGusev = 100,
	[InspectorName("GD/LeonidTolochenko")]
	[Description("tolochenko")]
	[DescriptionEmail("<tolochenko@owlcat.games>")]
	LeonidTolochenko = 101,
	[InspectorName("GD/ElenaMironova")]
	[Description("mironova.e")]
	[DescriptionEmail("<mironova@owlcat.games>")]
	ElenaMironova = 102,
	[InspectorName("GD/NikitaFilatov")]
	[Description("n.filatov")]
	[DescriptionEmail("<n.filatov@owlcat.games>")]
	NikitaFilatov = 103,
	[InspectorName("GD/IlyaPolitko")]
	[Description("politko")]
	[DescriptionEmail("<politko@owlcat.games>")]
	IlyaPolitko = 104,
	[InspectorName("GD/GeorgiiDoronin")]
	[Description("doronin")]
	[DescriptionEmail("<doronin@owlcat.games>")]
	GeorgiiDoronin = 105,
	[InspectorName("GD/AlexanderKompanets")]
	[Description("kompanets")]
	[DescriptionEmail("<kompanets@owlcat.games>")]
	AlexanderKompanets = 106,
	[InspectorName("GD/EvgeniyShanhiev")]
	[Description("shanhiev")]
	[DescriptionEmail("<shanhiev@owlcat.games>")]
	EvgeniyShanhiev = 107,
	[InspectorName("GD/MihailKrivohizhin")]
	[Description("krivohizhin")]
	[DescriptionEmail("<krivohizhin@owlcat.games>")]
	MihailKrivohizhin = 108,
	[InspectorName("GD/AmirDzhalilov")]
	[Description("dzhalilov")]
	[DescriptionEmail("<dzhalilov@owlcat.games>")]
	AmirDzhalilov = 109,
	[InspectorName("GD/YuriyElburih")]
	[Description("elburih")]
	[DescriptionEmail("<elburih@owlcat.games>")]
	YuriyElburih = 110,
	[InspectorName("GD/VasiliiArkhiplovLinev")]
	[Description("arhipovlinev")]
	[DescriptionEmail("<arhipovlinev@owlcat.games>")]
	VasiliiArkhiplovLinev = 111,
	[InspectorName("SD/KonstantinKuzenkov")]
	[Description("kuzenkov")]
	[DescriptionEmail("<kuzenkov@owlcat.games>")]
	KonstantinKuzenkov = 200,
	[InspectorName("SD/EfanovIlya")]
	[Description("efanov")]
	[DescriptionEmail("<efanov@owlcat.games>")]
	EfanovIlya = 201,
	[InspectorName("SD/ArtemBoksha")]
	[Description("boksha")]
	[DescriptionEmail("<boksha@owlcat.games>")]
	ArtemBoksha = 202,
	[InspectorName("SD/DenisFilippov")]
	[Description("filippov")]
	[DescriptionEmail("<filippov@owlcat.games>")]
	DenisFilippov = 203,
	[InspectorName("SD/PavelPerepelitsa")]
	[Description("perepelitsa")]
	[DescriptionEmail("<perepelitsa@owlcat.games>")]
	PavelPerepelitsa = 204,
	[InspectorName("ND/AntonFadeev")]
	[Description("fadeev")]
	[DescriptionEmail("<fadeev@owlcat.games>")]
	AntonFadeev = 300,
	[InspectorName("ND/AlekseyVorobiev")]
	[Description("a.vorobiev")]
	[DescriptionEmail("<vorobiev@owlcat.games>")]
	AlekseyVorobiev = 301,
	[InspectorName("ND/OlgaKellner")]
	[Description("kellner")]
	[DescriptionEmail("<kellner@owlcat.games>")]
	OlgaKellner = 302,
	[InspectorName("ND/VeronikaLarionova")]
	[Description("larionova")]
	[DescriptionEmail("<larionova@owlcat.games>")]
	VeronikaLarionova = 303,
	[InspectorName("ND/AlesandraHrolovich")]
	[Description("hrolovich")]
	[DescriptionEmail("<hrolovich@owlcat.games>")]
	AlesandraHrolovich = 304,
	[InspectorName("ND/AnastasiyaEgorova")]
	[Description("a.egorova")]
	[DescriptionEmail("<a.egorova@owlcat.games>")]
	AnastasiyaEgorova = 305,
	[InspectorName("ND/DinaraKoroleva")]
	[Description("d.koroleva")]
	[DescriptionEmail("<d.koroleva@owlcat.games>")]
	DinaraKoroleva = 306,
	[InspectorName("ND/MargaritaBeleckaya")]
	[Description("beleckaya")]
	[DescriptionEmail("<beleckaya@owlcat.games>")]
	MargaritaBeleckaya = 307
}

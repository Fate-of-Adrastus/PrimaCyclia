using Quintessential;

namespace PrimaCyclia;

internal static class Atoms{
	
	public static AtomType Sulfur, Oxygen, Phlogiston;
	public static readonly AtomType salt = class_175.field_1675;

	public static void AddAtomTypes(){
		// Sulfur atom type
		Sulfur = new(){
			field_2283 = 111, /*ID*/
			field_2284 = class_134.method_254("Sulfur"), /*Non-local Name*/
			field_2285 = class_134.method_253("Elemental Sulfur", string.Empty), /*Atomic Name*/
			field_2286 = class_134.method_253("Sulfur", string.Empty), /*Local name*/
			field_2287 = class_235.method_615("textures/prima_cyclia/atoms/sulfur_symbol"), /*Symbol*/
			field_2288 = class_238.field_1989.field_81.field_599, /*Shadow*/
			field_2290 = new class_106
			{
				field_994 = class_238.field_1989.field_81.field_596,
				field_995 = class_238.field_1989.field_81.field_597
			}
		};
		Sulfur.QuintAtomType = "PrimaCyclia:sulfur";

		QApi.AddAtomType(Sulfur);

		// Oxygen atom type
		Oxygen = new(){
			field_2283 = 112, /*ID*/
			field_2284 = class_134.method_254("Oxygen"), /*Non-local Name*/
			field_2285 = class_134.method_253("Elemental Oxygen", string.Empty), /*Atomic Name*/
			field_2286 = class_134.method_253("Oxygen", string.Empty), /*Local name*/
			field_2287 = class_235.method_615("textures/prima_cyclia/atoms/oxygen_symbol"), /*Symbol*/
			field_2288 = class_235.method_615("textures/prima_cyclia/atoms/oxygen_shadow"), /*Shadow*/
			field_2290 = new class_106
			{
				field_994 = class_235.method_615("textures/prima_cyclia/atoms/oxygen_diffuse"),
				field_995 = class_238.field_1989.field_81.field_613.field_633
			}
		};
		Oxygen.QuintAtomType = "PrimaCyclia:oxygen";

		QApi.AddAtomType(Oxygen);

		// Phlogiston atom type
		Phlogiston = new(){
			field_2283 = 113, /*ID*/
			field_2284 = class_134.method_254("Phlogiston"), /*Non-local Name*/
			field_2285 = class_134.method_253("Elemental Phlogiston", string.Empty), /*Atomic Name*/
			field_2286 = class_134.method_253("Phlogiston", string.Empty), /*Local name*/
			field_2287 = class_235.method_615("textures/prima_cyclia/atoms/phlogiston_symbol"), /*Symbol*/
			field_2288 = class_235.method_615("textures/prima_cyclia/atoms/phlogiston_shadow"), /*Shadow*/
			field_2290 = new class_106
			{
				field_994 = class_235.method_615("textures/prima_cyclia/atoms/phlogiston_diffuse"),
				field_995 = class_238.field_1989.field_81.field_613.field_633
			}
		};
		Phlogiston.QuintAtomType = "PrimaCyclia:phlogiston";

		QApi.AddAtomType(Phlogiston);

	}

	public static void Unload(){}
}
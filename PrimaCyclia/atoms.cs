using Quintessential;

namespace PrimaCyclia;

internal static class Atoms {
	
	public static AtomType Sulfur, Oxygen, Phlogiston;

	public static void AddAtomTypes() {
        // Sulfur atom type
        Sulfur = new() {
            defaultName = Translations.WithAllLanguages("Sulfur"), /*Non-local Name*/
            elementalName = Translations.Translate("Elemental Sulfur"), /*Atomic Name*/
            name = Translations.Translate("Sulfur"), /*Local name*/
            symbol = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/atoms/sulfur_symbol"), /*Symbol*/
            shadow = Assets.textures.atoms.shadow, /*Shadow*/
            matteTextures = new MatteAtomTextures {
                diffuse = Assets.textures.atoms.salt_diffuse,
                shade = Assets.textures.atoms.salt_shade
            },
        };
        PrimaCyclia.Instance.AddAtomType(Sulfur, "sulfur");

        // Oxygen atom type
        Oxygen = new() {
            defaultName = Translations.WithAllLanguages("Oxygen"), /*Non-local Name*/
            elementalName = Translations.Translate("Elemental Oxygen"), /*Atomic Name*/
            name = Translations.Translate("Oxygen"), /*Local name*/
            symbol = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/atoms/oxygen_symbol"), /*Symbol*/
            shadow = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/atoms/oxygen_shadow"), /*Shadow*/
            matteTextures = new MatteAtomTextures {
                diffuse = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/atoms/oxygen_diffuse"),
                shade = Assets.textures.atoms.elements.specular_highlight
            },
        };
        PrimaCyclia.Instance.AddAtomType(Oxygen, "oxygen");

        // Phlogiston atom type
        Phlogiston = new() {
            defaultName = Translations.WithAllLanguages("Phlogiston"), /*Non-local Name*/
            elementalName = Translations.Translate("Elemental Phlogiston"), /*Atomic Name*/
            name = Translations.Translate("Phlogiston"), /*Local name*/
            symbol = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/atoms/phlogiston_symbol"), /*Symbol*/
            shadow = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/atoms/phlogiston_shadow"), /*Shadow*/
            matteTextures = new MatteAtomTextures {
                diffuse = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/atoms/phlogiston_diffuse"),
                shade = Assets.textures.atoms.elements.specular_highlight
            },
        };
        PrimaCyclia.Instance.AddAtomType(Phlogiston, "phlogiston");

	}
}
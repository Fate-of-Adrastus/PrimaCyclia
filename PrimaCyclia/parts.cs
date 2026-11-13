using MonoMod.Utils;
using Quintessential;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrimaCyclia;

public static class Parts {
    public static PartType Polarity, Neutrality, Cyclicity, Primality, Empowerment, Combustion, Sublimation, Tolomeo;
    const float sixtyDegrees = 60f * (float)Math.PI / 180f;
    public static readonly HexIndex center = new(0, 0);
    public static readonly HexIndex hex_right = new(1,0);
    public static readonly HexIndex hex_left = new(-1,0);
    public static readonly HexIndex hex_up_right = new(0,1);
    public static readonly HexIndex hex_down_right = new(1,-1);
    public static readonly HexIndex hex_up_left = new(-1,1);
    public static readonly HexIndex hex_down_left = new(0,-1);

    public static readonly HexIndex[] sublimationIrises = {new HexIndex(-1, 2), new HexIndex(1, 1), new HexIndex(2, -1), new HexIndex(1, -2), new HexIndex(-1, -1), new HexIndex(-2, 1)};

    public static Dictionary<AtomType, int> sublimationDict = new();

    static HexRotation[] HexArmRotations => PartTypes.hexArm.armRotations;
    const string TolomeoWheelAtomsField = "PrimaCyclia_TolomeoMolecule";
    public static Vector2 hexGraphicalOffset(HexIndex hex) => HexGrid.standardGrid.ToPixelCoords(hex);
    static LightDirTextures atomCageLighting => Assets.textures.parts.atom_cage_lighting;

    // custom glyph textures
    public static Texture prime_symbols = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/prime_symbols");

    public static Texture oxygen_input = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/oxygen_input");
    public static Texture phlog_input = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/phlogiston_input");
    public static Texture sulfur_input = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/sulfur_input");
    public static Texture primes_input = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/primes_input");
    public static Texture neutrality_input = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/qs_sulfur_input");
    public static Texture volatiles_input = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/volatiles_input");

    public static Texture neutrality_base = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/neutrality/base");
    public static Texture cyclicity_base = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/cyclicity/base");
    public static Texture sublimation_base = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/sublimation/base");
    public static Texture empowerment_base = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/empowerment/base");
    public static Texture empowerment_mesh = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/empowerment/mesh");

    public static Texture[][] sublimation_iris_anim = [
        PrimaCyclia.GetAnimation("textures/prima_cyclia/parts/sublimation/iris_1_full.array","iris",16),
        PrimaCyclia.GetAnimation("textures/prima_cyclia/parts/sublimation/iris_2_full.array","iris",16),
        PrimaCyclia.GetAnimation("textures/prima_cyclia/parts/sublimation/iris_3_full.array","iris",16),
        PrimaCyclia.GetAnimation("textures/prima_cyclia/parts/sublimation/iris_4_full.array","iris",16),
        PrimaCyclia.GetAnimation("textures/prima_cyclia/parts/sublimation/iris_5_full.array","iris",16),
        PrimaCyclia.GetAnimation("textures/prima_cyclia/parts/sublimation/iris_6_full.array","iris",16),
    ];

    public static Texture[] oxygen_iris = PrimaCyclia.GetAnimation("textures/prima_cyclia/parts/iris_oxygen_full.array", "iris",16);
    public static Texture[] phlog_iris = PrimaCyclia.GetAnimation("textures/prima_cyclia/parts/iris_phlogiston_full.array", "iris",16);

    public static Texture empowerment_overlay = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/empowerment/overlay");
    public static Texture[] empowerment_lights = [
        AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/empowerment/b2_light"),
        AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/empowerment/b3_light"),
        AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/empowerment/b1_light")
    ];

    public static List<AtomType> primeAtoms = [
        AtomTypes.quicksilver,
        AtomTypes.mors,
        Atoms.Sulfur,
        AtomTypes.vitae
    ];
    public static List<AtomType> volatileAtoms = [
        Atoms.Sulfur,
        Atoms.Oxygen,
        Atoms.Phlogiston
    ];

    enum IrisMode {open, closed, opening, closing}

    private class SublimeState {
        public IrisMode mode = IrisMode.open;
        public int cycle_modified = 0;

        public SublimeState(IrisMode m, int cycle) {
            mode = m;
            cycle_modified = cycle;
        }
    }

    private class EmpowerState {
        public bool[] canBond = {false,false,false};
        public bool containsCharge = false;
        public EmpowerState(bool[] bondables) { canBond = bondables; }
        public bool IsEmpty(){return Enumerable.SequenceEqual(canBond, new bool[]{false,false,false});}
    }

    static Molecule TolomeoMolecule() {
        Molecule molecule = new();
        molecule.AddAtom(new Atom(AtomTypes.vitae), new HexIndex(0, 1));
        molecule.AddAtom(new Atom(AtomTypes.salt), new HexIndex(1, 0));
        molecule.AddAtom(new Atom(AtomTypes.quicksilver), new HexIndex(1, -1));
        molecule.AddAtom(new Atom(AtomTypes.mors), new HexIndex(0, -1));
        molecule.AddAtom(new Atom(AtomTypes.salt), new HexIndex(-1, 0));
        molecule.AddAtom(new Atom(Atoms.Sulfur), new HexIndex(-1, 1));
        return molecule;
    }

    public static Sound CombustionOxygenRelease, CombustionPhlogRelease, CombustionBothRelease, CyclicitySound, EmpowermentConsume, EmpowermentProduce, NeutralitySound, PolaritySound, PrimalitySound, SublimationSound;
    public static Sound[] EmpowermentBonds, SublimationProduction;
    public static void LoadSounds() {
        CombustionOxygenRelease = AssetLoaderHelper.LoadSound("sounds/prima_cyclia/combustion_oxyg");
        CombustionPhlogRelease = AssetLoaderHelper.LoadSound("sounds/prima_cyclia/combustion_phlog");
        CombustionBothRelease = AssetLoaderHelper.LoadSound("sounds/prima_cyclia/combustion_both");
        CyclicitySound = AssetLoaderHelper.LoadSound("sounds/prima_cyclia/cyclicity_sound");
        EmpowermentConsume = AssetLoaderHelper.LoadSound("sounds/prima_cyclia/empowerment_oxyg_in");
        EmpowermentProduce = AssetLoaderHelper.LoadSound("sounds/prima_cyclia/empowerment_phlog_out");
        NeutralitySound = AssetLoaderHelper.LoadSound("sounds/prima_cyclia/neutrality_sound");
        PolaritySound = AssetLoaderHelper.LoadSound("sounds/prima_cyclia/polarity_sound");
        PrimalitySound = AssetLoaderHelper.LoadSound("sounds/prima_cyclia/primality_sound");
        SublimationSound = AssetLoaderHelper.LoadSound("sounds/prima_cyclia/sublimation_sound");

        EmpowermentBonds = [
            AssetLoaderHelper.LoadSound("sounds/prima_cyclia/empowerment_supertriplex_a"),
            AssetLoaderHelper.LoadSound("sounds/prima_cyclia/empowerment_supertriplex_b"),
            AssetLoaderHelper.LoadSound("sounds/prima_cyclia/empowerment_supertriplex_c")
        ];

        SublimationProduction = [
            AssetLoaderHelper.LoadSound("sounds/prima_cyclia/sublimation_out_1_or_2"),
            AssetLoaderHelper.LoadSound("sounds/prima_cyclia/sublimation_out_3_or_4"),
            AssetLoaderHelper.LoadSound("sounds/prima_cyclia/sublimation_out_5_or_6")
        ];

        Volume.SetVolume(CombustionOxygenRelease, 0.3f);
        Volume.SetVolume(CombustionPhlogRelease, 0.3f);
        Volume.SetVolume(CombustionBothRelease, 0.3f);
        Volume.SetVolume(CyclicitySound, 0.3f);
        Volume.SetVolume(EmpowermentConsume, 0.3f);
        Volume.SetVolume(EmpowermentProduce, 0.3f);
        Volume.SetVolume(NeutralitySound, 0.3f);
        Volume.SetVolume(PolaritySound, 0.3f);
        Volume.SetVolume(PrimalitySound, 0.3f);
        Volume.SetVolume(SublimationSound, 0.3f);

        Volume.SetVolume(EmpowermentBonds[0], 0.3f);
        Volume.SetVolume(EmpowermentBonds[1], 0.3f);
        Volume.SetVolume(EmpowermentBonds[2], 0.3f);

        Volume.SetVolume(SublimationProduction[0], 0.3f);
        Volume.SetVolume(SublimationProduction[1], 0.3f);
        Volume.SetVolume(SublimationProduction[2], 0.3f);
    }

    public static void drawSelectionGlow(SolutionEditorBase seb_self, Part part, Vector2 pos, float alpha)
    {
        var cageSelectGlowTexture = Assets.textures.select.cage;
        int armLength = 1; // part.method_1165()
        IntermediatePartState class236 = seb_self.GetIntermState(part, pos);
        Color color = Color.White.WithAlpha(alpha);

        PrimaCyclia.PrivateMethod<SolutionEditorBase>("RenderArmSelection").Invoke(seb_self, [armLength, HexArmRotations, class236, color]);
        for (int index = 0; index < 6; ++index)
        {
            float num = index * sixtyDegrees;
            PrimaCyclia.PrivateMethod<SolutionEditorBase>("RenderRotated").Invoke(seb_self, [cageSelectGlowTexture, color, class236.pos, class236.rotation + num]);
        }
    }

    private static AtomType GetPrime(int i) {
        while (i < 0) {i += 4;}
        while (i > 3) {i -= 4;}
        return primeAtoms[i];
    }

    private static Molecule GetTolomeoWheelAtoms(PartSimState state) => GetTolomeoWheelData(state, TolomeoWheelAtomsField, TolomeoMolecule());

    public static Maybe<AtomReference> MaybeFindTolomeoWheelAtom(Sim sim_self, Part part, HexIndex offset) => MaybeFindTolomeoWheelAtom(sim_self, part.GlobalPosFromLocal(offset));

    public static Maybe<AtomReference> MaybeFindTolomeoWheelAtom(Sim sim_self, HexIndex hex) {
        var SEB = sim_self.solutionEditor;
        var solution = SEB.GetSolution();
        var partList = solution.parts;
        var partSimStates = sim_self.simulationDict;

        foreach (var Tolomeo in partList.Where(x => x.GetType() == Tolomeo))
        {
            var partSimState = partSimStates[Tolomeo];
            Molecule TolomeoAtoms = GetTolomeoWheelAtoms(partSimState);
            var hexIndex = partSimState.newPosition;
            var rotation = partSimState.rotation;
            var hexKey = (hex - hexIndex).Rotated(rotation.Negative());

            if (TolomeoAtoms.GetAtoms().TryGetValue(hexKey, out Atom atom))
            {
                return new AtomReference(TolomeoAtoms, hexKey, atom.atomType, atom, true);
            }
        }
        return MaybeHelper.empty;
    }

    // private methods
    private static void SetTolomeoWheelData<T>(PartSimState state, string field, T data) => new DynamicData(state).Set(field, data);
    private static T GetTolomeoWheelData<T>(PartSimState state, string field, T initial) {
        var data = new DynamicData(state).Get(field);
        if (data == null)
        {
            SetTolomeoWheelData(state, field, initial);
            return initial;
        }
        else
        {
            return (T)data;
        }
    }

    static void DrawTolomeoPart(Part part, Vector2 pos, SolutionEditorBase editor, PartRenderer renderer)
    {
        // draw atoms, if the simulation is stopped - otherwise, the running simulation will draw them
        if (editor.GetSimPlayState() == SimPlayState.Stopped)
        {
            drawTolomeoAtoms(editor, part, pos);
        }

        // draw arm stubs
        IntermediatePartState class236 = editor.GetIntermState(part, pos);
        PrimaCyclia.PrivateMethod<SolutionEditorBase>("RenderArmLights").Invoke(editor, [part.GetArmLength(), HexArmRotations, class236]);

        // draw cages
        PartSimState partSimState = editor.GetSimulation().GetSimState(part);
        for (int i = 0; i < 6; i++)
        {
            float radians = renderer.partRotation + (i * sixtyDegrees);
            Vector2 vector2_9 = renderer.partPos + hexGraphicalOffset(new HexIndex(1, 0)).Rotated(radians);
            PrimaCyclia.PrivateMethod<SolutionEditorBase>("RenderLights").Invoke(editor, [atomCageLighting, vector2_9, new Vector2(39f, 33f), radians]);
        }
    }
    public static void drawTolomeoAtoms(SolutionEditorBase seb_self, Part part, Vector2 pos)
    {
        if (part.GetType() != Tolomeo) return;
        PartSimState partSimState = seb_self.GetSimulation().GetSimState(part);

        IntermediatePartState class236 = seb_self.GetIntermState(part, pos);
        Editor.RenderMolecule(GetTolomeoWheelAtoms(partSimState), class236.pos, center, class236.rotation, 1f, 1f, 1f, false, seb_self);
    }

    public static void addSublimationAtom(string atom, int weight)
    {
        foreach (AtomType A in QApi.ModAtomTypes) { if (A.Id == atom) { addSublimationAtom(A, weight); } }
    }
    public static void addSublimationAtom(AtomType atom, int weight)
    {
        sublimationDict[atom] = weight;
    }

    public static bool isEmpowered(Part empowerment, Part prisma)
    {
        HashSet<HexIndex> checked_hexes = [empowerment.GlobalPosFromLocal(center), empowerment.GlobalPosFromLocal(hex_down_left), empowerment.GlobalPosFromLocal(hex_down_right)];
        HashSet<HexIndex> my_triplex = [prisma.GlobalPosFromLocal(center), prisma.GlobalPosFromLocal(hex_right), prisma.GlobalPosFromLocal(hex_up_right)];
        return checked_hexes.SetEquals(my_triplex);
    }

    static bool ContentLoaded = false;
    public static void AddPartTypes() {        

        if (ContentLoaded) throw new Exception( "Prima Cyclia tried to load content multiple times, this shouldn't happen." );
        ContentLoaded = true;
        //=========================//

        Tolomeo = new PartType() {
            //Id = "PrimaCyclia-tolomeo",
            name = Translations.Translate("Tolomeo's Wheel"),
            description = Translations.Translate("By using Tolomeo's wheel with the glyph of primality, neutral salt can be turned into any prime within the cycle of primes."),
            cost = 30,
            layer = PartTypeLayer.Top,
            isProgrammable = true,
            canRotateInSimulation = true,
            attachedAtoms = [],
            iconTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/tolomeo"),
            iconHoverTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/tolomeo_hover"),
            isUnique = true,
            CustomPermissionCheck = perms => perms.Contains(PrimaCyclia.Instance.GetIdentifier("tolomeos_wheel"))
        };
        Tolomeo.attachedAtoms.Add(new HexIndex(0, 1), AtomTypes.vitae);
        Tolomeo.attachedAtoms.Add(new HexIndex(1, 0), AtomTypes.salt);
        Tolomeo.attachedAtoms.Add(new HexIndex(1, -1), AtomTypes.quicksilver);
        Tolomeo.attachedAtoms.Add(new HexIndex(0, -1), AtomTypes.mors);
        Tolomeo.attachedAtoms.Add(new HexIndex(-1, 0), AtomTypes.salt);
        Tolomeo.attachedAtoms.Add(new HexIndex(-1, 1), Atoms.Sulfur);

        QApi.AddPartTypeToPanel(Tolomeo, PartTypes.berlosWheel);
        PrimaCyclia.Instance.AddPartType(Tolomeo, "tolomeos_wheel", DrawTolomeoPart);
        
        //QApi.AddPuzzlePermission("PrimaCyclia:TolomeoWheel", "Tolomeo's Wheel", "Prima Cyclia");
        PrimaCyclia.Instance.AddPuzzlePermission("tolomeos_wheel");

        Primality = new() {
            //Id = "PrimaCyclia-primality", // ID
            name = Translations.Translate("Glyph of Primality"), // Name
            description = Translations.Translate("The glyph of primality transforms an atom of salt into any atom within the cycle of primes."), // Description
            cost = 20, // Cost
            isFullHexCover = true, // Is a glyph
            glowTexture = Assets.textures.select.double_glow, // Shadow/glow
            strokeTexture = Assets.textures.select.double_stroke, // Stroke/outline
            iconTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/primality"), // Panel icon
            iconHoverTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/primality_hover"), // Hovered panel icon
            glyphHexes = [center, hex_right],
            permissionCategory = PuzzlePermissions.None,
            CustomPermissionCheck = perms => perms.Contains(PrimaCyclia.Instance.GetIdentifier("primality"))
        };

        QApi.AddPartTypeToPanel(Primality, false);
        //QApi.AddPuzzlePermission("PrimaCyclia:Primality", "Glyph of Primality", "Prima Cyclia");
        PrimaCyclia.Instance.AddPuzzlePermission("primality");
        PrimaCyclia.Instance.AddPartType(Primality, "primality", static (part, pos, editor, renderer) => {
			Vector2 vector10 = new(41f, 48f);
			renderer.RenderBase(Assets.textures.parts.projection_glyph.@base, new Vector2(-1f, -1f), vector10, 0f);
			renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, center, 3f);
			renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, hex_right, 3f);
			renderer.RenderRotating(Assets.textures.parts.calcinator_bowl, center, Vector2.Zero);
			renderer.RenderUpright(prime_symbols, center, new Vector2(0f, 0f));
			renderer.RenderRotating(Assets.textures.parts.projection_glyph.metal_bowl, hex_right, Vector2.Zero);
			renderer.RenderBase(Assets.textures.parts.duplicator_bond, vector10 + new Vector2(-72f, -27f));
        });


        Neutrality = new() {
            //Id = "PrimaCyclia-neutrality", // ID
            name = Translations.Translate("Glyph of Neutrality"), // Name
            description = Translations.Translate("The glyph of neutrality converts one atom of sulfur and one atom of quicksilver into a single neutral salt."), // Description
            cost = 15, // Cost
            isFullHexCover = true, // Is a glyph
            glowTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/neutrality/glow"), // Shadow/glow
            strokeTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/neutrality/stroke"), // Stroke/outline
            iconTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/neutrality"), // Panel icon
            iconHoverTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/neutrality_hover"), // Hovered panel icon
            glyphHexes = [center, hex_right, hex_left],
            permissionCategory = PuzzlePermissions.None,
            CustomPermissionCheck = perms => perms.Contains(PrimaCyclia.Instance.GetIdentifier("neutrality"))
        };

        QApi.AddPartTypeToPanel(Neutrality, false);
        //QApi.AddPuzzlePermission("PrimaCyclia:Neutrality", "Glyph of Neutrality", "Prima Cyclia");
        PrimaCyclia.Instance.AddPuzzlePermission("neutrality");
        PrimaCyclia.Instance.AddPartType(Neutrality, "neutrality", static (part, pos, editor, renderer) => {
            Vector2 offset = new(123f, 48f);

            renderer.RenderBase(neutrality_base, Vector2.Zero, offset, 0f);

            renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, center, 3f);
			renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, hex_left, 3f);
			renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, hex_right, 3f);

            renderer.RenderRotating(Assets.textures.parts.projection_glyph.quicksilver_input, hex_left, Vector2.Zero); //input bowl 1
            renderer.RenderRotating(Assets.textures.parts.projection_glyph.quicksilver_input, hex_right, Vector2.Zero); //input bowl 2
            renderer.RenderUpright(neutrality_input, hex_left, Vector2.Zero);
            renderer.RenderUpright(neutrality_input, hex_right, Vector2.Zero);

            PartSimState pss = editor.GetSimulation().GetSimState(part);
            IntermediatePartState uco = editor.GetIntermState(part, pos);
            float time = editor.GetCycleTime();

            int irisFrame = 15;
            bool afterIrisOpens = false;
            Molecule risingAtom = Molecule.CreateMonoatomic(AtomTypes.salt);

            Vector2 risingOffset = uco.pos + HexGrid.standardGrid.ToPixelCoords(center).Rotated(uco.rotation);

            if (pss.isProcessing) {
                irisFrame = Utils.Clamp((int)(Utils.InterpolateLinear(1f, -1f, time) * 16f), 0, 15);
                afterIrisOpens = time > 0.5f;
                if (!afterIrisOpens) {
                    // show atom rising behind iris
                    Editor.RenderMolecule(risingAtom, risingOffset, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                }
            }
            renderer.RenderUpright(Assets.textures.parts.iris_full[irisFrame], center, Vector2.Zero);
            renderer.RenderRotating(Assets.textures.parts.animismus.output_above_iris, center, Vector2.Zero);
            if (pss.isProcessing && afterIrisOpens) {
                // show atom rising infront of iris
                Editor.RenderMolecule(risingAtom, risingOffset, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
            }
        });

        Cyclicity = new() {
            //Id = "PrimaCyclia-cyclicity", // ID
            name = Translations.Translate("Glyph of Cyclicity"), // Name
            description = Translations.Translate("The glyph of cyclicity takes atoms of non-neutral primes and progresses them to their next step in the cycle of primes."), // Description
            cost = 20, // Cost
            isFullHexCover = true, // Is a glyph
            glowTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/cyclicity/glow"), // Shadow/glow
            strokeTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/cyclicity/stroke"), // Stroke/outline
            iconTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/cyclicity"), // Panel icon
            iconHoverTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/cyclicity_hover"), // Hovered panel icon
            glyphHexes = [center, hex_up_right, hex_down_right],
            permissionCategory = PuzzlePermissions.None,
            CustomPermissionCheck = perms => perms.Contains(PrimaCyclia.Instance.GetIdentifier("cyclicity"))
        };

        QApi.AddPartTypeToPanel(Cyclicity, false);
        //QApi.AddPuzzlePermission("PrimaCyclia:Cyclicity", "Glyph of Cyclicity", "Prima Cyclia");
        PrimaCyclia.Instance.AddPuzzlePermission("cyclicity");
        PrimaCyclia.Instance.AddPartType(Cyclicity, "cyclicity", static (part, pos, editor, renderer) => {
            Vector2 offset = new(82f, 120f);

            renderer.RenderBase(cyclicity_base, Vector2.Zero, offset, 0f);

			renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, hex_up_right, 3f);
			renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, hex_down_right, 3f);

            renderer.RenderRotating(Assets.textures.parts.projection_glyph.quicksilver_input, hex_up_right, Vector2.Zero); //input bowl 1
            renderer.RenderUpright(primes_input, hex_up_right, Vector2.Zero);

            PartSimState pss = editor.GetSimulation().GetSimState(part);
            IntermediatePartState uco = editor.GetIntermState(part, pos);
            float time = editor.GetCycleTime();

            int irisFrame = 15;
            bool afterIrisOpens = false;
            Molecule risingAtom = pss.isProcessing ? Molecule.CreateMonoatomic(pss.processingAtoms[0]) : Molecule.CreateMonoatomic(primeAtoms[0]);

            Vector2 risingOffset = uco.pos + HexGrid.standardGrid.ToPixelCoords(hex_down_right).Rotated(uco.rotation);

            if (pss.isProcessing) {
                irisFrame = Utils.Clamp((int)(Utils.InterpolateLinear(1f, -1f, time) * 16f), 0, 15);
                afterIrisOpens = time > 0.5f;
                if (!afterIrisOpens) {
                    // show atom rising behind iris
                    Editor.RenderMolecule(risingAtom, risingOffset, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                }
            }
            renderer.RenderUpright(Assets.textures.parts.iris_full[irisFrame], hex_down_right, Vector2.Zero);
            renderer.RenderRotating(Assets.textures.parts.animismus.output_above_iris, hex_down_right, Vector2.Zero);
            if (pss.isProcessing && afterIrisOpens) {
                // show atom rising infront of iris
                Editor.RenderMolecule(risingAtom, risingOffset, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
            }
        });


        Polarity = new() {
            //Id = "PrimaCyclia-polarity", // ID
            name = Translations.Translate("Glyph of Polarity"), // Name
            description = Translations.Translate("The glyph of polarity consumes salt, and in turn inverts atoms of non-neutral primes to their opposite in the cycle of primes."), // Description
            cost = 15, // Cost
            isFullHexCover = true, // Is a glyph
            glowTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/neutrality/glow"), // Shadow/glow
            strokeTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/neutrality/stroke"), // Stroke/outline
            iconTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/polarity"), // Panel icon
            iconHoverTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/polarity_hover"), // Hovered panel icon
            glyphHexes = [center, hex_left, hex_right],
            permissionCategory = PuzzlePermissions.None,
            CustomPermissionCheck = perms => perms.Contains(PrimaCyclia.Instance.GetIdentifier("polarity"))
        };

        QApi.AddPartTypeToPanel(Polarity, false);
        //QApi.AddPuzzlePermission("PrimaCyclia:Polarity", "Glyph of Polarity", "Prima Cyclia");
        PrimaCyclia.Instance.AddPuzzlePermission("polarity");
        PrimaCyclia.Instance.AddPartType(Polarity, "polarity", static (part, pos, editor, renderer) => {
             Vector2 offset = new(123f, 48f);

            renderer.RenderBase(neutrality_base, Vector2.Zero, offset, 0f);
			renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, hex_left, 3f);
			renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, hex_right, 3f);
			renderer.RenderRotating(Assets.textures.parts.calcinator_bowl, hex_right, Vector2.Zero);
            renderer.RenderRotating(Assets.textures.parts.projection_glyph.quicksilver_input, hex_left, Vector2.Zero); //input bowl 1
            renderer.RenderUpright(Assets.textures.parts.animismus.symbol_salt, hex_left, new Vector2(2, 0));
			renderer.RenderUpright(prime_symbols, hex_right, new Vector2(0f, 0f));
			//renderer.RenderBase(Assets.textures.parts.field_173, offset + new Vector2(-72f, -27f));
        });

        Combustion = new() {
            //Id = "PrimaCyclia-combustion", // ID
            name = Translations.Translate("Glyph of Combustion"), // Name
            description = Translations.Translate("The glyph of combustion takes two different volatile atoms and turns them into two of the third volatile. It can also turn two sulfur into one each of the other volatiles."), // Description
            cost = 20, // Cost
            isFullHexCover = true, // Is a glyph
            glowTexture = AssetLoaderHelper.LoadTexture("textures/select/diamond_glow"), // Shadow/glow
            strokeTexture = AssetLoaderHelper.LoadTexture("textures/select/diamond_stroke"), // Stroke/outline
            iconTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/combustion"), // Panel icon
            iconHoverTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/combustion_hover"), // Hovered panel icon
            glyphHexes = [center, hex_up_right, hex_down_right, hex_right],
            permissionCategory = PuzzlePermissions.None,
            CustomPermissionCheck = perms => perms.Contains(PrimaCyclia.Instance.GetIdentifier("combustion"))
        };

        QApi.AddPartTypeToPanel(Combustion, false);
        //QApi.AddPuzzlePermission("PrimaCyclia:Combustion", "Glyph of Combustion", "Prima Cyclia");
        PrimaCyclia.Instance.AddPuzzlePermission("combustion");
        PrimaCyclia.Instance.AddPartType(Combustion, "combustion", static (part, pos, editor, renderer) => {
            Vector2 offset = new(41f, 120f);

            renderer.RenderBase(Assets.textures.parts.animismus.@base, Vector2.Zero, offset, 0f);
            
            foreach (HexIndex h in Combustion.glyphHexes) {renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, h, 3f);}

            renderer.RenderRotating(Assets.textures.parts.projection_glyph.quicksilver_input, center, Vector2.Zero); //input bowl 1
            renderer.RenderRotating(Assets.textures.parts.projection_glyph.quicksilver_input, hex_right, Vector2.Zero); //input bowl 1
            renderer.RenderUpright(volatiles_input, center, Vector2.Zero);
            renderer.RenderUpright(volatiles_input, hex_right, Vector2.Zero);

            PartSimState pss = editor.GetSimulation().GetSimState(part);
            IntermediatePartState uco = editor.GetIntermState(part, pos);
            float time = editor.GetCycleTime();

            int irisFrame = 15;
            bool afterIrisOpens = false;
            Molecule risingAtom_1 = pss.isProcessing ? Molecule.CreateMonoatomic(pss.processingAtoms[0]) : Molecule.CreateMonoatomic(primeAtoms[2]);
            Molecule risingAtom_2 = pss.isProcessing ? Molecule.CreateMonoatomic(pss.processingAtoms[1]) : Molecule.CreateMonoatomic(primeAtoms[2]);

            Vector2 risingOffset_1 = uco.pos + HexGrid.standardGrid.ToPixelCoords(hex_down_right).Rotated(uco.rotation);
            Vector2 risingOffset_2 = uco.pos + HexGrid.standardGrid.ToPixelCoords(hex_up_right).Rotated(uco.rotation);

            if (pss.isProcessing) {
                irisFrame = Utils.Clamp((int)(Utils.InterpolateLinear(1f, -1f, time) * 16f), 0, 15);
                afterIrisOpens = time > 0.5f;
                if (!afterIrisOpens) {
                    // show atom rising behind iris
                    Editor.RenderMolecule(risingAtom_1, risingOffset_1, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                    Editor.RenderMolecule(risingAtom_2, risingOffset_2, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                }
            }
            renderer.RenderUpright(phlog_iris[irisFrame], hex_down_right, Vector2.Zero);
            renderer.RenderUpright(oxygen_iris[irisFrame], hex_up_right, Vector2.Zero);
            renderer.RenderRotating(Assets.textures.parts.animismus.output_above_iris, hex_down_right, Vector2.Zero);
            renderer.RenderRotating(Assets.textures.parts.animismus.output_above_iris, hex_up_right, Vector2.Zero);
            if (pss.isProcessing && afterIrisOpens) {
                // show atom rising infront of iris
                Editor.RenderMolecule(risingAtom_1, risingOffset_1, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                Editor.RenderMolecule(risingAtom_2, risingOffset_2, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
            }
        });

        AtomType[] singleMassAtoms = [
            AtomTypes.air, // air
            AtomTypes.earth, // earth
            AtomTypes.fire, // fire
            AtomTypes.water, // water
            AtomTypes.vitae, // vitae
            AtomTypes.mors, // mors
            AtomTypes.quicksilver, // quicksilver
            AtomTypes.lead, // lead
	    ];

        foreach (AtomType a in singleMassAtoms) {addSublimationAtom(a, 1);}

        addSublimationAtom(AtomTypes.quintessence, 4);

        addSublimationAtom(AtomTypes.gold, 6);
		addSublimationAtom(AtomTypes.silver, 5);
		addSublimationAtom(AtomTypes.copper, 4);
		addSublimationAtom(AtomTypes.iron, 3);
		addSublimationAtom(AtomTypes.tin, 2);

        addSublimationAtom(Atoms.Sulfur, 0);
        addSublimationAtom(Atoms.Oxygen, 0);
        addSublimationAtom(Atoms.Phlogiston, 0);

        Sublimation = new() {
            //Id = "PrimaCyclia-sublimation", // ID
            name = Translations.Translate("Glyph of Sublimation"), // Name
            description = Translations.Translate("The glyph of sublimation violently consumes any atom it receives, producing sulfur in return. The amount of sulfur produced is equal to the \"weight\" of the atom.\n\nMost atoms have a weight of 1, quintessence has a weight of 4, and all metals have a weight equal to their metallicity. All volatile atoms, however, do not produce any sulfur when consumed by this glyph."), // Description
            cost = 20, // Cost
            isFullHexCover = true, // Is a glyph
            glowTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/sublimation/glow"), // Shadow/glow
            strokeTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/sublimation/stroke"), // Stroke/outline
            iconTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/sublimation"), // Panel icon
            iconHoverTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/sublimation_hover"), // Hovered panel icon
            glyphHexes = [ center, hex_left, hex_right, hex_down_left, hex_down_right, hex_up_left, hex_up_right,
                                          sublimationIrises[0], sublimationIrises[1], sublimationIrises[2], sublimationIrises[3], sublimationIrises[4], sublimationIrises[5] ],
            permissionCategory = PuzzlePermissions.None,
            isUnique = true,
            CustomPermissionCheck = perms => perms.Contains(PrimaCyclia.Instance.GetIdentifier("sublimation"))
        };

        QApi.AddPartTypeToPanel(Sublimation, false);
        //QApi.AddPuzzlePermission("PrimaCyclia:Sublimation", "Glyph of Sublimation", "Prima Cyclia");
        PrimaCyclia.Instance.AddPuzzlePermission("sublimation");
        PrimaCyclia.Instance.AddPartType(Sublimation, "sublimation", static (part, pos, editor, renderer) => {
            PartSimState pss = editor.GetSimulation().GetSimState(part);

            IntermediatePartState class_424 = editor.GetIntermState(part, pos);
            PartRenderer class_425 = new(class_424.pos, class_424.rotation, Editor.GetLightSourcePosition());

            Texture tex_to_render = sublimation_base;
            renderer.RenderBase(tex_to_render, tex_to_render.GetHalfSizeRounded() + new Vector2(1f, 1f));
			renderer.RenderUpright(Assets.textures.parts.disposal.lower_ring, new HexIndex(0, 0), new Vector2(1f, 1f));
			renderer.RenderUpright(Assets.textures.parts.disposal.lower_fixed, new HexIndex(0, 0), new Vector2(1f, 1f));
			renderer.RenderUpright(Assets.textures.parts.disposal.upper_fixed, new HexIndex(0, 0), new Vector2(1f, 1f));
			renderer.RenderUpright(Assets.textures.parts.disposal.upper_rings, new HexIndex(0, 0), new Vector2(1f, 1f));
			Texture[] field_17 = Assets.textures.parts.disposal_gears;
			int num13 = 0;
			if (editor.GetSimPlayState() != SimPlayState.Stopped) {
				num13 = (int)(Time.NowInSeconds() * 30f) % field_17.Length;
			}
            TextureRenderer.Render(field_17[num13], class_425.partPos + new Vector2(-62f, -62f));

            DynamicData dyn_pss = new(pss);
            object stateOb = dyn_pss.Get("sublimation_state");
            SublimeState is_open = new(IrisMode.open, -1);

            if (stateOb is not null) {
                is_open = (SublimeState)stateOb;
            }
            else { dyn_pss.Set("sublimation_state", is_open); }

            IntermediatePartState uco = editor.GetIntermState(part, pos);
            float time = editor.GetCycleTime();

            int irisFrame = 15;

            if (is_open.mode == IrisMode.opening) {
                irisFrame = Utils.Clamp((int)(Utils.InterpolateLinear(1f, -1f, time) * 16f), 0, 15);
                if (time > 0.5f) {irisFrame = 0;}
            }
            if (is_open.mode == IrisMode.closing) {
                irisFrame = Utils.Clamp((int)(Utils.InterpolateLinear(1f, -1f, 1-time) * 16f), 0, 15);
            }
            if (is_open.mode == IrisMode.open) {irisFrame = 0;}
            if (is_open.mode == IrisMode.closed) {irisFrame = 15;}

            renderer.RenderUpright(Assets.textures.parts.iris_full[irisFrame], center, Vector2.Zero);

            for (int i = 0; i < 6; i++) {

                int outputIrisFrame = 15;
                bool afterIrisOpens = false;

                HexIndex h = sublimationIrises[i];
                Vector2 location = uco.pos + HexGrid.standardGrid.ToPixelCoords(h).Rotated(uco.rotation);

                bool renderRisingAtom = pss.processingAtoms is not null && pss.processingAtoms.Length > i;

                Molecule risingAtom = Molecule.CreateMonoatomic(renderRisingAtom ? pss.processingAtoms[i] : Atoms.Sulfur);

                renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, h, 3f);

                if (pss.isProcessing && renderRisingAtom) {
                    outputIrisFrame = Utils.Clamp((int)(Utils.InterpolateLinear(1f, -1f, time) * 16f), 0, 15);
                    afterIrisOpens = time > 0.5f;
                    if (!afterIrisOpens) {
                        // show atom rising behind iris
                        Editor.RenderMolecule(risingAtom, location, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                    }
                }
                renderer.RenderUpright(sublimation_iris_anim[i][outputIrisFrame], h, Vector2.Zero);
                renderer.RenderRotating(Assets.textures.parts.animismus.output_above_iris, h, Vector2.Zero);
                if (pss.isProcessing && afterIrisOpens) {
                    // show atom rising infront of iris
                    Editor.RenderMolecule(risingAtom, location, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                }
            }

        });

        
        Empowerment = new() {
            //Id = "PrimaCyclia-empowerment", // ID
            name = Translations.Translate("Glyph of Empowerment"), // Name
            description = Translations.Translate("The glyph of empowerment consumes atoms of oxygen to unlock the full potential of a glyph of triplex-bonding, allowing any two atoms to be triplex bonded.\n\nIn turn, once all three parts of a triplex bond have been made, an atom of Phlogiston is produced, and the bonding glyph will once again be limited in its power."), // Description
            cost = 10, // Cost
            isFullHexCover = true, // Is a glyph
            glowTexture = Assets.textures.select.disposal_glow, // Shadow/glow
            strokeTexture = Assets.textures.select.disposal_stroke, // Stroke/outline
            iconTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/empowerment"), // Panel icon
            iconHoverTexture = AssetLoaderHelper.LoadTexture("textures/prima_cyclia/parts/icons/empowerment_hover"), // Hovered panel icon
            glyphHexes = [hex_left, hex_right, hex_up_left, hex_up_right],
            permissionCategory = PuzzlePermissions.None,
            CustomPermissionCheck = perms => perms.Contains(PrimaCyclia.Instance.GetIdentifier("empowerment"))
        };

        QApi.AddPartTypeToPanel(Empowerment, false);
        //QApi.AddPuzzlePermission("PrimaCyclia:Empowerment", "Glyph of Empowerment", "Prima Cyclia");
        PrimaCyclia.Instance.AddPuzzlePermission("empowerment");
        PrimaCyclia.Instance.AddPartType(Empowerment, "empowerment", static (part, pos, editor, renderer) => {
        
            PartSimState pss = editor.GetSimulation().GetSimState(part);
            IntermediatePartState uco = editor.GetIntermState(part, pos);
            float time = editor.GetCycleTime();

            bool renderMesh = true;
            foreach (Part other in editor.GetSolution().parts) {
                if (other.GetType() == PartTypes.triplexBonder) {
                    if (isEmpowered(part,other)) {
                        renderMesh = false;
                    }
                }
            }

            if (renderMesh) {
                renderer.RenderBase(empowerment_mesh, empowerment_mesh.GetHalfSizeRounded() + new Vector2(1f, 1f));
            }


			renderer.RenderBase(empowerment_base, empowerment_base.GetHalfSizeRounded() + new Vector2(1f, 1f));
            renderer.RenderBase(empowerment_overlay, empowerment_overlay.GetHalfSizeRounded() + new Vector2(1f, 1f));

            DynamicData dyn_pss = new(pss);
            object stateOb = dyn_pss.Get("empowerment_state");
            EmpowerState emp = new([false,false,false]);

            if (stateOb is not null) {
                emp = (EmpowerState)stateOb;
            }
            else { dyn_pss.Set("empowerment_state", emp); }

            for (int i = 0; i < 3; i++) {
                if (emp.canBond[i]) {renderer.RenderBase(empowerment_lights[i], empowerment_lights[i].GetHalfSizeRounded() + new Vector2(1f, 1f));}
            }

			renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, hex_up_right, 3f);
			renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, hex_up_left, 3f);

            renderer.RenderRotating(Assets.textures.parts.projection_glyph.quicksilver_input, hex_up_left, Vector2.Zero); //input bowl 1

            renderer.RenderUpright(oxygen_input, hex_up_left, Vector2.Zero);

            int irisFrame = 15;
            bool afterIrisOpens = false;
            Molecule risingAtom = Molecule.CreateMonoatomic(Atoms.Phlogiston);

            Vector2 risingOffset = uco.pos + HexGrid.standardGrid.ToPixelCoords(hex_up_right).Rotated(uco.rotation);

            if (pss.isProcessing) {
                irisFrame = Utils.Clamp((int)(Utils.InterpolateLinear(1f, -1f, time) * 16f), 0, 15);
                afterIrisOpens = time > 0.5f;
                if (!afterIrisOpens) {
                    // show atom rising behind iris
                    Editor.RenderMolecule(risingAtom, risingOffset, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                }
            }
            renderer.RenderUpright(phlog_iris[irisFrame], hex_up_right, Vector2.Zero);
            renderer.RenderRotating(Assets.textures.parts.animismus.output_above_iris, hex_up_right, Vector2.Zero);
            if (pss.isProcessing && afterIrisOpens) {
                // show atom rising infront of iris
                Editor.RenderMolecule(risingAtom, risingOffset, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
            }
        });

        QApi.RunAfterCycle((sim, first) => {

            SolutionEditorBase seb = sim.solutionEditor;
            Dictionary<Part, PartSimState> pss = sim.simulationDict;
            List<Part> parts = seb.GetSolution().parts;

            Maybe<AtomReference> maybeFindAtom(Part part, HexIndex hex, List<Part> list, bool checkWheels = false) {
                return (Maybe<AtomReference>)PrimaCyclia.PrivateMethod<Sim>("GetAtomReference").Invoke(sim, new object[] { part, hex, list, checkWheels });
            }

            foreach (Part part in parts) {
                PartType type = part.GetType();

                if (type == Primality) {

                    if (sim.FindAtomRelative(part, hex_right).GetOrDefault(out AtomReference converted) && converted.atomType == AtomTypes.salt){
                        if (maybeFindAtom(part, center, new List<Part>(), true).GetOrDefault(out AtomReference check) && primeAtoms.Contains(check.atomType))
                        {
                            converted.molecule.ReplaceAtom(check.atomType, converted.pos);
                            converted.atom.transmutationEffect = new TransmutationEffect(seb, TransmutationEffectRenderMode.AsMask, converted.atomType, Assets.textures.atoms.duplicate_mask, 30f);
                            Vector2 param_5374 = HexGrid.standardGrid.ToPixelCoords(part.GetHexPos() + new HexIndex(1, 0).Rotated(part.GetRotation()));
                            seb.underneathPartsGlyphEffects.Add(new GlyphEffect(seb, EffectTimescaleType.RealTime, param_5374, Assets.textures.parts.projection_glyph_flash, 30f, Vector2.Zero, part.GetRotation().ToRadians()));
                            PrimalitySound.PlaySound(sim.solutionEditor.GetVolumeMultiplier());
                        }
                    }
                } else if (type == Neutrality) {

                    if (first && !pss[part].isProcessing) {
                        if (sim.FindAtomRelative(part, hex_left).GetOrDefault(out AtomReference atom_1) && !atom_1.inMultiAtomMolecule && !atom_1.isHeldByArm &&
                            sim.FindAtomRelative(part, hex_right).GetOrDefault(out AtomReference atom_2) && !atom_2.inMultiAtomMolecule && !atom_2.isHeldByArm &&
                            ((atom_1.atomType == Atoms.Sulfur && atom_2.atomType == AtomTypes.quicksilver) ||
                             (atom_2.atomType == Atoms.Sulfur && atom_1.atomType == AtomTypes.quicksilver))
                            )
                        {
                            if (!sim.FindAtomRelative(part, center).HasValue()) {
                                atom_1.molecule.RemoveAtom(atom_1.pos);
                                seb.consumptionEffects.Add(new ConsumptionEffect(seb, atom_1));
                                atom_2.molecule.RemoveAtom(atom_2.pos);
                                seb.consumptionEffects.Add(new ConsumptionEffect(seb, atom_2));
                                pss[part].isProcessing = true;
                                sim.additionalCollisions.Add(new(Sim.ColliderType.Atom, HexGrid.standardGrid.ToPixelCoords(part.GetHexPos())) { radius = 15f });
                                NeutralitySound.PlaySound(sim.solutionEditor.GetVolumeMultiplier());
                            }
                        }
                    }
                    else if (pss[part].isProcessing) {
                        Molecule m = new();
                        m.AddAtom(new Atom(AtomTypes.salt), part.GetHexPos());
                        sim.molecules.Add(m);
                    }
                } else if (type == Cyclicity) {

                    if (first && !pss[part].isProcessing) {
                        if (sim.FindAtomRelative(part, hex_up_right).GetOrDefault(out AtomReference atom_1) && !atom_1.inMultiAtomMolecule && !atom_1.isHeldByArm && primeAtoms.Contains(atom_1.atomType)) {
                            if (!sim.FindAtomRelative(part, hex_down_right).HasValue()) {
                                atom_1.molecule.RemoveAtom(atom_1.pos);
                                seb.consumptionEffects.Add(new ConsumptionEffect(seb, atom_1));
                                pss[part].isProcessing = true;
                                sim.additionalCollisions.Add(new(Sim.ColliderType.Atom, HexGrid.standardGrid.ToPixelCoords(part.GlobalPosFromLocal(hex_down_right))) { radius = 15f });
                                CyclicitySound.PlaySound(sim.solutionEditor.GetVolumeMultiplier());

                                pss[part].processingAtoms = new AtomType[] { GetPrime(primeAtoms.IndexOf(atom_1.atomType) - 1) };
                            }
                        }
                    } else if (pss[part].isProcessing) {
                        Molecule m = new();
                        m.AddAtom(new Atom(pss[part].processingAtoms[0]), part.GlobalPosFromLocal(hex_down_right));
                        sim.molecules.Add(m);
                    }
                } else if (type == Polarity) {

                    if (sim.FindAtomRelative(part, hex_left).GetOrDefault(out AtomReference input) && input.atomType == AtomTypes.salt && !input.inMultiAtomMolecule && !input.isHeldByArm) {
                        if (maybeFindAtom(part, hex_right, new List<Part>(), true).GetOrDefault(out AtomReference transmuted) && primeAtoms.Contains(transmuted.atomType)) {
                            input.molecule.RemoveAtom(input.pos);
                            seb.consumptionEffects.Add(new ConsumptionEffect(seb, input));
                            transmuted.molecule.ReplaceAtom(GetPrime(primeAtoms.IndexOf(transmuted.atomType) + 2), transmuted.pos);
                            transmuted.atom.transmutationEffect = new TransmutationEffect(seb, TransmutationEffectRenderMode.AsMask, transmuted.atomType, Assets.textures.atoms.duplicate_mask, 30f);
                            Vector2 param_5374 = HexGrid.standardGrid.ToPixelCoords(part.GetHexPos() + new HexIndex(1, 0).Rotated(part.GetRotation()));
                            seb.underneathPartsGlyphEffects.Add(new GlyphEffect(seb, EffectTimescaleType.RealTime, param_5374, Assets.textures.parts.projection_glyph_flash, 30f, Vector2.Zero, part.GetRotation().ToRadians()));
                            PolaritySound.PlaySound(sim.solutionEditor.GetVolumeMultiplier());
                        }
                    }
                } else if (type == Combustion) {
                    if (first && !pss[part].isProcessing) {
                        if (sim.FindAtomRelative(part, center).GetOrDefault(out AtomReference atom_1) && !atom_1.inMultiAtomMolecule && !atom_1.isHeldByArm &&
                            sim.FindAtomRelative(part, hex_right).GetOrDefault(out AtomReference atom_2) && !atom_2.inMultiAtomMolecule && !atom_2.isHeldByArm && (
                                (atom_1.atomType == Atoms.Sulfur && atom_2.atomType == Atoms.Sulfur) ||
                                (atom_1.atomType != atom_2.atomType && volatileAtoms.Contains(atom_1.atomType) && volatileAtoms.Contains(atom_2.atomType))
                            ))
                        {
                            if (!sim.FindAtomRelative(part, hex_down_right).HasValue() && !sim.FindAtomRelative(part, hex_up_right).HasValue()) {

                                AtomType[] produced_atoms;
                                if (atom_1.atomType == atom_2.atomType) {
                                    produced_atoms = new AtomType[] { Atoms.Phlogiston, Atoms.Oxygen };
                                }
                                else {
                                    AtomType to_create = Atoms.Sulfur;
                                    foreach (AtomType a in volatileAtoms)
                                    {
                                        if (a != atom_1.atomType && a != atom_2.atomType)
                                        {
                                            to_create = a;
                                        }
                                    }
                                    produced_atoms = new AtomType[] { to_create, to_create };
                                }

                                pss[part].processingAtoms = produced_atoms;

                                atom_1.molecule.RemoveAtom(atom_1.pos);
                                atom_2.molecule.RemoveAtom(atom_2.pos);
                                seb.consumptionEffects.Add(new ConsumptionEffect(seb, atom_1));
                                seb.consumptionEffects.Add(new ConsumptionEffect(seb, atom_2));
                                pss[part].isProcessing = true;
                                sim.additionalCollisions.Add(new(Sim.ColliderType.Atom, HexGrid.standardGrid.ToPixelCoords(part.GlobalPosFromLocal(hex_down_right))) { radius = 15f });
                                sim.additionalCollisions.Add(new(Sim.ColliderType.Atom, HexGrid.standardGrid.ToPixelCoords(part.GlobalPosFromLocal(hex_up_right))) { radius = 15f });

                                if (produced_atoms[0] == produced_atoms[1]) {
                                    if (produced_atoms[0] == Atoms.Phlogiston) { CombustionPhlogRelease.PlaySound(sim.solutionEditor.GetVolumeMultiplier()); }
                                    else if (produced_atoms[0] == Atoms.Phlogiston) { CombustionOxygenRelease.PlaySound(sim.solutionEditor.GetVolumeMultiplier()); }
                                    else { CombustionBothRelease.PlaySound(sim.solutionEditor.GetVolumeMultiplier()); }
                                } else {
                                    CombustionBothRelease.PlaySound(sim.solutionEditor.GetVolumeMultiplier());
                                }

                            }
                        }
                    } else if (pss[part].isProcessing) {
                        Molecule m = new();
                        m.AddAtom(new Atom(pss[part].processingAtoms[0]), part.GlobalPosFromLocal(hex_down_right));
                        sim.molecules.Add(m);
                        m = new();
                        m.AddAtom(new Atom(pss[part].processingAtoms[1]), part.GlobalPosFromLocal(hex_up_right));
                        sim.molecules.Add(m);
                    }
                } else if (type == Sublimation) {

                    PartSimState sublim_pss = pss[part];

                    DynamicData dyn_pss = new(sublim_pss);
                    object stateOb = dyn_pss.Get("sublimation_state");
                    SublimeState is_open = new(IrisMode.open, -1);

                    if (stateOb is not null) {
                        is_open = (SublimeState)stateOb;
                    }
                    else { dyn_pss.Set("sublimation_state", is_open); }

                    bool hexes_covered = false;
                    foreach (HexIndex h in sublimationIrises)
                        if (sim.FindAtomRelative(part, h).GetOrDefault(out AtomReference a)) { hexes_covered = true; }
                    if (sim.FindAtomRelative(part, center).GetOrDefault(out AtomReference positioned) && !sublimationDict.ContainsKey(positioned.atomType)) { hexes_covered = true; }

                    int current_cycle = sim.GetCycle();
                    if (is_open.cycle_modified < current_cycle && first) {
                        if (hexes_covered && is_open.mode == IrisMode.closing) {is_open.mode = IrisMode.closed; is_open.cycle_modified = current_cycle;}
                        if (hexes_covered && is_open.mode != IrisMode.closed) {is_open.mode = IrisMode.closing; is_open.cycle_modified = current_cycle;}
                        if (!hexes_covered && is_open.mode == IrisMode.opening) {is_open.mode = IrisMode.open; is_open.cycle_modified = current_cycle;}
                        if (!hexes_covered && is_open.mode != IrisMode.open) {is_open.mode = IrisMode.opening; is_open.cycle_modified = current_cycle;}
                    }

                    if (is_open.mode == IrisMode.open && first && !sublim_pss.isProcessing) {
                        if (sim.FindAtomRelative(part, center).GetOrDefault(out AtomReference input) && !input.inMultiAtomMolecule && !input.isHeldByArm) {
                            input.molecule.RemoveAtom(input.pos);
                            seb.consumptionEffects.Add(new ConsumptionEffect(seb, input));
                            int weight = 1;
                            if (sublimationDict.ContainsKey(input.atomType)) {weight = sublimationDict[input.atomType];}
                            SublimationSound.PlaySound(sim.solutionEditor.GetVolumeMultiplier());

                            if (weight > 0) {
                                is_open.mode = IrisMode.closing; is_open.cycle_modified = current_cycle;
                                sublim_pss.processingAtoms = new AtomType[weight];     
                                for (int i = 0; i < weight; i++) {
                                    HexIndex h = sublimationIrises[i];
                                    sim.additionalCollisions.Add(new(Sim.ColliderType.Atom, HexGrid.standardGrid.ToPixelCoords(part.GlobalPosFromLocal(h))) { radius = 15f });
                                    sublim_pss.processingAtoms[i] = Atoms.Sulfur;
                                }
                                SublimationProduction[(weight - 1) / 2].PlaySound(sim.solutionEditor.GetVolumeMultiplier());
                                sublim_pss.isProcessing = true;
                            }
                        }
                    } else if (sublim_pss.isProcessing) {

                        for (int i = 0; i < sublim_pss.processingAtoms.Length; i++) {
                            HexIndex h = sublimationIrises[i];
                            Molecule m = new();
                            m.AddAtom(new Atom(sublim_pss.processingAtoms[i]), part.GlobalPosFromLocal(h));
                            sim.molecules.Add(m);
                        }
                    }
                    dyn_pss.Set("sublimation_state", is_open);
                } else if (type == Empowerment) {

                    DynamicData dyn_pss = new(pss[part]);
                    object stateOb = dyn_pss.Get("empowerment_state");
                    EmpowerState emp = new([false, false, false]);

                    if (stateOb is not null) {
                        emp = (EmpowerState)stateOb;
                    }
                    else { dyn_pss.Set("empowerment_state", emp); }

                    if (first && !pss[part].isProcessing && emp.IsEmpty()) {
                        if (!emp.containsCharge) {
                            if (sim.FindAtomRelative(part, hex_up_left).GetOrDefault(out AtomReference atom_1) && !atom_1.inMultiAtomMolecule && !atom_1.isHeldByArm && atom_1.atomType == Atoms.Oxygen) {
                                emp.canBond = new bool[] {true, true, true};
                                emp.containsCharge = true;
                                atom_1.molecule.RemoveAtom(atom_1.pos);
                                seb.consumptionEffects.Add(new ConsumptionEffect(seb, atom_1));
                                EmpowermentConsume.PlaySound(sim.solutionEditor.GetVolumeMultiplier());
                            }
                        } else {
                            emp.containsCharge = false;
                            pss[part].isProcessing = true;
                            EmpowermentProduce.PlaySound(sim.solutionEditor.GetVolumeMultiplier());
                            sim.additionalCollisions.Add(new(Sim.ColliderType.Atom, HexGrid.standardGrid.ToPixelCoords(part.GlobalPosFromLocal(hex_up_right))) { radius = 15f });

                        }
                    } else if (pss[part].isProcessing) {
                        Molecule m = new();
                        m.AddAtom(new Atom(Atoms.Phlogiston), part.GlobalPosFromLocal(hex_up_right));
                        sim.molecules.Add(m);
                    }

                    foreach (Part other in parts) {
                        if (other.GetType() == PartTypes.triplexBonder) {
                            if (isEmpowered(part,other)) {
                                BonderInfo[] bonder_set = PartTypes.triplexBonder.bonders;
                                for (int index = 0; index < bonder_set.Length; index++) {
                                    BonderInfo bonder = bonder_set[index];
                                    if (!sim.FindAtomRelative(other, bonder.hexPos1).GetOrDefault(out AtomReference leftAtom) || !sim.FindAtomRelative(other, bonder.hexPos2).GetOrDefault(out AtomReference rightAtom)) {
                                        continue;
                                    }
                                    if (emp.canBond[index]) {
                                        if (leftAtom.molecule != rightAtom.molecule) {
                                            sim.molecules.Remove(leftAtom.molecule);
                                            sim.molecules.Remove(rightAtom.molecule);
                                            var joined = leftAtom.molecule.MergeWith(rightAtom.molecule);
                                            sim.molecules.Add(joined);
                                        }
                                        HexIndex h1 = part.GlobalPosFromLocal(bonder.hexPos1);
                                        HexIndex h2 = part.GlobalPosFromLocal(bonder.hexPos2);
                                        if (sim.FindAtom(h1).GetOrDefault(out AtomReference atom1) && sim.FindAtom(h2).GetOrDefault(out AtomReference atom2)) {
                                            Molecule subject = atom1.molecule;
                                            if (subject == atom2.molecule) {
                                                bool wasAnyAdded = false;
                                                foreach (BondType bondType in bonder.bondTypes) {
                                                    BondTexture bondTexture = bondType.bondTexture;
                                                    BondEffect bondEffect = new(sim.solutionEditor, bondTexture.bondAnim, 60f, bondTexture.textureOffset);
                                                    if (subject.AddBond(bondType, h1, h2, bondEffect)) {
                                                        Vector2 vector2 = Utils.InterpolateVect(HexGrid.standardGrid.ToPixelCoords(h1), HexGrid.standardGrid.ToPixelCoords(h2), 0.5f);
                                                        sim.solutionEditor.glyphEffects.Add(new GlyphEffect(sim.solutionEditor, EffectTimescaleType.RealTime, vector2, bondTexture.glyphAnim, 30f, Vector2.Zero, HexGrid.standardGrid.ToPixelCoords(h2 - h1).Angle(), 0f));
                                                        //sim.PlaySound(bondTexture.bondSound);
                                                        wasAnyAdded = true;
                                                    }
                                                }
                                                if (wasAnyAdded) {
                                                    EmpowermentBonds[index].PlaySound(sim.solutionEditor.GetVolumeMultiplier());
                                                    emp.canBond[index] = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    dyn_pss.Set("empowerment_state", emp);
                }
            }
        });
    }
}
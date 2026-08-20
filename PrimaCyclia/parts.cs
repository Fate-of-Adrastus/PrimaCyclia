using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
namespace PrimaCyclia;

using AtomTypes = class_175;
using Permissions = enum_149;
using PartType = class_139;
using PartTypes = class_191;
using Texture = class_256;

public static class Parts
{
    public static PartType Polarity, Neutrality, Cyclicity, Primality, Empowerment, Combustion, Sublimation, Tolomeo;
    public static PartType prismabonder = class_191.field_1775;
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

    static HexRotation[] HexArmRotations => PartTypes.field_1767.field_1534;
    const string TolomeoWheelAtomsField = "PrimaCyclia_TolomeoMolecule";
    public static Vector2 hexGraphicalOffset(HexIndex hex) => class_187.field_1742.method_492(hex);
    static class_126 atomCageLighting => class_238.field_1989.field_90.field_232;

    public static Texture bowl = class_238.field_1989.field_90.field_170;
    public static Texture singleHexBase = class_238.field_1989.field_90.field_169;
    public static Texture doubleHexBase = class_238.field_1989.field_90.field_255.field_288;
    public static Texture[] irisAnim = class_238.field_1989.field_90.field_246;
    public static Texture irisRing = class_238.field_1989.field_90.field_228.field_271;
    public static Texture irisWell = class_238.field_1989.field_90.field_228.field_272;
    public static Texture salt_input = class_238.field_1989.field_90.field_228.field_275;

    // custom glyph textures

    public static Texture prime_symbols = class_235.method_615("textures/PrimaCyclia/parts/prime_symbols");

    public static Texture oxygen_input = class_235.method_615("textures/PrimaCyclia/parts/oxygen_input");
    public static Texture phlog_input = class_235.method_615("textures/PrimaCyclia/parts/phlogiston_input");
    public static Texture sulfur_input = class_235.method_615("textures/PrimaCyclia/parts/sulfur_input");
    public static Texture primes_input = class_235.method_615("textures/PrimaCyclia/parts/primes_input");
    public static Texture neutrality_input = class_235.method_615("textures/PrimaCyclia/parts/qs_sulfur_input");
    public static Texture volatiles_input = class_235.method_615("textures/PrimaCyclia/parts/volatiles_input");

    public static Texture neutrality_base = class_235.method_615("textures/PrimaCyclia/parts/neutrality/base");
    public static Texture cyclicity_base = class_235.method_615("textures/PrimaCyclia/parts/cyclicity/base");
    public static Texture sublimation_base = class_235.method_615("textures/PrimaCyclia/parts/sublimation/base");
    public static Texture empowerment_base = class_235.method_615("textures/PrimaCyclia/parts/empowerment/base");
    public static Texture empowerment_mesh = class_235.method_615("textures/PrimaCyclia/parts/empowerment/mesh");

    public static Texture[][] sublimation_iris_anim = {
        Brimstone.API.GetAnimation("textures/PrimaCyclia/parts/sublimation/iris_1_full.array","iris",16),
        Brimstone.API.GetAnimation("textures/PrimaCyclia/parts/sublimation/iris_2_full.array","iris",16),
        Brimstone.API.GetAnimation("textures/PrimaCyclia/parts/sublimation/iris_3_full.array","iris",16),
        Brimstone.API.GetAnimation("textures/PrimaCyclia/parts/sublimation/iris_4_full.array","iris",16),
        Brimstone.API.GetAnimation("textures/PrimaCyclia/parts/sublimation/iris_5_full.array","iris",16),
        Brimstone.API.GetAnimation("textures/PrimaCyclia/parts/sublimation/iris_6_full.array","iris",16),
        };

    public static Texture[] oxygen_iris = Brimstone.API.GetAnimation("textures/PrimaCyclia/parts/iris_oxygen_full.array","iris",16);
    public static Texture[] phlog_iris = Brimstone.API.GetAnimation("textures/PrimaCyclia/parts/iris_phlogiston_full.array","iris",16);

    public static Texture empowerment_overlay = class_235.method_615("textures/PrimaCyclia/parts/empowerment/overlay");
    public static Texture[] empowerment_lights = {class_235.method_615("textures/PrimaCyclia/parts/empowerment/b2_light"),
                                                  class_235.method_615("textures/PrimaCyclia/parts/empowerment/b3_light"),
                                                  class_235.method_615("textures/PrimaCyclia/parts/empowerment/b1_light")};
    //

    public static List<AtomType> primeAtoms = new List<AtomType>{Brimstone.API.VanillaAtoms.quicksilver, Brimstone.API.VanillaAtoms.mors, Atoms.Sulfur, Brimstone.API.VanillaAtoms.vitae};
    public static List<AtomType> volatileAtoms = new List<AtomType>{Atoms.Sulfur, Atoms.Oxygen, Atoms.Phlogiston};

    enum IrisMode {open, closed, opening, closing}

    private class SublimeState
    {
        public IrisMode mode = IrisMode.open;
        public int cycle_modified = 0;

        public SublimeState(IrisMode m, int cycle)
        {
            this.mode = m;
            this.cycle_modified = cycle;
        }
    }

    private class EmpowerState
    {
        public bool[] canBond = {false,false,false};
        public bool containsCharge = false;
        public EmpowerState(bool[] bondables){ canBond = bondables; }
        public bool IsEmpty(){return Enumerable.SequenceEqual(canBond, new bool[]{false,false,false});}
    }

    static Molecule TolomeoMolecule()
    {
        Molecule molecule = new Molecule();
        molecule.method_1105(new Atom(Brimstone.API.VanillaAtoms.vitae), new HexIndex(0, 1));
        molecule.method_1105(new Atom(Brimstone.API.VanillaAtoms.salt), new HexIndex(1, 0));
        molecule.method_1105(new Atom(Brimstone.API.VanillaAtoms.quicksilver), new HexIndex(1, -1));
        molecule.method_1105(new Atom(Brimstone.API.VanillaAtoms.mors), new HexIndex(0, -1));
        molecule.method_1105(new Atom(Brimstone.API.VanillaAtoms.salt), new HexIndex(-1, 0));
        molecule.method_1105(new Atom(Atoms.Sulfur), new HexIndex(-1, 1));
        return molecule;
    }

    public static Sound CombustionOxygenRelease, CombustionPhlogRelease, CombustionBothRelease, CyclicitySound, EmpowermentConsume, EmpowermentProduce, NeutralitySound, PolaritySound, PrimalitySound, SublimationSound;
    public static Sound[] EmpowermentBonds, SublimationProduction;
    public static void LoadSounds()
    {
        string mainDir = Brimstone.API.GetContentPath("PrimaCyclia").method_1087();
        CombustionOxygenRelease = Brimstone.API.GetSound(mainDir, "sounds/combustion_oxyg").method_1087();
        CombustionPhlogRelease = Brimstone.API.GetSound(mainDir, "sounds/combustion_phlog").method_1087();
        CombustionBothRelease = Brimstone.API.GetSound(mainDir, "sounds/combustion_both").method_1087();
        CyclicitySound = Brimstone.API.GetSound(mainDir, "sounds/cyclicity_sound").method_1087();
        EmpowermentConsume = Brimstone.API.GetSound(mainDir, "sounds/empowerment_oxyg_in").method_1087();
        EmpowermentProduce = Brimstone.API.GetSound(mainDir, "sounds/empowerment_phlog_out").method_1087();
        NeutralitySound = Brimstone.API.GetSound(mainDir, "sounds/neutrality_sound").method_1087();
        PolaritySound = Brimstone.API.GetSound(mainDir, "sounds/polarity_sound").method_1087();
        PrimalitySound = Brimstone.API.GetSound(mainDir, "sounds/primality_sound").method_1087();
        SublimationSound = Brimstone.API.GetSound(mainDir, "sounds/sublimation_sound").method_1087();

        EmpowermentBonds = new Sound[] {
            Brimstone.API.GetSound(mainDir, "sounds/empowerment_supertriplex_a").method_1087(),
            Brimstone.API.GetSound(mainDir, "sounds/empowerment_supertriplex_b").method_1087(),
            Brimstone.API.GetSound(mainDir, "sounds/empowerment_supertriplex_c").method_1087()
        };

        SublimationProduction = new Sound[]
        {
            Brimstone.API.GetSound(mainDir, "sounds/sublimation_out_1_or_2").method_1087(),
            Brimstone.API.GetSound(mainDir, "sounds/sublimation_out_3_or_4").method_1087(),
            Brimstone.API.GetSound(mainDir, "sounds/sublimation_out_5_or_6").method_1087()
        };

        FieldInfo field = typeof(class_11).GetField("field_52", BindingFlags.Static | BindingFlags.NonPublic);
        Dictionary<string, float> volumeDictionary = (Dictionary<string, float>)field.GetValue(null);

        volumeDictionary.Add("combustion_oxyg", 0.3f);
        volumeDictionary.Add("combustion_phlog", 0.3f);
        volumeDictionary.Add("combustion_both", 0.3f);
        volumeDictionary.Add("cyclicity_sound", 0.3f);
        volumeDictionary.Add("empowerment_oxyg_in", 0.3f);
        volumeDictionary.Add("empowerment_phlog_out", 0.3f);
        volumeDictionary.Add("neutrality_sound", 0.3f);
        volumeDictionary.Add("polarity_sound", 0.3f);
        volumeDictionary.Add("primality_sound", 0.3f);
        volumeDictionary.Add("sublimation_sound", 0.3f);

        volumeDictionary.Add("empowerment_supertriplex_a", 0.3f);
        volumeDictionary.Add("empowerment_supertriplex_b", 0.3f);
        volumeDictionary.Add("empowerment_supertriplex_c", 0.3f);

        volumeDictionary.Add("sublimation_out_1_or_2", 0.3f);
        volumeDictionary.Add("sublimation_out_3_or_4", 0.3f);
        volumeDictionary.Add("sublimation_out_5_or_6", 0.3f);

        On.class_201.method_540 += enableSounds;
    }

    public static void Unload()
    {
        On.class_201.method_540 -= enableSounds;
    }

    public static void enableSounds(On.class_201.orig_method_540 orig, class_201 self)
    {
        CombustionOxygenRelease.field_4062 = false;
        CombustionPhlogRelease.field_4062 = false;
        CombustionBothRelease.field_4062 = false;
        CyclicitySound.field_4062 = false;
        EmpowermentConsume.field_4062 = false;
        EmpowermentProduce.field_4062 = false;
        NeutralitySound.field_4062 = false;
        PolaritySound.field_4062 = false;
        PrimalitySound.field_4062 = false;
        SublimationSound.field_4062 = false;

        foreach (Sound s in EmpowermentBonds) {s.field_4062 = false;}
        foreach (Sound s in SublimationProduction) {s.field_4062 = false;}

        orig(self);
    }

    public static void drawSelectionGlow(SolutionEditorBase seb_self, Part part, Vector2 pos, float alpha)
    {
        var cageSelectGlowTexture = class_238.field_1989.field_97.field_367;
        int armLength = 1; // part.method_1165()
        class_236 class236 = seb_self.method_1989(part, pos);
        Color color = Color.White.WithAlpha(alpha);

        PrimaCyclia.PrivateMethod<SolutionEditorBase>("method_2006").Invoke(seb_self, new object[] { armLength, HexArmRotations, class236, color });
        for (int index = 0; index < 6; ++index)
        {
            float num = index * sixtyDegrees;
            PrimaCyclia.PrivateMethod<SolutionEditorBase>("method_2016").Invoke(seb_self, new object[] { cageSelectGlowTexture, color, class236.field_1984, class236.field_1985 + num });
        }
    }

    private static AtomType GetPrime(int i)
    {
        while (i < 0) {i = i + 4;}
        while (i > 3) {i = i - 4;}
        return primeAtoms[i];
    }

    private static Molecule GetTolomeoWheelAtoms(PartSimState state) => GetTolomeoWheelData(state, TolomeoWheelAtomsField, TolomeoMolecule());

    public static Maybe<AtomReference> MaybeFindTolomeoWheelAtom(Sim sim_self, Part part, HexIndex offset) => MaybeFindTolomeoWheelAtom(sim_self, part.method_1184(offset));

    public static Maybe<AtomReference> MaybeFindTolomeoWheelAtom(Sim sim_self, HexIndex hex)
    {
        var SEB = sim_self.field_3818;
        var solution = SEB.method_502();
        var partList = solution.field_3919;
        var partSimStates = sim_self.field_3821;

        foreach (var Tolomeo in partList.Where(x => x.method_1159() == Tolomeo))
        {
            var partSimState = partSimStates[Tolomeo];
            Molecule TolomeoAtoms = GetTolomeoWheelAtoms(partSimState);
            var hexIndex = partSimState.field_2724;
            var rotation = partSimState.field_2726;
            var hexKey = (hex - hexIndex).Rotated(rotation.Negative());

            if (TolomeoAtoms.method_1100().TryGetValue(hexKey, out Atom atom))
            {
                return new AtomReference(TolomeoAtoms, hexKey, atom.field_2275, atom, true);
            }
        }
        return struct_18.field_1431;
    }

    // private methods
    private static void SetTolomeoWheelData<T>(PartSimState state, string field, T data) => new DynamicData(state).Set(field, data);
    private static T GetTolomeoWheelData<T>(PartSimState state, string field, T initial)
    {
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

    static void DrawTolomeoPart(Part part, Vector2 pos, SolutionEditorBase editor, class_195 renderer)
    {
        // draw atoms, if the simulation is stopped - otherwise, the running simulation will draw them
        if (editor.method_503() == enum_128.Stopped)
        {
            drawTolomeoAtoms(editor, part, pos);
        }

        // draw arm stubs
        class_236 class236 = editor.method_1989(part, pos);
        PrimaCyclia.PrivateMethod<SolutionEditorBase>("method_2005").Invoke(editor, new object[] { part.method_1165(), HexArmRotations, class236 });

        // draw cages
        PartSimState partSimState = editor.method_507().method_481(part);
        for (int i = 0; i < 6; i++)
        {
            float radians = renderer.field_1798 + (i * sixtyDegrees);
            Vector2 vector2_9 = renderer.field_1797 + hexGraphicalOffset(new HexIndex(1, 0)).Rotated(radians);
            PrimaCyclia.PrivateMethod<SolutionEditorBase>("method_2003").Invoke(editor, new object[] { atomCageLighting, vector2_9, new Vector2(39f, 33f), radians });
        }
    }
    public static void drawTolomeoAtoms(SolutionEditorBase seb_self, Part part, Vector2 pos)
    {
        if (part.method_1159() != Tolomeo) return;
        PartSimState partSimState = seb_self.method_507().method_481(part);

        class_236 class236 = seb_self.method_1989(part, pos);
        Editor.method_925(GetTolomeoWheelAtoms(partSimState), class236.field_1984, center, class236.field_1985, 1f, 1f, 1f, false, seb_self);
    }

    public static void addSublimationAtom(string atom, int weight)
    {
        foreach (AtomType A in QApi.ModAtomTypes) { if (A.QuintAtomType == atom) { addSublimationAtom(A, weight); } }
    }
    public static void addSublimationAtom(AtomType atom, int weight)
    {
        sublimationDict[atom] = weight;
    }

    public static bool isEmpowered(Part empowerment, Part prisma)
    {
        HashSet<HexIndex> checked_hexes = [empowerment.method_1184(center), empowerment.method_1184(hex_down_left), empowerment.method_1184(hex_down_right)];
        HashSet<HexIndex> my_triplex = [prisma.method_1184(center), prisma.method_1184(hex_right), prisma.method_1184(hex_up_right)];
        return checked_hexes.SetEquals(my_triplex);
    }

    static bool ContentLoaded = false;
    public static void AddPartTypes()
    {        

        if (ContentLoaded) return;
        ContentLoaded = true;
        //=========================//
        string iconpath = "textures/PrimaCyclia/parts/icons/tolomeo";
        Tolomeo = new PartType()
        {
            field_1528 = "PrimaCyclia-tolomeo",
            field_1529 = class_134.method_253("Tolomeo's Wheel", string.Empty),
            field_1530 = class_134.method_253("By using Tolomeo's wheel with the glyph of primality, neutral salt can be turned into any prime within the cycle of primes.", string.Empty),
            field_1531 = 30,
            field_1532 = (enum_2)1,
            field_1533 = true,
            field_1536 = true,
            field_1544 = new Dictionary<HexIndex, AtomType>(),
            field_1547 = class_235.method_615(iconpath),
            field_1548 = class_235.method_615(iconpath + "_hover"),
            field_1552 = true,
            CustomPermissionCheck = perms => perms.Contains("PrimaCyclia:TolomeoWheel")
        };
        Tolomeo.field_1544.Add(new HexIndex(0, 1), Brimstone.API.VanillaAtoms.vitae);
        Tolomeo.field_1544.Add(new HexIndex(1, 0), Brimstone.API.VanillaAtoms.salt);
        Tolomeo.field_1544.Add(new HexIndex(1, -1), Brimstone.API.VanillaAtoms.quicksilver);
        Tolomeo.field_1544.Add(new HexIndex(0, -1), Brimstone.API.VanillaAtoms.mors);
        Tolomeo.field_1544.Add(new HexIndex(-1, 0), Brimstone.API.VanillaAtoms.salt);
        Tolomeo.field_1544.Add(new HexIndex(-1, 1), Atoms.Sulfur);

        QApi.AddPartTypeToPanel(Tolomeo, PartTypes.field_1771);
        QApi.AddPartType(Tolomeo, DrawTolomeoPart);
        QApi.AddPuzzlePermission("PrimaCyclia:TolomeoWheel", "Tolomeo's Wheel", "Prima Cyclia");

        Primality = new()
        {
            field_1528 = "PrimaCyclia-primality", // ID
            field_1529 = class_134.method_253("Glyph of Primality", string.Empty), // Name
            field_1530 = class_134.method_253("The glyph of primality transforms an atom of salt into any atom within the cycle of primes.", string.Empty), // Description
            field_1531 = 20, // Cost
            field_1539 = true, // Is a glyph
            field_1549 = class_238.field_1989.field_97.field_374, // Shadow/glow
            field_1550 = class_238.field_1989.field_97.field_375, // Stroke/outline
            field_1547 = class_235.method_615("textures/PrimaCyclia/parts/icons/primality"), // Panel icon
            field_1548 = class_235.method_615("textures/PrimaCyclia/parts/icons/primality_hover"), // Hovered panel icon
            field_1540 = new HexIndex[] { center, hex_right },
            field_1551 = Permissions.None,
            CustomPermissionCheck = perms => perms.Contains("PrimaCyclia:Primality")
        };

        QApi.AddPartTypeToPanel(Primality, false);
        QApi.AddPuzzlePermission("PrimaCyclia:Primality", "Glyph of Primality", "Prima Cyclia");
        QApi.AddPartType(Primality, static (part, pos, editor, renderer) =>
        {
			Vector2 vector10 = new Vector2(41f, 48f);
			renderer.method_523(doubleHexBase, new Vector2(-1f, -1f), vector10, 0f);
			renderer.method_530(class_238.field_1989.field_90.field_164, center, 3f);
			renderer.method_530(class_238.field_1989.field_90.field_164, hex_right, 3f);
			Vector2 param_5593 = new Vector2(0f, 0f);
			renderer.method_528(bowl, center, param_5593);
			renderer.method_529(prime_symbols, center, new Vector2(0f, 0f));
			renderer.method_528(class_238.field_1989.field_90.field_255.field_292, hex_right, param_5593);
			renderer.method_521(class_238.field_1989.field_90.field_173, vector10 + new Vector2(-72f, -27f));
        });


        Neutrality = new()
        {
            field_1528 = "PrimaCyclia-neutrality", // ID
            field_1529 = class_134.method_253("Glyph of Neutrality", string.Empty), // Name
            field_1530 = class_134.method_253("The glyph of neutrality converts one atom of sulfur and one atom of quicksilver into a single neutral salt.", string.Empty), // Description
            field_1531 = 15, // Cost
            field_1539 = true, // Is a glyph
            field_1549 = class_235.method_615("textures/PrimaCyclia/parts/neutrality/glow"), // Shadow/glow
            field_1550 = class_235.method_615("textures/PrimaCyclia/parts/neutrality/stroke"), // Stroke/outline
            field_1547 = class_235.method_615("textures/PrimaCyclia/parts/icons/neutrality"), // Panel icon
            field_1548 = class_235.method_615("textures/PrimaCyclia/parts/icons/neutrality_hover"), // Hovered panel icon
            field_1540 = new HexIndex[] { center, hex_right, hex_left },
            field_1551 = Permissions.None,
            CustomPermissionCheck = perms => perms.Contains("PrimaCyclia:Neutrality")
        };

        QApi.AddPartTypeToPanel(Neutrality, false);
        QApi.AddPuzzlePermission("PrimaCyclia:Neutrality", "Glyph of Neutrality", "Prima Cyclia");
        QApi.AddPartType(Neutrality, static (part, pos, editor, renderer) =>
        {
            Vector2 offset = new(123f, 48f);

            renderer.method_523(neutrality_base, Vector2.Zero, offset, 0f);

            renderer.method_530(class_238.field_1989.field_90.field_164, center, 3f);
			renderer.method_530(class_238.field_1989.field_90.field_164, hex_left, 3f);
			renderer.method_530(class_238.field_1989.field_90.field_164, hex_right, 3f);

            renderer.method_528(class_238.field_1989.field_90.field_255.field_293, hex_left, Vector2.Zero); //input bowl 1
            renderer.method_528(class_238.field_1989.field_90.field_255.field_293, hex_right, Vector2.Zero); //input bowl 2
            renderer.method_529(neutrality_input, hex_left, Vector2.Zero);
            renderer.method_529(neutrality_input, hex_right, Vector2.Zero);

            PartSimState pss = editor.method_507().method_481(part);
            class_236 uco = editor.method_1989(part, pos);
            float time = editor.method_504();

            int irisFrame = 15;
            bool afterIrisOpens = false;
            Molecule risingAtom = Molecule.method_1121(Brimstone.API.VanillaAtoms.salt);

            Vector2 risingOffset = uco.field_1984 + class_187.field_1742.method_492(center).Rotated(uco.field_1985);

            if (pss.field_2743)
            {
                irisFrame = class_162.method_404((int)(class_162.method_411(1f, -1f, time) * 16f), 0, 15);
                afterIrisOpens = time > 0.5f;
                if (!afterIrisOpens)
                {
                    // show atom rising behind iris
                    Editor.method_925(risingAtom, risingOffset, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                }
            }
            renderer.method_529(irisAnim[irisFrame], center, Vector2.Zero);
            renderer.method_528(irisRing, center, Vector2.Zero);
            if (pss.field_2743 && afterIrisOpens)
            {
                // show atom rising infront of iris
                Editor.method_925(risingAtom, risingOffset, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
            }
        });

        Cyclicity = new()
        {
            field_1528 = "PrimaCyclia-cyclicity", // ID
            field_1529 = class_134.method_253("Glyph of Cyclicity", string.Empty), // Name
            field_1530 = class_134.method_253("The glyph of cyclicity takes atoms of non-neutral primes and progresses them to their next step in the cycle of primes.", string.Empty), // Description
            field_1531 = 20, // Cost
            field_1539 = true, // Is a glyph
            field_1549 = class_235.method_615("textures/PrimaCyclia/parts/cyclicity/glow"), // Shadow/glow
            field_1550 = class_235.method_615("textures/PrimaCyclia/parts/cyclicity/stroke"), // Stroke/outline
            field_1547 = class_235.method_615("textures/PrimaCyclia/parts/icons/cyclicity"), // Panel icon
            field_1548 = class_235.method_615("textures/PrimaCyclia/parts/icons/cyclicity_hover"), // Hovered panel icon
            field_1540 = new HexIndex[] { center, hex_up_right, hex_down_right },
            field_1551 = Permissions.None,
            CustomPermissionCheck = perms => perms.Contains("PrimaCyclia:Cyclicity")
        };

        QApi.AddPartTypeToPanel(Cyclicity, false);
        QApi.AddPuzzlePermission("PrimaCyclia:Cyclicity", "Glyph of Cyclicity", "Prima Cyclia");
        QApi.AddPartType(Cyclicity, static (part, pos, editor, renderer) =>
        {
            Vector2 offset = new(82f, 120f);

            renderer.method_523(cyclicity_base, Vector2.Zero, offset, 0f);

			renderer.method_530(class_238.field_1989.field_90.field_164, hex_up_right, 3f);
			renderer.method_530(class_238.field_1989.field_90.field_164, hex_down_right, 3f);

            renderer.method_528(class_238.field_1989.field_90.field_255.field_293, hex_up_right, Vector2.Zero); //input bowl 1
            renderer.method_529(primes_input, hex_up_right, Vector2.Zero);

            PartSimState pss = editor.method_507().method_481(part);
            class_236 uco = editor.method_1989(part, pos);
            float time = editor.method_504();

            int irisFrame = 15;
            bool afterIrisOpens = false;
            Molecule risingAtom = pss.field_2743 ? Molecule.method_1121(pss.field_2744[0]) : Molecule.method_1121(primeAtoms[0]);

            Vector2 risingOffset = uco.field_1984 + class_187.field_1742.method_492(hex_down_right).Rotated(uco.field_1985);

            if (pss.field_2743)
            {
                irisFrame = class_162.method_404((int)(class_162.method_411(1f, -1f, time) * 16f), 0, 15);
                afterIrisOpens = time > 0.5f;
                if (!afterIrisOpens)
                {
                    // show atom rising behind iris
                    Editor.method_925(risingAtom, risingOffset, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                }
            }
            renderer.method_529(irisAnim[irisFrame], hex_down_right, Vector2.Zero);
            renderer.method_528(irisRing, hex_down_right, Vector2.Zero);
            if (pss.field_2743 && afterIrisOpens)
            {
                // show atom rising infront of iris
                Editor.method_925(risingAtom, risingOffset, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
            }
        });


        Polarity = new()
        {
            field_1528 = "PrimaCyclia-polarity", // ID
            field_1529 = class_134.method_253("Glyph of Polarity", string.Empty), // Name
            field_1530 = class_134.method_253("The glyph of polarity consumes salt, and in turn inverts atoms of non-neutral primes to their opposite in the cycle of primes.", string.Empty), // Description
            field_1531 = 15, // Cost
            field_1539 = true, // Is a glyph
            field_1549 = class_235.method_615("textures/PrimaCyclia/parts/neutrality/glow"), // Shadow/glow
            field_1550 = class_235.method_615("textures/PrimaCyclia/parts/neutrality/stroke"), // Stroke/outline
            field_1547 = class_235.method_615("textures/PrimaCyclia/parts/icons/polarity"), // Panel icon
            field_1548 = class_235.method_615("textures/PrimaCyclia/parts/icons/polarity_hover"), // Hovered panel icon
            field_1540 = new HexIndex[] { center, hex_left, hex_right },
            field_1551 = Permissions.None,
            CustomPermissionCheck = perms => perms.Contains("PrimaCyclia:Polarity")
        };

        QApi.AddPartTypeToPanel(Polarity, false);
        QApi.AddPuzzlePermission("PrimaCyclia:Polarity", "Glyph of Polarity", "Prima Cyclia");
        QApi.AddPartType(Polarity, static (part, pos, editor, renderer) =>
        {
             Vector2 offset = new(123f, 48f);

            renderer.method_523(neutrality_base, Vector2.Zero, offset, 0f);
			renderer.method_530(class_238.field_1989.field_90.field_164, hex_left, 3f);
			renderer.method_530(class_238.field_1989.field_90.field_164, hex_right, 3f);
			Vector2 param_5593 = new Vector2(0f, 0f);
			renderer.method_528(bowl, hex_right, param_5593);
            renderer.method_528(class_238.field_1989.field_90.field_255.field_293, hex_left, Vector2.Zero); //input bowl 1
            renderer.method_529(salt_input, hex_left, new Vector2(2, 0));
			renderer.method_529(prime_symbols, hex_right, new Vector2(0f, 0f));
			//renderer.method_521(class_238.field_1989.field_90.field_173, offset + new Vector2(-72f, -27f));
        });

        Combustion = new()
        {
            field_1528 = "PrimaCyclia-combustion", // ID
            field_1529 = class_134.method_253("Glyph of Combustion", string.Empty), // Name
            field_1530 = class_134.method_253("The glyph of combustion takes two different volatile atoms and turns them into two of the third volatile. It can also turn two sulfur into one each of the other volatiles.", string.Empty), // Description
            field_1531 = 20, // Cost
            field_1539 = true, // Is a glyph
            field_1549 = class_235.method_615("textures/select/diamond_glow"), // Shadow/glow
            field_1550 = class_235.method_615("textures/select/diamond_stroke"), // Stroke/outline
            field_1547 = class_235.method_615("textures/PrimaCyclia/parts/icons/combustion"), // Panel icon
            field_1548 = class_235.method_615("textures/PrimaCyclia/parts/icons/combustion_hover"), // Hovered panel icon
            field_1540 = new HexIndex[] { center, hex_up_right, hex_down_right, hex_right },
            field_1551 = Permissions.None,
            CustomPermissionCheck = perms => perms.Contains("PrimaCyclia:Combustion")
        };

        QApi.AddPartTypeToPanel(Combustion, false);
        QApi.AddPuzzlePermission("PrimaCyclia:Combustion", "Glyph of Combustion", "Prima Cyclia");
        QApi.AddPartType(Combustion, static (part, pos, editor, renderer) =>
        {
            Vector2 offset = new(41f, 120f);

            renderer.method_523(class_238.field_1989.field_90.field_228.field_265, Vector2.Zero, offset, 0f);
            
            foreach (HexIndex h in Combustion.field_1540) {renderer.method_530(class_238.field_1989.field_90.field_164, h, 3f);}

            renderer.method_528(class_238.field_1989.field_90.field_255.field_293, center, Vector2.Zero); //input bowl 1
            renderer.method_528(class_238.field_1989.field_90.field_255.field_293, hex_right, Vector2.Zero); //input bowl 1
            renderer.method_529(volatiles_input, center, Vector2.Zero);
            renderer.method_529(volatiles_input, hex_right, Vector2.Zero);

            PartSimState pss = editor.method_507().method_481(part);
            class_236 uco = editor.method_1989(part, pos);
            float time = editor.method_504();

            int irisFrame = 15;
            bool afterIrisOpens = false;
            Molecule risingAtom_1 = pss.field_2743 ? Molecule.method_1121(pss.field_2744[0]) : Molecule.method_1121(primeAtoms[2]);
            Molecule risingAtom_2 = pss.field_2743 ? Molecule.method_1121(pss.field_2744[1]) : Molecule.method_1121(primeAtoms[2]);

            Vector2 risingOffset_1 = uco.field_1984 + class_187.field_1742.method_492(hex_down_right).Rotated(uco.field_1985);
            Vector2 risingOffset_2 = uco.field_1984 + class_187.field_1742.method_492(hex_up_right).Rotated(uco.field_1985);

            if (pss.field_2743)
            {
                irisFrame = class_162.method_404((int)(class_162.method_411(1f, -1f, time) * 16f), 0, 15);
                afterIrisOpens = time > 0.5f;
                if (!afterIrisOpens)
                {
                    // show atom rising behind iris
                    Editor.method_925(risingAtom_1, risingOffset_1, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                    Editor.method_925(risingAtom_2, risingOffset_2, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                }
            }
            renderer.method_529(phlog_iris[irisFrame], hex_down_right, Vector2.Zero);
            renderer.method_529(oxygen_iris[irisFrame], hex_up_right, Vector2.Zero);
            renderer.method_528(irisRing, hex_down_right, Vector2.Zero);
            renderer.method_528(irisRing, hex_up_right, Vector2.Zero);
            if (pss.field_2743 && afterIrisOpens)
            {
                // show atom rising infront of iris
                Editor.method_925(risingAtom_1, risingOffset_1, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                Editor.method_925(risingAtom_2, risingOffset_2, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
            }
        });

        AtomType[] singleMassAtoms = new AtomType[8] {
            AtomTypes.field_1676, // air
            AtomTypes.field_1677, // earth
            AtomTypes.field_1678, // fire
            AtomTypes.field_1679, // water
            AtomTypes.field_1687, // vitae
            AtomTypes.field_1688, // mors
            AtomTypes.field_1680, // quicksilver
            AtomTypes.field_1681, // lead
	    };

        foreach (AtomType a in singleMassAtoms) {addSublimationAtom(a, 1);}

        addSublimationAtom(Brimstone.API.VanillaAtoms.quintessence, 4);

        addSublimationAtom(Brimstone.API.VanillaAtoms.gold, 6);
		addSublimationAtom(Brimstone.API.VanillaAtoms.silver, 5);
		addSublimationAtom(Brimstone.API.VanillaAtoms.copper, 4);
		addSublimationAtom(Brimstone.API.VanillaAtoms.iron, 3);
		addSublimationAtom(Brimstone.API.VanillaAtoms.tin, 2);

        addSublimationAtom(Atoms.Sulfur, 0);
        addSublimationAtom(Atoms.Oxygen, 0);
        addSublimationAtom(Atoms.Phlogiston, 0);

        Sublimation = new()
        {
            field_1528 = "PrimaCyclia-sublimation", // ID
            field_1529 = class_134.method_253("Glyph of Sublimation", string.Empty), // Name
            field_1530 = class_134.method_253("The glyph of sublimation violently consumes any atom it receives, producing sulfur in return. The amount of sulfur produced is equal to the \"weight\" of the atom.\n\nMost atoms have a weight of 1, quintessence has a weight of 4, and all metals have a weight equal to their metallicity. All volatile atoms, however, do not produce any sulfur when consumed by this glyph.", string.Empty), // Description
            field_1531 = 20, // Cost
            field_1539 = true, // Is a glyph
            field_1549 = class_235.method_615("textures/PrimaCyclia/parts/sublimation/glow"), // Shadow/glow
            field_1550 = class_235.method_615("textures/PrimaCyclia/parts/sublimation/stroke"), // Stroke/outline
            field_1547 = class_235.method_615("textures/PrimaCyclia/parts/icons/sublimation"), // Panel icon
            field_1548 = class_235.method_615("textures/PrimaCyclia/parts/icons/sublimation_hover"), // Hovered panel icon
            field_1540 = new HexIndex[] { center, hex_left, hex_right, hex_down_left, hex_down_right, hex_up_left, hex_up_right,
                                          sublimationIrises[0], sublimationIrises[1], sublimationIrises[2], sublimationIrises[3], sublimationIrises[4], sublimationIrises[5] },
            field_1551 = Permissions.None,
            field_1552 = true,
            CustomPermissionCheck = perms => perms.Contains("PrimaCyclia:Sublimation")
        };

        QApi.AddPartTypeToPanel(Sublimation, false);
        QApi.AddPuzzlePermission("PrimaCyclia:Sublimation", "Glyph of Sublimation", "Prima Cyclia");
        QApi.AddPartType(Sublimation, static (part, pos, editor, renderer) =>
        {
            PartSimState pss = editor.method_507().method_481(part);

            class_236 class_424 = editor.method_1989(part, pos);
            class_195 class_425 = new class_195(class_424.field_1984, class_424.field_1985, Editor.method_922());

            Texture tex_to_render = sublimation_base;
            renderer.method_521(tex_to_render, tex_to_render.method_691() + new Vector2(1f, 1f));
			renderer.method_529(class_238.field_1989.field_90.field_239.field_285, new HexIndex(0, 0), new Vector2(1f, 1f));
			renderer.method_529(class_238.field_1989.field_90.field_239.field_284, new HexIndex(0, 0), new Vector2(1f, 1f));
			renderer.method_529(class_238.field_1989.field_90.field_239.field_286, new HexIndex(0, 0), new Vector2(1f, 1f));
			renderer.method_529(class_238.field_1989.field_90.field_239.field_287, new HexIndex(0, 0), new Vector2(1f, 1f));
			class_256[] field_17 = class_238.field_1989.field_90.field_241;
			int num13 = 0;
			if (editor.method_503() != enum_128.Stopped)
			{
				num13 = (int)(new struct_27(Time.Now().Ticks).method_603() * 30f) % field_17.Length;
			}
			class_135.method_272(field_17[num13], class_425.field_1797 + new Vector2(-62f, -62f));

            DynamicData dyn_pss = new(pss);
            object stateOb = dyn_pss.Get("sublimation_state");
            SublimeState is_open = new SublimeState(IrisMode.open, -1);

            if (stateOb is not null) {
                is_open = (SublimeState)stateOb;
            }
            else { dyn_pss.Set("sublimation_state", is_open); }

            class_236 uco = editor.method_1989(part, pos);
            float time = editor.method_504();

            int irisFrame = 15;

            if (is_open.mode == IrisMode.opening)
            {
                irisFrame = class_162.method_404((int)(class_162.method_411(1f, -1f, time) * 16f), 0, 15);
                if (time > 0.5f) {irisFrame = 0;}
            }
            if (is_open.mode == IrisMode.closing)
            {
                irisFrame = class_162.method_404((int)(class_162.method_411(1f, -1f, 1-time) * 16f), 0, 15);
   
            }
            if (is_open.mode == IrisMode.open) {irisFrame = 0;}
            if (is_open.mode == IrisMode.closed) {irisFrame = 15;}

            renderer.method_529(irisAnim[irisFrame], center, Vector2.Zero);

            for (int i = 0; i < 6; i++) {

                int outputIrisFrame = 15;
                bool afterIrisOpens = false;

                HexIndex h = sublimationIrises[i];
                Vector2 location = uco.field_1984 + class_187.field_1742.method_492(h).Rotated(uco.field_1985);

                bool renderRisingAtom = pss.field_2744 is not null && pss.field_2744.Length > i;

                Molecule risingAtom = Molecule.method_1121(renderRisingAtom ? pss.field_2744[i] : Atoms.Sulfur);

                renderer.method_530(class_238.field_1989.field_90.field_164, h, 3f);

                if (pss.field_2743 && renderRisingAtom)
                {
                    outputIrisFrame = class_162.method_404((int)(class_162.method_411(1f, -1f, time) * 16f), 0, 15);
                    afterIrisOpens = time > 0.5f;
                    if (!afterIrisOpens)
                    {
                        // show atom rising behind iris
                        Editor.method_925(risingAtom, location, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                    }
                }
                renderer.method_529(sublimation_iris_anim[i][outputIrisFrame], h, Vector2.Zero);
                renderer.method_528(irisRing, h, Vector2.Zero);
                if (pss.field_2743 && afterIrisOpens)
                {
                    // show atom rising infront of iris
                    Editor.method_925(risingAtom, location, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                }
            }

        });

        
        Empowerment = new()
        {
            field_1528 = "PrimaCyclia-empowerment", // ID
            field_1529 = class_134.method_253("Glyph of Empowerment", string.Empty), // Name
            field_1530 = class_134.method_253("The glyph of empowerment consumes atoms of oxygen to unlock the full potential of a glyph of triplex-bonding, allowing any two atoms to be triplex bonded.\n\nIn turn, once all three parts of a triplex bond have been made, an atom of Phlogiston is produced, and the bonding glyph will once again be limited in its power.", string.Empty), // Description
            field_1531 = 10, // Cost
            field_1539 = true, // Is a glyph
            field_1549 = class_238.field_1989.field_97.field_372, // Shadow/glow
            field_1550 = class_238.field_1989.field_97.field_373, // Stroke/outline
            field_1547 = class_235.method_615("textures/PrimaCyclia/parts/icons/empowerment"), // Panel icon
            field_1548 = class_235.method_615("textures/PrimaCyclia/parts/icons/empowerment_hover"), // Hovered panel icon
            field_1540 = new HexIndex[] { hex_left, hex_right, hex_up_left, hex_up_right },
            field_1551 = Permissions.None,
            CustomPermissionCheck = perms => perms.Contains("PrimaCyclia:Empowerment")
        };

        QApi.AddPartTypeToPanel(Empowerment, false);
        QApi.AddPuzzlePermission("PrimaCyclia:Empowerment", "Glyph of Empowerment", "Prima Cyclia");
        QApi.AddPartType(Empowerment, static (part, pos, editor, renderer) =>
        {
        
            PartSimState pss = editor.method_507().method_481(part);
            class_236 uco = editor.method_1989(part, pos);
            float time = editor.method_504();

            bool renderMesh = true;
            foreach (Part other in editor.method_502().field_3919)
            {
                if (other.method_1159() == prismabonder)
                {
                    if (isEmpowered(part,other))
                    {
                        renderMesh = false;
                    }
                }
            }

            if (renderMesh)
            {
                renderer.method_521(empowerment_mesh, empowerment_mesh.method_691() + new Vector2(1f, 1f));
            }


			renderer.method_521(empowerment_base, empowerment_base.method_691() + new Vector2(1f, 1f));
            renderer.method_521(empowerment_overlay, empowerment_overlay.method_691() + new Vector2(1f, 1f));

            DynamicData dyn_pss = new(pss);
            object stateOb = dyn_pss.Get("empowerment_state");
            EmpowerState emp = new EmpowerState(new bool[] {false,false,false});

            if (stateOb is not null) {
                emp = (EmpowerState)stateOb;
            }
            else { dyn_pss.Set("empowerment_state", emp); }

            for (int i = 0; i < 3; i++)
            {
                if (emp.canBond[i]) {renderer.method_521(empowerment_lights[i], empowerment_lights[i].method_691() + new Vector2(1f, 1f));}
            }

			renderer.method_530(class_238.field_1989.field_90.field_164, hex_up_right, 3f);
			renderer.method_530(class_238.field_1989.field_90.field_164, hex_up_left, 3f);

            renderer.method_528(class_238.field_1989.field_90.field_255.field_293, hex_up_left, Vector2.Zero); //input bowl 1

            renderer.method_529(oxygen_input, hex_up_left, Vector2.Zero);

            int irisFrame = 15;
            bool afterIrisOpens = false;
            Molecule risingAtom = Molecule.method_1121(Atoms.Phlogiston);

            Vector2 risingOffset = uco.field_1984 + class_187.field_1742.method_492(hex_up_right).Rotated(uco.field_1985);

            if (pss.field_2743)
            {
                irisFrame = class_162.method_404((int)(class_162.method_411(1f, -1f, time) * 16f), 0, 15);
                afterIrisOpens = time > 0.5f;
                if (!afterIrisOpens)
                {
                    // show atom rising behind iris
                    Editor.method_925(risingAtom, risingOffset, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
                }
            }
            renderer.method_529(phlog_iris[irisFrame], hex_up_right, Vector2.Zero);
            renderer.method_528(irisRing, hex_up_right, Vector2.Zero);
            if (pss.field_2743 && afterIrisOpens)
            {
                // show atom rising infront of iris
                Editor.method_925(risingAtom, risingOffset, new HexIndex(0, 0), 0f, 1f, time, 1f, false, null);
            }
        });

        QApi.RunAfterCycle((sim, first) =>
        {

            SolutionEditorBase seb = sim.field_3818;
            Dictionary<Part, PartSimState> pss = sim.field_3821;
            List<Part> parts = seb.method_502().field_3919;

            Maybe<AtomReference> maybeFindAtom(Part part, HexIndex hex, List<Part> list, bool checkWheels = false)
            {
                return (Maybe<AtomReference>)PrimaCyclia.PrivateMethod<Sim>("method_1850").Invoke(sim, new object[] { part, hex, list, checkWheels });
            }

            foreach (Part part in parts)
            {
                PartType type = part.method_1159();

                if (type == Primality) 
                {

                    if (sim.FindAtomRelative(part, hex_right).method_99(out AtomReference converted) && converted.field_2280 == Atoms.salt){
                        if (maybeFindAtom(part, center, new List<Part>(), true).method_99(out AtomReference check) && primeAtoms.Contains(check.field_2280))
                        {
                            Brimstone.API.ChangeAtom(converted, check.field_2280);
                            converted.field_2279.field_2276 = new class_168(seb, (enum_7)0, (enum_132)0, converted.field_2280, class_238.field_1989.field_81.field_612, 30f);
                            Vector2 param_5374 = class_187.field_1742.method_492(part.method_1161() + new HexIndex(1, 0).Rotated(part.method_1163()));
                            seb.field_3935.Add(new class_228(seb, (enum_7)1, param_5374, class_238.field_1989.field_90.field_256, 30f, Vector2.Zero, part.method_1163().ToRadians()));
                            Brimstone.API.PlaySound(sim, PrimalitySound);
                        }
                    }
                }
                else if (type == Neutrality)
                {

                    if (first && !pss[part].field_2743)
                    {
                        if (sim.FindAtomRelative(part, hex_left).method_99(out AtomReference atom_1) && !atom_1.field_2281 && !atom_1.field_2282 &&
                            sim.FindAtomRelative(part, hex_right).method_99(out AtomReference atom_2) && !atom_2.field_2281 && !atom_2.field_2282 &&
                            ((atom_1.field_2280 == Atoms.Sulfur && atom_2.field_2280 == Brimstone.API.VanillaAtoms.quicksilver) ||
                             (atom_2.field_2280 == Atoms.Sulfur && atom_1.field_2280 == Brimstone.API.VanillaAtoms.quicksilver))
                            )
                        {
                            if (!sim.FindAtomRelative(part, center).method_1085())
                            {
                                Brimstone.API.RemoveAtom(atom_1);
                                Brimstone.API.DrawFallingAtom(seb, atom_1);
                                Brimstone.API.RemoveAtom(atom_2);
                                Brimstone.API.DrawFallingAtom(seb, atom_2);
                                pss[part].field_2743 = true;
                                Brimstone.API.AddSmallCollider(sim, part, center);
                                Brimstone.API.PlaySound(sim, NeutralitySound);
                            }
                        }
                    }
                    else if (pss[part].field_2743)
                    {
                        Brimstone.API.AddAtom(sim, part, center, Brimstone.API.VanillaAtoms.salt);
                    }
                }
                else if (type == Cyclicity)
                {

                    if (first && !pss[part].field_2743)
                    {
                        if (sim.FindAtomRelative(part, hex_up_right).method_99(out AtomReference atom_1) && !atom_1.field_2281 && !atom_1.field_2282 && primeAtoms.Contains(atom_1.field_2280))
                        {
                            if (!sim.FindAtomRelative(part, hex_down_right).method_1085())
                            {
                                Brimstone.API.RemoveAtom(atom_1);
                                Brimstone.API.DrawFallingAtom(seb, atom_1);
                                pss[part].field_2743 = true;
                                Brimstone.API.AddSmallCollider(sim, part, hex_down_right);
                                Brimstone.API.PlaySound(sim, CyclicitySound);

                                pss[part].field_2744 = new AtomType[] { GetPrime(primeAtoms.IndexOf(atom_1.field_2280) - 1) };
                            }
                        }
                    }
                    else if (pss[part].field_2743)
                    {
                        Brimstone.API.AddAtom(sim, part, hex_down_right, pss[part].field_2744[0]);
                    }
                }
                else if (type == Polarity) 
                {

                    if (sim.FindAtomRelative(part, hex_left).method_99(out AtomReference input) && input.field_2280 == Atoms.salt && !input.field_2281 && !input.field_2282){
                        if (maybeFindAtom(part, hex_right, new List<Part>(), true).method_99(out AtomReference transmuted) && primeAtoms.Contains(transmuted.field_2280))
                        {
                            Brimstone.API.RemoveAtom(input);
                            Brimstone.API.DrawFallingAtom(seb, input);
                            Brimstone.API.ChangeAtom(transmuted, GetPrime(primeAtoms.IndexOf(transmuted.field_2280) + 2));
                            transmuted.field_2279.field_2276 = new class_168(seb, (enum_7)0, (enum_132)0, transmuted.field_2280, class_238.field_1989.field_81.field_612, 30f);
                            Vector2 param_5374 = class_187.field_1742.method_492(part.method_1161() + new HexIndex(1, 0).Rotated(part.method_1163()));
                            seb.field_3935.Add(new class_228(seb, (enum_7)1, param_5374, class_238.field_1989.field_90.field_256, 30f, Vector2.Zero, part.method_1163().ToRadians()));
                            Brimstone.API.PlaySound(sim, PolaritySound);
                        }
                    }
                }
                else if (type == Combustion) 
                {
                    if (first && !pss[part].field_2743)
                    {
                        if (sim.FindAtomRelative(part, center).method_99(out AtomReference atom_1) && !atom_1.field_2281 && !atom_1.field_2282 &&
                            sim.FindAtomRelative(part, hex_right).method_99(out AtomReference atom_2) && !atom_2.field_2281 && !atom_2.field_2282 && (
                                (atom_1.field_2280 == Atoms.Sulfur && atom_2.field_2280 == Atoms.Sulfur) ||
                                (atom_1.field_2280 != atom_2.field_2280 && volatileAtoms.Contains(atom_1.field_2280) && volatileAtoms.Contains(atom_2.field_2280))
                            ))
                        {
                            if (!sim.FindAtomRelative(part, hex_down_right).method_1085() && !sim.FindAtomRelative(part, hex_up_right).method_1085())
                            {

                                AtomType[] produced_atoms;
                                if (atom_1.field_2280 == atom_2.field_2280)
                                {
                                    produced_atoms = new AtomType[] { Atoms.Phlogiston, Atoms.Oxygen };
                                }
                                else
                                {
                                    AtomType to_create = Atoms.Sulfur;
                                    foreach (AtomType a in volatileAtoms)
                                    {
                                        if (a != atom_1.field_2280 && a != atom_2.field_2280)
                                        {
                                            to_create = a;
                                        }
                                    }
                                    produced_atoms = new AtomType[] { to_create, to_create };
                                }

                                pss[part].field_2744 = produced_atoms;

                                Brimstone.API.RemoveAtom(atom_1);
                                Brimstone.API.RemoveAtom(atom_2);
                                Brimstone.API.DrawFallingAtom(seb, atom_1);
                                Brimstone.API.DrawFallingAtom(seb, atom_2);
                                pss[part].field_2743 = true;
                                Brimstone.API.AddSmallCollider(sim, part, hex_down_right);
                                Brimstone.API.AddSmallCollider(sim, part, hex_up_right);

                                if (produced_atoms[0] == produced_atoms[1])
                                {
                                    if (produced_atoms[0] == Atoms.Phlogiston) { Brimstone.API.PlaySound(sim, CombustionPhlogRelease); }
                                    else if (produced_atoms[0] == Atoms.Phlogiston) { Brimstone.API.PlaySound(sim, CombustionOxygenRelease); }
                                    else { Brimstone.API.PlaySound(sim, CombustionBothRelease); }
                                }
                                else
                                {
                                    Brimstone.API.PlaySound(sim, CombustionBothRelease);
                                }

                            }
                        }
                    }
                    else if (pss[part].field_2743)
                    {
                        Brimstone.API.AddAtom(sim, part, hex_down_right, pss[part].field_2744[0]);
                        Brimstone.API.AddAtom(sim, part, hex_up_right, pss[part].field_2744[1]);
                    }
                }
                else if (type == Sublimation)
                {

                    PartSimState sublim_pss = pss[part];

                    DynamicData dyn_pss = new(sublim_pss);
                    object stateOb = dyn_pss.Get("sublimation_state");
                    SublimeState is_open = new SublimeState(IrisMode.open, -1);

                    if (stateOb is not null) {
                        is_open = (SublimeState)stateOb;
                    }
                    else { dyn_pss.Set("sublimation_state", is_open); }

                    bool hexes_covered = false;
                    foreach (HexIndex h in sublimationIrises)
                    { if (sim.FindAtomRelative(part, h).method_99(out AtomReference a)) { hexes_covered = true; } }
                    if (sim.FindAtomRelative(part, center).method_99(out AtomReference positioned) && !sublimationDict.ContainsKey(positioned.field_2280)) { hexes_covered = true; }

                    int current_cycle = sim.method_1818();
                    if (is_open.cycle_modified < current_cycle && first) {
                        if (hexes_covered && is_open.mode == IrisMode.closing) {is_open.mode = IrisMode.closed; is_open.cycle_modified = current_cycle;}
                        if (hexes_covered && is_open.mode != IrisMode.closed) {is_open.mode = IrisMode.closing; is_open.cycle_modified = current_cycle;}
                        if (!hexes_covered && is_open.mode == IrisMode.opening) {is_open.mode = IrisMode.open; is_open.cycle_modified = current_cycle;}
                        if (!hexes_covered && is_open.mode != IrisMode.open) {is_open.mode = IrisMode.opening; is_open.cycle_modified = current_cycle;}
                    }

                    if (is_open.mode == IrisMode.open && first && !sublim_pss.field_2743)
                    {
                        if (sim.FindAtomRelative(part, center).method_99(out AtomReference input) && !input.field_2281 && !input.field_2282)
                        {
                            Brimstone.API.RemoveAtom(input);
                            Brimstone.API.DrawFallingAtom(seb, input);
                            int weight = 1;
                            if (sublimationDict.ContainsKey(input.field_2280)) {weight = sublimationDict[input.field_2280];}
                            Brimstone.API.PlaySound(sim, SublimationSound);

                            if (weight > 0) {
                                is_open.mode = IrisMode.closing; is_open.cycle_modified = current_cycle;
                                sublim_pss.field_2744 = new AtomType[weight];     
                                for (int i = 0; i < weight; i++)
                                {
                                    HexIndex h = sublimationIrises[i];
                                    Brimstone.API.AddSmallCollider(sim, part, h);
                                    sublim_pss.field_2744[i] = Atoms.Sulfur;
                                }
                                Brimstone.API.PlaySound(sim, SublimationProduction[(weight-1)/2]);
                                sublim_pss.field_2743 = true;
                            }
                        }
                    }
                    else if (sublim_pss.field_2743)
                    {

                        for (int i = 0; i < sublim_pss.field_2744.Length; i++)
                        {
                            HexIndex h = sublimationIrises[i];
                            Brimstone.API.AddAtom(sim, part, h, sublim_pss.field_2744[i]);
                        }
                    }
                    dyn_pss.Set("sublimation_state", is_open);
                }

                else if (type == Empowerment)
                {

                    DynamicData dyn_pss = new(pss[part]);
                    object stateOb = dyn_pss.Get("empowerment_state");
                    EmpowerState emp = new EmpowerState(new bool[] {false,false,false});

                    if (stateOb is not null) {
                        emp = (EmpowerState)stateOb;
                    }
                    else { dyn_pss.Set("empowerment_state", emp); }

                    if (first && !pss[part].field_2743 && emp.IsEmpty())
                    {
                        if (!emp.containsCharge) {
                            if (sim.FindAtomRelative(part, hex_up_left).method_99(out AtomReference atom_1) && !atom_1.field_2281 && !atom_1.field_2282 && atom_1.field_2280 == Atoms.Oxygen)
                            {
                                emp.canBond = new bool[] {true, true, true};
                                emp.containsCharge = true;
                                Brimstone.API.RemoveAtom(atom_1);
                                Brimstone.API.DrawFallingAtom(seb, atom_1);
                                Brimstone.API.PlaySound(sim, EmpowermentConsume);
                            }
                        }
                        else
                        {
                            emp.containsCharge = false;
                            pss[part].field_2743 = true;
                            Brimstone.API.PlaySound(sim, EmpowermentProduce);
                            Brimstone.API.AddSmallCollider(sim, part, hex_up_right);
                            
                        }
                    }
                    else if (pss[part].field_2743)
                    {
                        Brimstone.API.AddAtom(sim, part, hex_up_right, Atoms.Phlogiston);
                    }

                    foreach (Part other in parts)
                    {
                        if (other.method_1159() == prismabonder)
                        {
                            if (isEmpowered(part,other))
                            {
                                class_222[] bonder_set = prismabonder.field_1538;
                                for (int index = 0; index < bonder_set.Length; index++)
                                {
                                    class_222 bonder = bonder_set[index];
                                    if (!sim.FindAtomRelative(other, bonder.field_1920).method_99(out AtomReference leftAtom) || !sim.FindAtomRelative(other, bonder.field_1921).method_99(out AtomReference rightAtom))
                                    {
                                        continue;
                                    }
                                    if (emp.canBond[index])
                                    {
                                        Brimstone.API.JoinMolecules(sim, leftAtom.field_2277, rightAtom.field_2277);
                                        Brimstone.API.SuccessInfo did_use_bond = Brimstone.API.AddBond(sim, other, bonder.field_1920, bonder.field_1921, bonder.field_1922, true, false);

                                        if (did_use_bond == Brimstone.API.SuccessInfo.success) {
                                            Brimstone.API.PlaySound(sim, EmpowermentBonds[index]);
                                            emp.canBond[index] = false;
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
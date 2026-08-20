using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
using Quintessential.Settings;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Linq;
//using System.Reflection;

using Texture = class_256;

namespace PrimaCyclia {

	public class PrimaCyclia : QuintessentialMod {
    public static MethodInfo PrivateMethod<T>(string method) => typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
	

	public static string FilePath = "";
	public static QuintessentialMod MainClassAsMod;

	public static Vector2 hexGraphicalOffset(HexIndex hex) => class_187.field_1742.method_492(hex);
        public static bool findModMetaFilepath(string name, out string filepath)
        {
            filepath = "<missing mod directory>";
            foreach (ModMeta mod in QuintessentialLoader.Mods)
            {
                if (mod.Name == name)
                {
                    filepath = mod.PathDirectory;
                    return true;
                }
            }
            return false;
        }

		public override void Load()
        {

            string name = "PrimaCyclia";
            foreach (ModMeta mod in QuintessentialLoader.Mods)
            {
                if (mod.Name == name)
                {
                    FilePath = mod.PathDirectory;
                    break;
                }
            }
    		MainClassAsMod = this;
        }

        public override void LoadPuzzleContent()
        {
            Atoms.AddAtomTypes();
            Parts.LoadSounds();
            Parts.AddPartTypes();

            IL.SolutionEditorBase.method_1984 += drawWheelAtoms;
        }
        private static void drawWheelAtoms(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            // skip ahead to roughly where method_2015 is called
            cursor.Goto(658);

            // jump ahead to just after the method_2015 for-loop
            if (!cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Ldarga_S))) return;

            // load the SolutionEditorBase self and the class423 local onto the stack so we can use it
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_0);
            // then run the new code
            cursor.EmitDelegate<Action<SolutionEditorBase, SolutionEditorBase.class_423>>((seb_self, class423) =>
            {
                if (seb_self.method_503() != enum_128.Stopped)
                {
                    var partList = seb_self.method_502().field_3919;
                    foreach (var servin in partList.Where(x => x.method_1159() == Parts.Tolomeo))
                    {
                        Parts.drawTolomeoAtoms(seb_self, servin, class423.field_3959);
                    }
                }
            });
        }

		public override void Unload()
        {
            Parts.Unload();
        }

		public override void PostLoad()
        {
            On.SolutionEditorBase.method_1997 += DrawPartSelectionGlows;
        }

        public void DrawPartSelectionGlows(On.SolutionEditorBase.orig_method_1997 orig, SolutionEditorBase seb_self, Part part, Vector2 pos, float alpha)
        {
            if (part.method_1159() == Parts.Tolomeo) Parts.drawSelectionGlow(seb_self, part, pos, alpha);
            orig(seb_self, part, pos, alpha);
        }
	}
}

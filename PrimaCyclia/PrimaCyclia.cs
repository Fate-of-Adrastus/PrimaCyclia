﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using Quintessential;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PrimaCyclia;

public class PrimaCyclia : QuintessentialMod {
    public static PrimaCyclia Instance { get; }
    public static Quintessential.Logging.Logger Logger => Instance.Log;
    public override string ModId => "prima_cyclia";
    
    public override void Load() { }

    public override void LoadContent() {
        Atoms.AddAtomTypes();
        Parts.LoadSounds();
        Parts.AddPartTypes();

        IL.SolutionEditorBase.RenderSolution += drawWheelAtoms;
    }
    public override void LoadCompatContent() { }
    public override void FinaliseContent() { }
    public override void PostLoad() {
        On.SolutionEditorBase.RenderGlow += DrawPartSelectionGlows;
    }
    public override void Unload() {
        Parts.Unload();
    }

    public static MethodInfo PrivateMethod<T>(string method) => typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
    private static void drawWheelAtoms(ILContext il) {
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
            if (seb_self.GetSimPlayState() != SimPlayState.Stopped) {
                var partList = seb_self.GetSolution().parts;
                foreach (var servin in partList.Where(x => x.GetType() == Parts.Tolomeo)) {
                    Parts.drawTolomeoAtoms(seb_self, servin, class423.boardOffset);
                }
            }
        });
    }
    public void DrawPartSelectionGlows(On.SolutionEditorBase.orig_RenderGlow orig, SolutionEditorBase seb_self, Part part, Vector2 pos, float alpha) {
        if (part.GetType() == Parts.Tolomeo) Parts.drawSelectionGlow(seb_self, part, pos, alpha);
        orig(seb_self, part, pos, alpha);
    }

    public static Texture[] GetAnimation(string containingFolder, string frameBaseName, int frameCount, int padding = 4) {
        Texture[] anim = new Texture[frameCount];
        for (int i = 0; i < frameCount; i++) {
            anim[i] = AssetLoaderHelper.LoadTexture(Path.Combine(containingFolder, frameBaseName) + "_" + (i + 1).ToString().PadLeft(padding, '0'));
        }
        return anim;
    }
}

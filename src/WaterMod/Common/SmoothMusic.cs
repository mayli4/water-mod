using Daybreak.Common.Features.Hooks;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using NVorbis;
using System;
using System.Reflection;
using Terraria.Audio;

namespace WaterMod.Common;

internal sealed class SmoothMusic
{
    private static int _previousMusic;
    private static long _previousSamplePosition;

    [UsedImplicitly]
    [OnLoad(Side = ModSide.Client)]
    static void SubmitPatch()
    {
        // This patch makes music tracks play subsequently, providing smooth transition between different songs.
        IL_OGGAudioTrack.PrepareBufferToSubmit += il =>
        {
            try
            {
                var c = new ILCursor(il);

                if (!c.TryGotoNext(i => i.MatchCallOrCallvirt(typeof(OGGAudioTrack).GetMethod("ApplyTemporaryBufferTo", BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException())))
                {
                    return;
                }

                c.Index--;

                c.Emit(OpCodes.Ldarg, 0);
                c.Emit(OpCodes.Ldfld, typeof(OGGAudioTrack).GetField("_vorbisReader", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException());

                c.EmitDelegate((VorbisReader reader) =>
                {

                    if (Main.curMusic != _previousMusic && _previousSamplePosition < reader.TotalSamples)
                    {
                        reader.SamplePosition = _previousSamplePosition;
                    }

                    _previousMusic = Main.curMusic;
                    _previousSamplePosition = reader.SamplePosition;
                });
            }
            catch (Exception)
            {
                MonoModHooks.DumpIL(ModContent.GetInstance<ModImpl>(), il);
            }
        };
    }

}
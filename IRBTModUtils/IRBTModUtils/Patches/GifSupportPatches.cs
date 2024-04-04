using BattleTech.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BattleTech.Save.Core.ThreadedSaveManagerRequest;
using UnityEngine;
using System.IO;

namespace IRBTModUtils.Patches
{
    [HarmonyPatch(typeof(DataManager.SpriteLoadRequest))]
    [HarmonyPatch("SpriteFromDisk")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(string) })]
    public static class SpriteLoadRequest_SpriteFromDisk
    {
        public static void Prefix(ref bool __runOriginal, DataManager.SpriteLoadRequest __instance, string assetPath, ref Sprite __result)
        {
            try
            {
                if (!File.Exists(assetPath)) { __result = null; __runOriginal = false; return; }
                try
                {
                    byte[] numArray = File.ReadAllBytes(assetPath);
                    Mod.Log.Info?.Write($"DataManager.SpriteLoadRequest.SpriteFromDisk:{__instance.resourceId}:{assetPath} isGIF:{numArray.IsGIF()}");
                    Texture2D texture2D = null;
                    if (numArray.IsGIF())
                    {
                        try
                        {
                            UniGif.GifImage gif = UniGif.GetTexturesList(numArray);
                            Mod.Log.Info?.Write($" frames:{gif.frames.Count} loop:{gif.loopCount} size:{gif.width}x{gif.height}");
                            gif.Register(__instance.resourceId);
                            UniGif.GifSprites gifSprites = new UniGif.GifSprites(gif);
                            gifSprites.Register(__instance.resourceId);
                            texture2D = gif.frames.Count > 0 ? gif.frames[0].m_texture2d : new Texture2D(1, 1);
                        }
                        catch (Exception e)
                        {
                            Mod.Log.Error?.Write(assetPath);
                            Mod.Log.Error?.Write(e.ToString());
                            __instance.dataManager.logger.LogException(e);
                            __result = null; __runOriginal = false; return;
                        }
                    }
                    else
                    {
                        if (TextureManager.IsDDS(numArray))
                            texture2D = TextureManager.LoadTextureDXT(numArray);
                        else if (TextureManager.IsPNG(numArray) || TextureManager.IsJPG(numArray))
                        {
                            texture2D = new Texture2D(2, 2, TextureFormat.DXT5, false);
                            if (!texture2D.LoadImage(numArray))
                            {
                                __result = null; __runOriginal = false; return;
                            }
                        }
                        else
                        {
                            Mod.Log.Error?.Write($"Unable to load unknown file type from disk (not DDS, PNG, or JPG) at: {assetPath}");
                            __result = null; __runOriginal = false; return;
                        }
                    }
                    __result = Sprite.Create(texture2D, new UnityEngine.Rect(0.0f, 0.0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f), 100f, 0U, SpriteMeshType.FullRect, Vector4.zero);
                    __runOriginal = false; return;
                }
                catch (Exception ex)
                {
                    Mod.Log.Error?.Write($"Unable to load image at: {assetPath}\nExceptionMessage:\n{ex.Message}");
                    __instance.dataManager.logger.LogException(ex);
                    __result = null; __runOriginal = false; return;
                }
            }
            catch (Exception e)
            {
                __instance.dataManager.logger.LogException(e);
                return;
            }
        }
    }

    [HarmonyPatch(typeof(TextureManager))]
    [HarmonyPatch("ProcessCompletedRequest")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(TextureManager.TextureLoadRequest) })]
    public static class TextureManager_ProcessCompletedRequest
    {
        public static void Prefix(ref bool __runOriginal, TextureManager __instance, TextureManager.TextureLoadRequest completed)
        {
            try
            {
                if (__runOriginal == false) { return; }
                Mod.Log.Info?.Write($"TextureManager.ProcessCompletedRequest:{completed.loadRequest.Id}:{completed.loadRequest.Path}");
                string message = "";
                if (completed.loadRequest.IsError(out message))
                {
                    string error = string.Format("Failed to load texture {0} with error {1}", (object)completed.loadRequest.Id, (object)message);
                    Mod.Log.Error?.Write(error);
                    if (completed.error == null) { __runOriginal = false;  return; }
                    completed.error(error);
                }
                else
                {
                    Texture2D texture;
                    if (__instance.loadedTextures.ContainsKey(completed.loadRequest.Id))
                    {
                        Mod.Log.Info?.Write($" cached");
                        texture = __instance.loadedTextures[completed.loadRequest.Id];
                    }
                    else
                    {
                        Mod.Log.Info?.Write($" non cached");
                        byte[] data = completed.loadRequest.GetBytes();
                        Mod.Log.Info?.Write($" is GIF:{data.IsGIF()}");
                        if (data.IsGIF())
                        {
                            try
                            {
                                UniGif.GifImage gif = UniGif.GetTexturesList(data);
                                Mod.Log.Info?.Write($" frames:{gif.frames.Count} loop:{gif.loopCount} size:{gif.width}x{gif.height}");
                                gif.Register(completed.loadRequest.Id);
                                texture = gif.frames.Count > 0 ? gif.frames[0].m_texture2d : new Texture2D(1, 1);
                            }
                            catch (Exception e)
                            {
                                Mod.Log.Error?.Write($"{completed.loadRequest.Id}:{completed.loadRequest.Path}");
                                Mod.Log.Error?.Write(e.ToString());
                                texture = TextureManager.TextureFromBytes(data);
                            }
                        }
                        else
                        {
                            texture = TextureManager.TextureFromBytes(data);
                        }
                        __instance.loadedTextures.Add(completed.loadRequest.Id, texture);
                    }
                    if (completed.callback == null) { __runOriginal = false; return; }
                    Mod.Log.Info?.Write($" completed.callback");
                    completed.callback(texture);
                }
                __runOriginal = false; return;
            }
            catch (Exception e)
            {
                Mod.Log.Error?.Write(e.ToString());
                return;
            }
        }
    }
}

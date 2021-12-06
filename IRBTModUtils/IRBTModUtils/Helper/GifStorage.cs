using BattleTech;
using System.Collections.Generic;
using UnityEngine;

namespace IRBTModUtils {
  public static class GifStorageHelper {
    private static Dictionary<string, UniGif.GifImage> gifImages = new Dictionary<string, UniGif.GifImage>();
    private static Dictionary<string, UniGif.GifSprites> gifSprites = new Dictionary<string, UniGif.GifSprites>();
    //private static Dictionary<int, string> imageDataId = new Dictionary<int, string>();
    public static UniGif.GifSprites GetPortraitGifSprite(this Pilot pilot) {
      if (gifSprites.TryGetValue(pilot.pilotDef.Description.Icon, out var result)) {
        return result;
      }
      return null;
    }
    public static UniGif.GifImage GetPortraitGifImage(this BaseDescriptionDef descr) {
      if (gifImages.TryGetValue(descr.Icon, out var result)) {
        return result;
      }
      return null;
    }
    public static bool IsGIF(this byte[] data) {
      return (data[0] == 0x47) && (data[1] == 0x49) && (data[2] == 0x46);
    }
    public static void Register(this UniGif.GifImage gif, string id) {
      gifImages.Add(id, gif);
    }
    public static void Register(this UniGif.GifSprites gif, string id) {
      gifSprites.Add(id, gif);
    }
    //public static void Register(this Sprite sprite, string id) {
    //  imageDataId.Add(sprite.GetInstanceID(), id);
    //}
    //public static void Register(this Texture2D texture, string id) {
    //  imageDataId.Add(texture.GetInstanceID(), id);
    //}
  }
}
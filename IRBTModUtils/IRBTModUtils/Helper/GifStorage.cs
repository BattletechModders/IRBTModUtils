using BattleTech;
using System.Collections.Generic;
using UnityEngine;

namespace IRBTModUtils {
  public static class GifStorageHelper {
    private static Dictionary<string, UniGif.GifImage> gifImages = new Dictionary<string, UniGif.GifImage>();
    private static Dictionary<string, UniGif.GifSprites> gifSprites = new Dictionary<string, UniGif.GifSprites>();
    //private static Dictionary<int, string> imageDataId = new Dictionary<int, string>();
    public static UniGif.GifSprites GetPortraitGifSprite(this Pilot pilot) {
      if (pilot == null) { return null; }
      if (pilot.pilotDef.PortraitSettings != null) {
        if (pilot.pilotDef.PortraitSettings.Description != null) {
          if (string.IsNullOrEmpty(pilot.pilotDef.PortraitSettings.Description.Icon) == false) {
            if (gifSprites.TryGetValue(pilot.pilotDef.PortraitSettings.Description.Icon, out var result)) {
              return result;
            }
          }
        }
      }
      if (pilot.pilotDef.Description != null) {
        if (string.IsNullOrEmpty(pilot.pilotDef.Description.Icon) == false) {
          if (gifSprites.TryGetValue(pilot.pilotDef.Description.Icon, out var result)) {
            return result;
          }
        }
      }
      return null;
    }
    public static UniGif.GifSprites GetSprites(string id){
      if (string.IsNullOrEmpty(id) == false) {
        if (gifSprites.TryGetValue(id, out var result)) {
          return result;
        }
      }
      return null;
    }
    public static UniGif.GifSprites GetPortraitGifSprite(this PilotDef pilotDef) {
      if (pilotDef.PortraitSettings != null) {
        if (pilotDef.PortraitSettings.Description != null) {
          if (string.IsNullOrEmpty(pilotDef.PortraitSettings.Description.Icon) == false) {
            if (gifSprites.TryGetValue(pilotDef.PortraitSettings.Description.Icon, out var result)) {
              return result;
            }
          }
        }
      }
      if (pilotDef.Description != null) {
        if (string.IsNullOrEmpty(pilotDef.Description.Icon) == false) {
          if (gifSprites.TryGetValue(pilotDef.Description.Icon, out var result)) {
            return result;
          }
        }
      }
      return null;
    }
    public static UniGif.GifImage GetPortraitGifImage(this Pilot pilot) {
      if (pilot == null) { return null; }
      if (pilot.pilotDef.PortraitSettings != null) {
        if (pilot.pilotDef.PortraitSettings.Description != null) {
          if (string.IsNullOrEmpty(pilot.pilotDef.PortraitSettings.Description.Icon) == false) {
            if (gifImages.TryGetValue(pilot.pilotDef.PortraitSettings.Description.Icon, out var result)) {
              return result;
            }
          }
        }
      }
      if (pilot.pilotDef.Description != null) {
        if (string.IsNullOrEmpty(pilot.pilotDef.Description.Icon) == false) {
          if (gifImages.TryGetValue(pilot.pilotDef.Description.Icon, out var result)) {
            return result;
          }
        }
      }
      return null;
    }
    public static UniGif.GifImage GetPortraitGifImage(this PilotDef pilotDef) {
      if (pilotDef.PortraitSettings != null) {
        if (pilotDef.PortraitSettings.Description != null) {
          if (string.IsNullOrEmpty(pilotDef.PortraitSettings.Description.Icon) == false) {
            if (gifImages.TryGetValue(pilotDef.PortraitSettings.Description.Icon, out var result)) {
              return result;
            }
          }
        }
      }
      if (pilotDef.Description != null) {
        if (string.IsNullOrEmpty(pilotDef.Description.Icon) == false) {
          if (gifImages.TryGetValue(pilotDef.Description.Icon, out var result)) {
            return result;
          }
        }
      }
      return null;
    }
    public static UniGif.GifImage GetDescrGifImage(this BaseDescriptionDef descr) {
      if (string.IsNullOrEmpty(descr.Icon) == false) {
        if (gifImages.TryGetValue(descr.Icon, out var result)) {
          return result;
        }
      }
      return null;
    }
    public static UniGif.GifSprites GetDescrGifSprite(this BaseDescriptionDef descr) {
      if (string.IsNullOrEmpty(descr.Icon) == false) {
        if (gifSprites.TryGetValue(descr.Icon, out var result)) {
          return result;
        }
      }
      return null;
    }
    public static bool IsGIF(this byte[] data) {
      return (data[0] == 0x47) && (data[1] == 0x49) && (data[2] == 0x46);
    }
    public static void Register(this UniGif.GifImage gif, string id) {
      if (gifImages.ContainsKey(id) == false) {
        gifImages.Add(id, gif);
      } else {
        gifImages[id] = gif;
      }
    }
    public static void Register(this UniGif.GifSprites gif, string id) {
      if (gifSprites.ContainsKey(id) == false) {
        gifSprites.Add(id, gif);
      } else {
        gifSprites[id] = gif;
      }
    }
    //public static void Register(this Sprite sprite, string id) {
    //  imageDataId.Add(sprite.GetInstanceID(), id);
    //}
    //public static void Register(this Texture2D texture, string id) {
    //  imageDataId.Add(texture.GetInstanceID(), id);
    //}
  }
}
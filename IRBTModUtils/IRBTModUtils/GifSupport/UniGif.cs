/*
UniGif
Copyright (c) 2015 WestHillApps (Hironari Nishioka)
This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IRBTModUtils {
  public static partial class UniGif {
    public class GifSprite {
      // Texture
      public Sprite m_sprite;
      // Delay time until the next texture.
      public float m_delaySec;

      public GifSprite(Texture2D texture2D, float delaySec) {
        m_sprite = Sprite.Create(texture2D, new UnityEngine.Rect(0.0f, 0.0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f), 100f, 0U, SpriteMeshType.FullRect, Vector4.zero);
        m_delaySec = delaySec;
      }
    }

    public class GifSprites {
      public List<GifSprite> frames { get; set; } = new List<GifSprite>();
      public int loopCount { get; set; } = -1;
      public int width { get; set; } = 0;
      public int height { get; set; } = 0;
      public GifSprites(GifImage img) {
        this.loopCount = img.loopCount;
        this.width = img.width;
        this.height = img.height;
        foreach(var frame in img.frames) {
          this.frames.Add(new GifSprite(frame.m_texture2d, frame.m_delaySec));
        }
      }
    }
    public class GifImage {
      public List<GifTexture> frames { get; set; } = new List<GifTexture>();
      public int loopCount { get; set; } = -1;
      public int width { get; set; } = 0;
      public int height { get; set; } = 0;
    }
    /// <summary>
    /// Get GIF texture list Coroutine
    /// </summary>
    /// <param name="bytes">GIF file byte data</param>
    /// <param name="callback">Callback method(param is GIF texture list, Animation loop count, GIF image width (px), GIF image height (px))</param>
    /// <param name="filterMode">Textures filter mode</param>
    /// <param name="wrapMode">Textures wrap mode</param>
    /// <param name="debugLog">Debug Log Flag</param>
    /// <returns>IEnumerator</returns>
    public static GifImage GetTexturesList(byte[] bytes, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp, bool debugLog = false) {
      GifImage image = new GifImage();

      // Set GIF data
      var gifData = new GifData();
      if (SetGifData(bytes, ref gifData, debugLog) == false) {
        Mod.Log.Error?.Write("GIF file data set error.");
        //Log.TWL(0,"GIF file data set error.",true);
        return image;
      }

      // Decode to textures from GIF data
      List<GifTexture> gifTexList = DecodeTexture(gifData, filterMode, wrapMode);

      if (gifTexList == null || gifTexList.Count <= 0) {
        //Log.TWL(0, "GIF texture decode error.",true);
        Mod.Log.Error?.Write("GIF texture decode error.");
        return image;
      }
      image.frames = gifTexList;
      image.loopCount = gifData.m_appEx.loopCount;
      image.width = gifData.m_logicalScreenWidth;
      image.height = gifData.m_logicalScreenHeight;


      return image;
    }
  }
}
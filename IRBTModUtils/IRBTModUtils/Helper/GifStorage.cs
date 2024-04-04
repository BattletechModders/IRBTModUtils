using BattleTech;
using System.Collections.Generic;
using UnityEngine;
using static BattleTech.Save.Core.ThreadedSaveManagerRequest;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IRBTModUtils
{
    public class GifSpriteAnimatorHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GifSpriteAnimator imageAnimator = null;
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (imageAnimator == null) { return; }
            imageAnimator.OnPointerEnter(eventData);
        }
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (imageAnimator == null) { return; }
            imageAnimator.OnPointerExit(eventData);
        }
    }
    public abstract class IHoverAnimated : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool hovered = false;
        public bool alwaysAnimate = false;
        protected float t = 0f;
        protected int index = 0;
        public virtual void Reset() { }
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            this.Reset();
        }
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            this.Reset();
            this.OnUnhover();
        }
        public virtual void OnUnhover()
        {

        }
    }
    public class GifSpriteAnimator : IHoverAnimated
    {
        public UniGif.GifSprites gif { get; set; }
        public Image portrait = null;
        public Sprite origSprite = null;
        public void SetPortrait(Image p, bool useOrigSprite)
        {
            if (useOrigSprite) { origSprite = p.sprite; } else { origSprite = null; }
            portrait = p;
        }
        public override void OnUnhover()
        {
            if ((origSprite != null) && (portrait != null))
            {
                portrait.sprite = origSprite;
            }
        }
        public override void Reset()
        {
            index = 0;
            t = 0f;
            if (portrait == null) { return; }
            if (gif == null) { return; }
            if (gif.frames.Count == 0) { return; }
            portrait.sprite = gif.frames[0].m_sprite;
        }
        public void LateUpdate()
        {
            if (portrait == null) { return; }
            if (gif == null) { return; }
            if (gif.frames.Count == 0) { return; }
            if ((hovered == false) && (alwaysAnimate == false)) { return; }
            t += Time.deltaTime;
            if (t > gif.frames[index].m_delaySec)
            {
                this.index = (this.index + 1) % gif.frames.Count;
                t = 0f;
                portrait.sprite = gif.frames[this.index].m_sprite;
            }
        }
    }
    public class GifImageAnimator : IHoverAnimated
    {
        public UniGif.GifImage gif { get; set; }
        public RawImage portrait { get; set; } = null;
        public Texture origImage = null;
        public void SetPortrait(RawImage p, bool useOrigSprite)
        {
            if (useOrigSprite) { origImage = p.texture; } else { origImage = null; }
            portrait = p;
        }
        public override void OnUnhover()
        {
            if ((origImage != null) && (portrait != null))
            {
                portrait.texture = origImage;
            }
        }
        public override void Reset()
        {
            index = 0;
            t = 0f;
            if (portrait == null) { return; }
            if (gif == null) { return; }
            if (gif.frames.Count == 0) { return; }
            portrait.texture = gif.frames[0].m_texture2d;
        }
        public void LateUpdate()
        {
            if (portrait == null) { return; }
            if (gif == null) { return; }
            if (gif.frames.Count == 0) { return; }
            if ((hovered == false) && (alwaysAnimate == false)) { return; }
            t += Time.deltaTime;
            if (t > gif.frames[index].m_delaySec)
            {
                this.index = (this.index + 1) % gif.frames.Count;
                t = 0f;
                portrait.texture = gif.frames[this.index].m_texture2d;
            }
        }
    }
    public static class GifStorageHelper
    {
        private static Dictionary<string, UniGif.GifImage> gifImages = new Dictionary<string, UniGif.GifImage>();
        private static Dictionary<string, UniGif.GifSprites> gifSprites = new Dictionary<string, UniGif.GifSprites>();
        //private static Dictionary<int, string> imageDataId = new Dictionary<int, string>();
        public static UniGif.GifSprites GetPortraitGifSprite(this Pilot pilot)
        {
            if (pilot == null) { return null; }
            if (pilot.pilotDef.PortraitSettings != null)
            {
                if (pilot.pilotDef.PortraitSettings.Description != null)
                {
                    if (string.IsNullOrEmpty(pilot.pilotDef.PortraitSettings.Description.Icon) == false)
                    {
                        if (gifSprites.TryGetValue(pilot.pilotDef.PortraitSettings.Description.Icon, out var result))
                        {
                            return result;
                        }
                    }
                }
            }
            if (pilot.pilotDef.Description != null)
            {
                if (string.IsNullOrEmpty(pilot.pilotDef.Description.Icon) == false)
                {
                    if (gifSprites.TryGetValue(pilot.pilotDef.Description.Icon, out var result))
                    {
                        return result;
                    }
                }
            }
            return null;
        }
        public static UniGif.GifSprites GetSprites(string id)
        {
            if (string.IsNullOrEmpty(id) == false)
            {
                if (gifSprites.TryGetValue(id, out var result))
                {
                    return result;
                }
            }
            return null;
        }
        public static UniGif.GifSprites GetPortraitGifSprite(this PilotDef pilotDef)
        {
            if (pilotDef.PortraitSettings != null)
            {
                if (pilotDef.PortraitSettings.Description != null)
                {
                    if (string.IsNullOrEmpty(pilotDef.PortraitSettings.Description.Icon) == false)
                    {
                        if (gifSprites.TryGetValue(pilotDef.PortraitSettings.Description.Icon, out var result))
                        {
                            return result;
                        }
                    }
                }
            }
            if (pilotDef.Description != null)
            {
                if (string.IsNullOrEmpty(pilotDef.Description.Icon) == false)
                {
                    if (gifSprites.TryGetValue(pilotDef.Description.Icon, out var result))
                    {
                        return result;
                    }
                }
            }
            return null;
        }
        public static UniGif.GifImage GetPortraitGifImage(this Pilot pilot)
        {
            if (pilot == null) { return null; }
            if (pilot.pilotDef.PortraitSettings != null)
            {
                if (pilot.pilotDef.PortraitSettings.Description != null)
                {
                    if (string.IsNullOrEmpty(pilot.pilotDef.PortraitSettings.Description.Icon) == false)
                    {
                        if (gifImages.TryGetValue(pilot.pilotDef.PortraitSettings.Description.Icon, out var result))
                        {
                            return result;
                        }
                    }
                }
            }
            if (pilot.pilotDef.Description != null)
            {
                if (string.IsNullOrEmpty(pilot.pilotDef.Description.Icon) == false)
                {
                    if (gifImages.TryGetValue(pilot.pilotDef.Description.Icon, out var result))
                    {
                        return result;
                    }
                }
            }
            return null;
        }
        public static UniGif.GifImage GetPortraitGifImage(this PilotDef pilotDef)
        {
            if (pilotDef.PortraitSettings != null)
            {
                if (pilotDef.PortraitSettings.Description != null)
                {
                    if (string.IsNullOrEmpty(pilotDef.PortraitSettings.Description.Icon) == false)
                    {
                        if (gifImages.TryGetValue(pilotDef.PortraitSettings.Description.Icon, out var result))
                        {
                            return result;
                        }
                    }
                }
            }
            if (pilotDef.Description != null)
            {
                if (string.IsNullOrEmpty(pilotDef.Description.Icon) == false)
                {
                    if (gifImages.TryGetValue(pilotDef.Description.Icon, out var result))
                    {
                        return result;
                    }
                }
            }
            return null;
        }
        public static UniGif.GifImage GetDescrGifImage(this BaseDescriptionDef descr)
        {
            if (string.IsNullOrEmpty(descr.Icon) == false)
            {
                if (gifImages.TryGetValue(descr.Icon, out var result))
                {
                    return result;
                }
            }
            return null;
        }
        public static UniGif.GifSprites GetDescrGifSprite(this BaseDescriptionDef descr)
        {
            if (string.IsNullOrEmpty(descr.Icon) == false)
            {
                if (gifSprites.TryGetValue(descr.Icon, out var result))
                {
                    return result;
                }
            }
            return null;
        }
        public static bool IsGIF(this byte[] data)
        {
            return (data[0] == 0x47) && (data[1] == 0x49) && (data[2] == 0x46);
        }
        public static void Register(this UniGif.GifImage gif, string id)
        {
            if (gifImages.ContainsKey(id) == false)
            {
                gifImages.Add(id, gif);
            }
            else
            {
                gifImages[id] = gif;
            }
        }
        public static void Register(this UniGif.GifSprites gif, string id)
        {
            if (gifSprites.ContainsKey(id) == false)
            {
                gifSprites.Add(id, gif);
            }
            else
            {
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
using System.Collections.Generic;
using UnityEngine;

namespace PatternSurvivor.Infrastructure
{
    public static class RuntimeSpriteLibrary
    {
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        public static Sprite GetCircle(string key, Color color, int size = 32)
        {
            string cacheKey = $"circle:{key}:{ColorUtility.ToHtmlStringRGBA(color)}";
            if (Sprites.TryGetValue(cacheKey, out Sprite cached))
            {
                return cached;
            }

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = cacheKey
            };
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.46f;
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    pixels[y * size + x] = distance <= radius ? color : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = cacheKey;
            Sprites[cacheKey] = sprite;
            return sprite;
        }

        public static Sprite GetSquare(string key, Color color, int size = 16)
        {
            string cacheKey = $"square:{key}:{ColorUtility.ToHtmlStringRGBA(color)}";
            if (Sprites.TryGetValue(cacheKey, out Sprite cached))
            {
                return cached;
            }

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = cacheKey
            };
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = cacheKey;
            Sprites[cacheKey] = sprite;
            return sprite;
        }
    }
}

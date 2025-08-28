#if UNITY_EDITOR

using UnityEngine;
using TMPro;
using UnityEngine.TextCore;

public class TestScript : MonoBehaviour
{
    public TMP_SpriteAsset rootAsset;

    public float BX, BY;
    public bool setFallback;

    private void OnValidate()
    {
        if(setFallback)
        {
            setFallback = false;

            foreach(TMP_SpriteAsset spriteAsset in rootAsset.fallbackSpriteAssets)
            {
                if(spriteAsset == null)
                    continue;
                if (spriteAsset.name == "keyboard" || spriteAsset.name == "gamepad")
                    continue;

                foreach(TMP_SpriteCharacter character in spriteAsset.spriteCharacterTable)
                {
                    GlyphMetrics glyphMetrics = character.glyph.metrics;
                    glyphMetrics.horizontalBearingX = BX;
                    glyphMetrics.horizontalBearingY = BY;
                    character.glyph.metrics = glyphMetrics;
                }
            }
        }
    }
}

#endif

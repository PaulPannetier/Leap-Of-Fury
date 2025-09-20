#if UNITY_EDITOR

using UnityEngine;
using TMPro;
using UnityEngine.TextCore;

public class SetTmpIconBxBy : MonoBehaviour
{
    public TMP_SpriteAsset[] spriteAssets;

    public float BX, BY;
    public bool setFallback;

    private void OnValidate()
    {
        if (setFallback)
        {
            setFallback = false;

            foreach (TMP_SpriteAsset spriteAsset in spriteAssets)
            {
                if (spriteAsset == null)
                    continue;
                if (spriteAsset.name == "keyboard" || spriteAsset.name == "gamepad")
                    continue;

                foreach (TMP_SpriteCharacter character in spriteAsset.spriteCharacterTable)
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

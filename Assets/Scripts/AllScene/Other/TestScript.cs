using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] int seed;
    [SerializeField] Vector2 scale;
    [SerializeField] bool recalculateTexture = false;

    private void Start()
    {
        Material material = GetComponent<SpriteRenderer>().material;

        Texture2D texture = new Texture2D(500, 500);
        Random.SetSeed(seed);
        Array2D<float> textValue = Random.CellsNoise(500, 500, scale);
        for (int x = 0; x < 500; x++)
        {
            for (int y = 0; y < 500; y++)
            {
                texture.SetPixel(x, y, Color.Lerp(Color.white, Color.black, textValue[x, y]));
            }
        }
        texture.Apply();

        material.mainTexture = texture;
    }

    private void OnValidate()
    {
        if (recalculateTexture)
            Start();
        recalculateTexture = false;
    }
}

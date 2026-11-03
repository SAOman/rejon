using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class QuadrantBiomeGenerator : MonoBehaviour
{
    public Terrain terrain;

    [Header("Ustawienia Przejść (Blend)")]
    public float transitionWidth = 0.02f;

    [Header("Nieregularność Granic (Szum)")]
    public float noiseScale = 15f;
    public float noiseStrength = 0.2f;

    [Header("Kształt Terenu (Wysokości)")]
    [Tooltip("Główny mnożnik wysokości całego terenu")]
    public float heightMultiplier = 0.2f;
    [Tooltip("Wysokość łagodnych pagórków na trawie")]
    public float grassHeight = 0.3f;
    [Tooltip("Wysokość wydm na pustyni")]
    public float desertHeight = 0.15f;
    [Tooltip("Ilość wydm na pustyni")]
    public float desertDuneFrequency = 40f;
    [Tooltip("Wysokość ostrych skał w górach")]
    public float mountainHeight = 1.0f;

    [ContextMenu("Generuj Teren (Tekstury + Kształt)")]
    public void GenerateTerrain()
    {
        if (terrain == null) terrain = GetComponent<Terrain>();

        GenerateSplatmap();
        GenerateHeights();
        Debug.Log("Wygenerowano tekstury i rzeźbę terenu!");
    }

    // Funkcja obliczająca te same granice dla tekstur i dla kształtu terenu
    private float[] GetBiomeWeights(float nx, float ny)
    {
        float noise1 = Mathf.PerlinNoise(nx * noiseScale, ny * noiseScale) - 0.5f;
        float noise2 = Mathf.PerlinNoise(nx * noiseScale * 2f, ny * noiseScale * 2f) * 0.5f;
        float finalNoiseX = (noise1 + noise2) * noiseStrength;

        float noise3 = Mathf.PerlinNoise(nx * noiseScale + 100f, ny * noiseScale + 100f) - 0.5f;
        float noise4 = Mathf.PerlinNoise(nx * noiseScale * 2f + 100f, ny * noiseScale * 2f + 100f) * 0.5f;
        float finalNoiseY = (noise3 + noise4) * noiseStrength;

        float warpedX = nx + finalNoiseX;
        float warpedY = ny + finalNoiseY;

        float minStep = 0.5f - transitionWidth;
        float maxStep = 0.5f + transitionWidth;

        float weightRight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(minStep, maxStep, warpedX));
        float weightLeft = 1f - weightRight;

        float weightTop = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(minStep, maxStep, warpedY));
        float weightBottom = 1f - weightTop;

        return new float[] {
            weightLeft * weightBottom,   // 0: Trawa
            weightRight * weightBottom,  // 1: Pustynia
            weightLeft * weightTop,      // 2: Zima
            weightRight * weightTop      // 3: Góry
        };
    }

    private void GenerateSplatmap()
    {
        TerrainData terrainData = terrain.terrainData;
        if (terrainData.terrainLayers.Length < 4) return;

        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;
        float[,,] splatmapData = new float[height, width, terrainData.terrainLayers.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float[] weights = GetBiomeWeights((float)x / width, (float)y / height);
                splatmapData[y, x, 0] = weights[0];
                splatmapData[y, x, 1] = weights[1];
                splatmapData[y, x, 2] = weights[2];
                splatmapData[y, x, 3] = weights[3];
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    private void GenerateHeights()
    {
        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;
        float[,] heightData = new float[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = (float)x / (width - 1);
                float ny = (float)y / (height - 1);

                float[] weights = GetBiomeWeights(nx, ny);

                // --- 1. BIOM TRAWY (Łagodne pagórki) ---
                float grassShape = Mathf.PerlinNoise(nx * 8f, ny * 8f) * grassHeight;

                // --- 2. BIOM PUSTYNI (Wydmy) ---
                // Zniekształcamy falę szumem, aby wydmy nie były idealnie proste
                float duneDistortion = Mathf.PerlinNoise(nx * 10f, ny * 10f) * 0.1f;
                // Sinusoida tworzy efekt piaszczystych fal
                float desertShape = (Mathf.Sin((nx + duneDistortion) * desertDuneFrequency) + 1f) * 0.5f * desertHeight;
                // Drobny szum na powierzchni piasku
                desertShape += Mathf.PerlinNoise(nx * 30f, ny * 30f) * 0.02f;

                // --- 3. BIOM ZIMOWY (Lekko pofalowany teren) ---
                float winterShape = Mathf.PerlinNoise(nx * 5f, ny * 5f) * 0.4f;

                // --- 4. BIOM GÓRSKI (Ostre szczyty) ---
                // Potęgowanie szumu sprawia, że doliny są płaskie, a szczyty strzeliste
                float mountainBase = Mathf.PerlinNoise(nx * 12f, ny * 12f);
                float mountainShape = Mathf.Pow(mountainBase, 2.5f) * mountainHeight;

                // Miksowanie rzeźby terenu z wagami biomów, aby zachować płynne przejścia
                float finalHeight =
                    (grassShape * weights[0]) +
                    (desertShape * weights[1]) +
                    (winterShape * weights[2]) +
                    (mountainShape * weights[3]);

                heightData[y, x] = finalHeight * heightMultiplier;
            }
        }
        terrainData.SetHeights(0, 0, heightData);
    }
}
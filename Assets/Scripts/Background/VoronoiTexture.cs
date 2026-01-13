using UnityEngine;

public class VoronoiTexture : MonoBehaviour
{
    [Header("Voronoi Settings")]
    [SerializeField] private int PointPerCells = 15;
    [SerializeField] private int GridSize = 256;

    [Header("Effects")]
    [SerializeField] private float Emission = 1f;
    [SerializeField] private Gradient colorRamp;
    
    [Header("Animation")]
    [SerializeField] private bool enableAnimation = false;
    [SerializeField] private float animationSpeed = 1f;
    [SerializeField] private float movementRadius = 0.1f;
    
    [Header("Parallax")]
    [SerializeField] private bool enableParallax = false;
    [SerializeField] private float parallaxFactor = 0.1f;
    [SerializeField] private Transform playerTransform;
    
    [SerializeField] private Texture2D texture;
    [SerializeField] private Vector2[] points;
    private Material voronoiMaterial;
    
    // Variables pour détecter les changements
    private int lastPointPerCells;
    private int lastGridSize;
    private float lastEmission;
     
    void Start()
    {
        GenerateDiagram();
        SaveCurrentValues();
    }

    void Update()
    {
        // Animation
        if (enableAnimation) VoronoiAnimate();
        
        // Parallax
        if (enableParallax) ParrallaxTexture();
        
        // Changements de paramètres
        if (ParametersChanged())
        {
            GenerateDiagram();
            SaveCurrentValues();
        }
    }
    
    bool ParametersChanged()
    {
        return PointPerCells != lastPointPerCells ||
               GridSize != lastGridSize ||
               Emission != lastEmission;
    }
    
    void SaveCurrentValues()
    {
        lastPointPerCells = PointPerCells;
        lastGridSize = GridSize;
        lastEmission = Emission;
    }

    void GenerateDiagram()
    {
        GeneratePoints();
        
        texture = new Texture2D(GridSize, GridSize);
        
        // Générer texture
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                Vector2 uv = new Vector2((float)x / GridSize, (float)y / GridSize);
                float minDist = float.MaxValue;
                
                for (int i = 0; i < PointPerCells; i++)
                    minDist = Mathf.Min(minDist, Vector2.Distance(uv, points[i]));
                
                float t = 1 - minDist * 2;
                Color c = colorRamp.Evaluate(t);
                
                texture.SetPixel(x, y, c);
            }
        }
        
        texture.Apply();
        ApplyEmission();
    }

    void GeneratePoints()
    {
        // Créer des points qui se répètent naturellement
        points = new Vector2[PointPerCells];
        
        for (int i = 0; i < PointPerCells; i++)
        {
            // Générer des points dans une grille plus grande
            // pour éviter les répétitions visibles
            float x = (float)i / PointPerCells + Random.Range(-0.1f, 0.1f);
            float y = Random.value;
            
            points[i] = new Vector2(x % 1f, y % 1f);
        }
    }

    void ApplyEmission()
    {
        Renderer renderer = GetComponent<Renderer>();
        
        voronoiMaterial = new Material(Shader.Find("Standard"));
        voronoiMaterial.mainTexture = texture;
        voronoiMaterial.EnableKeyword("_EMISSION");
        voronoiMaterial.SetColor("_EmissionColor", colorRamp.Evaluate(0.5f) * Emission);
        voronoiMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        renderer.material = voronoiMaterial;
    }

    void VoronoiAnimate()
    {
        if (!enableAnimation) return;
        
        // Déplacer les points doucement
        for (int i = 0; i < PointPerCells; i++)
        {
            float time = Time.time * animationSpeed;
            float offsetX = Mathf.Sin(time + i) * movementRadius;
            float offsetY = Mathf.Cos(time * 1.3f + i) * movementRadius;
            
            points[i] = new Vector2(
                (points[i].x + offsetX * Time.deltaTime) % 1f,
                (points[i].y + offsetY * Time.deltaTime) % 1f
            );
        }
        
        UpdateTextureFromPoints();
    }

    void UpdateTextureFromPoints()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                Vector2 uv = new Vector2((float)x / GridSize, (float)y / GridSize);
                float minDist = float.MaxValue;
                
                for (int i = 0; i < PointPerCells; i++)
                    minDist = Mathf.Min(minDist, Vector2.Distance(uv, points[i]));
                
                float t = 1 - minDist * 2;
                Color c = colorRamp.Evaluate(t);
                
                texture.SetPixel(x, y, c);
            }
        }
        
        texture.Apply();
    }

    void ParrallaxTexture()
    {
        if (!enableParallax) return;
        
        // Trouver le joueur si besoin
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else return;
        }
        
        // Calculer l'offset
        Vector2 offset = new Vector2(
            playerTransform.position.x * parallaxFactor,
            playerTransform.position.z * parallaxFactor
        );
        
        // Appliquer avec wrap mode pour seamless
        if (voronoiMaterial != null)
        {
            voronoiMaterial.mainTextureOffset = offset;
            voronoiMaterial.mainTexture.wrapMode = TextureWrapMode.Mirror;
        }
    }

    void UpdatePointsToAnimate()
    {
        // Simple : régénérer les points
        for (int i = 0; i < PointPerCells; i++)
        {
            points[i] = new Vector2(Random.value, Random.value);
        }
    }
}

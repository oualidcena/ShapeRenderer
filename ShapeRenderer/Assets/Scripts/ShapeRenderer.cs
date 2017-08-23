﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Evan Pezent | evanpezent.com | epezent@rice.edu
// 08/2017

[ExecuteInEditMode]
public class ShapeRenderer : MonoBehaviour
{
    //-------------------------------------------------------------------------
    // SHAPE FILL APPEARANCE
    //-------------------------------------------------------------------------

    public enum FillType { Solid, LinearGradient, RadialGradient, Custom };

    [SerializeField]
    [Tooltip("Enables/disables shape fill.")]
    private bool fill_ = true;
    public  bool fill
    {
        get { return fill_; }
        set
        {
            if (fill_ != value)
            {
                fill_ = value;
                updateFill = true;
            }
        }
    }

    [SerializeField]
    [Tooltip("The fill type to use.")]
    public FillType fillType_ = FillType.Solid;
    public FillType fillType
    {
        get { return fillType_; }
        set
        {
            if (fillType_ != value)
            {
                fillType_ = value;
                updateFill = true;
            }
        }
    }

    [Tooltip("The fill color of the shape.")]
    public Color fillColor1 = Color.white;

    [Tooltip("The second fill color of the shape.")]
    public Color fillColor2 = Color.black;

    [SerializeField]
    [Tooltip("The angle at which the linear gradient is applied.")]
    [Range(0.0f, 360.0f)]
    private float fillAngle_;
    public float fillAngle
    {
        get { return fillAngle_; }
        set
        {
            if (fillAngle_ != value)
            {
                fillAngle_ = value;
                updateFill = true;
            }
        }
    }

    [Tooltip("Controls the position of the gradient along its axis axis.")]
    [Range(-1.0f, 1.0f)]
    public float slider1 = 0.0f;

    [Tooltip("Controls the position of the gradient along its second axis.")]
    [Range(-1.0f, 1.0f)]
    public float slider2 = 0.0f;

    [Tooltip("Texture to be applied to the fill (multiply).")]
    public Texture fillTexture;

    [Tooltip("Texture tiling in X and Y directions.")]
    public Vector2 fillTextrueTiling = Vector2.one;

    [Tooltip("Texture offset in X and Y directions.")]
    public Vector2 fillTextureOffset = Vector2.zero;

    [Tooltip("Custom material to be applied to the fill.")]
    public Material customFillMaterial;

    private Material fillMaterial;

    //-------------------------------------------------------------------------
    // SHAPE STROKE APPEARANCE
    //-------------------------------------------------------------------------

    public enum StrokeType { Solid, MultiGradient, Custom };

    [Tooltip("Enables/disables shape stroke.")]
    public bool stroke = false;

    [Tooltip("The stroke type to use.")]
    public StrokeType strokeType = StrokeType.Solid;

    [Tooltip("The solid color used along the stroke.")]
    public Color strokeSolid = Color.black;

    [Tooltip("The gradient describing the color along the stroke.")]
    public Gradient strokeColor;

    [Tooltip("The shape stroke width in world units.")]
    public float strokeWidth = 10;
    public Texture strokeTexture;
    public Material customStrokeMaterial;

    private Material strokeMaterial;

    //-------------------------------------------------------------------------
    // SHAPE GEOMETRY
    //-------------------------------------------------------------------------

    [Tooltip("The shape anchor points in world units, relative to this GameObject's transform.")]
    public Vector2[] shapeAnchors = new Vector2[4] { new Vector2(100, -100), new Vector2(100, 100), new Vector2(-100, 100), new Vector2(-100, -100) };

    [Tooltip("The radii, in world units, applied to corresponding shape anchor points.")]
    public float[] shapeRadii = new float[4] { 0f, 0f, 0f, 0f };

    [Tooltip("The number of line segment used to render each radius. Use as few as necessary for best performance.")]
    public int[] radiiSmoothness = new int[4] { 50, 50, 50, 50 };

    private int defaultSmoothness = 50;

    //-------------------------------------------------------------------------
    // SORTING LAYERS
    //-------------------------------------------------------------------------

    [Tooltip("The name of the ShapeRenderer's sorting layer. First add the desired sorting layer Unity's Layers dialog (top-right), then type it here.")]
    [SortingLayer]
    public int sortingLayer = 0;

    [Tooltip("The ShapeRenderer's order within a sorting layer.")]
    public int sortingOrder = 0;

    //-------------------------------------------------------------------------
    // COLLIDER
    //-------------------------------------------------------------------------

    public enum ColliderMode { Disabled, ToCollider, FromCollider }
    public enum SetColliderTo { Anchors, Vertices }

    [Tooltip("Updates the attached PolygonCollider2D to match the shape geometry. Adds a new PolygonCollider2D if none exists.")]
    public ColliderMode colliderMode = ColliderMode.Disabled;
    public SetColliderTo setColliderTo = SetColliderTo.Anchors;
    [Tooltip("Shows/hides the LineRenderer, MeshFilter, and MeshRenderer required by this ShapeRenderer. Hidden by default to reduce clutter.")]
    public bool showComponents = false;

    //-------------------------------------------------------------------------
    // UPDATE FLAGS
    //-------------------------------------------------------------------------

    bool updateGeometry = false;
    bool updateFill = false;
    bool updateStroke = false;
    bool updateCollider = false;

    //-------------------------------------------------------------------------
    // COMPONENET HANDLES
    //-------------------------------------------------------------------------

    private LineRenderer lr;
    private MeshFilter mf;
    private MeshRenderer mr;
    private MaterialPropertyBlock mpb_fill;
    private MaterialPropertyBlock mpb_stroke;
    private PolygonCollider2D pc2d;   

    //-------------------------------------------------------------------------
    // MONOBEHAVIOR CALLBACKS
    //-------------------------------------------------------------------------

    void Awake()
    {
        // Add required Unity components and check ranges
        ValidateComponents();
        ValidateValues();

        // Check for a PolgonCollider2D
        pc2d = GetComponent<PolygonCollider2D>();

        // Set default Materials
        if (fillMaterial == null)
            fillMaterial = Resources.Load("SR_FillLinearGradient") as Material;
        if (strokeMaterial == null)
            strokeMaterial = Resources.Load("SR_Stroke") as Material;

        // Set start sorting layers
        mr.sortingLayerID = sortingLayer;
        mr.sortingOrder = sortingOrder;
        mr.sortingLayerID = sortingLayer;
        mr.sortingOrder = sortingOrder;

        // Show/Hide required components
        ShowComponenets();
    }
    
    private void Start()
    {
        UpdateShapeAll();
    }

    void Update()
    {
        #if UNITY_EDITOR
        ShowComponenets();
        if (!EditorApplication.isPlaying)
            UpdateShapeAll(); // we don't want to call this every frame when the game is playing in the editor
        #endif

        if (updateGeometry)
        {
            UpdateShapeGeometry();
            updateGeometry = false;
        }
        if (updateFill)
        {
            UpdateFillAppearance();
            updateFill = false;
        }
        if (updateStroke)
        {
            UpdateStrokeAppearance();
            updateStroke = false;
        }
        if (updateCollider)
        {

        }
      

    }

    private void OnEnable()
    {
        mr.enabled = fill_;
        lr.enabled = stroke;
    }

    private void OnDisable()
    {
        mr.enabled = false;
        lr.enabled = false;
    }

    // Called when script added or inspector element is changed (in editor only)
    public void OnValidate()
    {
        ValidateComponents();
        ValidateValues();
    }

    //-------------------------------------------------------------------------
    // SHAPERENDERER DLL IMPORTS
    //-------------------------------------------------------------------------

    [DllImport("ShapeRenderer", EntryPoint = "compute_shape1")]
    private static extern int ComputeShape1(float[] anchorsX, float[] anchorsY, float[] radii, int[] N, int anchorsSize,
                                         float[] verticesX, float[] verticesY, int verticesSize,
                                         int[] indices, int indicesSize, float[] u, float[] v);

    [DllImport("ShapeRenderer", EntryPoint = "compute_shape2")]
    private static extern int ComputeShape2(float[] anchorsX, float[] anchorsY, float[] radii, int[] N, int anchorsSize,
                                         float[] verticesX, float[] verticesY, int verticesSize,
                                         int[] indices, int indicesSize, float[] u, float[] v);

    //-------------------------------------------------------------------------
    // PUBLIC FUNCTIONS
    //-------------------------------------------------------------------------



    //-------------------------------------------------------------------------
    // PRIVATE FUNCTIONS
    //-------------------------------------------------------------------------

    private void ValidateComponents()
    {
        // validate LineRenderer
        lr = GetComponent<LineRenderer>();
        if (lr == null)
            lr = gameObject.AddComponent<LineRenderer>() as LineRenderer;
        // validate MeshFilter
        mf = GetComponent<MeshFilter>();
        if (mf == null)
        {
            mf = gameObject.AddComponent<MeshFilter>() as MeshFilter;
            mf.sharedMesh = new Mesh();
            mf.sharedMesh.MarkDynamic();
        }
        // validate MeshRenderer
        mr = GetComponent<MeshRenderer>();
        if (mr == null)
            mr = gameObject.AddComponent<MeshRenderer>() as MeshRenderer;
        // validate MaterialPropertyBlock
        if (mpb_fill == null)
            mpb_fill = new MaterialPropertyBlock();
        if (mpb_stroke == null)
            mpb_stroke = new MaterialPropertyBlock();
    }

    private void ValidateValues()
    {
        if (shapeAnchors.Length < 3)
            Array.Resize(ref shapeAnchors, 3);
        if (shapeRadii.Length != shapeAnchors.Length)
        {
            Array.Resize(ref shapeRadii, shapeAnchors.Length);
            for (int i = 0; i < shapeAnchors.Length; i++)
            {
                if (shapeRadii[i] < 0.0f)
                    shapeRadii[i] = 0.0f;
                if (radiiSmoothness[i] < 1)
                    radiiSmoothness[i] = 1;
            }
        }
        if (radiiSmoothness.Length != shapeAnchors.Length)
        {
            Array.Resize(ref radiiSmoothness, shapeAnchors.Length);
            for (int i = 0; i < radiiSmoothness.Length; i++)
            {
                if (radiiSmoothness[i] == 0)
                    radiiSmoothness[i] = defaultSmoothness;
            }
        }
        if (strokeWidth < 0)
            strokeWidth = 0;
    }

    /// <summary>
    /// Shows or hides required components in the inspector
    /// </summary>
    private void ShowComponenets()
    {
        if (showComponents)
        {
            mr.hideFlags = HideFlags.None;
            mf.hideFlags = HideFlags.None;
            lr.hideFlags = HideFlags.None;
        }
        else
        {
            mr.hideFlags = HideFlags.HideInInspector;
            mf.hideFlags = HideFlags.HideInInspector;
            lr.hideFlags = HideFlags.HideInInspector;
        }
    }

    /// <summary>
    /// Enables/disables certains components depending on what shape options have been selected.
    /// </summary>
    private void UpdateEnabled()
    {
        if (fill_)
            mr.enabled = true;
        else
            mr.enabled = false;
        if (stroke)
            lr.enabled = true;
        else
            lr.enabled = false;
    }

    //-------------------------------------------------------------------------
    // PUBLIC API FUNCTIONS
    //-------------------------------------------------------------------------

    /// <summary>
    /// Generates new vertices then updates shape appearance fill and stroke.
    /// </summary>
    public void UpdateShapeAll()
    {
        UpdateShapeGeometry();
        UpdateShapeAppearance();
        UpdateEnabled();
    }

    /// <summary>
    /// Updates the shape fill and stroke geometry.
    /// </summary>
    public void UpdateShapeGeometry()
    {
        ValidateValues();

        // calculate sizes
        int anchorsSize = shapeAnchors.Length;
        int verticesSize = 0;
        for (int i = 0; i < anchorsSize; i++)
        {
            if (radiiSmoothness[i] == 0 || radiiSmoothness[i] == 1 || shapeRadii[i] <= 0.0)
                verticesSize += 1;
            else
                verticesSize += radiiSmoothness[i];
        }
        int indicesSize = (verticesSize - 2) * 3;

        // unpack Unity types containers
        float[] anchorsX = new float[shapeAnchors.Length];
        float[] anchorsY = new float[shapeAnchors.Length];
        for (int i = 0; i < anchorsSize; ++i)
        {
            anchorsX[i] = shapeAnchors[i].x;
            anchorsY[i] = shapeAnchors[i].y;
        }
        float[] verticesX = new float[verticesSize];
        float[] verticesY = new float[verticesSize];
        float[] u = new float[verticesSize];
        float[] v = new float[verticesSize];
        int[] indices = new int[indicesSize];

        // call DLL
        int result = 0;
        result = ComputeShape1(anchorsX, anchorsY, shapeRadii, radiiSmoothness, anchorsSize, verticesX, verticesY, verticesSize, indices, indicesSize, u, v);

        if (result == 1)
        {
            // repack Unity types
            Vector3[] vertices = new Vector3[verticesSize];
            Vector2[] uv = new Vector2[verticesSize];
            float z = transform.position.z;
            for (int i = 0; i < verticesSize; i++)
            {
                vertices[i] = new Vector3(verticesX[i], verticesY[i], z);
                uv[i] = new Vector2(u[i], v[i]);
            }
            UpdateFillGeometry(vertices, indices, uv);
            UpdateStrokeGeometry(vertices);
            UpdateCollider(vertices);
        }
    }

    /// <summary>
    /// Updates the shape fill geometry.
    /// </summary>
    public void UpdateFillGeometry(Vector3[] vertices, int[] indices, Vector2[] uv)
    {
        if (fill_)
        {
            Mesh mesh = mf.sharedMesh;
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.uv = uv;
        }
    }

    /// <summary>
    /// Updates the shape stroke geometry.
    /// </summary>
    public void UpdateStrokeGeometry(Vector3[] vertices)
    {
        if (stroke)
        {
            lr.loop = true;
            lr.useWorldSpace = false;
            lr.positionCount = vertices.Length;
            lr.SetPositions(vertices);
        }
        else
        {
            lr.positionCount = 0;
        }
    }

    /// <summary>
    /// Updates the shape fill and stroke appearance.
    /// </summary>
    void UpdateShapeAppearance()
    {
        UpdateFillAppearance();
        UpdateStrokeAppearance();
    }

    /// <summary>
    /// Updates the shape fill appearance.
    /// </summary>
    public void UpdateFillAppearance()
    {
        if (fill_)
        {
            if (fillType_ != FillType.Custom)
            {
                if (fillTexture != null)
                {
                    mpb_fill.SetTexture("_MainTex", fillTexture);
                    mpb_fill.SetVector("_TileOff", new Vector4(fillTextrueTiling.x, fillTextrueTiling.y, fillTextureOffset.x, fillTextureOffset.y));
                }
                else
                    mpb_fill.Clear();

                if (fillType_ == FillType.Solid)
                {
                    fillMaterial = Resources.Load("SR_FillLinearGradient") as Material;
                    mpb_fill.SetColor("_Color1", fillColor1);
                    mpb_fill.SetColor("_Color2", fillColor1);
                }
                else if (fillType_ == FillType.LinearGradient)
                {
                    fillMaterial = Resources.Load("SR_FillLinearGradient") as Material;
                    mpb_fill.SetColor("_Color1", fillColor1);
                    mpb_fill.SetColor("_Color2", fillColor2);
                }
                else if (fillType_ == FillType.RadialGradient)
                {
                    fillMaterial = Resources.Load("SR_FillRadialGradient") as Material;
                    mpb_fill.SetColor("_Color1", fillColor1);
                    mpb_fill.SetColor("_Color2", fillColor2);

                }
                mpb_fill.SetFloat("_Angle", fillAngle_);
                mpb_fill.SetFloat("_Slider1", slider1);
                mpb_fill.SetFloat("_Slider2", slider2);
                mr.SetPropertyBlock(mpb_fill);
                mr.sharedMaterial = fillMaterial;
            }
            else
            {
                mpb_fill.Clear();
                mr.SetPropertyBlock(null);
                mr.sharedMaterial = customFillMaterial;
            }
            mr.sortingLayerID = sortingLayer;
            mr.sortingOrder = sortingOrder;
        }
        else
        {
            mr.sharedMaterial = null;
        }
    }

    /// <summary>
    /// Updates the shape stroke appearance.
    /// </summary>
    public void UpdateStrokeAppearance()
    {
        if (stroke)
        {
            if (strokeType != StrokeType.Custom)
            {
                if (strokeTexture != null)
                {
                    mpb_stroke.SetTexture("_MainTex", strokeTexture);
                }
                else
                    mpb_stroke.Clear();
                
                if (strokeType == StrokeType.Solid)
                {
                    GradientColorKey[] gck = new GradientColorKey[2];
                    GradientAlphaKey[] gak = new GradientAlphaKey[2];

                    gck[0].color = strokeSolid; gck[0].time = 0.0f;
                    gck[1].color = strokeSolid; gck[1].time = 1.0f;
                    gak[0].alpha = strokeSolid.a; gak[0].time = 0.0f;
                    gak[1].alpha = strokeSolid.a; gak[1].time = 1.0f;

                    Gradient g = new Gradient();
                    g.SetKeys(gck, gak);

                    lr.colorGradient = g;
                } 
                else if (strokeType == StrokeType.MultiGradient)
                {
                    lr.colorGradient = strokeColor;
                }
                lr.SetPropertyBlock(mpb_stroke);
                strokeMaterial = Resources.Load("SR_Stroke") as Material;
                lr.sharedMaterial = strokeMaterial;
            }
            else
            {
                mpb_stroke.Clear();
                lr.SetPropertyBlock(null);
                lr.sharedMaterial = customStrokeMaterial;
            }
            lr.startWidth = strokeWidth;
            lr.endWidth = strokeWidth;
            lr.sortingLayerID = sortingLayer;
            lr.sortingOrder = sortingOrder + 1;
        }
    }

    /// <summary>
    /// Updates the attached PolygonCollider2D points if updateCollider is true. If no PC2D is attached, a new instance is created.
    /// </summary>
    public void UpdateCollider(Vector3[] vertices)
    {
        if (colliderMode == ColliderMode.ToCollider)
        {
            if (pc2d == null)
                pc2d = gameObject.AddComponent<PolygonCollider2D>() as PolygonCollider2D;
            if (setColliderTo == SetColliderTo.Anchors)
                pc2d.points = shapeAnchors;
            if (setColliderTo == SetColliderTo.Vertices)
            {                
                pc2d.points = System.Array.ConvertAll<Vector3, Vector2>(vertices, Vector3toVector2);
            }
        }
        else if (colliderMode == ColliderMode.FromCollider)
        {
            if (pc2d == null)
            {
                pc2d = gameObject.AddComponent<PolygonCollider2D>() as PolygonCollider2D;
                pc2d.points = System.Array.ConvertAll<Vector3, Vector2>(vertices, Vector3toVector2);
            }
            else
            {
                shapeAnchors = pc2d.points;
                for (int i = 0; i < pc2d.points.Length; i++)
                {
                    shapeAnchors[i] += pc2d.offset;
                }
            }
        }
    }

    //-------------------------------------------------------------------------
    // PUBLIC STATIC FUNCTIONS
    //-------------------------------------------------------------------------

    public static Vector2 Vector3toVector2(Vector3 V3)
    {
        return new Vector2(V3.x, V3.y);
    }

    public static Vector2 RotateVector2(Vector2 vector, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = vector.x;
        float ty = vector.y;
        vector.x = (cos * tx) - (sin * ty);
        vector.y = (sin * tx) + (cos * ty);
        return vector;
    }

    public static Vector2[] RotateVertices(Vector2[] vertices, float degrees)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = RotateVector2(vertices[i], degrees);
        }
        return vertices;
    }

}

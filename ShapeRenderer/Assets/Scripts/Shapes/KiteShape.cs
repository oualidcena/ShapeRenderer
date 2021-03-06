﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Evan Pezent | evanpezent.com | epezent@rice.edu
// 08/2017

public class KiteShape : Shape {

    public float width = 200;
    public float height = 200;
    public float shift = 50;
    public float cornerRadius = 0f;
    [Range(0,100)]
    public int cornerSmoothness = 50;

    public override void Draw()
    {
        float half_w = width * 0.5f;
        float half_h = height * 0.5f;
        Vector2[] shapeAnchors = new Vector2[4] {
            new Vector2(half_w, shift),
            new Vector2(0, half_h),
            new Vector2(-half_w, shift),
            new Vector2(0, -half_h)
        };
        sr.shapeAnchors = shapeAnchors;
        sr.shapeRadii = new float[4] { cornerRadius, cornerRadius, cornerRadius, cornerRadius };
        sr.radiiSmoothness = new int[4] { cornerSmoothness, cornerSmoothness, cornerSmoothness, cornerSmoothness };
    }
}

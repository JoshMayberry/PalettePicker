using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

// See: https://docs.unity3d.com/Packages/com.unity.splines@1.0/api/UnityEngine.Splines.Spline.html
public class SplineHandler : MonoBehaviour {

	public SplineContainer splineContainer;
	SplineExtrude splineExtrude;
    MeshFilter meshFilter;
    public SphereBox sphereBox;

    List<ColorSphere> connectedSpheres;
    [SerializeField] private Material splineMaterial;

    public void Awake() {
		this.connectedSpheres = new List<ColorSphere>();
		this.splineContainer = GetComponent<SplineContainer>();
		this.splineExtrude = GetComponent<SplineExtrude>();
        this.meshFilter = GetComponent<MeshFilter>();

        this.GenerateMesh();
    }

	void GenerateMesh() {
        this.meshFilter.mesh = new Mesh();
        splineExtrude.Rebuild();
    }

    public void Clear() {
		foreach (var sphere in connectedSpheres) {
			sphere.connectedSplines.Clear();
        }
		this.splineContainer.Spline.Clear();
		this.Hide();
	}

	public void Add(ColorSphere sphere) {
        this.connectedSpheres.Add(sphere);
        sphere.connectedSplines.Add(new SplineConnection(this, (int)this.splineContainer.Spline.Count));
		this.splineContainer.Spline.Add(new BezierKnot(sphere.transform.localPosition), TangentMode.AutoSmooth);
		this.Show();
    }
   
	public void UpdateTexture() {
		this.splineExtrude.Rebuild();
	}

    public void Hide() {
        this.splineExtrude.Radius = 0;
        this.gameObject.SetActive(false);
    }
    public void Show() {
        this.splineExtrude.Radius = float.Parse(SphereBox.instance.InputSplineSize.text);
        this.gameObject.SetActive(true);
        this.splineExtrude.Rebuild();
    }
}

[Serializable]
public class SplineConnection {
    public SplineHandler splineHandler;
    public int knotIndex;

    float updateTimer;
    float timeBetweenDragUpdates = 0.05f;

    public SplineConnection(SplineHandler handler, int index) {
        splineHandler = handler;
        knotIndex = index;
    }

    public void UpdateKnotPosition(Vector3 newPosition) {
        if (SphereBox.instance.isDraggingSphere) {
            if (Time.time < this.updateTimer) {
                return;
            }

            this.updateTimer = Time.time + this.timeBetweenDragUpdates;
        }

		if (this.knotIndex < 0 || (this.knotIndex >= this.splineHandler.splineContainer.Spline.Count)) {
			return;
		}

        BezierKnot knot = this.splineHandler.splineContainer.Spline[this.knotIndex];
        knot.Position = newPosition;
        this.splineHandler.splineContainer.Spline.SetKnot(this.knotIndex, knot);

		this.splineHandler.UpdateTexture();
    }
}

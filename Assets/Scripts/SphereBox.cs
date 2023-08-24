using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

// [ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class SphereBox : MonoBehaviour {
	public float box_size = 400f;
	public Color box_color = Color.white;
	Vector3 lastPosition;

	public SplineHandler splinePrefab;
	public GameObject spherePrefab;
	public FlexibleColorPicker colorPickerOffset;
	public TMP_InputField InputSphereSize;
	public TMP_InputField InputSplineSize;
	public GameObject resetButton;

	public float hueOffset;
	public float saturationOffset;
	public float valueOffset;
	public float rotationSpeed = 1f;
	Vector3 mouseOrigin;
	bool isRotating = false;
    public bool isDraggingSphere = false;

    public List<ColorSphere> sphereList;
	public List<SplineHandler> activeSplines;
	public List<SplineHandler> inactiveSplines;
	public static SphereBox instance { get; private set; }

	Vector3 centerPoint;
	Vector3 initialOrientation;

    void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one SphereBox in the scene.");
		}

		instance = this;

		this.activeSplines = new List<SplineHandler>();
		this.inactiveSplines = new List<SplineHandler>();
	}

	void Start() {
        this.centerPoint = this.transform.position + new Vector3(this.box_size / 2, this.box_size / 2, this.box_size / 2);
		this.initialOrientation = this.transform.position;

        DrawWireframeBox();
		UpdateSphereSize();
	}

	void OnValidate() {
		DrawWireframeBox();
	}

	 void Update() {
		this.HandleRotation();

		if (this.transform.position != this.lastPosition) {
			DrawWireframeBox();
			this.lastPosition = this.transform.position;
		}
	}

	private void HandleRotation() {
		if (Input.GetMouseButtonDown(2)) {
			mouseOrigin = Input.mousePosition;
			this.isRotating = true;
		}

		if (Input.GetMouseButtonUp(2)) {
			this.isRotating = false;
		}

		if (this.isRotating) {
			this.resetButton.SetActive(true);
            Vector3 delta = Input.mousePosition - mouseOrigin;
			Vector3 rotation = new Vector3(delta.y, -delta.x) * this.rotationSpeed * Time.deltaTime;

			// Move cube so that the center point is at the origin
			this.transform.position -= this.centerPoint;

			// Perform the rotation
			this.transform.RotateAround(Vector3.zero, rotation.normalized, rotation.magnitude);

			// Move the cube back to its original position
			this.transform.position += this.centerPoint;
		}
	}

	public void ResetOrientation() {
		this.transform.position = this.initialOrientation;
		this.transform.rotation = Quaternion.identity;
		this.resetButton.SetActive(false);
    }

    public void UpdateSphereSize() {
		float sphereSize;
		try {
			sphereSize = Mathf.Max(1, float.Parse(this.InputSphereSize.text));
		}
		catch (Exception error) {
			Debug.Log(error);
			sphereSize = 1;
		} 

		this.spherePrefab.transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);

		foreach (ColorSphere sphere in this.sphereList) {
			sphere.transform.localScale = this.spherePrefab.transform.localScale;
		}
	}

	public void UpdateSpherePosition() {
		this.hueOffset = this.colorPickerOffset.GetValue1D(FlexibleColorPicker.PickerType.H);
		this.saturationOffset = this.colorPickerOffset.GetValue1D(FlexibleColorPicker.PickerType.S);
		this.valueOffset = this.colorPickerOffset.GetValue1D(FlexibleColorPicker.PickerType.V);

		foreach (ColorSphere sphere in this.sphereList) {
			if (sphere.owner.isEnabled) {
				sphere.transform.localPosition = this.CalculateSpherePosition(sphere.color);
			}
		}

		this.UpdateSplinePositions();
	}

	public void UpdateSplinePositions() {
		foreach (ColorSphere sphere in this.sphereList) {
			sphere.UpdateSplinePositions();
		}
    }

	public void UpdateSplines() {
		foreach (SplineHandler spline in activeSplines) {
			spline.Clear();
			inactiveSplines.Add(spline);
		}
		activeSplines.Clear();

        if (float.Parse(this.InputSplineSize.text) == 0) {
			return;
        }

        foreach (ColorRamp ramp in ColorGrid.instance.colorRamps) {
			// Try to reuse an inactive spline, or create a new one
			SplineHandler spline;
			if (inactiveSplines.Count > 0) {
				spline = inactiveSplines[inactiveSplines.Count - 1];
				inactiveSplines.RemoveAt(inactiveSplines.Count - 1);
			}
			else {
				spline = Instantiate(splinePrefab, this.transform);
			}

			foreach (ColorSquare square in ramp) {
				spline.Add(square.sphere);
			}
			
			activeSplines.Add(spline);
		}
	}

	void DrawWireframeBox() {
		LineRenderer lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = 13;
		lineRenderer.startColor = box_color;
		lineRenderer.endColor = box_color;
		lineRenderer.startWidth = 2f;
		lineRenderer.endWidth = 2f;

		Vector3[] points = {
			new Vector3(0, 0, 0),
			new Vector3(0, 0, this.box_size),
			new Vector3(0, this.box_size, this.box_size),
			new Vector3(0, this.box_size, 0),
			new Vector3(0, 0, 0),

			new Vector3(0, this.box_size, 0),
			new Vector3(this.box_size, this.box_size, 0),
			new Vector3(this.box_size, 0, 0),
			new Vector3(0, 0, 0),

			new Vector3(this.box_size, 0, 0),
			new Vector3(this.box_size, 0, this.box_size),
			new Vector3(0, 0, this.box_size),
			new Vector3(0, 0, 0),
		};

		// Convert the points from local space to world space
		for (int i = 0; i < points.Length; i++) {
			points[i] = transform.TransformPoint(points[i]);
		}

		lineRenderer.SetPositions(points);
	}

	public Vector3 CalculateSpherePosition(Color color) {
		Color.RGBToHSV(color, out float h, out float s, out float v);

		h = (h + this.hueOffset) % 1f;
		s = (s + this.saturationOffset) % 1f;
		v = (v + this.valueOffset) % 1f;

		float x = Mathf.Lerp(0, 1, s) * this.box_size;
		float y = Mathf.Lerp(0, 1, v) * this.box_size;
		float z = Mathf.Lerp(0, 1, h) * this.box_size;

		return new Vector3(x, y, z);
	}

	public Color CalculateSphereColor(Vector3 position) {
        float x = position.x / this.box_size;
        float y = position.y / this.box_size;
        float z = position.z / this.box_size;

        float h = (z - this.hueOffset) % 1f;
        float s = (x - this.saturationOffset) % 1f;
        float v = (y - this.valueOffset) % 1f;

        h = Mathf.Clamp01(h);
        s = Mathf.Clamp01(s);
        v = Mathf.Clamp01(v);

        return Color.HSVToRGB(h, s, v);
    }
}

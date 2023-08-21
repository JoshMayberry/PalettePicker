using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// [ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class SphereBox : MonoBehaviour {
	public float box_size = 400f;
	public Color box_color = Color.white;
	Vector3 lastPosition;

	public GameObject spherePrefab;
	public FlexibleColorPicker colorPickerOffset;
	public TMP_InputField InputSphereSize;

	public float hueOffset;
	public float saturationOffset;
	public float valueOffset;

	public List<ColorSphere> sphereList;
	
	void Start() {
		DrawWireframeBox();
		UpdateSphereSize();
	}

	void OnValidate() {
		DrawWireframeBox();
	}

	// void Update() {
	// 	if (transform.position != lastPosition) {
	// 		DrawWireframeBox();
	// 		lastPosition = transform.position;
	// 	}
	// }

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
}

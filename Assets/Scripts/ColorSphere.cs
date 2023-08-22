using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class ColorSphere : MonoBehaviour {
	Renderer myRenderer;
	public ColorSquare owner;
	public Color color;

	public List<SplineConnection> connectedSplines;

	void Awake() {
		this.connectedSplines = new List<SplineConnection>();
		this.myRenderer = GetComponent<Renderer>();
	}

	public void Init(ColorSquare myOwner) {
		this.owner = myOwner;
		SphereBox.instance.sphereList.Add(this);
	}

	// Method to update the color of the sphere
	public void SetColor(Color newColor) {
		this.color = newColor;
		this.color.a = 0.75f;
		this.myRenderer.material.color = this.color;
		this.transform.localPosition = SphereBox.instance.CalculateSpherePosition(this.color);

		// Update any connected spline knots
		foreach (SplineConnection connection in this.connectedSplines) {
			connection.UpdateKnotPosition(this.transform.localPosition);
		}
	}
}



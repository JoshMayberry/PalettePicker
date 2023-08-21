using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSphere : MonoBehaviour {
	Renderer rend;
	SphereBox parent;
	public ColorSquare owner;
	public Color color;

	Vector3 minBounds = new Vector3(0, 0, 0);
	Vector3 maxBounds = new Vector3(1, 1, 1);

	void Awake() {
		rend = GetComponent<Renderer>();
	}

	public void Init(ColorSquare myOwner, SphereBox myParent) {
		this.owner = myOwner;
		this.parent = myParent;
		this.parent.sphereList.Add(this);
	}

	// Method to update the color of the sphere
	public void SetColor(Color newColor) {
		this.color = newColor;
		this.color.a = 0.75f;
		this.rend.material.color = this.color;
		this.transform.localPosition = this.parent.CalculateSpherePosition(this.color);
	}
}

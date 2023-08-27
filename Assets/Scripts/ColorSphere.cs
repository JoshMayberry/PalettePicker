using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;

public class ColorSphere : MonoBehaviour {
	Renderer myRenderer;
	public ColorSquare owner;
	public Color color;

    public bool isDragging = false;
    private float distanceFromCamera;
    public Vector3 offset;
    private Vector3 screenPosition;

    public List<SplineConnection> connectedSplines;

	void Awake() {
		this.connectedSplines = new List<SplineConnection>();
		this.myRenderer = GetComponent<Renderer>();
	}
    private void Update() {
        // On mouse down, begin dragging
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit) && hit.transform == this.transform) {
                this.isDragging = true;
                SphereBox.instance.isDraggingSphere = true;
                this.distanceFromCamera = Vector3.Distance(Camera.main.transform.position, hit.point);
                this.screenPosition = Camera.main.WorldToScreenPoint(transform.position);
            }
        }

        // On mouse up, end dragging
        if (Input.GetMouseButtonUp(0)) {
            this.isDragging = false;
            SphereBox.instance.isDraggingSphere = false;
        }

        // While dragging, update the local position
        if (this.isDragging) {
            Vector3 cursorScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);
            Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(cursorScreenPosition);
            Ray ray = new Ray(Camera.main.transform.position, cursorWorldPosition - Camera.main.transform.position);
            Vector3 newPosition = ray.GetPoint(distanceFromCamera);

            // Convert the new world position to local space of the parent object
            Vector3 localPosition = this.transform.parent.InverseTransformPoint(newPosition);

            // Now you can clamp this local position using the local bounds
            localPosition.x = Mathf.Clamp(localPosition.x, 0, SphereBox.instance.box_size);
            localPosition.y = Mathf.Clamp(localPosition.y, 0, SphereBox.instance.box_size);
            localPosition.z = Mathf.Clamp(localPosition.z, 0, SphereBox.instance.box_size);

            // Lerp to this clamped position
            float lerpSpeed = 0.15f; // Adjust this value as needed for desired smoothing
            Vector3 smoothedPosition = Vector3.Lerp(this.transform.localPosition, localPosition, lerpSpeed);

            // Convert the clamped local position back to world space
            this.SetPosition(smoothedPosition);
        }
    }

    public void SetPosition(Vector3 newPosition) {
        this.owner.PickThis();
        ColorGrid.instance.colorPicker.color = SphereBox.instance.CalculateSphereColor(newPosition);
    } 

    public void UpdateSplinePositions() {
        foreach (SplineConnection connection in this.connectedSplines) {
            connection.UpdateKnotPosition(this.transform.localPosition);
        }
    }

    public void Init(ColorSquare myOwner) {
		this.owner = myOwner;
		SphereBox.instance.sphereList.Add(this);
	}

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

    public void OnPointerClick(PointerEventData eventData) {
		this.owner.OnPointerClick(eventData);
    }
}



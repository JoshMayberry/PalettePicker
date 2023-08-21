using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorSquare : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
	Image image;
	ColorGrid parent;
	ColorSphere mySphere;
	public int grid_x;
	public int grid_y;

	public Vector2 initialPosition;
	public RectTransform rectTransform;

	public bool isEnabled = false;

	void Awake() {
		this.image = GetComponent<Image>();
		this.rectTransform = GetComponent<RectTransform>();
	}

	public void Init(ColorGrid myParent, int x, int y) {
		this.parent = myParent;
		this.grid_x = x;
		this.grid_y = y;
		this.parent.squareList[x, y] = this;

		GameObject sphereObject = Instantiate(this.parent.sphereBox.spherePrefab, this.parent.sphereBox.transform);
		this.mySphere = sphereObject.GetComponent<ColorSphere>();
		this.mySphere.Init(this, this.parent.sphereBox);

		this.ToggleEnabled(false);
	}

	public void Reset() {
		this.ToggleEnabled(false);
		this.SetColor(Color.white);
	}

	public void SetColor(Color newColor) {
		newColor.a = (this.isEnabled ? 1f : 0.5f);
		this.image.color = newColor;
		this.mySphere.SetColor(newColor);
	}

	public void SetColor(float h, float s, float v) {
		this.SetColor(Color.HSVToRGB(h / 359f, s / 100f, v / 100f));
	}

	public Color GetColor() {
		Color color = this.image.color;
		color.a = 1;
		return color;
	}

	public void ToggleEnabled(bool? enabled = null) {
		this.isEnabled = (enabled.HasValue ? enabled.Value : !this.isEnabled);
		this.mySphere.gameObject.SetActive(this.isEnabled);
		this.SetColor(this.image.color);
	}

	public void OnPointerClick(PointerEventData eventData) {
		bool doToggle = (eventData.button == PointerEventData.InputButton.Right);
		if (doToggle) {
			this.ToggleEnabled(!isEnabled);
		}

		if (isEnabled && (doToggle || (eventData.button == PointerEventData.InputButton.Left))) {
			this.parent.ShowPicker(this);
		}
	}

	public void OnBeginDrag(PointerEventData eventData) {
	}

	public void OnDrag(PointerEventData eventData) {
	}

	public void OnEndDrag(PointerEventData eventData) {
		Vector2 position = eventData.position;
		foreach (ColorSquare square in this.parent.squareList) {
			if (RectTransformUtility.RectangleContainsScreenPoint(square.GetComponent<RectTransform>(), position)) {
				this.parent.SwapColors(this, square);
				break;
			}
		}
	}
}


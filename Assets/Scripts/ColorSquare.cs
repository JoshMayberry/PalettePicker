using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorSquare : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
	Image image;
	public ColorSphere sphere;
	public int grid_x;
	public int grid_y;

	public Vector2 initialPosition;
	public RectTransform rectTransform;

	public bool isEnabled = false;

	bool isInitializing = true;

	void Awake() {
		this.image = GetComponent<Image>();
		this.rectTransform = GetComponent<RectTransform>();
	}

	public void Init(int x, int y) {
		this.grid_x = x;
		this.grid_y = y;
        ColorGrid.instance.squareList[x, y] = this;

		GameObject sphereObject = Instantiate(SphereBox.instance.spherePrefab, SphereBox.instance.transform);
		this.sphere = sphereObject.GetComponent<ColorSphere>();
		this.sphere.Init(this);

		this.ToggleEnabled(false);

		this.isInitializing = false;
	}

	public void Reset() {
		this.ToggleEnabled(false);
		this.SetColor(Color.white);
	}

	public void SetColor(Color newColor) {
		newColor.a = (this.isEnabled ? 1f : 0.5f);
		this.image.color = newColor;
		this.sphere.SetColor(newColor);
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
		this.sphere.gameObject.SetActive(this.isEnabled);
		this.SetColor(this.image.color);

		if (!isInitializing) {
            ColorGrid.instance.UpdateRamps();
		}
	}

	public void OnPointerClick(PointerEventData eventData) {
		bool doToggle = (eventData.button == PointerEventData.InputButton.Right);
		if (doToggle) {
			this.ToggleEnabled(!isEnabled);
		}

		if (isEnabled && (doToggle || (eventData.button == PointerEventData.InputButton.Left))) {
			ColorGrid.instance.ShowPicker(this);
		}
	}

	public void OnBeginDrag(PointerEventData eventData) {
	}

	public void OnDrag(PointerEventData eventData) {
	}

	public void OnEndDrag(PointerEventData eventData) {
		Vector2 position = eventData.position;
		foreach (ColorSquare square in ColorGrid.instance.squareList) {
			if (RectTransformUtility.RectangleContainsScreenPoint(square.GetComponent<RectTransform>(), position)) {
				ColorGrid.instance.SwapColors(this, square);
				break;
			}
		}
	}
}

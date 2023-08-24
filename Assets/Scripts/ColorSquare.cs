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

		if (isInitializing) {
			GameObject sphereObject = Instantiate(SphereBox.instance.spherePrefab, SphereBox.instance.transform);
			this.sphere = sphereObject.GetComponent<ColorSphere>();
			this.sphere.Init(this);
		}

		this.isInitializing = true;
        this.ToggleEnabled(false);

		this.isInitializing = false;
	}

    public void Reset() {
		this.ToggleEnabled(false);
		this.SetColor(Color.white);
	}

	public void SetColor(Color newColor, bool updateSphere=true) {
		newColor.a = (this.isEnabled ? 1f : 0.5f);
		this.image.color = newColor;

		if (updateSphere) {
			this.sphere.SetColor(newColor);
		}
	}

	public void SetColor(float h, float s, float v, bool updateSphere=true) {
		this.SetColor(Color.HSVToRGB(h / 359f, s / 100f, v / 100f), updateSphere);
	}

	public Color GetColor() {
		Color color = this.image.color;
		color.a = 1;
		return color;
	}

	public void ToggleEnabled(bool? enabled=null, bool doRampUpdate=true) {
		this.isEnabled = (enabled.HasValue ? enabled.Value : !this.isEnabled);
		this.sphere.gameObject.SetActive(this.isEnabled);
		this.SetColor(this.image.color);

		if (doRampUpdate && !isInitializing && this.gameObject.activeSelf) {
            ColorGrid.instance.UpdateRamps();
		}
	}

	public void OnPointerClick(PointerEventData eventData) {
		if (eventData.button == PointerEventData.InputButton.Right) {
			this.ToggleThis();
            return;
        }
		
		if (!this.isEnabled) {
			return;
		}

        if (eventData.button == PointerEventData.InputButton.Left) {
			this.PickThis();
        }
	}

	public void PickThis() {
		if (this == ColorGrid.instance.currentSquare) {
			return;
		}

		if (!this.isEnabled) {
            this.ToggleEnabled(true);
        }

        AudioManager.instance.PlayOneShot(ColorGrid.instance.ClickSound, Vector3.zero);
        ColorGrid.instance.ShowPicker(this);
    }

    public void ToggleThis() {
        this.ToggleEnabled();
        AudioManager.instance.PlayOneShot(ColorGrid.instance.HoverSound, Vector3.zero);

		if (this.isEnabled) {
			ColorGrid.instance.ShowPicker(this);
		}
		else {
            ColorGrid.instance.HidePicker();
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
	}

	public void OnDrag(PointerEventData eventData) {
	}

	public void OnEndDrag(PointerEventData eventData) {
		Vector2 position = eventData.position;
		foreach (List<ColorSquare> column in ColorGrid.instance.squareList) {
			foreach (ColorSquare square in column) {
				if (RectTransformUtility.RectangleContainsScreenPoint(square.GetComponent<RectTransform>(), position)) {
					ColorGrid.instance.SwapColors(this, square);
					break;
				}
			}
		}
    }
}

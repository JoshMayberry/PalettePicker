using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using TMPro;

[Serializable]
public class ColorRamp : IEnumerable {
	public List<ColorSquare> myRamp;

	public IEnumerator GetEnumerator() {
		return ((IEnumerable)myRamp).GetEnumerator();
	}

	public ColorRamp() {
		this.myRamp = new List<ColorSquare>();
	}

	public void Add(ColorSquare item) {
		this.myRamp.Add(item);
	}

	public int Count() {
		return this.myRamp.Count;
	}

	public void Clear() {
		this.myRamp.Clear();
	}
}

public class ColorGrid : MonoBehaviour {
	public FlexibleColorPicker colorPicker;
	public TMP_Dropdown fileTypeDropdown;

	public GameObject squarePrefab;
	public int gridWidth;
	public int gridHeight;

	RectTransform rectTransform;
	ColorSquare currentSquare;
	public ColorSquare[,] squareList;
	public List<ColorRamp> colorRamps;

	float lastWidth;
	bool _showingPicker;

    public static ColorGrid instance { get; private set; }
    void Awake() {
        if (instance != null) {
            Debug.LogError("Found more than one ColorGrid in the scene.");
        }

        instance = this;

		this.colorRamps = new List<ColorRamp>();
		this.rectTransform = GetComponent<RectTransform>();
	}

	void Start() {
		this.squareList = new ColorSquare[this.gridWidth, this.gridHeight];
		this.lastWidth = Screen.width;

		this.HidePicker();
		this.UpdateGrid();

		this.currentSquare = this.squareList[0, 0];
	}

	void Update() {
		if (Screen.width != this.lastWidth) {
			this.lastWidth = Screen.width;
			this.UpdateGrid();
		}
	}

	void UpdateGrid() {
		float squareSize = this.rectTransform.rect.width / this.gridWidth;

		for (int y = 0; y < this.gridHeight; y++) {
			for (int x = 0; x < this.gridWidth; x++) {
				if (squareList[x, y] == null) {
					this.AddSquare(x, y);
				}

				ColorSquare square = squareList[x, y];
				square.rectTransform.sizeDelta = new Vector2(squareSize, squareSize);
				square.rectTransform.anchoredPosition = new Vector2(x * squareSize, -y * squareSize);
			}
		}
	}

	public void UpdateRamps() {
		this.colorRamps.Clear();

		// Horizontal Scan
		for (int y = 0; y < this.gridHeight; y++) {
			ColorRamp row = new ColorRamp();
			for (int x = 0; x < this.gridWidth; x++) {
				ColorSquare square = this.squareList[x, y];
				if (square.isEnabled) {
					row.Add(square);
					continue;
				}

				if (row.Count() <= 1) {
					row.Clear();
					continue;
				}

				this.colorRamps.Add(row);
				row = new ColorRamp();
			}
			if (row.Count() > 1) {
				this.colorRamps.Add(row);
			}
		}

		// Vertical Scan
		for (int x = 0; x < this.gridWidth; x++) {
			ColorRamp column = new ColorRamp();
			for (int y = 0; y < this.gridHeight; y++) {
				ColorSquare square = this.squareList[x, y];
				if (square.isEnabled) {
					column.Add(square);
					continue;
				}

				if (column.Count() <= 1) {
					column.Clear();
					continue;
				}

				this.colorRamps.Add(column);
				column = new ColorRamp();
			}
			if (column.Count() > 1) {
				this.colorRamps.Add(column);
			}
		}

        SphereBox.instance.UpdateSplines();
    }

    public void AddSquare(int x, int y) {
		GameObject squareObject = Instantiate(this.squarePrefab, this.transform);
		ColorSquare square = squareObject.GetComponent<ColorSquare>();
		square.Init(x, y);
	}

	public void SwapColors(ColorSquare squareA, ColorSquare squareB) {
		bool tempEnabled = squareA.isEnabled;
		squareA.ToggleEnabled(squareB.isEnabled);
		squareB.ToggleEnabled(tempEnabled);

		Color tempColor = squareA.GetColor();
		squareA.SetColor(squareB.GetColor());
		squareB.SetColor(tempColor);

		this.currentSquare = squareB;

        UpdateRamps();
	}

	public void OnColorChanged(Color newColor) {
		if (!this._showingPicker) {
			return;
		}

		this.currentSquare.SetColor(newColor);
	}

	public void ShowPicker(ColorSquare square) {
		this.currentSquare = square;
		this.colorPicker.color = this.currentSquare.GetColor();
		this.colorPicker.gameObject.SetActive(true);
		this._showingPicker = true;
	}

	public void HidePicker() {
		this._showingPicker = false;
		this.colorPicker.gameObject.SetActive(false);
	}

	string GetFileType() {
		return fileTypeDropdown.options[fileTypeDropdown.value].text;
	}

	public void ExportColorPalette() {
		string fileExtension;
		switch (this.GetFileType()) {
			case "JSON":
				fileExtension = ".json";
				break;

			case "PNG 1": 
				fileExtension = ".png";
				break;

			case "PNG 2": 
				fileExtension = ".png";
				break;

			case "HEX": 
				fileExtension = ".hex";
				break;

			default:
				Debug.Log("Unknown export method: '" + fileTypeDropdown.value + "'");
				return;
		}

		FileBrowser.ShowSaveDialog((string[] paths) => {
			int count = 0;
			List<ColorData> colorDataList = new List<ColorData>();
			for (int y = 0; y < this.gridHeight; y++) {
				for (int x = 0; x < this.gridWidth; x++) {
					ColorSquare square = this.squareList[x, y];
					if (square.isEnabled) {
						ColorData data = new ColorData(square.GetColor(), x, y);
						colorDataList.Add(data);
						count++;
					}
				}
			}

			switch (this.GetFileType()) {
				case "JSON": {
					string json = JsonHelper.ToJson(colorDataList.ToArray(), true);
					FileBrowserHelpers.WriteTextToFile(paths[0], json);
				} break;

				case "PNG 1": {
					Texture2D texture = new Texture2D(count, 1);

					int i = 0;
					foreach (ColorData item in colorDataList) {
						texture.SetPixel(i, 0, item.color);
						i++;
					}
					texture.Apply();
					byte[] pngData = texture.EncodeToPNG();
					FileBrowserHelpers.WriteBytesToFile(paths[0], pngData);
				} break;

				case "PNG 2": {
					Texture2D texture = new Texture2D(this.gridWidth, this.gridHeight);
					Color blank = new Color(0, 0, 0, 0);
					for (int y = 0; y < this.gridHeight; y++) {
						for (int x = 0; x < this.gridWidth; x++) {
							texture.SetPixel(x, y, blank);
						}
					}

					foreach (ColorData item in colorDataList) {
						texture.SetPixel(item.x, item.y, item.color);
					}

					texture.Apply();
					byte[] pngData = texture.EncodeToPNG();
					FileBrowserHelpers.WriteBytesToFile(paths[0], pngData);
				} break;

				case "HEX": {
					List<string> hexColors = new List<string>();
					foreach (ColorData item in colorDataList) {
						hexColors.Add(item.hex);
					}

					string hexFileContent = string.Join("\n", hexColors);
					FileBrowserHelpers.WriteTextToFile(paths[0], hexFileContent);
				} break;

				default:
					Debug.Log("Unknown export method: '" + fileTypeDropdown.value + "'");
					break;
			}

		}, null, SimpleFileBrowser.FileBrowser.PickMode.Files, initialFilename: "palette" + fileExtension, title:"Export Colors", saveButtonText:"Export");
	}

	public void ImportColorPalette() {
		string fileExtension;
		switch (this.GetFileType()) {
			case "JSON":
				fileExtension = ".json";
				break;

			case "PNG 1": 
				fileExtension = ".png";
				break;

			case "PNG 2": 
				fileExtension = ".png";
				break;

			case "HEX": 
				fileExtension = ".hex";
				break;

			default:
				Debug.Log("Unknown import method: '" + fileTypeDropdown.value + "'");
				return;
		}

		FileBrowser.ShowLoadDialog((string[] paths) => {
			for (int x = 0; x < this.gridWidth; x++) {
				for (int y = 0; y < this.gridHeight; y++) {
					ColorSquare square = this.squareList[x, y];
					square.Reset();
				}
			}

			switch (this.GetFileType()) {
				case "JSON": {
					string json = FileBrowserHelpers.ReadTextFromFile(paths[0]);
					ColorData[] colorDataArray = JsonHelper.FromJson<ColorData>(json);
					foreach (ColorData data in colorDataArray) {
						ColorSquare square = this.squareList[data.x, data.y];
						square.ToggleEnabled(true);
						square.SetColor(data.h, data.s, data.v);
					}
				} break;

				case "PNG 1": {
					byte[] fileData = FileBrowserHelpers.ReadBytesFromFile(paths[0]);
					Texture2D texture = new Texture2D(1, 1);
					texture.LoadImage(fileData);

					int i = 0;
					for (int y = 0; y < this.gridHeight; y++) {
						for (int x = 0; x < this.gridWidth; x++) {
							if (i >= texture.width) {
								return;
							}

							ColorSquare square = this.squareList[x, y];
							square.ToggleEnabled(true);

							Color color = texture.GetPixel(i, 0);
							square.SetColor(color);
							i++;
						}
					}
				} break;

				case "PNG 2": {
					byte[] fileData = FileBrowserHelpers.ReadBytesFromFile(paths[0]);
					Texture2D texture = new Texture2D(1, 1);
					texture.LoadImage(fileData);

					// TODO: Check that the height and width match what is in the texture
					for (int y = 0; y < this.gridHeight; y++) {
						for (int x = 0; x < this.gridWidth; x++) {
							ColorSquare square = this.squareList[x, y];
							Color color = texture.GetPixel(x, y);

							if (color.a > 0.5) {
								square.ToggleEnabled(true);
								square.SetColor(color);
							}
						}
					}
				} break;

				case "HEX": {
					string rawString = FileBrowserHelpers.ReadTextFromFile(paths[0]);
					string[] hexColors = rawString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

					int i = 0;
					for (int y = 0; y < this.gridHeight; y++) {
						for (int x = 0; x < this.gridWidth; x++) {
							if (i >= hexColors.Length) {
								return;
							}

							Color color;
							if (ColorUtility.TryParseHtmlString("#" + hexColors[x], out color)) {
								ColorSquare square = this.squareList[x, y];
								square.ToggleEnabled(true);
								square.SetColor(color);
							}
						}
					}
				} break;

				default:
					Debug.Log("Unknown import method: '" + fileTypeDropdown.value + "'");
					break;
			}
		}, null, SimpleFileBrowser.FileBrowser.PickMode.Files, initialFilename: "palette" + fileExtension, title:"Import Colors", loadButtonText:"Import");
	}

	[Serializable]
	public class ColorData {
		public Color color;
		public string hex;
		public float h;
		public float s;
		public float v;
		public int x;
		public int y;

		public ColorData(Color _color, int _x, int _y) {
			Color.RGBToHSV(_color, out float _h, out float _s, out float _v);
			
			this.color = _color;
			this.hex = ColorUtility.ToHtmlStringRGB(color);
			this.h = float.Parse((_h * 359f).ToString("F2"));
			this.s = float.Parse((_s * 100f).ToString("F2"));
			this.v = float.Parse((_v * 100f).ToString("F2"));
			this.x = _x;
			this.y = _y;
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using TMPro;
using FMODUnity;
using UnityEngine.UI;

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
	public ColorSquare currentSquare;
	public List<List<ColorSquare>> squareList;
	public List<ColorRamp> colorRamps;
	public List<ColorSquare> inactiveSquares;


	float lastWidth;
	bool _showingPicker;

	[SerializeField] internal EventReference HoverSound;
	[SerializeField] internal EventReference ClickSound;

	public static ColorGrid instance { get; private set; }
	void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one ColorGrid in the scene.");
		}

		instance = this;

		this.colorRamps = new List<ColorRamp>();
		this.rectTransform = GetComponent<RectTransform>();
		this.inactiveSquares = new List<ColorSquare>();
	}

	void Start() {
		this.squareList = new List<List<ColorSquare>>();
		this.lastWidth = Screen.width;

		this.HidePicker();
		this.UpdateGrid();

		this.currentSquare = this.squareList[0][0];
	}

	void Update() {
		if (Screen.width != this.lastWidth) {
			this.lastWidth = Screen.width;
			this.UpdateGrid();
		}
	}

	void UpdateGrid() {
		float squareSize = Mathf.Min(this.rectTransform.rect.width / this.gridWidth, this.rectTransform.rect.height / this.gridHeight);

		// Resize squareList to match gridHeight
		while (this.squareList.Count < this.gridHeight) {
			this.squareList.Add(new List<ColorSquare>());
		}

		// Remove squares that don't fit anymore
		while (this.squareList.Count > this.gridHeight) {
			List<ColorSquare> lastRow = this.squareList[this.squareList.Count - 1];
			foreach (ColorSquare square in lastRow) {
				square.gameObject.SetActive(false);
				square.Reset();
				this.inactiveSquares.Add(square);
			}
			this.squareList.RemoveAt(this.squareList.Count - 1);
		}

		for (int y = 0; y < this.gridHeight; y++) {
			// Resize each row to match gridWidth
			while (this.squareList[y].Count < this.gridWidth) {
				this.AddSquare(this.squareList[y].Count, y);
			}

			// Remove squares that don't fit anymore
			while (this.squareList[y].Count > this.gridWidth) {
				ColorSquare square = this.squareList[y][this.squareList[y].Count - 1];
				square.gameObject.SetActive(false);
				square.Reset();
				this.inactiveSquares.Add(square);
				this.squareList[y].RemoveAt(this.squareList[y].Count - 1);
			}

			for (int x = 0; x < this.gridWidth; x++) {
				ColorSquare square = this.squareList[y][x];
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
				ColorSquare square = this.squareList[y][x];
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
				ColorSquare square = this.squareList[y][x];
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
		ColorSquare square;
		if (this.inactiveSquares.Count > 0) {
			square = this.inactiveSquares[0];
			this.inactiveSquares.RemoveAt(0);
		}
		else {
			GameObject squareObject = Instantiate(this.squarePrefab, this.transform);
			square = squareObject.GetComponent<ColorSquare>();
		}

		square.gameObject.SetActive(true);
		square.Init(x, y);
		this.squareList[y].Add(square);
	}

	public void SwapColors(ColorSquare squareA, ColorSquare squareB, bool updateRamps=true) {
		bool tempEnabled = squareA.isEnabled;
		squareA.ToggleEnabled(squareB.isEnabled, false);
		squareB.ToggleEnabled(tempEnabled, false);

		Color tempColor = squareA.GetColor();
		squareA.SetColor(squareB.GetColor());
		squareB.SetColor(tempColor);

		this.currentSquare = squareB;

		if (updateRamps) {
			UpdateRamps();
		}
	}
	public void ShiftUp() {
		for (int x = 0; x < this.gridWidth; x++) {
			Color tempColor = this.squareList[0][x].GetColor();
			bool tempEnabled = this.squareList[0][x].isEnabled;
			for (int y = 0; y < this.gridHeight - 1; y++) {
				SwapColors(this.squareList[y][x], this.squareList[y + 1][x], false);
			}
			this.squareList[this.gridHeight - 1][x].SetColor(tempColor);
			this.squareList[this.gridHeight - 1][x].ToggleEnabled(tempEnabled, false);
		}
		UpdateRamps();
	}
	public void ShiftDown() {
		for (int x = 0; x < this.gridWidth; x++) {
			Color tempColor = this.squareList[this.gridHeight - 1][x].GetColor();
			bool tempEnabled = this.squareList[this.gridHeight - 1][x].isEnabled;
			for (int y = this.gridHeight - 1; y > 0; y--) {
				SwapColors(this.squareList[y][x], this.squareList[y - 1][x], false);
			}
			this.squareList[0][x].SetColor(tempColor);
			this.squareList[0][x].ToggleEnabled(tempEnabled, false);
		}
		UpdateRamps();
	}

	public void ShiftLeft() {
		for (int y = 0; y < this.gridHeight; y++) {
			Color tempColor = this.squareList[y][0].GetColor();
			bool tempEnabled = this.squareList[y][0].isEnabled;
			for (int x = 0; x < this.gridWidth - 1; x++) {
				SwapColors(this.squareList[y][x], this.squareList[y][x + 1], false);
			}
			this.squareList[y][this.gridWidth - 1].SetColor(tempColor);
			this.squareList[y][this.gridWidth - 1].ToggleEnabled(tempEnabled, false);
		}
		UpdateRamps();
	}

	public void ShiftRight() {
		for (int y = 0; y < this.gridHeight; y++) {
			Color tempColor = this.squareList[y][this.gridWidth - 1].GetColor();
			bool tempEnabled = this.squareList[y][this.gridWidth - 1].isEnabled;
			for (int x = this.gridWidth - 1; x > 0; x--) {
				SwapColors(this.squareList[y][x], this.squareList[y][x - 1], false);
			}
			this.squareList[y][0].SetColor(tempColor);
			this.squareList[y][0].ToggleEnabled(tempEnabled, false);
		}
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
					ColorSquare square = this.squareList[y][x];
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
					ColorSquare square = this.squareList[y][x];
					square.Reset();
				}
			}

			switch (this.GetFileType()) {
				case "JSON": {
					string json = FileBrowserHelpers.ReadTextFromFile(paths[0]);
					ColorData[] colorDataArray = JsonHelper.FromJson<ColorData>(json);

					// Determine if grid is too small
					int maxX = 0;
					int maxY = 0;
					foreach (ColorData data in colorDataArray) {
						maxX = Mathf.Max(maxX, data.x + 1);
						maxY = Mathf.Max(maxY, data.y + 1);
					}

					if ((maxX > this.gridWidth) || (maxY > this.gridHeight)) {
						this.gridWidth = maxX;
						this.gridHeight = maxY;
						this.UpdateGrid();
					}

					foreach (ColorData data in colorDataArray) {
						ColorSquare square = this.squareList[data.y][data.x];
						square.ToggleEnabled(true);
						square.SetColor(data.h, data.s, data.v);
					}
				} break;

				case "PNG 1": {
					byte[] fileData = FileBrowserHelpers.ReadBytesFromFile(paths[0]);
					Texture2D texture = new Texture2D(1, 1);
					texture.LoadImage(fileData);

                    // Determine if grid is too small
                    if (this.gridHeight * this.gridWidth < texture.width) {
                        this.gridHeight = (int)Mathf.Ceil(Mathf.Sqrt(texture.width));
                        this.gridWidth = this.gridHeight;
                    }

                    int i = 0;
					for (int y = 0; y < this.gridHeight; y++) {
						for (int x = 0; x < this.gridWidth; x++) {
							if (i >= texture.width) {
								return;
							}

							ColorSquare square = this.squareList[y][x];
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

                    // Determine if grid is too small
                    if ((texture.width > this.gridWidth) || (texture.height > this.gridHeight)) {
                        this.gridWidth = texture.width;
                        this.gridHeight = texture.height;
                        this.UpdateGrid();
                    }

                    for (int y = 0; y < this.gridHeight; y++) {
						for (int x = 0; x < this.gridWidth; x++) {
							ColorSquare square = this.squareList[y][x];
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

                    // Determine if grid is too small
                    if (this.gridHeight * this.gridWidth < hexColors.Length) {
                        this.gridHeight = (int)Mathf.Ceil(Mathf.Sqrt(hexColors.Length));
                        this.gridWidth = this.gridHeight;
                    }

                    int i = 0;
					for (int y = 0; y < this.gridHeight; y++) {
						for (int x = 0; x < this.gridWidth; x++) {
							if (i >= hexColors.Length) {
								return;
							}

							Color color;
							if (ColorUtility.TryParseHtmlString("#" + hexColors[x], out color)) {
								ColorSquare square = this.squareList[y][x];
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

	public void IncreaseColumns() {
		this.gridWidth++;
		this.UpdateGrid();
	}
	public void DecreaseColumns() {
		if (this.gridWidth == 1) {
			return;
		}

		this.gridWidth--;
		this.UpdateGrid();
	}
	public void IncreaseRows() {
		this.gridHeight++;
		this.UpdateGrid();
	}
	public void DecreaseRows() {
		if (this.gridHeight == 1) {
			return;
		}

		this.gridHeight--;
		this.UpdateGrid();
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

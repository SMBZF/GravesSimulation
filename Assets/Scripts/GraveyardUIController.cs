using UnityEngine;
using UnityEngine.UI;

public class GraveyardUIController : MonoBehaviour
{
    public GraveyardGenerator generator;
    public DayNightController dayNightController;

    [Header("UI Elements")]
    public InputField widthInput;
    public InputField heightInput;
    public Slider treeDensitySlider;
    public Dropdown fenceStyleDropdown;
    public Dropdown seasonDropdown;
    public Button resetButton;
    public Slider dayNightSlider; // 新增 Day/Night 时间滑块

    void Start()
    {
        if (generator == null)
        {
            Debug.LogError("GraveyardGenerator 未绑定！");
            return;
        }

        widthInput.text = generator.width.ToString();
        heightInput.text = generator.height.ToString();
        treeDensitySlider.value = generator.treeSpawnChance;
        fenceStyleDropdown.value = generator.selectedFenceIndex;
        seasonDropdown.value = (int)generator.currentSeason;

        widthInput.onEndEdit.AddListener(OnWidthChanged);
        heightInput.onEndEdit.AddListener(OnHeightChanged);
        treeDensitySlider.onValueChanged.AddListener(OnTreeDensityChanged);
        fenceStyleDropdown.onValueChanged.AddListener(OnFenceStyleChanged);
        seasonDropdown.onValueChanged.AddListener(OnSeasonChanged);
        resetButton.onClick.AddListener(OnResetClicked);

        if (dayNightSlider != null && dayNightController != null)
        {
            dayNightSlider.value = dayNightController.timeValue;
            dayNightSlider.onValueChanged.AddListener(OnDayNightValueChanged);
        }
    }

    void OnWidthChanged(string value)
    {
        if (int.TryParse(value, out int result))
        {
            generator.width = result;
            generator.GenerateGraveyard();
        }
    }

    void OnHeightChanged(string value)
    {
        if (int.TryParse(value, out int result))
        {
            generator.height = result;
            generator.GenerateGraveyard();
        }
    }

    void OnTreeDensityChanged(float value)
    {
        generator.treeSpawnChance = value;
        generator.GenerateGraveyard();
    }

    void OnFenceStyleChanged(int index)
    {
        generator.SetFenceStyle(index);
    }

    void OnSeasonChanged(int index)
    {
        switch (index)
        {
            case 0:
                generator.SetSeasonToSummer();
                break;
            case 1:
                generator.SetSeasonToAutumn();
                break;
            case 2:
                generator.SetSeasonToWinter();
                break;
        }
    }

    void OnResetClicked()
    {
        generator.GenerateGraveyard();
    }

    void OnDayNightValueChanged(float value)
    {
        if (dayNightController != null)
        {
            dayNightController.timeValue = value;
        }
    }
}

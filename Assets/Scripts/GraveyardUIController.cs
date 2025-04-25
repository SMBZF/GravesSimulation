using UnityEngine;
using UnityEngine.UI;

public class GraveyardUIController : MonoBehaviour
{
    public GraveyardGenerator generator;
    public DayNightController dayNightController;
    public VisitorManager visitorManager;

    [Header("UI Elements")]
    public InputField widthInput;
    public InputField heightInput;
    public Slider treeDensitySlider;
    public Slider flowerDensitySlider;
    public Dropdown fenceStyleDropdown;
    public Dropdown seasonDropdown;
    public Button resetButton;
    public Button startButton;
    public Button backToSetupButton; // ✅ 新增：返回按钮
    public Slider dayNightSlider;

    [Header("Panels")]
    public GameObject panelInitialUI;
    public GameObject panelInGameUI;

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
        flowerDensitySlider.onValueChanged.AddListener(OnFlowerDensityChanged);
        fenceStyleDropdown.onValueChanged.AddListener(OnFenceStyleChanged);
        seasonDropdown.onValueChanged.AddListener(OnSeasonChanged);
        resetButton.onClick.AddListener(OnResetClicked);

        if (startButton != null)
            startButton.onClick.AddListener(OnStartGameClicked);

        if (backToSetupButton != null)
            backToSetupButton.onClick.AddListener(OnBackToSetupClicked); // ✅ 新增绑定返回按钮

        if (flowerDensitySlider != null)
        {
            flowerDensitySlider.minValue = 0f;
            flowerDensitySlider.maxValue = 0.23f;
            flowerDensitySlider.value = generator.flowerSpawnChance;
        }

        if (dayNightSlider != null && dayNightController != null)
        {
            dayNightSlider.value = dayNightController.timeValue;
            dayNightSlider.onValueChanged.AddListener(OnDayNightValueChanged);
        }
    }

    void Update()
    {
        if (dayNightSlider != null && dayNightController != null)
        {
            dayNightSlider.value = dayNightController.timeValue;
        }
    }

    void OnWidthChanged(string value)
    {
        if (int.TryParse(value, out int result))
        {
            result = ClampOdd(result);
            generator.width = result;
            widthInput.text = result.ToString();
            generator.GenerateGraveyard();
            visitorManager?.RefreshPoints();
        }
    }

    void OnHeightChanged(string value)
    {
        if (int.TryParse(value, out int result))
        {
            result = ClampOdd(result);
            generator.height = result;
            heightInput.text = result.ToString();
            generator.GenerateGraveyard();
            visitorManager?.RefreshPoints();
        }
    }

    int ClampOdd(int value)
    {
        value = Mathf.Clamp(value, 5, 19);
        if (value % 2 == 0) value += 1;
        return Mathf.Clamp(value, 5, 19);
    }

    void OnTreeDensityChanged(float value)
    {
        generator.treeSpawnChance = value;
        generator.GenerateGraveyard();
        visitorManager?.RefreshPoints();
    }

    void OnFlowerDensityChanged(float value)
    {
        generator.flowerSpawnChance = value;
        generator.GenerateGraveyard();
        visitorManager?.RefreshPoints();
    }

    void OnFenceStyleChanged(int index)
    {
        generator.SetFenceStyle(index);
        visitorManager?.RefreshPoints();
    }

    void OnSeasonChanged(int index)
    {
        switch (index)
        {
            case 0: generator.SetSeasonToSpring(); break;
            case 1: generator.SetSeasonToSummer(); break;
            case 2: generator.SetSeasonToAutumn(); break;
            case 3: generator.SetSeasonToWinter(); break;
        }

        visitorManager?.RefreshPoints();
    }

    void OnResetClicked()
    {
        generator.GenerateGraveyard();
        visitorManager?.RefreshPoints();
    }

    void OnDayNightValueChanged(float value)
    {
        if (dayNightController != null)
        {
            dayNightController.timeValue = value;
        }
    }

    void OnStartGameClicked()
    {
        if (panelInitialUI != null) panelInitialUI.SetActive(false);
        if (panelInGameUI != null) panelInGameUI.SetActive(true);

        if (dayNightController != null)
            dayNightController.isPaused = false;

        if (visitorManager != null)
            visitorManager.StartDay();
    }

    void OnBackToSetupClicked()
    {
        if (panelInitialUI != null) panelInitialUI.SetActive(true);
        if (panelInGameUI != null) panelInGameUI.SetActive(false);

        if (dayNightController != null)
        {
            dayNightController.isPaused = true;
            dayNightController.timeValue = dayNightController.dayStart - 0.005f;
        }


        if (visitorManager != null)
        {
            visitorManager.EndDay();
            visitorManager.ForceAllVisitorsToExit(); // 这里强制访客去出口
        }
        ClearAllGhosts();

        if (dayNightSlider != null && dayNightController != null)
            dayNightSlider.value = dayNightController.timeValue;
    }

    void ClearAllGhosts()
    {
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (var ghost in ghosts)
        {
            if (ghost != null)
                Destroy(ghost);
        }
    }
}

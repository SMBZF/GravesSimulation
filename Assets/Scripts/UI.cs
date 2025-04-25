using UnityEngine;
using UnityEngine.UI;

public class GraveyardGameStartUI : MonoBehaviour
{
    [Header("引用")]
    public GraveyardGenerator generator;
    public DayNightController dayNightController;
    public GameObject uiPanel;

    [Header("UI 元素")]
    public Dropdown seasonDropdown;
    public InputField widthInput;
    public InputField heightInput;
    public Button startButton;

    void Start()
    {
        widthInput.onEndEdit.AddListener(OnWidthChanged);
        heightInput.onEndEdit.AddListener(OnHeightChanged);
        seasonDropdown.onValueChanged.AddListener(OnSeasonChanged);
        startButton.onClick.AddListener(OnStartGameClicked);

        // 初始化默认值
        widthInput.text = generator.width.ToString();
        heightInput.text = generator.height.ToString();
        seasonDropdown.value = (int)generator.currentSeason;
    }

    void OnWidthChanged(string value)
    {
        if (int.TryParse(value, out int result))
        {
            result = ClampOdd(result);
            generator.width = result;
            widthInput.text = result.ToString();
        }
    }

    void OnHeightChanged(string value)
    {
        if (int.TryParse(value, out int result))
        {
            result = ClampOdd(result);
            generator.height = result;
            heightInput.text = result.ToString();
        }
    }

    int ClampOdd(int value)
    {
        value = Mathf.Clamp(value, 5, 19);
        if (value % 2 == 0) value += 1;
        return Mathf.Clamp(value, 5, 19); // 防止超出上限
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
    }

    void OnStartGameClicked()
    {
        generator.GenerateGraveyard();
        if (dayNightController != null)
            dayNightController.isPaused = false;

        if (uiPanel != null)
            uiPanel.SetActive(false);
    }
}

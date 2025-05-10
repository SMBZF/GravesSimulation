using UnityEngine;
using UnityEngine.UI;

public class GhostCodexManager : MonoBehaviour
{
    [Header("图鉴 UI 引用")]
    public GameObject codexPanel; // 图鉴整体面板
    public Text captionText;
    public Text gluttonText;
    public Text snowWhiteText;
    public Text ordinaryText;

    private bool unlockedCaption = false;
    private bool unlockedGlutton = false;
    private bool unlockedSnowWhite = false;
    private bool unlockedOrdinary = false;

    private void Start()
    {
        UpdateCodexDisplay();
        if (codexPanel != null)
            codexPanel.SetActive(false); // 初始隐藏
    }

    public void UnlockCaption()
    {
        if (!unlockedCaption)
        {
            unlockedCaption = true;
            UpdateCodexDisplay();
        }
    }

    public void UnlockGlutton()
    {
        if (!unlockedGlutton)
        {
            unlockedGlutton = true;
            UpdateCodexDisplay();
        }
    }

    public void UnlockSnowWhite()
    {
        if (!unlockedSnowWhite)
        {
            unlockedSnowWhite = true;
            UpdateCodexDisplay();
        }
    }

    public void UnlockOrdinary()
    {
        if (!unlockedOrdinary)
        {
            unlockedOrdinary = true;
            UpdateCodexDisplay();
        }
    }

    private void UpdateCodexDisplay()
    {
        if (captionText != null)
            captionText.text = unlockedCaption ? "Captain: I was once a captain... lost at sea." : "???";

        if (gluttonText != null)
            gluttonText.text = unlockedGlutton ? "Glutton: No more food! Do you want me to die twice?" : "???";

        if (snowWhiteText != null)
            snowWhiteText.text = unlockedSnowWhite ? "Snow White: Poisoned apples...? My wicked stepmother still haunts me!" : "???";

        if (ordinaryText != null)
            ordinaryText.text = unlockedOrdinary ? "Ordinary Soul: Plain candles for a plain life... and death." : "???";
    }

    public void ToggleCodexPanel()
    {
        if (codexPanel != null)
            codexPanel.SetActive(!codexPanel.activeSelf);
    }
}

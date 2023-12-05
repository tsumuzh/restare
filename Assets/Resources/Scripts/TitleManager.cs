using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class TitleManager : MonoBehaviour
{
    [SerializeField] Image overlapPanel;
    private bool isSceneMove;
    int phase = 0;
    [SerializeField] TextMeshProUGUI titleMainLabel, creditLabel, titleLogo;
    void Start()
    {
        //  PlayerPrefs.DeleteAll();
        if (PlayerPrefs.GetInt("phase") == 5) PlayerPrefs.DeleteKey("phase");

        if (!PlayerPrefs.HasKey("phase")) PlayerPrefs.SetInt("phase", 0);

        phase = PlayerPrefs.GetInt("phase");
        float colorBase = (phase * 40 + 135) / 255f;
        Color uiColor = new Color(colorBase, colorBase, colorBase);
        titleMainLabel.color = uiColor;
        creditLabel.color = uiColor;
        titleLogo.color = uiColor;

//        Debug.Log(phase);

        isSceneMove = false;
        StartCoroutine(FadePanel(false));
    }
    void Update()
    {
        if (!isSceneMove)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StartCoroutine(FadePanelAndLoadScene("Main"));
            }
        }
    }
    IEnumerator FadePanel(bool fadeMode)//false:fadeout, true:fadein
    {
        float t = 0;
        while (t <= 1)
        {
            if (!fadeMode) overlapPanel.color = Color.Lerp(new Color(0, 0, 0, 1), new Color(0, 0, 0, 0), t);
            else overlapPanel.color = Color.Lerp(new Color(0, 0, 0, 1), new Color(0, 0, 0, 0), 1 - t);
            yield return new WaitForFixedUpdate();
            t += Time.fixedDeltaTime;
        }
        yield break;
    }
    IEnumerator FadePanelAndLoadScene(string sceneName)
    {
        isSceneMove = true;
        Coroutine coroutine = StartCoroutine(FadePanel(true));
        yield return coroutine;
        SceneManager.LoadScene(sceneName);
        yield break;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndrollPlayer : MonoBehaviour
{
    [SerializeField] RectTransform endroll;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Image panel;
    void Start()
    {
        StartCoroutine(PlayEndroll());
        panel.color = new Color(0, 0, 0, 1);
    }

    IEnumerator PlayEndroll()
    {
        float t = 0;
        yield return new WaitForSeconds(2);
        panel.color = new Color(0, 0, 0, 0);
        yield return new WaitForSeconds(3);
        audioSource.PlayOneShot(audioSource.clip);
        while (t < 1)
        {
            t += 0.0008f;
            endroll.localPosition = Vector3.Lerp(new Vector3(0, 50, 0), new Vector3(0, 3520, 0), t);
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(1);
        t = 0;
        while (t < 1)
        {
            t += Time.fixedDeltaTime / 2;
            panel.color = Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), t);
            yield return new WaitForFixedUpdate();
        }
        SceneManager.LoadScene("Main");
    }
}

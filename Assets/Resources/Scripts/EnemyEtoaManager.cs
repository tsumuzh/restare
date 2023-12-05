using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEtoaManager : MonoBehaviour
{
    public bool isBattleStarted;
    public SpriteRenderer sr;
    public Sprite[] sprites = new Sprite[8];
    [SerializeField] Transform playerTransform;
    Rigidbody2D rb;
    [SerializeField] Vector2 dist;
    void Start()
    {
        isBattleStarted = false;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        rb.velocity = Vector2.zero;
        if (isBattleStarted && !GameManager.isEvent)
        {
            dist = transform.position - playerTransform.position;
            float angle = Mathf.Atan2(dist.y, dist.x);
            int n = 0;
            for (int i = -4; i < 4; i++)
            {
                n = i;
                if (angle <= Mathf.PI * i / 4 + Mathf.PI / 8) break;
                if (i == 3) n = -4;
            }
            dist = -new Vector2(Mathf.Cos(n * Mathf.PI / 4), Mathf.Sin(n * Mathf.PI / 4));
            rb.velocity = dist * 3;
            sr.sprite = sprites[n + 4];
        }
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.name == "Player" && isBattleStarted)
        {
            GameManager.isEvent = true;
            PlayerPrefs.SetInt("phase", 6);
            isBattleStarted = false;
            StartCoroutine(GameObject.Find("Manager").GetComponent<GameManager>().AfterBattle(false));
        }
    }
}

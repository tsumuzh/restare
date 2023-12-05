using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        if (rb.bodyType != RigidbodyType2D.Static)
        {
            if (rb.velocity.magnitude < 1)
            {
                rb.bodyType = RigidbodyType2D.Static;
                gameObject.tag = "EventObj";
            }
        }

    }
    void OnCollisionEnter2D(Collision2D collisionInfo)
    {
        if (collisionInfo.gameObject.name == "Etoa")
        {
            // Destroy(gameObject);
            if (PlayerPrefs.GetInt("phase") == 4 && gameObject.tag == "AttackObj")
            {
                HealthManager healthManager = collisionInfo.gameObject.GetComponent<HealthManager>();
                healthManager.hp--;
                GameManager gameManager = GameObject.Find("Manager").GetComponent<GameManager>();
                gameManager.etoaHealthGauge.fillAmount = healthManager.hp / 20f;
                gameManager.stoneCountLabel.text = "×" + gameManager.stoneCount;
                gameObject.tag = "EventObj";
                if (healthManager.hp == 0)
                {
                    GameManager.isEvent = true;
                    GameObject etoa = GameObject.Find("Etoa");
                    EnemyEtoaManager enemyEtoaManager = etoa.GetComponent<EnemyEtoaManager>();
                    enemyEtoaManager.isBattleStarted = false;
                    PlayerPrefs.SetInt("phase", 5);
                    StartCoroutine(gameManager.AfterBattle(true));
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    private float playerSp, playerWalkSp, playerRunSp, spriteTime, spriteWalkTime, spriteRunTIme, t;
    private SpriteRenderer sr_player;
    public Sprite[] sp_player, sp_player_play;
    private int spriteIndex, spriteSetIndex, playerFacingIndex/*プレイヤーの向きを整数に保管しておく*/;
    private Rigidbody2D rb;
    private int hp;
    [SerializeField] Image hpGauge, panel;
    private Vector2 playerFacing;
    private Vector3 oldPos;
    public bool isEventActive;
    private GameObject targetEventObj, closestEventObj, generatedFudaGuard;
    [SerializeField] GameObject stone, fireballBullet, fudaGuard, shiguremaru;
    AudioSource audioSource;
    AudioClip[] audioClips = new AudioClip[2];
    private bool footstepMode;
    GameManager gameManager;
    void Start()
    {
        playerWalkSp = 3f;
        playerRunSp = 7f;
        spriteWalkTime = 0.2f;
        spriteRunTIme = 0.08f;
        t = 0f;
        sp_player = new Sprite[24];
        sp_player_play = new Sprite[4];
        sr_player = GetComponent<SpriteRenderer>();
        spriteIndex = 0;

        LoadPlayerSprites("Player");
        SetPlaySprite(Vector2.down);

        rb = GetComponent<Rigidbody2D>();

        hp = 10;
        panel = GameObject.Find("MainPanel").GetComponent<Image>();
        oldPos = transform.position;
        isEventActive = false;

        audioSource = GetComponent<AudioSource>();

        audioClips[0] = Resources.Load<AudioClip>("Sounds/Effects/footstep1");
        audioClips[1] = Resources.Load<AudioClip>("Sounds/Effects/footstep2");

        SetVolume();

        footstepMode = false;

        gameManager = GameObject.Find("Manager").GetComponent<GameManager>();
    }
    void Update()
    {
        rb.velocity = Vector2.zero;
        //画像動作
        if (!GameManager.isEvent && !GameManager.isMenu && !GameManager.isSelectionOnMessageWindow && !GameManager.isSave && !GameManager.isLoad)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                spriteTime = spriteRunTIme;
                playerSp = playerRunSp;
            }
            else
            {
                spriteTime = spriteWalkTime;
                playerSp = playerWalkSp;
            }
            if (!Input.anyKey)
            {
                spriteIndex = 0;
                t = 0;
            }
            else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                playerFacing = Vector2.zero;
                t += Time.deltaTime;

                playerFacing = Vector2.zero;
                if (Input.GetKey(KeyCode.UpArrow)) playerFacing += Vector2.up;
                if (Input.GetKey(KeyCode.LeftArrow)) playerFacing += Vector2.left;
                if (Input.GetKey(KeyCode.DownArrow)) playerFacing += Vector2.down;
                if (Input.GetKey(KeyCode.RightArrow)) playerFacing += Vector2.right;
                if (playerFacing != Vector2.zero) SetPlaySprite(playerFacing); //上と下、左と右のように反対方向が同時に入力されている場合は何もしない。これがないと反対方向同時入力時に正面を向いてしまう。
                rb.velocity = playerFacing.normalized * playerSp;
            }
            if (isEventActive) //NPC、看板への話しかけ
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    EventManager em = targetEventObj.GetComponent<EventManager>();
                    if (em.isPerson) //対象がNPCならこちらを向かせる
                    {
                        Vector2 dist = transform.position - targetEventObj.transform.position;
                        float angle = Mathf.Atan2(dist.y, dist.x);
                        int n = 0;
                        for (int i = -4; i < 4; i++)
                        {
                            n = i;
                            if (angle <= Mathf.PI * i / 4 + Mathf.PI / 8) break;
                            if (i == 3) n = -4;
                        }
                        targetEventObj.GetComponent<SpriteRenderer>().sprite = em.spriteOnMap[n + 4];
                    }
                    em.StartEvent(0, 0);
                }
            }
            if (!GameManager.isAttacking && PlayerPrefs.GetInt("phase") == 4 && gameManager.stoneCount > 0)
            {
                if (Input.GetKeyDown(KeyCode.Z)) Attack(0); //遠距離
                                                            //    if (Input.GetKeyDown(KeyCode.X)) Attack(1); //近距離
                                                            //    if (Input.GetKeyDown(KeyCode.C)) Attack(2); //特殊
            }
            else
            {
                if (generatedFudaGuard != null) generatedFudaGuard.transform.position = transform.position;
            }
        }

        if (closestEventObj != null)
        {
            if (closestEventObj.transform.position.y < transform.position.y) sr_player.sortingOrder = 0;
            else sr_player.sortingOrder = 2;
        }

        //イベント発生
        /*   if (Input.GetKeyDown(KeyCode.Return) && !GameManager.isEvent && !GameManager.isMenu)
           {
               //イベントを発生させるオブジェクトがあるかを確認する
               Vector3 sightDir = Vector3.zero;
               if (spriteSetIndex == 0) sightDir = transform.forward;
               if (spriteSetIndex == 1) sightDir = -transform.forward;
               if (spriteSetIndex == 2) sightDir = transform.right;
               if (spriteSetIndex == 3) sightDir = -transform.right;
               Collider[] cols = Physics.OverlapSphere(transform.position + sightDir * 1, 1);
               for (int i = 0; i < cols.Length; i++)
               {
                   if (cols[i].gameObject.tag == "EventObj" && !GameManager.isEvent) cols[i].gameObject.GetComponent<EventManager>().startEvent(); ;
               }
           }*/
    }
    void FixedUpdate()
    {
        if (Vector3.Distance(oldPos, transform.position) < playerSp * Time.deltaTime / 10) sr_player.sprite = sp_player_play[0];
        else
        {
            sr_player.sprite = sp_player_play[spriteIndex];
            if (t > spriteTime)
            {
                t = 0;
                spriteIndex = ++spriteIndex % 4;
                if (spriteIndex % 2 == 1) RingFootstep();
            }
        }
        oldPos = transform.position;
    }
    public void SetVolume()
    {
        audioSource.volume = GameManager.soundEffectVolume;
    }
    public void GiveDamage()
    {
        StartCoroutine(DamageEffect());
    }
    void RingFootstep()
    {
        footstepMode = !footstepMode;
        audioSource.PlayOneShot(audioClips[Convert.ToInt32(footstepMode)]);
    }
    public void Attack(int n) //攻撃
    {
        if (!GameManager.playingCharacter)
        {
            switch (n)
            {
                case 0:
                    GameObject obj = Instantiate(stone, transform.position + new Vector3(playerFacing.x, playerFacing.y - 0.5f, 0), Quaternion.Euler(0, 0, Mathf.Atan2(playerFacing.y, playerFacing.x) * Mathf.Rad2Deg - 90));
                    obj.GetComponent<Rigidbody2D>().velocity = playerFacing.normalized * 12;
                    obj.tag = "AttackObj";
                    obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                    gameManager.stoneCount--;
                    gameManager.stoneCountLabel.text = "×" + gameManager.stoneCount;
                    break;

                    /*   case 2:
                           GameManager.isAttacking = true;
                           generatedFudaGuard = Instantiate(fudaGuard, transform.position, Quaternion.identity);
                           Invoke("setAttackInactive", 2);
                           break;*/
            }
        }
        else
        {
            switch (n)
            {
                case 0:
                    GameObject obj = Instantiate(fireballBullet, transform.position + new Vector3(playerFacing.x, playerFacing.y - 0.5f, 0), Quaternion.Euler(0, 0, Mathf.Atan2(playerFacing.y, playerFacing.x) * Mathf.Rad2Deg - 90));
                    obj.GetComponent<Rigidbody2D>().velocity = playerFacing.normalized * 16;
                    break;
            }
        }
    }
    void SetAttackInactive()
    {
        GameManager.isAttacking = false;
        if (generatedFudaGuard != null) generatedFudaGuard = null;
    }
    IEnumerator DamageEffect()
    {
        sr_player.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr_player.color = Color.white;
        yield break;
    }
    public void LoadPlayerSprites(string str) //プレイヤーの画像を素材から読み込む
    {
        for (int i = 0; i < 24; i++)
        {
            sp_player[i] = Resources.Load<Sprite>("Textures/Characters/" + str + "/Dots/" + i.ToString());
        }
    }
    void SetPlaySprite(Vector2 v)
    {
        int[] indexs = new int[4];
        if (v == Vector2.up) { indexs = new int[] { 3, 4, 3, 5 }; playerFacingIndex = 0; }
        else if (v == Vector2.left) { indexs = new int[] { 6, 7, 6, 8 }; playerFacingIndex = 1; }
        else if (v == Vector2.down) { indexs = new int[] { 0, 1, 0, 2 }; playerFacingIndex = 2; }
        else if (v == Vector2.right) { indexs = new int[] { 9, 10, 9, 11 }; playerFacingIndex = 3; }
        else if (v == new Vector2(-1, 1)) { indexs = new int[] { 18, 19, 18, 20 }; playerFacingIndex = 4; }
        else if (v == -Vector2.one) { indexs = new int[] { 12, 13, 12, 14 }; playerFacingIndex = 5; }
        else if (v == new Vector2(1, -1)) { indexs = new int[] { 15, 16, 15, 17 }; playerFacingIndex = 6; }
        else if (v == Vector2.one) { indexs = new int[] { 21, 22, 21, 23 }; playerFacingIndex = 7; }
        else if (v == Vector2.zero) { }
        else Debug.Log("Player Facing Error:" + v.ToString());

        for (int i = 0; i < 4; i++) sp_player_play[i] = sp_player[indexs[i]];
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "EventTrigger") other.gameObject.GetComponent<EventManager>().StartEvent(0, 0);
        if (other.gameObject.tag == "EventObj")
        {
            closestEventObj = other.gameObject;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == closestEventObj) closestEventObj = null;
    }
    void OnCollisionEnter2D(Collision2D collisionInfo)
    {
        if (collisionInfo.gameObject.tag == "EventObj")
        {
            isEventActive = true;
            targetEventObj = collisionInfo.gameObject;
        }
    }
    void OnCollisionExit2D(Collision2D collisionInfo)
    {
        if (collisionInfo.gameObject.tag == "EventObj")
        {
            isEventActive = false;
            targetEventObj = null;
        }
    }
    /*  IEnumerator blackOut() //HPが0になった時
      {
          float t = 0;
          while (t <= 1)
          {
              panel.color = Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), t);
              t += 0.025f;
              yield return new WaitForFixedUpdate();
          }
          MainGameManager.isEvent = false;
          transform.position = Vector3.forward * 16;
          hp = 10;
          hpGauge.fillAmount = 1;
          while (t >= 0)
          {
              panel.color = Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), t);
              t -= 0.025f;
              yield return new WaitForFixedUpdate();
          }
          yield break;
      }*/
}
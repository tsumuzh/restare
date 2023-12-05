using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static bool isEvent, isMenu, isItemCheck, isOption, playingCharacter, isAttacking, isSelectionOnMessageWindow, isSave, isLoad;
    public static float musicVolume, soundEffectVolume;
    public float timePerChar;
    public bool[] flags = new bool[6];
    public string[] messages, names, messageControlPrefixs, selections, selectionsPrefixs, messageTalkPosition;
    public Sprite[] center_base, center_eye, center_eyeblow, center_mouth, center_nose, center_face;
    public Sprite[] left_base, left_eye, left_eyeblow, left_mouth, left_nose, left_face;
    public Sprite[] right_base, right_eye, right_eyeblow, right_mouth, right_nose, right_face;
    private Image[] centerImage, leftImage, rightImage;
    private TextMeshProUGUI messageLabel, nameLabel, menuMainLabel, menuDescriptionLabel, menuSideLabel, saveDataLabel, centerLabel;
    private GameObject messageGroup, menuGroup, optionGroup, saveDataGroup;
    private int[] inventory = new int[8]; //持ち物をアイテムIDで指定
    private int inventoryIndex, menuSideIndex, filledInventoryAmount, selectionIndex, currentTextIndex; //currentTextIndex:選択肢が複数ある会話にて、その選択肢が会話データの何行目かを示す
    private string[] menuSideBaseString, menuDescriptionBaseString;
    private Slider musicVolumeSlider, soundEffecrVolumeSlider;
    private PlayerManager playerManager;
    private items _items;
    public TextAsset scenarioData;
    public EventManager currentEventManager;
    private int maxSaveSlotNum, currentSaveSlotNum;
    private System.IO.FileInfo[] saveFileInfos;
    private float playTime;
    bool rinneTalkMode = false; //temp, debug only
    public static int loadDataIndexWhenStartGame;
    private AudioSource[] audioSource = new AudioSource[2];//0:sound effect, 1,background
    public int stoneCount;
    [SerializeField] GameObject finalWall, battleGroup;
    public TextMeshProUGUI stoneCountLabel;
    public Image etoaHealthGauge;
    void Start()
    {
        isEvent = false;
        isMenu = false;
        playingCharacter = false; //false:Kikyo, true:Kuya

        //temp
        musicVolume = 0.5f;
        soundEffectVolume = 0.5f;
        musicVolumeSlider = GameObject.Find("MusicVolumeSlider").GetComponent<Slider>();
        musicVolumeSlider.value = musicVolume;
        soundEffecrVolumeSlider = GameObject.Find("SoundEffectVolumeSlider").GetComponent<Slider>();
        soundEffecrVolumeSlider.value = soundEffectVolume;

        messageGroup = GameObject.Find("MessageGroup");
        messageLabel = GameObject.Find("MessageLabel").GetComponent<TextMeshProUGUI>();
        nameLabel = GameObject.Find("NameLabel").GetComponent<TextMeshProUGUI>();
        menuMainLabel = GameObject.Find("MenuMainLabel").GetComponent<TextMeshProUGUI>();
        menuDescriptionLabel = GameObject.Find("MenuDescriptionLabel").GetComponent<TextMeshProUGUI>();
        menuSideLabel = GameObject.Find("MenuSideLabel").GetComponent<TextMeshProUGUI>();

        centerImage = new Image[6];
        leftImage = new Image[6];
        rightImage = new Image[6];
        for (int i = 0; i < 6; i++)
        {
            centerImage[i] = GameObject.Find("CenterImage_" + i).GetComponent<Image>();
            leftImage[i] = GameObject.Find("LeftImage_" + i).GetComponent<Image>();
            rightImage[i] = GameObject.Find("RightImage_" + i).GetComponent<Image>();
        }
        messageGroup.SetActive(false);
        optionGroup = GameObject.Find("OptionGroup");
        optionGroup.SetActive(false);
        menuGroup = GameObject.Find("MenuGroup");
        menuGroup.SetActive(false);

        _items = new items();
        menuMainLabel.text = "";

        inventory[0] = 0;
        inventory[1] = 1;
        inventory[2] = 2;
        inventory[3] = 3;
        inventory[4] = -1;
        inventory[5] = -1;
        inventory[6] = -1;
        inventory[7] = -1;

        inventoryIndex = 0;
        menuSideIndex = 0;

        menuSideBaseString = new string[4];
        menuSideBaseString[0] = "持ち物\n";
        menuSideBaseString[1] = "オプション\n";
        menuSideBaseString[2] = "ロード\n";
        menuSideBaseString[3] = "タイトル";
        menuDescriptionBaseString = new string[4];
        menuDescriptionBaseString[0] = "持ち物の確認";
        menuDescriptionBaseString[1] = "ゲームの各種設定";
        menuDescriptionBaseString[2] = "セーブデータをロードする";
        menuDescriptionBaseString[3] = "タイトルに戻る";

        playerManager = GameObject.Find("Player").GetComponent<PlayerManager>();

        saveDataGroup = GameObject.Find("SaveDataGroup");
        saveDataLabel = GameObject.Find("SaveDataLabel").GetComponent<TextMeshProUGUI>();
        saveDataGroup.SetActive(false);

        audioSource = GetComponents<AudioSource>();

        StartCoroutine(FadeBackSound(false));

        //  PlayerPrefs.SetInt("phase", 5);
        //PlayerPrefs.SetInt("phase", 6);
        switch (PlayerPrefs.GetInt("phase"))
        {
            case 0:
                StartCoroutine(FadePanel(false));
                GameObject.Find("ProloguePlayer1").GetComponent<EventManager>().StartEvent(0, 0);
                GameObject.Find("Player").transform.position = Vector3.zero;
                break;
            case 1:
                StartCoroutine(FadePanel(false));
                GameObject.Find("ProloguePlayer2").GetComponent<EventManager>().StartEvent(0, 0);
                GameObject.Find("Player").transform.position = new Vector3(-130.5f, 3.5f, 0);
                break;
            case 2:
                StartCoroutine(FadePanel(false));
                GameObject.Find("ProloguePlayer3").GetComponent<EventManager>().StartEvent(0, 0);
                GameObject.Find("Player").transform.position = new Vector3(-9.5f, -59, 0);
                break;
            case 3:
            case 4:
                StartCoroutine(FadePanel(false));
                GameObject.Find("ProloguePlayer4").GetComponent<EventManager>().StartEvent(0, 0);
                GameObject.Find("Player").transform.position = Vector3.zero;
                Destroy(GameObject.Find("Fragment"));
                GameObject.Find("Etoa").GetComponent<EventManager>().scenarioData = Resources.Load<TextAsset>("ScenarioDatas/etoa_allfragment");
                PlayerPrefs.SetInt("phase", 3);
                break;
            case 5:
                StartCoroutine(FadePanel(false));
                GameObject.Find("ProloguePlayer5").GetComponent<EventManager>().StartEvent(0, 0);
                GameObject.Find("Etoa").GetComponent<EventManager>().scenarioData = Resources.Load<TextAsset>("ScenarioDatas/etoa_afterbattlewin");
                Destroy(GameObject.Find("Fragment"));
                GameObject[] obj = GameObject.FindGameObjectsWithTag("EventObj");
                for (int i = 0; i < obj.Length; i++)
                {
                    if (obj[i].name.Contains("Stone")) obj[i].SetActive(false);
                }
                break;
            case 6:
                GameObject.Find("AllDataDeleted").GetComponent<EventManager>().StartEvent(0, 0);
                PlayerPrefs.DeleteKey("phase");
                PlayerPrefs.SetInt("haslost", 1);
                break;
        }
    }
    void Update()
    {
        if (isMenu) //メニューを開いているとき
        {
            if (Input.anyKeyDown)
            {
                if (!Input.GetKeyDown(KeyCode.Return)) audioSource[0].PlayOneShot(Resources.Load<AudioClip>("Sounds/Effects/click3"));
                else audioSource[0].PlayOneShot(Resources.Load<AudioClip>("Sounds/Effects/click2"));
            }

            if (!isItemCheck && !isOption && !isLoad) //どの画面も開かれていないとき
            {
                menuDescriptionLabel.text = menuDescriptionBaseString[menuSideIndex];
                if (Input.GetKeyDown(KeyCode.UpArrow)) menuSideIndex--;
                if (Input.GetKeyDown(KeyCode.DownArrow)) menuSideIndex++;
                menuSideIndex = (int)Mathf.Clamp(menuSideIndex, 0, 3);
                if (Input.anyKeyDown)
                {
                    menuSideLabel.text = "";
                    for (int i = 0; i < 4; i++)
                    {
                        if (i == menuSideIndex) menuSideLabel.text += "・" + menuSideBaseString[i];
                        else menuSideLabel.text += "　" + menuSideBaseString[i];
                    }
                }
            }
            if (isItemCheck) //持ち物画面
            {
                if (Input.GetKeyDown(KeyCode.UpArrow)) inventoryIndex -= 2;
                if (Input.GetKeyDown(KeyCode.DownArrow)) inventoryIndex += 2;
                if (Input.GetKeyDown(KeyCode.LeftArrow)) inventoryIndex--;
                if (Input.GetKeyDown(KeyCode.RightArrow)) inventoryIndex++;
                if (Input.anyKeyDown) SetItemCheckLabel();
            }
            if (isOption) //オプション画面
            {
                menuMainLabel.text = "　音楽の大きさ:" + musicVolume.ToString("f2") + "\n" + "効果音の大きさ:" + soundEffectVolume.ToString("f2") + "\n";
            }
            if (isLoad) //ロード
            {
                if (Input.GetKeyDown(KeyCode.UpArrow)) currentSaveSlotNum--;
                if (Input.GetKeyDown(KeyCode.DownArrow)) currentSaveSlotNum++;
                if (Input.GetKeyDown(KeyCode.LeftArrow)) currentSaveSlotNum = 0; ;
                if (Input.GetKeyDown(KeyCode.RightArrow)) currentSaveSlotNum = maxSaveSlotNum;
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (System.IO.File.Exists("save/" + currentSaveSlotNum.ToString() + ".txt"))
                    {
                        if (!IsSaveDataExist(currentSaveSlotNum)) menuDescriptionLabel.text = "セーブデータが破損しています。";
                        else
                        {
                            menuDescriptionLabel.text = "";
                            LoadData(currentSaveSlotNum);
                            menuSideIndex = 0;
                            menuMainLabel.text = "";
                            menuDescriptionLabel.text = "";
                            isLoad = false;
                            isMenu = false;
                            menuGroup.SetActive(false);
                            SetEventActiveFalse();
                        }
                    }
                    else menuDescriptionLabel.text = "セーブデータが存在しません。";
                }
                if (Input.anyKeyDown)
                {
                    if (!Directory.Exists("save")) Directory.CreateDirectory("save");
                    CheckSavedDatas();
                    saveFileInfos = new System.IO.DirectoryInfo("save").GetFiles("*.txt", System.IO.SearchOption.AllDirectories);
                    maxSaveSlotNum = saveFileInfos.Length;
                    currentSaveSlotNum = (int)Mathf.Clamp(currentSaveSlotNum, 0, maxSaveSlotNum - 1);
                    if (maxSaveSlotNum != 0) SetSaveDataLabel(menuMainLabel, currentSaveSlotNum);
                    if (!Input.GetKeyDown(KeyCode.Return)) audioSource[0].PlayOneShot(Resources.Load<AudioClip>("Sounds/Effects/click3"));
                    else audioSource[0].PlayOneShot(Resources.Load<AudioClip>("Sounds/Effects/click2"));
                }
            }
            /**   if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.LeftShift))
               {
                   if (isItemCheck || isOption || isLoad)
                   {
                       optionGroup.SetActive(false);
                       menuMainLabel.text = "";
                       isItemCheck = false;
                       isOption = false;
                       isLoad = false;
                   }
                   else
                   {
                       menuMainLabel.text = "";
                       isMenu = false;
                       menuSideIndex = 0;
                       isItemCheck = false;
                       isOption = false;
                       menuGroup.SetActive(isMenu);
                   }
               }*/
        }
        else
        {
            /* if (!isSave)
             {
                 if (Input.GetKeyDown(KeyCode.Escape)) //メニュー切り替え
                 {
                     if (!isMenu) audioSource[0].PlayOneShot(Resources.Load<AudioClip>("Sounds/Effects/click3"));
                     isMenu = !isMenu;
                     menuMainLabel.text = "";
                     if (!isMenu)
                     {
                         menuSideIndex = 0;
                         isItemCheck = false;
                         isOption = false;
                         isLoad = false;
                     }
                     menuDescriptionLabel.text = menuDescriptionBaseString[menuSideIndex];
                     if (Input.anyKeyDown)
                     {
                         menuSideLabel.text = "";
                         for (int i = 0; i < 4; i++)
                         {
                             if (i == menuSideIndex) menuSideLabel.text += "・" + menuSideBaseString[i];
                             else menuSideLabel.text += "　" + menuSideBaseString[i];
                         }
                     }
                     menuGroup.SetActive(isMenu);
                 }
             }*/
        }
    }
    void LateUpdate()
    {
        /*  if (isSave) //保存する時
          {
              if (Input.GetKeyDown(KeyCode.UpArrow)) currentSaveSlotNum--;
              if (Input.GetKeyDown(KeyCode.DownArrow)) currentSaveSlotNum++;
              if (Input.GetKeyDown(KeyCode.LeftArrow)) currentSaveSlotNum = 0; ;
              if (Input.GetKeyDown(KeyCode.RightArrow)) currentSaveSlotNum = maxSaveSlotNum;
              if (Input.GetKeyDown(KeyCode.Return))
              {
                  SaveData(currentSaveSlotNum);
                  isSave = false;
                  saveDataGroup.SetActive(false);
                  audioSource[0].PlayOneShot(Resources.Load<AudioClip>("Sounds/Effects/click2"));
              }
              if (Input.GetKeyDown(KeyCode.Escape))
              {
                  isSave = false;
                  saveDataGroup.SetActive(false);
              }
              if (Input.anyKeyDown)
              {
                  if (!Input.GetKeyDown(KeyCode.Return)) audioSource[0].PlayOneShot(Resources.Load<AudioClip>("Sounds/Effects/click3"));
                  else audioSource[0].PlayOneShot(Resources.Load<AudioClip>("Sounds/Effects/click2"));
                  currentSaveSlotNum = (int)Mathf.Clamp(currentSaveSlotNum, 0, maxSaveSlotNum);
                  if (maxSaveSlotNum != 0)
                  {
                      SetSaveDataLabel(saveDataLabel, currentSaveSlotNum);
                      if (maxSaveSlotNum == currentSaveSlotNum) saveDataLabel.text += "・新規";
                      else saveDataLabel.text += "　新規";
                  }
              }
          }
          if (isMenu && !isItemCheck && !isOption && !isLoad)//メニュー画面で項目を選択する
          {
              if (Input.GetKeyDown(KeyCode.Return))//エンターキーが押されたら
              {
                  switch (menuSideIndex)
                  {
                      case 0:
                          isItemCheck = true;
                          SetItemCheckLabel();
                          break;
                      case 1:
                          isOption = true;
                          optionGroup.SetActive(true);
                          break;
                      case 2:
                          isLoad = true;
                          SetSaveDataLabel(menuMainLabel, currentSaveSlotNum);
                          if (maxSaveSlotNum == 0) menuMainLabel.text = "セーブデータが存在しません。";
                          break;
                      case 3:
                          SceneManager.LoadScene("Title");
                          break;
                      default:
                          Debug.Log("Menu Side Index Error");
                          break;
                  }
              }
          }*/
        if (isSelectionOnMessageWindow) //会話中の選択肢
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) selectionIndex -= 2;
            if (Input.GetKeyDown(KeyCode.DownArrow)) selectionIndex += 2;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) selectionIndex--;
            if (Input.GetKeyDown(KeyCode.RightArrow)) selectionIndex++;
            selectionIndex = (int)Mathf.Clamp(selectionIndex, 0, selections.Length - 1);
            if (Input.anyKeyDown)
            {
                if (!Input.GetKeyDown(KeyCode.Return)) audioSource[0].PlayOneShot(Resources.Load<AudioClip>("Sounds/Effects/click3"));
                else audioSource[0].PlayOneShot(Resources.Load<AudioClip>("Sounds/Effects/click2"));
                messageLabel.text = messages[messages.Length - 1] + "\n";
                for (int i = 0; i < selections.Length; i++)
                {
                    if (i == selectionIndex)
                    {
                        if (i % 2 == 0) messageLabel.text += "・" + selections[i].PadRight(9, '　');
                        else messageLabel.text += "・" + selections[i].PadRight(9, '　') + "\n";
                    }
                    else
                    {
                        if (i % 2 == 0) messageLabel.text += "　" + selections[i].PadRight(9, '　');
                        else messageLabel.text += "　" + selections[i].PadRight(9, '　') + "\n";
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Return)) //選択肢を決定したとき
            {
                // currentTextIndex += messages.Length;
                switch (selectionsPrefixs[selectionIndex].Substring(0, 1)) //設定された分岐飛ばし先に応じて処理を決定
                {
                    case "r": //会話ルート分岐
                        currentEventManager.StartEvent(int.Parse(selectionsPrefixs[selectionIndex].Replace("r", "")), currentTextIndex - 1);
                        break;
                    case "d": //会話終了
                        isSelectionOnMessageWindow = false;
                        isEvent = false;
                        messageGroup.SetActive(false);
                        FinishEvent();
                        break;
                    case "s": //保存
                        SetSaveDataLabel(saveDataLabel, currentSaveSlotNum);
                        if (maxSaveSlotNum == 0) saveDataLabel.text += "・新規";
                        else saveDataLabel.text += "　新規";
                        isSave = true;
                        saveDataGroup.SetActive(true);
                        isEvent = false;
                        messageGroup.SetActive(false);
                        FinishEvent();
                        break;
                    case "g": //全体分岐指定
                        break;
                    case "i": //全体フラグ条件付きルート分岐 //TODO
                        isSelectionOnMessageWindow = false;
                        if (!rinneTalkMode) currentEventManager.StartEvent(0, currentTextIndex); //仮
                        else currentEventManager.StartEvent(1, currentTextIndex); //仮
                        break;
                    case "f": //フラグ変更
                        int flagIndex = int.Parse(selectionsPrefixs[selectionIndex].Replace("f", ""));
                        flags[flagIndex] = true;
                        OnFlagChanged(flagIndex);
                        break;
                    case "n": //何もしない
                        /* currentTextIndex++;
                         currentEventManager.StartEvent(0, currentTextIndex);*/
                        break;
                    case "p": //暗転又は明転
                        StartCoroutine(FadePanel(Convert.ToBoolean(selectionsPrefixs[selectionIndex].Replace("p", ""))));
                        break;
                    case "t":
                        isSelectionOnMessageWindow = false;
                        currentTextIndex++;
                        currentEventManager.StartEvent(Convert.ToInt32(selectionsPrefixs[selectionIndex].Replace("t", "")), currentTextIndex);
                        break;
                    case "j"://今のイベントを終了し、別のイベントを即座に開始
                        //終了
                        isEvent = false;
                        messageGroup.SetActive(false);
                        isSelectionOnMessageWindow = false;
                        //開始
                        int firstSeparationIndex = selectionsPrefixs[selectionIndex].IndexOf("^");
                        GameObject eventObj = GameObject.Find(selectionsPrefixs[selectionIndex].Substring(1, firstSeparationIndex - 1));
                        int secondSeparationIndex = selectionsPrefixs[selectionIndex].IndexOf("^", firstSeparationIndex + 1);
                        int currentRoute = Convert.ToInt32(selectionsPrefixs[selectionIndex].Substring(firstSeparationIndex + 1, secondSeparationIndex - firstSeparationIndex - 1));
                        int startTextIndex = Convert.ToInt32(selectionsPrefixs[selectionIndex].Substring(secondSeparationIndex + 1, selectionsPrefixs[selectionIndex].Length - secondSeparationIndex - 1));
                        currentTextIndex = startTextIndex;
                        eventObj.GetComponent<EventManager>().StartEvent(currentRoute, startTextIndex);
                        break;
                    default:
                        Debug.Log("Selection Prefix Error:" + selectionsPrefixs[selectionIndex]);
                        break;
                }
                selectionIndex = 0;
                isSelectionOnMessageWindow = false;
            }
        }

    }
    public void ChangeRinneMode() //temp, debug only
    {
        rinneTalkMode = !rinneTalkMode;
    }
    private void SetEventActiveFalse() //あまり使いたくないが、どうしようもなくなった時にのみ使う
    {
        playerManager.isEventActive = false;
        Invoke("setEventActiveTrue", Time.deltaTime);
    }
    private void SetEventActiveTrue() //同上
    {
        playerManager.isEventActive = true;
    }
    public void ChangeVolume(int n) //n=0:music n=1:soundeffect
    {
        switch (n)
        {
            case 0:
                musicVolume = musicVolumeSlider.value;
                break;
            case 1:
                soundEffectVolume = soundEffecrVolumeSlider.value;
                playerManager.SetVolume();
                break;
            default:
                Debug.Log("Slider Value Error:" + n);
                break;
        }
    }
    private void SetItemCheckLabel()
    {
        filledInventoryAmount = 0;
        while (_items.GetItemData(inventory[filledInventoryAmount]).id != -1)
        {
            filledInventoryAmount++;
        }
        inventoryIndex = (int)Mathf.Clamp(inventoryIndex, 0, filledInventoryAmount - 1);

        menuMainLabel.text = "";
        for (int i = 0; i < filledInventoryAmount; i++)
        {
            if (i == inventoryIndex) menuMainLabel.text += ("▶" + _items.GetItemData(inventory[i]).name).PadRight(10, '　');
            else menuMainLabel.text += ("・" + _items.GetItemData(inventory[i]).name).PadRight(10, '　');
        }
        menuDescriptionLabel.text = _items.GetItemData(inventory[inventoryIndex]).description;
    }
    public void SetSaveDataLabel(TextMeshProUGUI label, int selectedSlotNum)
    {
        if (!Directory.Exists("save")) Directory.CreateDirectory("save");
        CheckSavedDatas();
        saveFileInfos = new System.IO.DirectoryInfo("save").GetFiles("*.txt", System.IO.SearchOption.AllDirectories);
        label.text = "";
        maxSaveSlotNum = saveFileInfos.Length;
        for (int i = 0; i < maxSaveSlotNum; i++)
        {
            if (System.IO.File.Exists("save/" + i.ToString() + ".txt"))
            {
                //   if (Path.GetFileName(saveFileInfos[i].FullName).Equals(i.ToString() + ".txt"))
                //   {
                if (i == selectedSlotNum) label.text += "・" + i.ToString().PadRight(8, ' ') + System.IO.File.GetLastWriteTime(saveFileInfos[i].FullName).ToString() + "\n";
                else label.text += "　" + i.ToString().PadRight(8, ' ') + System.IO.File.GetLastWriteTime(saveFileInfos[i].FullName).ToString() + "\n";
                //  }
            }
            else
            {
                if (i == selectedSlotNum) label.text += "・" + i.ToString().PadRight(8, ' ') + "空\n";
                else label.text += "　" + i.ToString().PadRight(8, ' ') + "空\n";
            }
        }
    }
    public void CheckSavedDatas() //saveフォルダ内のセーブファイルを確認する
    {
        if (!Directory.Exists("save")) Directory.CreateDirectory("save");
        saveFileInfos = new System.IO.DirectoryInfo("save").GetFiles("*.txt", System.IO.SearchOption.AllDirectories);
        for (int i = 0; i < saveFileInfos.Length; i++)
        {
            int fileNumber = Convert.ToInt32(Path.GetFileName(saveFileInfos[i].FullName.Substring(0, saveFileInfos[i].FullName.IndexOf("."))));
            if (i != fileNumber)
            {
                StreamWriter sw = new StreamWriter("save/" + i.ToString() + ".txt", false, Encoding.UTF8);
                sw.Close();
                saveFileInfos = new System.IO.DirectoryInfo("save").GetFiles("*.txt", System.IO.SearchOption.AllDirectories);
            }
        }
    }
    public void SaveData(int slot) //進行状況を保存する
    {
        if (!Directory.Exists("save")) Directory.CreateDirectory("save");
        StreamWriter sw = new StreamWriter("save/" + slot.ToString() + ".txt", false, Encoding.UTF8);
        sw.Write(".");
        foreach (bool b in flags) sw.Write(Convert.ToByte(b));
        sw.Write(":");
        sw.Write(GameObject.Find("Player").transform.position.x + ",");
        sw.Write(GameObject.Find("Player").transform.position.y + ",");
        sw.Close();
    }
    public bool IsSaveDataExist(int slot)//指定された番号のセーブデータが存在、破損しているか確認し、存在しないか又は破損しているならfalse、問題なく存在するならtrueを返す。
    {
        StreamReader sr = new StreamReader("save/" + slot.ToString() + ".txt");
        string str = sr.ReadToEnd();
        sr.Close();
        if (str.Length == 0 || !str.Substring(0, 1).Equals(".")) return false;
        else return true;

    }
    public void LoadData(int slot)//この関数を使う前には上記のセーブデータが不正か否かを判定する関数を用いて条件分岐させる。
    {
        StreamReader sr = new StreamReader("save/" + slot.ToString() + ".txt");
        string str = sr.ReadToEnd();
        sr.Close();
        int index = 1;
        while (str.Substring(index, 1) != ":")
        {
            flags[index - 1] = Convert.ToBoolean(Convert.ToByte(str.Substring(index, 1)));
            index++;
        }
        index++;
        GameObject player = GameObject.Find("Player");
        float posx = float.Parse(str.Substring(index, str.IndexOf(",", index) - index));
        index = str.IndexOf(",", index) + 1;
        float posy = float.Parse(str.Substring(index, str.IndexOf(",", index) - index));
        player.transform.position = new Vector3(posx, posy, 0);
    }
    public IEnumerator PlayMessage(bool isCenterMessage, bool isNameBlank) //会話
    {
        isEvent = true;
        TextMeshProUGUI label;
        if (isCenterMessage) label = GameObject.Find("CenterLabel").GetComponent<TextMeshProUGUI>();
        else
        {
            messageGroup.SetActive(true);
            label = messageLabel;
            if (isNameBlank) GameObject.Find("NameBox").GetComponent<Image>().color = new Color(1, 1, 1, 0);
            else GameObject.Find("NameBox").GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }
        int charCount = 0;
        int index = 0;
        while (index < messages.Length)
        {
            charCount = 0;
            if (!isCenterMessage) nameLabel.text = names[index];

            centerImage[0].sprite = center_base[index];
            centerImage[1].sprite = center_eyeblow[index];
            centerImage[2].sprite = center_eye[index];
            centerImage[3].sprite = center_nose[index];
            centerImage[4].sprite = center_mouth[index];
            centerImage[5].sprite = center_face[index];
            leftImage[0].sprite = left_base[index];
            leftImage[1].sprite = left_eyeblow[index];
            leftImage[2].sprite = left_eye[index];
            leftImage[3].sprite = left_nose[index];
            leftImage[4].sprite = left_mouth[index];
            leftImage[5].sprite = left_face[index];
            rightImage[0].sprite = right_base[index];
            rightImage[1].sprite = right_eyeblow[index];
            rightImage[2].sprite = right_eye[index];
            rightImage[3].sprite = right_nose[index];
            rightImage[4].sprite = right_mouth[index];
            rightImage[5].sprite = right_face[index];

            switch (messageTalkPosition[index])
            {
                case "C":
                    for (int i = 0; i < 6; i++)
                    {
                        centerImage[i].color = Color.white;
                        leftImage[i].color = Color.gray;
                        rightImage[i].color = Color.gray;
                    }
                    break;
                case "L":
                    for (int i = 0; i < 6; i++)
                    {
                        centerImage[i].color = Color.gray;
                        leftImage[i].color = Color.white;
                        rightImage[i].color = Color.gray;
                    }
                    break;
                case "R":
                    for (int i = 0; i < 6; i++)
                    {
                        centerImage[i].color = Color.gray;
                        leftImage[i].color = Color.gray;
                        rightImage[i].color = Color.white;
                    }
                    break;
                default:
                    Debug.Log("Message Talk Position Error:" + messageTalkPosition[index]);
                    break;
            }

            if (index == messages.Length - 1 && selections.Length != 0) //選択肢が存在する場合の例外処理
            {
                string str = messages[index];
                str += "\n";
                for (int i = 0; i < selections.Length; i++)
                {
                    if (i == 0) str += "・" + selections[i].PadRight(9, '　');
                    else if (i % 2 == 0) str += "　" + selections[i].PadRight(9, '　');
                    else str += "　" + selections[i].PadRight(9, '　') + "\n";
                }
                int strIndex = 0;
                while (strIndex < str.Length)
                {
                    strIndex++;
                    label.text = str.Substring(0, strIndex);
                    yield return new WaitForSeconds(timePerChar);

                }
                isSelectionOnMessageWindow = true;
            }
            if (isSelectionOnMessageWindow) yield break;

            while (charCount < messages[index].Length) //通常の文字送り処理
            {
                charCount++;
                label.text = messages[index].Substring(0, charCount);
                yield return new WaitForSeconds(timePerChar);
            }
            if (!isSelectionOnMessageWindow) while (!Input.GetKeyDown(KeyCode.Return)) yield return null;
            if (!messageControlPrefixs[index].Equals("n")) //文末の文字による特殊処理指示
            {
                string[] controllers = new string[1];
                if (messageControlPrefixs[index].IndexOf(":") != -1)
                {
                    controllers = new string[messageControlPrefixs[index].Length - messageControlPrefixs[index].Replace(":", "").Length + 1];
                    int currentIndex = messageControlPrefixs[index].IndexOf(":");
                    int formerIndex = 0;
                    for (int i = 0; i < controllers.Length; i++)
                    {
                        if (i == 0) controllers[i] = messageControlPrefixs[index].Substring(0, currentIndex);
                        else if (i == controllers.Length - 1) controllers[i] = messageControlPrefixs[index].Substring(formerIndex + 1, messageControlPrefixs[index].Length - formerIndex - 1);
                        else controllers[i] = messageControlPrefixs[index].Substring(formerIndex + 1, currentIndex - formerIndex - 1);
                        formerIndex = currentIndex;
                        currentIndex = messageControlPrefixs[index].IndexOf(":", currentIndex + 1);
                    }
                }
                else controllers[0] = messageControlPrefixs[index];

                for (int i = 0; i < controllers.Length; i++)
                {
                    switch (controllers[i].Substring(0, 1)) //設定された分岐飛ばし先に応じて処理を決定
                    {
                        case "d": //会話終了
                            isEvent = false;
                            FinishEvent();
                            currentTextIndex = 0;
                            label.text = "";
                            if (isNameBlank) GameObject.Find("NameBox").GetComponent<Image>().color = new Color(1, 1, 1, 1);
                            messageGroup.SetActive(false);
                            break;
                        case "f": //フラグ変更
                            int flagIndex = int.Parse(controllers[i].Replace("f", ""));
                            flags[flagIndex] = true;
                            OnFlagChanged(flagIndex);
                            break;
                        case "p": //暗転又は明転
                            StartCoroutine(FadePanel(Convert.ToBoolean(Convert.ToInt32(controllers[i].Replace("p", "")))));
                            break;
                        case "j"://今のイベントを終了し、別のイベントを即座に開始
                            {
                                //終了
                                isEvent = false;
                                label.text = "";
                                if (isNameBlank) GameObject.Find("NameBox").GetComponent<Image>().color = new Color(1, 1, 1, 1);
                                messageGroup.SetActive(false);
                                //開始
                                int firstSeparationIndex = controllers[i].IndexOf("^");
                                GameObject eventObj = GameObject.Find(controllers[i].Substring(1, firstSeparationIndex - 1));
                                int secondSeparationIndex = controllers[i].IndexOf("^", firstSeparationIndex + 1);
                                int currentRoute = Convert.ToInt32(controllers[i].Substring(firstSeparationIndex + 1, secondSeparationIndex - firstSeparationIndex - 1));
                                int startTextIndex = Convert.ToInt32(controllers[i].Substring(secondSeparationIndex + 1, controllers[i].Length - secondSeparationIndex - 1));
                                currentTextIndex = startTextIndex;
                                eventObj.GetComponent<EventManager>().StartEvent(currentRoute, startTextIndex);
                            }
                            yield break;
                        case "l":
                            {
                                //終了
                                isEvent = false;
                                label.text = "";
                                if (isNameBlank) GameObject.Find("NameBox").GetComponent<Image>().color = new Color(1, 1, 1, 1);
                                messageGroup.SetActive(false);
                                //開始
                                int firstSeparationIndex = controllers[i].IndexOf("^");
                                GameObject eventObj = GameObject.Find(controllers[i].Substring(1, firstSeparationIndex - 1));
                                int secondSeparationIndex = controllers[i].IndexOf("^", firstSeparationIndex + 1);
                                int currentRoute = Convert.ToInt32(controllers[i].Substring(firstSeparationIndex + 1, secondSeparationIndex - firstSeparationIndex - 1));
                                int thirdSeparationIndex = controllers[i].IndexOf("^", secondSeparationIndex + 1);
                                int startTextIndex = Convert.ToInt32(controllers[i].Substring(secondSeparationIndex + 1, thirdSeparationIndex - secondSeparationIndex - 1));
                                float delayTIme = float.Parse(controllers[i].Substring(thirdSeparationIndex + 1, controllers[i].Length - thirdSeparationIndex - 1));
                                yield return new WaitForSeconds(delayTIme);
                                currentTextIndex = startTextIndex;
                                eventObj.GetComponent<EventManager>().StartEvent(currentRoute, startTextIndex);
                            }
                            yield break;
                        case "m":
                            {
                                if (GameObject.Find("OverlapPanel").GetComponent<Image>().color == new Color(0, 0, 0, 0)) yield return StartCoroutine(FadePanel(true));
                                int firstSeparationIndex = controllers[i].IndexOf("^");
                                SceneManager.LoadScene(controllers[i].Substring(1, firstSeparationIndex - 1));
                            }
                            break;
                        case "t":
                            currentEventManager.StartEvent(Convert.ToInt32(selectionsPrefixs[selectionIndex].Replace("t", "")), index + 1);
                            break;
                        case "s":
                            PlayerPrefs.SetInt("phase", PlayerPrefs.GetInt("phase") + 1);
                            if (PlayerPrefs.GetInt("phase") == 4)
                            {
                                GameObject.Find("Player").transform.position = new Vector3(-17.5f, 16.5f, 0);
                                GameObject.Find("Etoa").GetComponent<EnemyEtoaManager>().isBattleStarted = true;
                                GameObject[] obj = GameObject.FindGameObjectsWithTag("EventObj");
                                audioSource[1].clip = Resources.Load<AudioClip>("Sounds/Musics/Etoa");
                                audioSource[1].Play();
                                for (int j = 0; j < obj.Length; j++)
                                {
                                    if (obj[j].name.Contains("Stone")) obj[j].GetComponent<EventManager>().scenarioData = Resources.Load<TextAsset>("ScenarioDatas/pickupstone");
                                }
                                finalWall.SetActive(true);
                                battleGroup.SetActive(true);
                                stoneCountLabel = GameObject.Find("StoneCount").GetComponent<TextMeshProUGUI>();
                                etoaHealthGauge = GameObject.Find("EtoaHealth").GetComponent<Image>();
                            }
                            break;
                        case "b":
                            isEvent = false;
                            FinishEvent();
                            Destroy(currentEventManager.gameObject);
                            stoneCount++;
                            stoneCountLabel.text = "×" + stoneCount; ;
                            break;
                        default:
                            Debug.Log("Meessage Control Prefix Error:" + controllers[i]);
                            break;
                    }
                }
            }
            index++;
            currentTextIndex++;
        }
        while (!Input.GetKeyDown(KeyCode.Return)) yield return null;
        isEvent = false;
        FinishEvent();
        currentTextIndex = 0;
        label.text = "";
        if (isNameBlank) GameObject.Find("NameBox").GetComponent<Image>().color = new Color(1, 1, 1, 1);
        messageGroup.SetActive(false);
    }

    IEnumerator FadePanel(bool fadeMode)//false:パネルが透明になる, true:パネルが黒くなる
    {
        Image overlapPanel = GameObject.Find("OverlapPanel").GetComponent<Image>();
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

    IEnumerator FadeBackSound(bool fademode)
    {
        float t = 0;
        while (t <= 1)
        {
            if (!fademode) audioSource[1].volume = Mathf.Lerp(0, 0.5f, t);
            else audioSource[1].volume = Mathf.Lerp(0.5f, 0, t);
            yield return new WaitForFixedUpdate();
            t += Time.fixedDeltaTime;
        }
        yield break;
    }

    private void FinishEvent()
    {
        messages = new string[0];
        names = new string[0];
        messageControlPrefixs = new string[0];
        selections = new string[0];
        selectionsPrefixs = new string[0];
        //  currentEventManager = null;
    }

    private void OnFlagChanged(int index)
    {
        switch (index)
        {
            case 0:
                GameObject.Find("Etoa").GetComponent<EventManager>().scenarioData = Resources.Load<TextAsset>("ScenarioDatas/etoa_after");
                EventManager eventManager = GameObject.Find("Fragment").GetComponent<EventManager>();
                eventManager.scenarioData = Resources.Load<TextAsset>("ScenarioDatas/findfragment");
                eventManager.isNameBlankOnMessage = false;
                break;
        }
    }
    public IEnumerator AfterBattle(bool b) //true:win, false:lose
    {
        GameObject etoa = GameObject.Find("Etoa");
        EnemyEtoaManager enemyEtoaManager = etoa.GetComponent<EnemyEtoaManager>();
        Coroutine coroutine = StartCoroutine(FadePanel(true));
        if (b)
        {
            yield return coroutine;
            etoa.transform.position = new Vector3(-3.5f, 29, 0);
            enemyEtoaManager.sr.sprite = enemyEtoaManager.sprites[6];
            /*    coroutine = StartCoroutine(FadePanel(false));
                yield return coroutine;*/
            // GameObject.Find("BattleWin").GetComponent<EventManager>().StartEvent(0, 0);
            SceneManager.LoadScene("Ending");
        }
        else
        {
            yield return coroutine;
            SceneManager.LoadScene("Title");
        }
        yield break;
    }
}
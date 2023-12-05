using UnityEngine;
using System.IO;
using System.Text;
using System;
public class EventManager : MonoBehaviour
{
    public bool isMessageEvent, isFlagChangeEvent, isItemCheckEvent, isSelectionOnMessageWindowEvent, isPerson, isCenterBlank, isLeftBlank, isRightBlank, isCenterMessageEvent, isNameBlankOnMessage;
    [SerializeField] float timePerChar;
    public TextAsset scenarioData;
    public Sprite[] spriteOnMap = new Sprite[8];
    [SerializeField] string[] messages, names, messageControlPrefixs, selections, selectionsPrefixs, messageTalkPosition;
    [SerializeField] Sprite[] center_base, center_eye, center_eyeblow, center_mouth, center_nose, center_face;
    [SerializeField] Sprite[] left_base, left_eye, left_eyeblow, left_mouth, left_nose, left_face;
    [SerializeField] Sprite[] right_base, right_eye, right_eyeblow, right_mouth, right_nose, right_face;
    private Sprite blank;
    void Start()
    {

    }
    public void StartEvent(int currentRoute, int startTextIndex)
    {
        isLeftBlank = true;
        isCenterBlank = true;
        isRightBlank = true;
        blank = Resources.Load<Sprite>("Textures/UI/characterBlank");
        GetScenarioData(currentRoute, startTextIndex);
        int len = messages.Length;
        if (isCenterBlank)
        {
            center_base = new Sprite[len];
            center_eye = new Sprite[len];
            center_eyeblow = new Sprite[len];
            center_face = new Sprite[len];
            center_mouth = new Sprite[len];
            center_nose = new Sprite[len];
            for (int i = 0; i < len; i++)
            {
                center_base[i] = blank;
                center_eye[i] = blank;
                center_eyeblow[i] = blank;
                center_face[i] = blank;
                center_mouth[i] = blank;
                center_nose[i] = blank;
            }
        }
        if (isLeftBlank)
        {
            left_base = new Sprite[len];
            left_eye = new Sprite[len];
            left_eyeblow = new Sprite[len];
            left_face = new Sprite[len];
            left_mouth = new Sprite[len];
            left_nose = new Sprite[len];
            for (int i = 0; i < len; i++)
            {
                left_base[i] = blank;
                left_eye[i] = blank;
                left_eyeblow[i] = blank;
                left_face[i] = blank;
                left_mouth[i] = blank;
                left_nose[i] = blank;
            }
        }
        if (isRightBlank)
        {
            right_base = new Sprite[len];
            right_eye = new Sprite[len];
            right_eyeblow = new Sprite[len];
            right_face = new Sprite[len];
            right_mouth = new Sprite[len];
            right_nose = new Sprite[len];
            for (int i = 0; i < len; i++)
            {
                right_base[i] = blank;
                right_eye[i] = blank;
                right_eyeblow[i] = blank;
                right_face[i] = blank;
                right_mouth[i] = blank;
                right_nose[i] = blank;
            }
        }
        if (isMessageEvent)
        {
            GameManager gameManager = GameObject.Find("Manager").GetComponent<GameManager>();
            gameManager.currentEventManager = GetComponent<EventManager>();
            gameManager.timePerChar = timePerChar;
            gameManager.messages = messages;
            gameManager.names = names;
            gameManager.messageControlPrefixs = messageControlPrefixs;
            gameManager.messageTalkPosition = messageTalkPosition;

            gameManager.center_base = center_base;
            gameManager.center_eye = center_eye;
            gameManager.center_eyeblow = center_eyeblow;
            gameManager.center_mouth = center_mouth;
            gameManager.center_nose = center_nose;
            gameManager.center_face = center_face;

            gameManager.left_base = left_base;
            gameManager.left_eye = left_eye;
            gameManager.left_eyeblow = left_eyeblow;
            gameManager.left_mouth = left_mouth;
            gameManager.left_nose = left_nose;
            gameManager.left_face = left_face;

            gameManager.right_base = right_base;
            gameManager.right_eye = right_eye;
            gameManager.right_eyeblow = right_eyeblow;
            gameManager.right_mouth = right_mouth;
            gameManager.right_nose = right_nose;
            gameManager.right_face = right_face;


            if (isSelectionOnMessageWindowEvent)
            {
                gameManager.selections = selections;
                gameManager.selectionsPrefixs = selectionsPrefixs;
            }

            StartCoroutine(gameManager.PlayMessage(isCenterMessageEvent, isNameBlankOnMessage));
        }
    }
    public void GetScenarioData(int currentRoute, int startTextIndex)
    {
        //全部初期化
        messages = new string[0];
        names = new string[0];
        selections = new string[0];
        string str = scenarioData.text;
        int strIndex = 0; //何文字目か
        int textIndex = 0; //messagesとnamesのインデックス
        bool isSelection = false; //選択肢か
        bool isRouteEmpty = false; //現行のルートは会話終了しているか

        int semicolonCount = 0;
        //会話情報の読み込み
        while (semicolonCount < startTextIndex) //開始位置まで飛ばす
        {
            if (str.Substring(strIndex, 1).Equals(";")) semicolonCount++;
            strIndex++;
        }

        int separationCount = 0;
        while (strIndex < str.IndexOf("_") && !(isSelection && str.Substring(strIndex, 1).Equals("|")))
        {
            Array.Resize(ref messages, textIndex + 1);
            Array.Resize(ref names, textIndex + 1);
            Array.Resize(ref messageControlPrefixs, textIndex + 1);
            messageControlPrefixs[textIndex] = "n";
            /*   for (int i = 0; i <= currentRoute; i++)
               {
                   strIndex = str.IndexOf("%", strIndex) + 1;
               }*/
            separationCount = 0;
            while (separationCount <= currentRoute)
            {
                if (str.Substring(strIndex, 1).Equals("|")) isSelection = !isSelection;
                if (str.Substring(strIndex, 1).Equals("%") && !isSelection) separationCount++;
                if (str.Substring(strIndex, 1).Equals(";")) separationCount = 0;
                strIndex++;
            }
            if (strIndex >= str.IndexOf("_") - 1) //ループ内でstrIndexが超えてしまったときは即座に終了する
            {
                Array.Resize(ref messages, textIndex);
                Array.Resize(ref names, textIndex);
                Array.Resize(ref messageControlPrefixs, textIndex);
                break;
            }
            isSelection = false;
            while (!str.Substring(strIndex, 1).Equals("$")) //名前取得
            {
                if (str.Substring(strIndex, 1).Equals("%"))
                {
                    isRouteEmpty = true;
                    break;
                }
                names[textIndex] += str.Substring(strIndex, 1);
                strIndex++;
            }
            if (isRouteEmpty)
            {
                Array.Resize(ref messages, textIndex);
                Array.Resize(ref names, textIndex);
                Array.Resize(ref messageControlPrefixs, textIndex);
                break;
            }
            strIndex++;
            while (!str.Substring(strIndex, 1).Equals(":") && !str.Substring(strIndex, 1).Equals(";") && !str.Substring(strIndex, 1).Equals("|") && !str.Substring(strIndex, 1).Equals("%")) //会話文取得
            {
                messages[textIndex] += str.Substring(strIndex, 1);
                strIndex++;
            }
            switch (str.Substring(strIndex, 1)) //会話文後の記号判別
            {
                case ":": //分岐飛ばし
                    if (str.IndexOf(";", strIndex) > str.IndexOf("%", strIndex)) messageControlPrefixs[textIndex] = str.Substring(strIndex + 1, str.IndexOf("%", strIndex) - strIndex - 1);
                    else messageControlPrefixs[textIndex] = str.Substring(strIndex + 1, str.IndexOf(";", strIndex) - strIndex - 1);
                    strIndex = str.IndexOf(";", strIndex);
                    break;
                case ";": //文の終了
                    break;
                case "|": //選択肢の開始
                    isSelection = true;
                    break;
                case "%": //ルート区切り
                    strIndex = str.IndexOf(";", strIndex);
                    break;
                default: //それ以外ならエラー
                    Debug.Log("Scenario Data Load Error:" + str.Substring(strIndex, 1));
                    break;
            }
            strIndex++;
            int selectionsCount = 0;
            bool isSelectionLengthOver = false;
            while (isSelection && !isSelectionLengthOver) //選択肢なら
            {
                Array.Resize(ref selections, selectionsCount + 1);
                Array.Resize(ref selectionsPrefixs, selectionsCount + 1);
                while (!str.Substring(strIndex, 1).Equals(":") && !str.Substring(strIndex, 1).Equals("|"))
                {
                    selections[selectionsCount] += str.Substring(strIndex, 1);
                    strIndex++;
                }
                switch (str.Substring(strIndex, 1))
                {
                    case ":":
                        selectionsPrefixs[selectionsCount] = str.Substring(strIndex + 1, str.IndexOf("%", strIndex) - strIndex - 1);
                        strIndex = str.IndexOf("%", strIndex);
                        break;
                    case "|":
                        isSelectionLengthOver = true;
                        strIndex += 2;
                        break;
                    default:
                        Debug.Log("Scenario Data Selection Error:" + str.Substring(strIndex, 1));
                        break;
                }
                strIndex++;
                selectionsCount++;
            }
            if (isSelection)
            {
                //上のループを抜けるときに余分に1が足されている
                selectionsCount--;
                Array.Resize(ref selections, selectionsCount);
                Array.Resize(ref selectionsPrefixs, selectionsCount);
                isSelection = false;
                /*  messages[textIndex] += "\n";
                  for (int i = 0; i < selectionsCount; i++)
                  {
                      if (i % 2 == 0) messages[textIndex] += "　" + selections[i].PadRight(9, '　');
                      else messages[textIndex] += "　" + selections[i].PadRight(9, '　') + "\n";
                  }*/
                break;
            }
            textIndex++;
        }

        //画像情報の読み込み
        strIndex = str.IndexOf("_") + 1;
        str = (str.Substring(strIndex, str.Length - strIndex)).Replace("\n", "");
        strIndex = 0;

        semicolonCount = 0;
        while (semicolonCount < startTextIndex) //開始位置まで飛ばす
        {
            strIndex++;
            if (str.Substring(strIndex, 1).Equals(";")) semicolonCount++;
        }
        //strIndex++;
        textIndex = 0;
        while (strIndex < str.Length)
        {
            separationCount = 0;
            while (separationCount <= currentRoute)
            {
                if (strIndex == str.Length) break;
                if (str.Substring(strIndex, 1).Equals("%")) separationCount++;
                if (str.Substring(strIndex, 1).Equals(";")) separationCount = 0;
                strIndex++;
            }
            if (strIndex == str.Length) break;
            strIndex++;
            Array.Resize(ref left_base, textIndex + 1);
            Array.Resize(ref left_eye, textIndex + 1);
            Array.Resize(ref left_eyeblow, textIndex + 1);
            Array.Resize(ref left_face, textIndex + 1);
            Array.Resize(ref left_mouth, textIndex + 1);
            Array.Resize(ref left_nose, textIndex + 1);
            if (!str.Substring(strIndex, 1).Equals("0"))
            {
                isLeftBlank = false;
                string characterName = str.Substring(strIndex, str.IndexOf("-", strIndex) - strIndex);
                left_base[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_body");
                strIndex = str.IndexOf("-", strIndex) + 1;
                left_eyeblow[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_eyeblow" + str.Substring(strIndex, 1));
                strIndex++;
                left_eye[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_eye" + str.Substring(strIndex, 1));
                strIndex++;
                left_nose[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_nose" + str.Substring(strIndex, 1));
                strIndex++;
                left_mouth[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_mouth" + str.Substring(strIndex, 1));
                strIndex++;
                left_face[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_face" + str.Substring(strIndex, 1));
                if (str.Substring(strIndex, 1).Equals("0")) left_face[textIndex] = blank;
                else left_face[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_face" + str.Substring(strIndex, 1));
            }
            else
            {
                left_base[textIndex] = blank;
                left_eyeblow[textIndex] = blank;
                left_eye[textIndex] = blank;
                left_nose[textIndex] = blank;
                left_mouth[textIndex] = blank;
                left_face[textIndex] = blank;
            }

            strIndex = str.IndexOf("$", strIndex) + 1;
            Array.Resize(ref center_base, textIndex + 1);
            Array.Resize(ref center_eye, textIndex + 1);
            Array.Resize(ref center_eyeblow, textIndex + 1);
            Array.Resize(ref center_face, textIndex + 1);
            Array.Resize(ref center_mouth, textIndex + 1);
            Array.Resize(ref center_nose, textIndex + 1);
            if (!str.Substring(strIndex, 1).Equals("0"))
            {
                isCenterBlank = false;
                string characterName = str.Substring(strIndex, str.IndexOf("-", strIndex) - strIndex);
                center_base[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_body");
                strIndex = str.IndexOf("-", strIndex) + 1;
                center_eyeblow[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_eyeblow" + str.Substring(strIndex, 1));
                strIndex++;
                center_eye[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_eye" + str.Substring(strIndex, 1));
                strIndex++;
                center_nose[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_nose" + str.Substring(strIndex, 1));
                strIndex++;
                center_mouth[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_mouth" + str.Substring(strIndex, 1));
                strIndex++;
                if (str.Substring(strIndex, 1).Equals("0")) center_face[textIndex] = blank;
                else center_face[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_face" + str.Substring(strIndex, 1));
            }
            else
            {
                center_base[textIndex] = blank;
                center_eyeblow[textIndex] = blank;
                center_eye[textIndex] = blank;
                center_nose[textIndex] = blank;
                center_mouth[textIndex] = blank;
                center_face[textIndex] = blank;
            }
            strIndex = str.IndexOf("$", strIndex) + 1;
            Array.Resize(ref right_base, textIndex + 1);
            Array.Resize(ref right_eye, textIndex + 1);
            Array.Resize(ref right_eyeblow, textIndex + 1);
            Array.Resize(ref right_face, textIndex + 1);
            Array.Resize(ref right_mouth, textIndex + 1);
            Array.Resize(ref right_nose, textIndex + 1);
            if (!str.Substring(strIndex, 1).Equals("0"))
            {
                isRightBlank = false;
                string characterName = str.Substring(strIndex, str.IndexOf("-", strIndex) - strIndex);
                right_base[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_body");
                strIndex = str.IndexOf("-", strIndex) + 1;
                right_eyeblow[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_eyeblow" + str.Substring(strIndex, 1));
                strIndex++;
                right_eye[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_eye" + str.Substring(strIndex, 1));
                strIndex++;
                right_nose[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_nose" + str.Substring(strIndex, 1));
                strIndex++;
                right_mouth[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_mouth" + str.Substring(strIndex, 1));
                strIndex++;
                if (str.Substring(strIndex, 1).Equals("0")) right_face[textIndex] = blank;
                else right_face[textIndex] = Resources.Load<Sprite>("Textures/Characters/" + characterName + "/Pictures/" + characterName.ToLower() + "_face" + str.Substring(strIndex, 1));
            }
            else
            {
                right_base[textIndex] = blank;
                right_eyeblow[textIndex] = blank;
                right_eye[textIndex] = blank;
                right_nose[textIndex] = blank;
                right_mouth[textIndex] = blank;
                right_face[textIndex] = blank;
            }
            strIndex++;
            Array.Resize(ref messageTalkPosition, textIndex + 1);
            messageTalkPosition[textIndex] = str.Substring(strIndex, 1);
            strIndex = str.IndexOf(";", strIndex - 1) - 1;
            textIndex++;
        }
    }
}

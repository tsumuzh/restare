using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct item
{
    public int id;
    public string name;
    public string description;
}

public class items
{
    public item GetItemData(int itemId)
    {
        item _item = new item();
        switch (itemId)
        {
            case 0:
                _item.id = itemId;
                _item.name = "御札";
                _item.description = "陰陽師の必需品。たくさんある。";
                break;

            case 1:
                _item.id = itemId;
                _item.name = "護身用小刀";
                _item.description = "妖怪にも効く特別製。敵に接近されたらこれを使おう。";
                break;

            case 2:
                _item.id = itemId;
                _item.name = "携帯食";
                _item.description = "長旅もこれがあれば安心。多めに持ってきた。";
                break;

            case 3:
                _item.id = itemId;
                _item.name = "空の水筒";
                _item.description = "空の水筒。どこかで補給しなければ......";
                break;

            case 4:
                _item.id = itemId;
                _item.name = "水入り水筒";
                _item.description = "小さな水筒。樹海で汲んだ湧き水が入っている。";
                break;

            default:
                _item.id = -1;
                _item.name = "";
                _item.description = "";
                break;
        }
        return _item;
    }
}

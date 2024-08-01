using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class StorageBoardController : MonoBehaviour
{
    public bool isOccupied = false;

    public bool isOpen = false;

    public GameObject roll;

    private Sprite sprite;

    public int levelNum = 0;

    // 对已经被骰子占用的方格进行标记
    public bool CheckOccupied()
    {
        return isOccupied;
    }

    private void Update()
    {

    }

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }

    public void AddBoard()
    {
        if (gameObject.tag == "Add" && GameUtils.isAdd)
        {
            GameUtils.UpRound++;
        }
        else
        {
            GameUtils.UpRound = 0;
        }
        if (GameUtils.UpRound == 2 && gameObject.tag == "Add" && GameUtils.isAdd && roll != null)
        {
            if (roll.GetComponent<RollController>().num < 6)
            {
                levelNum++;
                gameObject.transform.GetChild(0).GetComponent<TextMeshPro>().text = "+" + levelNum.ToString();
                roll.GetComponent<RollController>().num++;
                int num = roll.GetComponent<RollController>().num;
                GameUtils.RollType type = roll.GetComponent<RollController>().type;
                if (type == GameUtils.RollType.rowType)
                {
                    sprite = Resources.Load<Sprite>("Arts/Rolls/black/black" + num.ToString());
                    roll.GetComponent<SpriteRenderer>().sprite = sprite;
                }
                else if (type == GameUtils.RollType.colType)
                {

                    sprite = Resources.Load<Sprite>("Arts/Rolls/blue/blue" + num.ToString());
                    roll.GetComponent<SpriteRenderer>().sprite = sprite;

                }
                else
                {

                    sprite = Resources.Load<Sprite>("Arts/Rolls/silver/silver" + num.ToString());
                    roll.GetComponent<SpriteRenderer>().sprite = sprite;

                }
            }
            GameUtils.UpRound = 0;
        }
    }

    public void ClearAddBoard()
    {
        if (gameObject.tag == "Add" && !GameUtils.isAdd)
        {
            levelNum = 0;
            gameObject.transform.GetChild(0).GetComponent<TextMeshPro>().text = "+" + levelNum.ToString();
        }
    }

    public void FrozenBoard()
    {
        if (gameObject.tag == "Frozen" && GameUtils.isFrozen)
        {
            GameUtils.FrozenRound++;
        }
        else
        {
            GameUtils.FrozenRound = 0;
        }
        if (GameUtils.FrozenRound == 2 && gameObject.tag == "Frozen" && GameUtils.isFrozen && roll != null)
        {
            roll.GetComponent<RollController>().isFrozen = true;
            roll.GetComponent<RollController>().type = GameUtils.RollType.frozenType;
            GameUtils.FrozenRound = 0;
            roll.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    public void FireBoard()
    {
        if (gameObject.tag == "Fire" && GameUtils.isFire)
        {
            GameUtils.FireRound++;
        }
        else
        {
            GameUtils.FireRound = 0;
        }
        if (GameUtils.FireRound == 2 && gameObject.tag == "Fire" && GameUtils.isFire && roll != null)
        {
            roll.GetComponent<RollController>().isFire = true;
            roll.GetComponent<RollController>().type = GameUtils.RollType.fireType;
            GameUtils.FireRound = 0;
            roll.transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}
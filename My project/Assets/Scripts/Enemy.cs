using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ExcelDataReader;
using System.IO;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class Enemy : MonoBehaviour
{
    //基本属性
    public int hp;

    [SerializeField]
    private int damage;

    public int row;

    public int col;

    public int type;

    public bool isFrozen = false; //增加敌人被冰冻状态

    public bool isFire = false;

    private int fireNum = 0;

    public int fireDamage = 0;

    public int score; // 消灭敌人增加的分数

    public GameObject chessBoard;

    public GameObject hpObj;

    private Sprite sprite;

    private Animator anim;

    private int hpScore;

    public Sprite[] sprites;

    public RuntimeAnimatorController[] anims;

    public GameObject enemy;

    public GameObject endPos;

    public GameObject bullet;

    private GameObject newBullet;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            anim = gameObject.AddComponent<Animator>();
        }
    }

    private void Start()
    {

    }

    // 初始化被创建出来的敌人
    public void Initialize()
    {
        GetComponent<SpriteRenderer>().sprite = sprites[type];
        hpScore = hp;

        Animator anim = GetComponent<Animator>();
        if (anim == null)
        {
            anim = gameObject.AddComponent<Animator>();
        }
        anim.runtimeAnimatorController = anims[type];

        UpdateHP();
    }

    // 敌人的攻击逻辑
    public void Attack()
    {
        if (type == 5)
        {
            StartCoroutine(HideTipsAfterDelay(0.5f));
        }
        if (row == 0)
        {
            chessBoard.GetComponent<GameMainView>().slider.value -= damage;
        }
    }

    IEnumerator HideTipsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        chessBoard.GetComponent<GameMainView>().slider.value -= damage;
        newBullet = Instantiate(bullet);
        newBullet.transform.position = bullet.transform.position;
        newBullet.SetActive(true);
        StartCoroutine(MoveBullet(endPos.transform.position));
    }

    // 特殊敌人生成时的处理
    public void Spawn()
    {

    }

    // 受到玩家攻击
    public void TakeDamage()
    {
        if (fireNum == 1)
        {
            fireNum = 0;
            hp -= fireDamage;
        }
        if (isFire)
        {
            fireNum++;
            isFire = false;
        }
        Transform blockTransform = chessBoard.transform.Find("block_" + row.ToString() + col.ToString());
        if (blockTransform != null)
        {
            int damage = int.Parse(blockTransform.GetChild(1).GetComponent<TextMeshPro>().text);
            if (damage != 0)
            {
                hp -= damage;
                GetComponent<SpriteRenderer>().sprite = sprites[type];
                gameObject.GetComponent<Animator>().SetBool("isHit", true);
                StartCoroutine(ResetHitFlag());
            }
            if (hp <= 0)
            {
                Die();
                return;
            }
            UpdateHP();
        }
    }

    private IEnumerator ResetHitFlag()
    {
        yield return new WaitForSeconds(1); // 等待动画结束
        anim.SetBool("isHit", false);
    }

    private IEnumerator ResetDieFlag()
    {
        yield return new WaitForSeconds(0.3f);
        anim.SetBool("isDie", false);
        SpawnSurroundingEnemies();
        Destroy(gameObject);
    }

    // 更新敌人血量
    public void UpdateHP()
    {
        hpObj.GetComponent<TextMeshPro>().text = hp.ToString();
    }

    // 敌人死亡时的处理，增加游戏得分、销毁游戏对象、播放死亡动画等
    public void Die()
    {
        chessBoard.GetComponent<GameMainView>().AddScore(hpScore);
        GameUtils.RemovePosPair(row, col);
        GameUtils.enemysArr.Remove(gameObject);
        anim.SetBool("isDie", true);
        chessBoard.GetComponent<GameMainView>().UpdateHisScore();
        StartCoroutine(ResetDieFlag());
    }

    // 生成周围新的敌人
    private void SpawnSurroundingEnemies()
    {
        if (type != 3)
        {
            return;
        }
        int[,] directions = new int[,]
        {
            {0,1},{0,-1},{1,0},{0,0}
        };
        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int newRow = row + directions[i, 0];
            int newCol = col + directions[i, 1];
            // 确保新生成的敌人位置没有敌人存在
            if (!GameUtils.findPos(newRow, newCol))
            {
                // 生成敌人
                GameObject newEnemy = Instantiate(enemy);
                chessBoard.GetComponent<GameMainView>().UpdateEnemyProperties(newEnemy.GetComponent<Enemy>(), new int[] { 1, 3 }, newCol, newRow, 0);
                newEnemy.GetComponent<Enemy>().Initialize();
                Transform blockTransform = chessBoard.transform.Find("block_" + row.ToString() + col.ToString());
                newEnemy.transform.position = blockTransform.position;
                GameUtils.enemysArr.Add(newEnemy);
                GameUtils.AddPosPair(newRow, newCol);
                newEnemy.GetComponent<Enemy>().Move(true);                // 更新GameUtils中的位置和敌人数组
            }
        }
    }

    // 敌人回合开始时向前移动
    public void Move(bool isFirstCreated)
    {
        // 如果被道具冰冻，则敌人无法移动
        if (isFrozen)
        {
            isFrozen = false;
            return;
        }

        if (!isFirstCreated && row > 0)
        {
            if (type != 4)
            {
                row--;
            }
            else
            {
                row = Mathf.Max(row - 2, 0);
            }
            StartCoroutine(MoveAnim(chessBoard.transform.Find("block_" + row.ToString() + col.ToString()).position));
        }
        else
        {
            StartCoroutine(MoveAnim(chessBoard.transform.Find("block_" + row.ToString() + col.ToString()).position));
        }
    }

    // 敌人沿x轴移动过程
    IEnumerator MoveAnim(Vector3 target)
    {
        Vector3 startPosition = transform.position;
        float journey = 0f;
        float duration = 1f; // 移动所需时间，可以根据需要调整

        while (journey < duration)
        {
            journey += Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);
            transform.position = Vector3.Lerp(startPosition, target, percent);
            yield return null;
        }

        // 确保最后位置精确设置为目标位置
        transform.position = target;
    }

    IEnumerator MoveBullet(Vector3 target)
    {
        Vector3 startPosition = newBullet.transform.position;
        float journey = 0f;
        float duration = 1f; // 移动所需时间，可以根据需要调整

        while (journey < duration)
        {
            journey += Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);
            newBullet.transform.position = Vector3.Lerp(startPosition, target, percent);
            yield return null;
        }

        // 确保最后位置精确设置为目标位置

        newBullet.transform.position = target;
        Destroy(newBullet);
    }
}
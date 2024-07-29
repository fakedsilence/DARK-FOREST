using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using ExcelDataReader;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.TextCore.Text;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Android;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameMainView : MonoBehaviour
{
    public GameObject chessBoard;

    public GameObject[] rolls;

    public GameObject enemy;

    public GameObject curScoreObj;

    public GameObject hisScoreObj;

    private Camera mainCamera;

    private Transform chessBoardTransform;

    public GameObject[,] blocks;

    private int level = 0;

    public GameObject enemyPos;

    private string isSpawn = null;

    private int beginLevel = 0;

    private int[] levelArr;

    public GameObject addBoard;

    public GameObject frozenBoard;

    public GameObject fireBoard;

    public UnityEngine.UI.Slider slider;

    public static int score;

    public AudioSource[] soundEffect;

    public string[] stringArr;

    public GameObject tips;

    private DataSet data;

    public static int onlineScore;



    // 确保该实例在场景加载时的唯一性
    void Awake()
    {
        
        levelArr = new int[] { 1, 8, 14, 19, 26, 33, 40, 47, 58 };
        stringArr = new string[] { "移动骰子攻击,抵御外星威胁", "信心大增", "铁血壁垒", "全力以赴", "闪电突袭", "血雨腥风", "暗流涌动", "万军之隙", "久经沙场" };
        mainCamera = Camera.main;

        chessBoardTransform = chessBoard.transform;  // 缓存棋盘的Transform引用
        hisScoreObj.GetComponent<TextMeshPro>().text = onlineScore.ToString();

    }



    void Start()
    {
        string filePath = Application.streamingAssetsPath + "/Config/EnemySpawn.csv";
        StartCoroutine(ReadCSVFile(filePath));
        StartCoroutine(PlayFirstRound());
    }

    void Update()
    {

    }

    #region 回合内方法
    // 回合开始时创建三个骰子
    public void CreateRoll()
    {
        int[] randomNumArr = GameUtils.CreateRandomNum();
        int[][] randomPosArr = GameUtils.CreateRandomPos();
        for (int i = 0; i < 3; i++)
        {
            int x = randomPosArr[i][0];
            int y = randomPosArr[i][1];
            GameUtils.RollType type = GameUtils.CreateRandomType();
            string blockName = "block_" + x.ToString() + y.ToString();
            Transform blockTransform = chessBoardTransform.Find(blockName);
            if (blockTransform == null)
            {
                Debug.LogError("Block transform not found: " + blockName);
                return;
            }
            else
            {
                GameObject newRoll = Instantiate(rolls[randomNumArr[i] - 1]);
                newRoll.GetComponent<RollController>().row = x;
                newRoll.GetComponent<RollController>().col = y;
                newRoll.GetComponent<RollController>().num = randomNumArr[i];
                newRoll.GetComponent<RollController>().type = type;
                newRoll.GetComponent<RollController>().Initialize();
                GameUtils.rollsArr.Add(newRoll);
                // newRoll.transform.position = blockTransform.position;
            }
        }
    }


    private IEnumerator ReadCSVFile(string filePath)
    {
        string result = "";
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Failed to load file from: " + filePath);
                yield break;
            }
            result = www.downloadHandler.text;
        }
        else
        {
            result = File.ReadAllText(filePath);
        }

        // 将读取的 CSV 内容转换为 DataSet
        data = ParseCSV(result);

        // 调用创建敌人方法
        CreateEnemy(data);
    }

    private DataSet ParseCSV(string csvText)
    {
        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvText)))
        {
            using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
            {
                return reader.AsDataSet();
            }
        }
    }

    // 回合开始时创建敌人
    public void CreateEnemy(DataSet result)
    {
        if (GameUtils.enemysArr.Count == 0)
        {
            for (int index = 0; index < 2; index++)
            {
                level++;
                roundEffect();
                isSpawn = result.Tables[0].Rows[level][4].ToString();
                string flag = result.Tables[0].Rows[level][5].ToString();
                if (flag != "" && index != 1)
                {
                    level = levelArr[++beginLevel];
                }
                int[] type = GameUtils.ParseIntArray1D(result.Tables[0].Rows[level][1].ToString());
                if (type.Length == 0)
                {
                    break;
                }
                int[][] hp = GameUtils.ParseIntArray2D(result.Tables[0].Rows[level][2].ToString());
                int[] pos = GameUtils.ParseIntArray1D(result.Tables[0].Rows[level][3].ToString());

                for (int i = 0; i < type.Length; i++)
                {
                    GameObject newEnemy = Instantiate(enemy);  //先用0，之后再用type中的类型
                    UpdateEnemyProperties(newEnemy.GetComponent<Enemy>(), new int[] { hp[i][0], hp[i][1] }, pos[i], index == 0 ? 4 : 5, type[i]);
                    newEnemy.GetComponent<Enemy>().Initialize();
                    GameUtils.enemysArr.Add(newEnemy);
                    GameUtils.posArr.Add(new List<int> { newEnemy.GetComponent<Enemy>().row, newEnemy.GetComponent<Enemy>().col });
                }

            }
        }
        else
        {
            isSpawn = result.Tables[0].Rows[level][4].ToString();
            if (isSpawn != "" || isCreate())
            {
                return;
            }
            level++;
            int[] type = GameUtils.ParseIntArray1D(result.Tables[0].Rows[level][1].ToString());
            int[][] hp = GameUtils.ParseIntArray2D(result.Tables[0].Rows[level][2].ToString());
            int[] pos = GameUtils.ParseIntArray1D(result.Tables[0].Rows[level][3].ToString());

            for (int i = 0; i < type.Length; i++)
            {
                GameObject newEnemy = Instantiate(enemy);  //先用0，之后再用type中的类型
                UpdateEnemyProperties(newEnemy.GetComponent<Enemy>(), new int[] { hp[i][0], hp[i][1] }, pos[i], 5, type[i]);
                newEnemy.GetComponent<Enemy>().Initialize();
                GameUtils.enemysArr.Add(newEnemy);
                GameUtils.posArr.Add(new List<int> { newEnemy.GetComponent<Enemy>().row, newEnemy.GetComponent<Enemy>().col });
            }
        }
    }

    private bool isCreate()
    {
        for (int i = 0; i < GameUtils.enemysArr.Count; i++)
        {
            if (GameUtils.enemysArr[i].GetComponent<Enemy>().row <= 2)
            {
                return true;
            }
        }
        return false;
    }

    // 更新敌人属性，包括HP、位置和行数
    public void UpdateEnemyProperties(Enemy enemy, int[] hpRange, int col, int row, int type)
    {
        enemy.row = row;
        enemy.col = col;
        enemy.type = type;
        enemy.hp = UnityEngine.Random.Range(hpRange[0], hpRange[1]);
        enemy.transform.position = enemyPos.transform.Find("block_" + "5" + enemy.col).position;
        enemy.Move(true);
    }

    // 更新方块颜色方法
    public void SetBlockColor()
    {
        if (GameUtils.rollsArr.Count == 0)
        {
            return;
        }

        foreach (var roll in GameUtils.rollsArr)
        {
            GameUtils.RollType type = roll.GetComponent<RollController>().type;
            int row = roll.GetComponent<RollController>().row;
            int col = roll.GetComponent<RollController>().col;

            UpdateBlockBasedOnType(type, row, col, 0, true, false);
        }
    }

    // 设置块的数字方法
    public void SetBlockNum()
    {
        if (GameUtils.rollsArr.Count == 0)
        {
            return;
        }

        foreach (var roll in GameUtils.rollsArr)
        {
            GameUtils.RollType type = roll.GetComponent<RollController>().type;
            int row = roll.GetComponent<RollController>().row;
            int col = roll.GetComponent<RollController>().col;
            int num = roll.GetComponent<RollController>().num;

            UpdateBlockBasedOnType(type, row, col, num, false, true);
        }
    }

    // 更新方块数值
    public void UpdateBlockNum()
    {
        for (int i = 0; i < GameUtils.blockNumArr.GetLength(0); i++)
        {
            for (int j = 0; j < GameUtils.blockNumArr.GetLength(1); j++)
            {
                Transform blockTransform = chessBoardTransform.Find("block_" + i.ToString() + j.ToString());
                if (blockTransform != null)
                {
                    if (GameUtils.blockNumArr[i, j] != 0)
                    {
                        blockTransform.GetChild(1).gameObject.SetActive(true);
                        blockTransform.GetChild(1).GetComponent<TextMeshPro>().text = GameUtils.blockNumArr[i, j].ToString();
                    }
                    else
                    {
                        blockTransform.GetChild(1).GetComponent<TextMeshPro>().text = 0.ToString();
                        blockTransform.GetChild(1).gameObject.SetActive(false);
                    }
                }
                else
                {
                    Debug.LogError("Error!");
                }
            }
        }
    }

    // 提取出的共用方法，用于更新块的颜色或数字
    public void UpdateBlockBasedOnType(GameUtils.RollType type, int row, int col, int num, bool updateColor, bool updateNumber)
    {
        if (type == GameUtils.RollType.rowType)
        {
            for (int j = 0; j < 6; j++)
            {
                UpdateBlock(j, col, num, updateColor, updateNumber, type);
            }
        }
        else if (type == GameUtils.RollType.colType)
        {
            for (int j = 0; j < 5; j++)
            {
                UpdateBlock(row, j, num, updateColor, updateNumber, type);
            }
        }
        else if (type == GameUtils.RollType.aroundType)
        {
            UpdateBlock(row, col, num, updateColor, updateNumber, type);
            if (row - 1 >= 0)
            {
                UpdateBlock(row - 1, col, num, updateColor, updateNumber, type);
            }
            if (row + 1 <= 5)
            {
                UpdateBlock(row + 1, col, num, updateColor, updateNumber, type);
            }
            if (col - 1 >= 0)
            {
                UpdateBlock(row, col - 1, num, updateColor, updateNumber, type);
            }
            if (col + 1 <= 4)
            {
                UpdateBlock(row, col + 1, num, updateColor, updateNumber, type);
            }
        }
        else
        {
            int[,] directions = new int[,] { { 0, 0 }, { 0, -1 }, { -1, 0 }, { -1, -1 }, { 1, 0 }, { 0, 1 }, { 1, 1 }, { 1, -1 }, { -1, 1 } };
            for (int j = 0; j < directions.GetLength(0); j++)
            {
                int newRow = row + directions[j, 0];
                int newCol = col + directions[j, 1];
                if (newRow >= 0 && newRow <= 5 && newCol >= 0 && newCol <= 4)
                {
                    UpdateBlock(newRow, newCol, num, updateColor, updateNumber, type);
                }
            }
        }
    }

    // 共用方法，根据参数更新方块状态
    private void UpdateBlock(int row, int col, int num, bool updateColor, bool updateNumber, GameUtils.RollType type)
    {
        Transform blockTransform = chessBoardTransform.Find("block_" + row.ToString() + col.ToString());
        if (blockTransform == null)
        {
            Debug.LogError("未找到方块的 Transform: block_" + row.ToString() + col.ToString());
            return;
        }
        blockTransform.GetChild(0).gameObject.SetActive(updateColor);

        if (updateNumber)
        {
            int currentNum = GameUtils.blockNumArr[row, col] + num;
            GameUtils.blockNumArr[row, col] = currentNum;
            TextMeshPro textMeshPro = blockTransform.GetChild(1).GetComponent<TextMeshPro>();
            textMeshPro.text = currentNum.ToString();
            textMeshPro.gameObject.SetActive(currentNum != 0);
            Color newColor = GetColorForNumber(type);
            SpriteRenderer spriteRenderer = blockTransform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = newColor;
            }
        }
        else
        {
            blockTransform.GetChild(1).gameObject.SetActive(false);
        }
    }

    private Color GetColorForNumber(GameUtils.RollType type)
    {
        Color color;
        // 计算 alpha 值

        if (type == GameUtils.RollType.frozenType)
        {
            color = new Color32(205, 233, 236, 100);
        }
        else if (type == GameUtils.RollType.fireType)
        {
            color = new Color32(183, 57, 57, 100);
        }
        else
        {
            color = new Color32(255, 176, 5, 100);  // 白色但透明度根据 alpha 计算
        }

        return color;
    }

    // 检测是否按下攻击键  MARK:应该加一个判断骰子是否存在，如果不存在，无法按下攻击键
    public void DetectAttack()
    {
        // 在移动端上运行
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended)
            {
                Debug.Log(1);
                PlayAttack();
            }
        }
    }

    // 玩家点击攻击
    private void PlayAttack()
    {
        if (!GameUtils.isAttack)
        {
            return;
        }
        GameUtils.isAttack = false;
        //特殊骰子判断逻辑
        // 获取 RollController 和 Enemy 组件
        for (int i = 0; i < GameUtils.rollsArr.Count; i++)
        {
            RollController rollController = GameUtils.rollsArr[i].GetComponent<RollController>();
            if (rollController.isFrozen)
            {
                int[,] directions = new int[,] { { 0, 0 }, { 0, -1 }, { -1, 0 }, { -1, -1 }, { 1, 0 }, { 0, 1 }, { 1, 1 }, { 1, -1 }, { -1, 1 } };
                for (int j = 0; j < directions.GetLength(0); j++)
                {
                    int newRow = rollController.row + directions[j, 0];
                    int newCol = rollController.col + directions[j, 1];

                    for (int z = 0; z < GameUtils.enemysArr.Count; z++)
                    {
                        Enemy enemy = GameUtils.enemysArr[z].GetComponent<Enemy>();
                        if (newRow == enemy.row && newCol == enemy.col)
                        {
                            enemy.isFrozen = true;
                        }
                    }
                }
            }
            else if (rollController.isFire)
            {
                int[,] directions = new int[,] { { 0, 0 }, { 0, -1 }, { -1, 0 }, { -1, -1 }, { 1, 0 }, { 0, 1 }, { 1, 1 }, { 1, -1 }, { -1, 1 } };
                for (int j = 0; j < directions.GetLength(0); j++)
                {
                    int newRow = rollController.row + directions[j, 0];
                    int newCol = rollController.col + directions[j, 1];

                    for (int z = 0; z < GameUtils.enemysArr.Count; z++)
                    {
                        Enemy enemy = GameUtils.enemysArr[z].GetComponent<Enemy>();
                        if (newRow == enemy.row && newCol == enemy.col)
                        {
                            enemy.isFire = true;
                            enemy.GetComponent<Enemy>().fireDamage = rollController.num;
                        }
                    }
                }
            }
        }

        DelPosRollArr();
        DestroyRoll();
        addBoard.GetComponent<StorageBoardController>().ClearAddBoard();

        // 倒序遍历数组 防止因删除敌人出错
        for (int i = GameUtils.enemysArr.Count - 1; i >= 0; i--)
        {
            GameUtils.enemysArr[i].GetComponent<Enemy>().TakeDamage();
        }
        Debug.Log("PlayAttack called");
        GameUtils.delBlockNumArr();
        StartCoroutine(PlayAIRound());
    }

    // 取消所有方块颜色
    private void SetBlockColorFalse()
    {
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Transform blockTransform = chessBoardTransform.Find("block_" + i.ToString() + j.ToString());
                blockTransform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    #endregion

    // 回合结束时销毁除存储骰子外所有骰子
    public void DestroyRoll()
    {
        foreach (var roll in GameUtils.rollsArr)
        {
            Destroy(roll);
        }
        GameUtils.rollsArr.Clear();
    }

    //回合结束时销毁pos数组中所有骰子的索引
    public void DelPosRollArr()
    {
        for (int i = 0; i < GameUtils.rollsArr.Count; i++)
        {
            int row = GameUtils.rollsArr[i].GetComponent<RollController>().row;
            int col = GameUtils.rollsArr[i].GetComponent<RollController>().col;
            GameUtils.RemovePosPair(row, col);
        }
    }

    // 游戏结束后统一清除棋盘上所有骰子
    // public void ClearBlock(int row, int col)
    // {
    //     Transform blockTransform = chessBoardTransform.Find("block_" + row + "_" + col);
    //     if (blockTransform == null)
    //     {
    //         Debug.LogError("未找到方块的 Transform: block_" + row + "_" + col);
    //         return;
    //     }

    //     // 清除的颜色和数字可见性
    //     blockTransform.GetChild(0).gameObject.SetActive(false);
    //     blockTransform.GetChild(1).gameObject.SetActive(false);

    //     // 清楚blockNumArr数组的所有元素
    //     GameUtils.blockNumArr[row, col] = 0;
    // }

    #region 回合逻辑

    private void roundEffect()
    {
        for (int i = 0; i < levelArr.Length; i++)
        {
            if (level == levelArr[i])
            {
                soundEffect[0].Play();
                tips.SetActive(true);
                tips.GetComponent<TextMeshProUGUI>().text = stringArr[i];
                break;
            }
        }
    }

    // 开始第一个回合
    private IEnumerator PlayFirstRound()
    {
        CreateEnemy(data);
        yield return new WaitForSeconds(1f);
        CreateRoll();  //for test 
        SetBlockNum();
        SetBlockColor();
        UpdateBlockNum();
    }

    // 轮到AI的回合
    private IEnumerator PlayAIRound()
    {
        SubmitScore(true);
        SetBlockNum();
        SetBlockColorFalse();
        SetBlockColor();
        UpdateBlockNum();

        for (int i = 0; i < GameUtils.enemysArr.Count; i++)
        {
            GameUtils.enemysArr[i].GetComponent<Enemy>().Attack();
        }
        yield return new WaitForSeconds(1f);
        StartCoroutine(NextRound());
    }

    // 进行下一个回合
    private IEnumerator NextRound()
    {
        isAddStorage();
        isFrozenStorage();
        isFireStorage();
        addBoard.GetComponent<StorageBoardController>().AddBoard();
        frozenBoard.GetComponent<StorageBoardController>().FrozenBoard();
        fireBoard.GetComponent<StorageBoardController>().FireBoard();
        for (int i = 0; i < GameUtils.enemysArr.Count; i++)
        {
            if (GameUtils.posArr[i][0] > 0)
            {
                if (!GameUtils.enemysArr[i].GetComponent<Enemy>().isFrozen)
                {
                    GameUtils.posArr[i][0]--;
                }
            }
            GameUtils.enemysArr[i].GetComponent<Enemy>().Move(false);
        }
        CreateEnemy(data);  //先创建敌人数组，防止随后创建的骰子位置和敌人重复
        yield return new WaitForSeconds(1f);
        CreateRoll();
        SetBlockNum();
        SetBlockColorFalse();
        SetBlockColor();
        UpdateBlockNum();
        // isAddStorage();
        // isFrozenStorage();
        // isFireStorage();
    }
    #endregion

    #region 判断特殊存储槽是否能够开启

    public void isAddStorage()
    {
        if (level >= 8)
        {
            addBoard.GetComponent<StorageBoardController>().isOpen = true;
        }
    }

    public void isFrozenStorage()
    {
        if (level >= 14)
        {
            frozenBoard.GetComponent<StorageBoardController>().isOpen = true;
        }
    }

    public void isFireStorage()
    {
        if (level >= 19)
        {
            fireBoard.GetComponent<StorageBoardController>().isOpen = true;
        }
    }

    #endregion

    #region 分数和排行相关
    // 消灭敌人后增加分数
    public void AddScore(int hpScore)
    {
        score = int.Parse(curScoreObj.GetComponent<TextMeshPro>().text);
        score += hpScore;
        curScoreObj.GetComponent<TextMeshPro>().text = score.ToString();
    }

    // 更新历史总得分
    public void UpdateHisScore()
    {
        int curScore = int.Parse(curScoreObj.GetComponent<TextMeshPro>().text);
        int hisScore = int.Parse(hisScoreObj.GetComponent<TextMeshPro>().text);
        if (curScore > hisScore)
        {
            hisScoreObj.GetComponent<TextMeshPro>().text = curScore.ToString();
        }
    }

    #endregion

    public void SubmitScore(bool shouldJump)
    {
        if(shouldJump)
        {
            if (slider != null && slider.value <= 0)
            {
                AccountManager.Instance.SendScore(AccountManager.Instance.UserId, AccountManager.Instance.Password, score);
                ChangeScene();

            }
        }
        else
        {
            AccountManager.Instance.SendScore(AccountManager.Instance.UserId, AccountManager.Instance.Password, score);
        }
        // 提交得分的逻辑
        // 确保在任何时候调用这个方法都不会导致异常
        
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("paused, submit score");
            SubmitScore(false);
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("game killed, submit score");
        SubmitScore(false);
    }

    private void ChangeScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
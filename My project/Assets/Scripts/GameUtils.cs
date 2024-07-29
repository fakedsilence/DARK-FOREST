using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameUtils
{
    public enum RollType
    {
        rowType,
        colType,
        aroundType,
        frozenType,
        fireType,
    }

    // 存放敌人的位置，随时更新
    // public static int[][] enenyPos = new int[10][];

    // 棋盘数组
    public static int[,] blockNumArr = new int[6, 5];

    // 存放棋盘上所有骰子
    public static List<GameObject> rollsArr = new List<GameObject>();

    // 存放棋盘上所有敌人
    public static List<GameObject> enemysArr = new List<GameObject>();

    // 存放棋盘中已存在的位置
    public static List<List<int>> posArr = new List<List<int>>();

    public static int UpRound = 0;

    public static int FrozenRound = 0;

    public static int FireRound = 0;

    public static bool isAttack = false;

    public static bool isAdd = false;

    public static bool isFrozen = false;

    public static bool isFire = false;

    // 随机创建1~6出三个随机数的数组
    public static int[] CreateRandomNum()
    {
        int[] randomArr = new int[3];
        System.Random rand = new System.Random();
        for (int i = 0; i < 3; i++)
        {
            randomArr[i] = rand.Next(1, 7);
        }
        return randomArr;
    }

    // 随机生成6行5列的三个坐标数组，确保坐标不重复
    public static int[][] CreateRandomPos()
    {
        int[][] randomPos = new int[3][];
        System.Random rand = new System.Random();
        HashSet<(int, int)> coordinatesSet = new HashSet<(int, int)>();
        foreach (List<int> pair in posArr)
        {
            if (pair.Count == 2)
            {
                coordinatesSet.Add((pair[0], pair[1]));
            }
        }
        for (int i = 0; i < 3; i++)
        {
            int x, y;
            do
            {
                x = rand.Next(0, 6);
                y = rand.Next(0, 5);
            } while (!coordinatesSet.Add((x, y)));
            randomPos[i] = new int[] { x, y };
            AddPosPair(x, y);
        }
        return randomPos;
    }

    // 随机生成骰子类型，但不包含frozenType、FireType
    // 随机生成骰子类型，但不包含frozenType、fireType
    public static RollType CreateRandomType()
    {
        // 获取所有RollType枚举值
        RollType[] allTypes = (RollType[])Enum.GetValues(typeof(RollType));

        // 过滤掉frozenType和fireType
        List<RollType> filteredTypes = new List<RollType>(allTypes);
        filteredTypes.Remove(RollType.frozenType);
        filteredTypes.Remove(RollType.fireType);

        // 随机选择一个类型
        int randomIndex = UnityEngine.Random.Range(0, filteredTypes.Count);
        RollType randomType = filteredTypes[randomIndex];

        return randomType;
    }


    // 查找骰子当前位置是否重复
    public static bool findPos(int x, int y)
    {
        for (int i = 0; i < posArr.Count; i++)
        {
            if (x == posArr[i][0] && y == posArr[i][1])
            {
                return true;
            }
        }
        return false;
    }

    // 更新骰子位置
    public static void updatePos(int oldPosX, int oldPosY, int newPosX, int newPosY)
    {
        for (int i = 0; i < posArr.Count; i++)
        {
            if (oldPosX == posArr[i][0] && oldPosY == posArr[i][1])
            {
                posArr[i][0] = newPosX;
                posArr[i][1] = newPosY;
                break;
            }
        }
    }

    // 将blockNumArr中所有数字归零
    public static void delBlockNumArr()
    {
        blockNumArr = new int[6, 5];
    }

    // 移除posArr中不需要的坐标
    public static void RemovePosPair(int x, int y)
    {
        posArr.RemoveAll(pair => pair.Count == 2 && pair[0] == x && pair[1] == y);
    }

    // 向posArr中添加坐标
    public static void AddPosPair(int x, int y)
    {
        List<int> pair = new List<int> { x, y };
        posArr.Add(pair);
    }

    // 检查posArr中是否已经存在坐标x和y
    public static bool IsCoordinatePair(int x, int y)
    {
        foreach (List<int> pair in posArr)
        {
            if (pair.Count == 2 && pair[0] == x && pair[1] == y)
            {
                return true;
            }
        }
        return false;
    }

    // 解析带方括号的字符串，将其转化为二维数组
    public static int[][] ParseIntArray2D(string input)
    {
        // 分割多个方括号
        string[] arrayStrings = input.Split(new[] { "][" }, StringSplitOptions.RemoveEmptyEntries);
        int[][] result = new int[arrayStrings.Length][];

        for (int i = 0; i < arrayStrings.Length; i++)
        {
            // 移除首尾的方括号
            string cleanedString = arrayStrings[i].Trim('[', ']');
            // 分割并解析为整数数组
            result[i] = cleanedString.Split(',').Select(int.Parse).ToArray();
        }

        return result;
    }

    // 解析带方括号的字符串，将其转化为一维数组
    public static int[] ParseIntArray1D(string input)
    {
        // 检查输入是否为空或仅包含方括号
        if (string.IsNullOrWhiteSpace(input) || input == "[]")
        {
            return new int[0]; // 返回一个空数组
        }
        // 移除首尾的方括号
        string cleanedString = input.Trim('[', ']');
        // 分割并解析为整数数组
        int[] result = cleanedString.Split(',')
                                    .Select(int.Parse)
                                    .ToArray();
        return result;
    }

    public static List<string> ConvertToMatrix(string input)
    {
        // 使用增强的正则表达式来匹配数组部分（包括多维数组）
        var regex = new Regex(@"\[([^\[\]]|\[(?>[^\[\]]+|\[(?<Depth>)|\](?<-Depth>))*(?(Depth)(?!))\])*?(\]\[([^\[\]]|\[(?>[^\[\]]+|\[(?<Depth>)|\](?<-Depth>))*(?(Depth)(?!))\])*?)*\]");
        var matches = regex.Matches(input);

        // 创建一个列表来保存所有匹配的字符串
        var resultList = new List<string>();

        foreach (Match match in matches)
        {
            resultList.Add(match.Value);
        }


        return resultList;
    }

}
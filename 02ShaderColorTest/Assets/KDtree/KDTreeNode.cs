using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KDTreeNode
{
    /// 分裂點
    public Point DivisionPoint { get; set; }
    /// 分裂型別
    public EnumDivisionType DivisionType { get; set; }
    /// 左子節點
    public KDTreeNode LeftChild { get; set; }
    /// 右子節點
    public KDTreeNode RightChild { get; set; }
}

public class Point
{
    public double X { get; set; }
    public double Y { get; set; }
}

public enum EnumDivisionType
{
    X = 0,
    Y = 1,
    NULL = 2
}
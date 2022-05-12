using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBlockPattern", menuName = "BlockPattern", order = 1)]
public class BlockPattern : ScriptableObject
{
    [System.Serializable]
    public struct BlockAngle
    {
        public bool[] topRow;
        public bool[] middleRow;
        public bool[] bottomRow;

        public BlockAngle(int top, int middle, int bottom)
        {
            topRow = new bool[top];
            middleRow = new bool[middle];
            bottomRow = new bool[bottom];
        }
    }

    public BlockAngle[] blockAngles;

    public BlockPattern()
    {
        blockAngles = new BlockAngle[4]
        {
            new BlockAngle(3,3,3),
            new BlockAngle(3,3,3),
            new BlockAngle(3,3,3),
            new BlockAngle(3,3,3)
        };
    }

}

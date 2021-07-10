using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MyNamespace.databridge;
using MyNamespace.databridge.AllowedParaments;

namespace MyNamespace.spriteSortingOrder
{
    public class TSpriteSortingOrder : MonoBehaviour
    {
        private class PairsStorage
        {
            public PairsStorage(int number)
            {
                _current = 0;
                Count = number;
                spriteRenderers = new MyPair_Transform_SpriteRenderer[number];
            }

            public void Expand()
            {
                MyPair_Transform_SpriteRenderer[] container = new MyPair_Transform_SpriteRenderer[(Length << 1)];
                for(int i= 0;i< Length; i++ )
                {
                    container[i] = spriteRenderers[i];
                }
                spriteRenderers = container;
            }

            public void Add(MyPair_Transform_SpriteRenderer pair)
            {
                if (_current == Length) Expand();

            }

            public void Sort()
            {

            }
            
            public int Count { get => _lengthCount; set => _lengthCount = value; }
            public int Length { get => _lengthCount; set => _lengthCount = value; }
            public int Size { get => _lengthCount; set => _lengthCount = value; }

            private int _lengthCount;
            private int _current;
            public MyPair_Transform_SpriteRenderer[] spriteRenderers;

        }

        PairsStorage pairsStorage = new PairsStorage(2);
        
        void Update()
        {

        }
    }
}

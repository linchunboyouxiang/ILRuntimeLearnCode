using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotUpdate
{
    public class TestGeneric
    {
        public void Print<T>(T param1)
        {
            Debug.LogFormat("TestGeneric.Print<T>({0})", param1);
        }
        
        public string Format<T, U>(T param1, U param2)
        {
            return string.Format("TestGeneric.Format<T, U>({0}, {1})", param1, param2);
        }
    }
}


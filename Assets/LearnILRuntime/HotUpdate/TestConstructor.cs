using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotUpdate
{
    public class TestConstructor
    {
        public TestConstructor()
        {
            Debug.Log("TestConstructor()");
        }

        public TestConstructor(string param1)
        {
            Debug.LogFormat("TestConstructor({0})", param1);
        }
        
        public TestConstructor(string param1, int param2)
        {
            Debug.LogFormat("TestConstructor({0}, {1})", param1, param2);
        }
    }
}


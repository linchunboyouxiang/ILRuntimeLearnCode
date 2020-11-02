using UnityEngine;

namespace HotUpdate
{
    public class TestStatic 
    {
        public static void HelloWorld()
        {
            Debug.LogFormat("TestStatic::HelloWorld!");
        }

        public static int Add(int num1, int num2)
        {
            return num1 + num2;
        }
    }
}


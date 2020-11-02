using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotUpdate
{
    public class TestCLRBinding
    {
        private static Func<float, float, float> _add;
        
        public static float FloatAdd(float a, float b)
        {
            _add = Add;
            
            float c = 0;
            for (int i = 0; i < 1000000; i++)
            {
                c += App.MathUtil.FloatAdd(a, b);
            }

            return c;
        }

        public static float Add(float a, float b)
        {
            return a + b;
        }
        
        public static float FloatAdd2(float a, float b)
        {
            float c = 0;
            for (int i = 0; i < 1000000; i++)
            {
                c += Add2(a, b);
            }

            return c;
        }

        public static float Add2(float a, float b)
        {
            return a + b;
        }
        
    }
}


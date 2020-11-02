using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotUpdate
{
    public class TestMember
    {
        private int member1 = 111;
        private string member2 = "member2";

        public int Member1
        {
            get { return this.member1; }
        }

        public string Member2
        {
            get { return this.member2; }
        }

        public void HelloWorld()
        {
            Debug.LogFormat("TestMember.HelloWorld()");
        }

        public string Append(string str1, string str2)
        {
            return str1 + "...." + str2;
        }
    }

}



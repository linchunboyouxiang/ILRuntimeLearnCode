using System.Collections;
using System.Collections.Generic;
using App;
using UnityEngine;

namespace HotUpdate
{
    public class TestInherit : UIWindow
    {
        public override int UID
        {
            get { return 666; }
        }

        public override void OnStart(string str)
        {
            base.OnStart(str);
            Debug.LogFormat("TestInheritance.OnStart({0})", str);
        }

        public override void OnClose(int arg)
        {
            Debug.LogFormat("TestInheritance.OnClose({0})", arg);
        }
    }


}


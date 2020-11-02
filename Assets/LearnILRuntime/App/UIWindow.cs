using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    public abstract class UIWindow
    {
        public virtual int UID
        {
            get { return 1; }
        }
        
        public virtual void OnStart(string str)
        {
            Debug.LogFormat("ExtendTest.OnStart({0})", str);
        }

        public abstract void OnClose(int arg);
    }
}


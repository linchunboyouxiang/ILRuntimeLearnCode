using System;
using App;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate
{
    public class TestDelegate
    {
        private Func<int, int, int> funcAdd;        // 最后一个表示返回值，string
        
        public void UIEelemtBindAction()
        {
            GameObject.Find("Button").GetComponent<Button>().onClick.AddListener(OnButtonClick);
            GameObject.Find("InputField").GetComponent<InputField>().onValueChanged.AddListener(OnInputValueChange);
        }
        
        private void OnButtonClick()
        {
            Debug.Log("TestDelegate.OnButtonClick()");

            if (funcAdd == null)
            {
                funcAdd = this.Add;
            }

            int sum = funcAdd(1, 2);
            Debug.LogFormat("TestDelegate.OnButtonClick().res = {0}", sum);

            if (GlobalFunc.OnNumChange == null)
            {
                GlobalFunc.OnNumChange = Append;
            }
            
            string append = GlobalFunc.OnNumChange(1, 2, 3);
            Debug.LogFormat("TestDelegate.OnButtonClick().append = {0}", append);
        }

        private void OnInputValueChange(string val)
        {
            Debug.LogFormat("OnInputValueChange({0})", val);
        }
        
        private int Add(int num1, int num2)
        {
            return num1 + num2;
        }
        
        private string Append(int num1, int num2, int num3)
        {
            return string.Format("{0} .. {1} .. {2}", num1, num2, num3);
        }
    }
}


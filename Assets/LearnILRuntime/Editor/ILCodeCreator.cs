using System;
using System.Collections;
using System.Collections.Generic;
using App;
using UnityEditor;
using UnityEngine;

[System.Reflection.Obfuscation(Exclude = true)]
public class ILCodeCreator
{
    private const string HOT_UPDATE_DLL_PATH = @"D:\gitlab\project\unity-okeyfun-family\family-client\HotFix\HotUpdate.dll";
    private const string INHERIT_ADAPTER_CREATE_DIR = @"Assets/LearnILRuntime/Generate/Adapter/";        // 注意目录需要存在
    private const string CLR_CODE_CREATE_DIR = @"Assets/LearnILRuntime/Generate/CLR/";                 // 注意目录需要存在

    [MenuItem("ILRuntime/生成跨域继承适配器")]
    static void GenerateCrossInheritAdapter()
    {
        //由于跨域继承特殊性太多，自动生成无法实现完全无副作用生成，所以这里提供的代码自动生成主要是给大家生成个初始模版，简化大家的工作
        //大多数情况直接使用自动生成的模版即可，如果遇到问题可以手动去修改生成后的文件，因此这里需要大家自行处理是否覆盖的问题
        
        CreateInheritAdapterClass(typeof(UIWindow), "AppILRAdapter");

        AssetDatabase.Refresh();
    }
    
    [MenuItem("ILRuntime/生成CLR绑定-自动分析热更DLL")]
    static void GenerateCLRBindingByAnalysis()
    {
        //用新的分析热更dll调用引用来生成绑定代码
        ILRuntime.Runtime.Enviorment.AppDomain domain = new ILRuntime.Runtime.Enviorment.AppDomain();
        using (System.IO.FileStream fs = new System.IO.FileStream(HOT_UPDATE_DLL_PATH, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
            domain.LoadAssembly(fs);

            //Crossbind Adapter is needed to generate the correct binding code
            InitILRuntime(domain);
            ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(domain, CLR_CODE_CREATE_DIR);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    [MenuItem("ILRuntime/生成CLR绑定")]
    static void GenerateCLRBinding()
    {
        //添加我们需要重定向的类 
        List<Type> types = new List<Type>();
//        types.Add(typeof(int));
//        types.Add(typeof(float));
//        types.Add(typeof(long));
//        types.Add(typeof(object));
//        types.Add(typeof(string));
//        types.Add(typeof(Array));
//        types.Add(typeof(Vector2));
//        types.Add(typeof(Vector3));
//        types.Add(typeof(Quaternion));
//        types.Add(typeof(GameObject));
//        types.Add(typeof(UnityEngine.Object));
//        types.Add(typeof(Transform));
//        types.Add(typeof(RectTransform));
//        types.Add(typeof(Time));
//        types.Add(typeof(Debug));
//        //所有DLL内的类型的真实C#类型都是ILTypeInstance
//        types.Add(typeof(List<ILRuntime.Runtime.Intepreter.ILTypeInstance>));

        //自定义添加需要重定向的方法
        types.Add(typeof(App.MathUtil));

        //第二个参数 是生成的脚本的路径
        ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(types, CLR_CODE_CREATE_DIR);
        
        AssetDatabase.Refresh();
    }

    // 生成基础适配器类
    private static void CreateInheritAdapterClass(Type baseClass, string nameSpace)
    {
        string adapterName = baseClass.Name + "Adapter.cs";
        using(System.IO.StreamWriter sw = new System.IO.StreamWriter(INHERIT_ADAPTER_CREATE_DIR + adapterName))
        {
            sw.WriteLine(ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(baseClass, nameSpace));
        }
    }
    
    static void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain domain)
    {
        //这里需要注册所有热更DLL中用到的跨域继承Adapter，否则无法正确抓取引用
//        domain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
//        domain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
//        domain.RegisterCrossBindingAdaptor(new TestClassBaseAdapter());
//        domain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
//        domain.RegisterCrossBindingAdaptor(new UIWindowAdapter());
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using App;
using AppILRAdapter;
using HotDynamic;
using HotUpdate;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public class ILManager : MonoBehaviour
{
    //AppDomain是ILRuntime的入口，最好是在一个单例类中保存，整个游戏全局就一个，这里为了示例方便，每个例子里面都单独做了一个
    //大家在正式项目中请全局只创建一个AppDomain
    AppDomain appdomain;

    System.IO.MemoryStream fs;
    System.IO.MemoryStream p;

    private const string HOT_UPDATE_DLL_DIR = @"D:\gitlab\project\unity-okeyfun-family\family-client\HotFix\";    // 热更dll目录
    
    void Start()
    {
        StartCoroutine(LoadHotFixAssembly());
    }
    
    void OnDestroy()
    {
        if (fs != null)
            fs.Close();
        if (p != null)
            p.Close();
        fs = null;
        p = null;
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(100, 100, 100, 100), "TestCLRBind"))
        {
            TestCLRBind();
            StartCoroutine(Pause());
        }
        
        if (GUI.Button(new Rect(100, 200, 100, 100), "TestNoCLRBind"))
        {
            TestCLRBind2();
            StartCoroutine(Pause());
        }
        
        if (GUI.Button(new Rect(200, 100, 100, 100), "TestCLRBindDirect"))
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            
            sw.Reset();
            sw.Start();
            Profiler.BeginSample("TestCLRBinding");
            float sum = TestCLRBinding.FloatAdd(1.11f, 2.22f);
            
            Profiler.EndSample();
            sw.Stop();
            
            Debug.LogFormat("ILManager.TestCLRBind().sum = {0}", sum);
            Debug.LogFormat("ILManager.TestCLRBind()方法执行时间:{0} ms", sw.ElapsedMilliseconds);
            
            
            StartCoroutine(Pause());
        }
    }

    IEnumerator Pause()
    {
        yield return new WaitForSeconds(0.15f);
        EditorApplication.isPaused = true;
    }

    IEnumerator LoadHotFixAssembly()
    {
        //首先实例化ILRuntime的AppDomain，AppDomain是一个应用程序域，每个AppDomain都是一个独立的沙盒
        appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();
        //正常项目中应该是自行从其他地方下载dll，或者打包在AssetBundle中读取，平时开发以及为了演示方便直接从StreammingAssets中读取，
        //正式发布的时候需要大家自行从其他地方读取dll

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //这个DLL文件是直接编译HotFix_Project.sln生成的，已经在项目中设置好输出目录为StreamingAssets，在VS里直接编译即可生成到对应目录，无需手动拷贝
        //工程目录在Assets\Samples\ILRuntime\1.6\Demo\HotFix_Project~
        //以下加载写法只为演示，并没有处理在编辑器切换到Android平台的读取，需要自行修改
#if UNITY_ANDROID
        WWW www = new WWW(Application.streamingAssetsPath + "/HotFix_Project.dll");
#else
        //WWW www = new WWW("file:///" + Application.streamingAssetsPath + "/HotFix_Project.dll");
        WWW www = new WWW(HOT_UPDATE_DLL_DIR + "HotUpdate.dll");
#endif
        while (!www.isDone)
            yield return null;
        if (!string.IsNullOrEmpty(www.error))
            UnityEngine.Debug.LogError(www.error);
        byte[] dll = www.bytes;
        www.Dispose();

        //PDB文件是调试数据库，如需要在日志中显示报错的行号，则必须提供PDB文件，不过由于会额外耗用内存，正式发布时请将PDB去掉，下面LoadAssembly的时候pdb传null即可
#if UNITY_ANDROID
        www = new WWW(Application.streamingAssetsPath + "/HotFix_Project.pdb");
#else
        //www = new WWW("file:///" + Application.streamingAssetsPath + "/HotFix_Project.pdb");
        www = new WWW(HOT_UPDATE_DLL_DIR + "HotUpdate.pdb");
#endif
        while (!www.isDone)
            yield return null;
        if (!string.IsNullOrEmpty(www.error))
            UnityEngine.Debug.LogError(www.error);
        byte[] pdb = www.bytes;
        fs = new MemoryStream(dll);
        p = new MemoryStream(pdb);
        try
        {
            appdomain.LoadAssembly(fs, p, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
        }
        catch
        {
            Debug.LogError("加载热更DLL失败，请确保已经通过VS打开Assets/Samples/ILRuntime/1.6/Demo/HotFix_Project/HotFix_Project.sln编译过热更DLL");
        }

        InitializeILRuntime();
        OnHotFixLoaded();
    }

    void InitializeILRuntime()
    {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
        //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
        appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
        //这里做一些ILRuntime的注册，HelloWorld示例暂时没有需要注册的
        RegistCLRRedirection(appdomain);
        RegistCLRBinding(appdomain);
        
        RegistDelegateConvertor();
        RegistCrossInheritAdapter();
    }
    
    void OnHotFixLoaded()
    {
        Debug.LogWarning("=======》 TestStatic");
        TestStatic();
        Debug.LogWarning("=======》 TestConstructor");
        TestConstructor();
        Debug.LogWarning("=======》 TestMember");
        TestMember();
        Debug.LogWarning("=======》 TestGeneric");
        TestGeneric();
        Debug.LogWarning("=======》 TestInherit");
        TestInherit();
        Debug.LogWarning("=======》 TestDelegate");
        TestDelegate();
        Debug.LogWarning("=======》 TestCLR");
    }

    // 注册委托转换器
    private void RegistDelegateConvertor()
    {
        // button
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((act) =>
        {
            return new UnityEngine.Events.UnityAction(() =>
            {
                ((Action) act)();
            });
        });
        
        // input
        // 多参数需要加这个
        appdomain.DelegateManager.RegisterMethodDelegate<string>();
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<System.String>>((act) =>
        {
            return new UnityEngine.Events.UnityAction<System.String>((arg0) =>
            {
                ((Action<System.String>) act)(arg0);
            });
        });
        
        // func
        // 跨域需要加这个
        appdomain.DelegateManager.RegisterFunctionDelegate<int, int, int, string>();
    }

    // 注册跨域基础适配器
    private void RegistCrossInheritAdapter()
    {
        appdomain.RegisterCrossBindingAdaptor(new UIWindowAdapter());
    }

    private void TestStatic()
    {
        const string CLASS_NAME = "HotUpdate.TestStatic";
        
        // 方式1
        appdomain.Invoke(CLASS_NAME, "HelloWorld", null, null);
        int sum1 = (int)appdomain.Invoke(CLASS_NAME, "Add", null, new object[]{1, 2});
        Debug.LogFormat("ILManager.TestStatic().sum1 = {0}", sum1);
        
        // 方式2
        IType type = appdomain.LoadedTypes[CLASS_NAME];
        IMethod method1 = type.GetMethod("HelloWorld", 0);
        appdomain.Invoke(method1, null, null);

        IType intType2 = appdomain.GetType(typeof(int));
        List<IType> listParam2 = new List<IType>();
        listParam2.Add(intType2);
        listParam2.Add(intType2);
        IMethod method2 = type.GetMethod("Add", listParam2, null);
        int sum2 = (int)appdomain.Invoke(method2, null,  new object[]{3, 4});
        Debug.LogFormat("ILManager.TestStatic().sum2 = {0}", sum2);
    }

    private void TestMember()
    {
        const string CLASS_NAME = "HotUpdate.TestMember";
        
        object obj = appdomain.Instantiate(CLASS_NAME, null);
        
        // 属性
        int member1 = (int)appdomain.Invoke(CLASS_NAME, "get_Member1", obj, null);
        string member2 = (string)appdomain.Invoke(CLASS_NAME, "get_Member2", obj, null);
        Debug.LogFormat("ILManager.TestMember().member1 = {0}", member1);
        Debug.LogFormat("ILManager.TestMember().member2 = {0}", member2);
        
        // 成员方法
        appdomain.Invoke(CLASS_NAME, "HelloWorld", obj, null);
        string append = (string)appdomain.Invoke(CLASS_NAME, "Append", obj, new object[]{"str1", "str2"});
        Debug.LogFormat("ILManager.TestMember().append = {0}", append);
    }

    private void TestGeneric()
    {
        const string CLASS_NAME = "HotUpdate.TestGeneric";
        
        object obj = appdomain.Instantiate(CLASS_NAME, null);
    
        // 方式1
        IType stringType = appdomain.GetType(typeof(string));
        IType[] genericArguments = new IType[]{stringType};
        appdomain.InvokeGenericMethod(CLASS_NAME, "Print", genericArguments, obj, "hello world");
        
        // 方式2
        IType type = appdomain.LoadedTypes[CLASS_NAME];
        
        List<IType> listParam = new List<IType>();
        IType stringType2 = appdomain.GetType(typeof(string));
        IType intType2 = appdomain.GetType(typeof(int));
        listParam.Add(stringType2);
        listParam.Add(intType2);
        
        IType[] genericArguments2 = new IType[]{stringType2, intType2};
        IMethod method = type.GetMethod("Format", listParam, genericArguments2);

        string format = (string)appdomain.Invoke(method, obj, new object[] {"hello world", 1024});
        Debug.LogFormat("ILManager.TestGeneric().format = {0}", format);
    }

    private void TestConstructor()
    {
        const string CLASS_NAME = "HotUpdate.TestConstructor";
        
        // 方式1
        object obj1 = appdomain.Instantiate(CLASS_NAME, null);
        object obj2 = appdomain.Instantiate(CLASS_NAME, new object[]{"constructor2"});
        object obj3 = appdomain.Instantiate(CLASS_NAME, new object[]{"constructor3", 3});
        
        // 方式2
        IType type = appdomain.LoadedTypes[CLASS_NAME];
        object obj4 = ((ILType) type).Instantiate();
        
        // 带参数构造函数，需要手动调用构造函数
        object obj5 = ((ILType) type).Instantiate(false);
        IType stringType5 = appdomain.GetType(typeof(string));
        List<IType> listParam5 = new List<IType>();
        listParam5.Add(stringType5);
        IMethod method5 = type.GetConstructor(listParam5);
        appdomain.Invoke(method5, obj5, "constructor5");
        
        object obj6 = ((ILType) type).Instantiate(false);
        IType stringType6 = appdomain.GetType(typeof(string));
        IType intType6 = appdomain.GetType(typeof(int));
        List<IType> listParam6 = new List<IType>();
        listParam6.Add(stringType6);
        listParam6.Add(intType6);
        IMethod method6 = type.GetConstructor(listParam6);
        appdomain.Invoke(method6, obj6, new object[]{"constructor6", 6});
    }
    
    private void TestInherit()
    {
        const string CLASS_NAME = "HotUpdate.TestInherit";
        
        UIWindow window = appdomain.Instantiate<UIWindow>(CLASS_NAME);
        window.OnStart("abcdefg");
        window.OnClose(2048);
        
        Debug.LogFormat("ILManager.TestInherit().UID = {0}", window.UID);
    }
    
    private void TestDelegate()
    {
        const string CLASS_NAME = "HotUpdate.TestDelegate";
        
        object obj = appdomain.Instantiate(CLASS_NAME, null);
        appdomain.Invoke(CLASS_NAME, "UIEelemtBindAction", obj, null);
    }

    private void RegistCLRRedirection(AppDomain app)
    {
       
    }

    private void RegistCLRBinding(AppDomain app)
    { 
        ILRuntime.Runtime.Generated.CLRBindings.Initialize(app);
    }

    private void TestCLRBind()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            
        var type = appdomain.LoadedTypes["HotUpdate.TestCLRBinding"];
        var m = type.GetMethod("FloatAdd", 2);
        
        sw.Reset();
        sw.Start();
        Profiler.BeginSample("TestCLRBinding");
        float sum = (float)appdomain.Invoke(m, null, new object[]{1.1f, 2.2f});
        Profiler.EndSample();
        sw.Stop();
            
        Debug.LogFormat("ILManager.TestCLRBind().sum = {0}", sum);
        Debug.LogFormat("ILManager.TestCLRBind()方法执行时间:{0} ms", sw.ElapsedMilliseconds);
    }
    
    private void TestCLRBind2()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            
        var type = appdomain.LoadedTypes["HotUpdate.TestCLRBinding"];
        var m = type.GetMethod("FloatAdd2", 2);
        
        sw.Reset();
        sw.Start();
        Profiler.BeginSample("TestCLRBinding");
        float sum = (float)appdomain.Invoke(m, null, new object[]{1.1f, 2.2f});
        Profiler.EndSample();
        sw.Stop();
            
        Debug.LogFormat("ILManager.TestCLRBind().sum = {0}", sum);
        Debug.LogFormat("ILManager.TestCLRBind()方法执行时间:{0} ms", sw.ElapsedMilliseconds);
    }
    
    
}

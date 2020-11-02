using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;
using UnityEngine;

 namespace HotDynamic
 {
     unsafe public class TestClrRedirection
     {
         public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
         {
             var mi = typeof(Debug).GetMethod("Log", new Type[] {typeof(object)});
             app.RegisterCLRMethodRedirection(mi, Log_11);
         }
    
         static StackObject* Log_11(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
         {
             ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
             StackObject* ptr_of_this_method;
             StackObject* __ret = ILIntepreter.Minus(__esp, 1);

             ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
             System.Object @message = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
             __intp.Free(ptr_of_this_method);
        
             //在真实调用Debug.Log前，我们先获取DLL内的堆栈
             var stacktrace = __domain.DebugService.GetStackTrace(__intp);

             //我们在输出信息后面加上DLL堆栈
             UnityEngine.Debug.Log("Log_11 crl重定向:" + message + "\n" + stacktrace);

             return __ret;
         }
     }
 }
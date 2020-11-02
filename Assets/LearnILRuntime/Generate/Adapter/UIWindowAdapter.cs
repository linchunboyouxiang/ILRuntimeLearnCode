using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace AppILRAdapter
{   
    public class UIWindowAdapter : CrossBindingAdaptor
    {
        static CrossBindingFunctionInfo<System.Int32> mget_UID_0 = new CrossBindingFunctionInfo<System.Int32>("get_UID");
        static CrossBindingMethodInfo<System.String> mOnStart_1 = new CrossBindingMethodInfo<System.String>("OnStart");
        static CrossBindingMethodInfo<System.Int32> mOnClose_2 = new CrossBindingMethodInfo<System.Int32>("OnClose");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(App.UIWindow);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        public class Adapter : App.UIWindow, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adapter()
            {

            }

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            public override void OnStart(System.String str)
            {
                if (mOnStart_1.CheckShouldInvokeBase(this.instance))
                    base.OnStart(str);
                else
                    mOnStart_1.Invoke(this.instance, str);
            }

            public override void OnClose(System.Int32 arg)
            {
                mOnClose_2.Invoke(this.instance, arg);
            }

            public override System.Int32 UID
            {
            get
            {
                if (mget_UID_0.CheckShouldInvokeBase(this.instance))
                    return base.UID;
                else
                    return mget_UID_0.Invoke(this.instance);

            }
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    return instance.ToString();
                }
                else
                    return instance.Type.FullName;
            }
        }
    }
}


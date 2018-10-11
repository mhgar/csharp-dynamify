using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Wander.Pigeon.Dynamic {
    public static class Dynamify {
        const string DYNACTION_DECL_TYPE = "Wander.Pigeon.Dynamic.ActionConverters";
        const string DYNFUNC_DECL_TYPE = "Wander.Pigeon.Dynamic.FuncConverters";
        const string DYNACTION_METHOD_NAME = "MakeAction";
        const string DYNFUNC_METHOD_NAME = "MakeFunc";

        public static Func<object[], object> Make(Delegate method) {
            var methodInfo = method.Method;
            var returnType = methodInfo.ReturnType;
            var returnsVoid = returnType == typeof(void); // Is an action if true.
            var parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType);

            // Decide what method we need to call.
            var genericMethodArgs = returnsVoid ? parameterTypes.ToArray() : parameterTypes.Concat(new[]{returnType}).ToArray();
            var converterType = Type.GetType(returnsVoid ? DYNACTION_DECL_TYPE : DYNFUNC_DECL_TYPE);
            var converterMethodName = returnsVoid ? DYNACTION_METHOD_NAME : DYNFUNC_METHOD_NAME;

            // Find the method we want, we need to see if the given delegate has more than 16 args.
            var converterMethod = converterType.GetMethods().ToList().First(m => m.GetGenericArguments().Length == genericMethodArgs.Length);
            var genericMethod = converterMethod.MakeGenericMethod(genericMethodArgs);
            var func = genericMethod.Invoke(null, new object[]{method});
            return func as Func<object[], object>;
        }

        // Clean up this garbage
        public static Delegate AutoDelegate(MethodInfo method, object target = null) {
            if (method == null) throw new ArgumentNullException(nameof(method));

            // Info about the method and target that we need to check for consistency.
            var shouldBeStatic = target == null;
            var isStatic = method.IsStatic;
            var declaringType = method.DeclaringType;            

            if (!shouldBeStatic) {
                if (declaringType != target.GetType()) throw new ArgumentException($"Target type does not contain the public method {method.Name}.", nameof(target));
            } else {
                if (!isStatic) throw new ArgumentException($"Target method is not static, but the target object is null.", nameof(method));
            }

            var methodParams = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var hasReturnType = method.ReturnType != typeof(void);

            Type delegateType;
            if (methodParams.Length == 0 && !hasReturnType) {
                delegateType = typeof(System.Action);
            } else if (!hasReturnType) {
                delegateType = MakeGenericType("System.Action", methodParams);
            } else {
                delegateType = MakeGenericType("System.Func", methodParams.Concat(new []{method.ReturnType}).ToArray());
            }

            return isStatic ? method.CreateDelegate(delegateType) : method.CreateDelegate(delegateType, target);
        }

        public static Delegate AutoDelegate(string methodName, object target) {
            if (methodName == null) throw new ArgumentNullException(nameof(methodName));
            if (target == null) throw new ArgumentNullException(nameof(target));

            var method = target.GetType().GetMethod(methodName);
            if (method == null) throw new ArgumentException("There is no public method by that name on the target object.", nameof(methodName));

            return AutoDelegate(method, target);
        }

        public static Delegate AutoDelegate(string methodName, Type targetType) {
            if (methodName == null) throw new ArgumentNullException(nameof(methodName));
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));

            var method = targetType.GetMethod(methodName);
            if (method == null) throw new ArgumentException("There is no public method by that name on the target type.", nameof(methodName));

            return AutoDelegate(method, null);
        }

        public static Type MakeGenericType(string genericName, params Type[] genericArguments) {
            var genericArgsLength = genericArguments.Length;
            var type = genericArgsLength == 0 ? Type.GetType(genericName) : Type.GetType($"{genericName}`{genericArgsLength}");            

            if (type == null) {
                throw new ArgumentException($"Generic type {genericName} with {genericArgsLength} generic args is not valid.");
            }

            return genericArgsLength == 0 ? type : type.MakeGenericType(genericArguments);
        }
    }
}
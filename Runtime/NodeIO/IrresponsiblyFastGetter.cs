using System;
using System.Reflection;
using System.Reflection.Emit;

namespace GraphFramework
{
    internal static class CreateFastGetter
    {
        //An important note, this is not run every time we do a value lookup. This is run up to
        //once per port a single time per graph to retrieve the value key. Eventually this 
        //should converge to be only slightly worse than a dictionary lookup and function call.
        /// <summary>
        /// Creates a fast function for getting a target class' FieldType based on a FieldInfo
        /// This is strictly faster than conventional GetValue reflection.
        /// </summary>
        public static Func<TargetType, OutFieldType> Create<TargetType, OutFieldType>(FieldInfo targetField)
        {
            var dynamicGetter = new DynamicMethod(
                typeof(TargetType).Name + "_quickget_" + targetField.Name, 
                typeof(OutFieldType),
                new[] { typeof(TargetType) },
                typeof(TargetType), 
                true);
            
            var il = dynamicGetter.GetILGenerator();
            //Loads our functions argument (an instance of TargetType) onto the stack
            il.Emit(OpCodes.Ldarg_0);
            //Loads the field object we want from the object we just put on the stack (the TargetType inst)
            il.Emit(OpCodes.Ldfld, targetField);
            //Return from the function (the field value is now on the stack,
            //so this is equiv to | return fieldObject; |
            il.Emit(OpCodes.Ret);
            
            /* Snapshot of stack after execution.
             * TargetTypeInst      : Ldarg_0 -> Push Instance of TargetType onto Stack
             * [Instance of Field] : Ldfld -> Load instance of concrete field from object on stack.
             */
            
            /*
            Full "c#"ish translation:
            public OutFieldType GetValue(TargetType inst) {
                //Where OutFieldType is the target field as defined in the provided FieldInfo
                return inst.[Instance of target field];
            }
            */

            return (Func<TargetType, OutFieldType>)dynamicGetter.CreateDelegate(typeof(Func<TargetType, OutFieldType>));
        }
    }
}
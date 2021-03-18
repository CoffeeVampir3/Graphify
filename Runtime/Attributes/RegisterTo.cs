using System;

namespace GraphFramework.Attributes
{
    /// <summary>
    /// Registers this object to the provided GraphController type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterTo : RegistrationAttribute
    {
        public RegisterTo(Type registerTo, string nodePath = "") : base(registerTo, nodePath)
        {
        }
    }
}
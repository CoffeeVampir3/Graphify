using System;
using GraphFramework.Attributes;

namespace GraphFramework.EditorOnly.Attributes
{
    public class RegisterStackTo : RegistrationAttribute
    {
        public RegisterStackTo(Type registerTo, string nodePath = "") : base(registerTo, nodePath)
        {
        }
    }
}
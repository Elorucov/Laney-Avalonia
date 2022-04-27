using System;
using System.Collections.Generic;
using System.Text;

namespace ELOR.VKAPILib.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodAttribute : Attribute {
        private string _name;
        public string Name { get { return _name; } }

        public MethodAttribute(string name) {
            if(String.IsNullOrEmpty(name)) throw new ArgumentException("Name is empty.");
            _name = name;
        }
    }
}

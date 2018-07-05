using System;

namespace FontAwesomeEnum
{
    // this is a sample prefix attribute, but you can define anything else in the namespace of your choice
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class PrefixAttribute : Attribute
    {
        public PrefixAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}

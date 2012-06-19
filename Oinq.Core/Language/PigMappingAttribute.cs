using System;

namespace Oinq.Core
{
    public class PigMapping : Attribute
    {
        // private fields
        private String _name;

        // constructors
        public PigMapping(String name)
        {
            _name = name;
        }

        // public properties
        public String Name
        {
            get { return _name; }
        }
    }
}

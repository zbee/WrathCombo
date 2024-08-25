using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVSlothCombo.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class AutoActionAttribute : Attribute
    {
        public bool IsAoE;
        public bool IsHeal;
        internal AutoActionAttribute(bool isAoE, bool isHeal) 
        { 
            IsAoE = isAoE;
            IsHeal = isHeal;
        }
    }
}

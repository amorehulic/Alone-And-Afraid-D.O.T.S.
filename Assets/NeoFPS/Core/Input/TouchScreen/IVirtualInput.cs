
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoFPS
{
    public interface IVirtualInput
    {
        bool GetVirtualButton(FpsInputButton button);
        float GetVirtualAxis(FpsInputAxis axis);
    }
}

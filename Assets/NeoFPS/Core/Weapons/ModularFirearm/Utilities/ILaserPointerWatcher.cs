using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoFPS
{
    public interface ILaserPointerWatcher
    {
        void RegisterLaserPointer(ILaserPointer laserPointer);
        void UnregisterLaserPointer(ILaserPointer laserPointer);
    }
}

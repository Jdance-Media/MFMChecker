using Rocket.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFMChecker
{
    public class MFMChecker : RocketPlugin
    {
        protected override void Load()
        {
            UnturnedPatches.Init();
        }
    }
}

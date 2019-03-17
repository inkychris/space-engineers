using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class LandingGearGroup : BlockGroup<IMyLandingGear>
        {
            public LandingGearGroup(List<IMyLandingGear> blocklist) : base(blocklist) { }

            private bool AreAll(LandingGearMode mode)
            {
                foreach (IMyLandingGear gear in group)
                    if (gear.LockMode != mode)
                        return false;
                return true;
            }

            private bool IsAtLeastOne(LandingGearMode mode)
            {
                foreach (IMyLandingGear gear in group)
                    if (gear.LockMode == mode)
                        return true;
                return false;
            }

            public bool AllLocked() { return AreAll(LandingGearMode.Locked); }
            public bool AtLeastOneLocked() { return IsAtLeastOne(LandingGearMode.Locked); }

            public bool AllUnlocked() { return AreAll(LandingGearMode.Unlocked); }
            public bool AtLeastOneUnlocked() { return IsAtLeastOne(LandingGearMode.Unlocked); }

            public bool AllLockable() { return AreAll(LandingGearMode.ReadyToLock); }
            public bool AtLeastOneLockable() { return IsAtLeastOne(LandingGearMode.ReadyToLock); }

            public void Lock()
            {
                foreach (IMyLandingGear gear in group)
                    gear.Lock();
            }

            public void Unlock()
            {
                foreach (IMyLandingGear gear in group)
                    gear.Unlock();
            }
        }
    }
}

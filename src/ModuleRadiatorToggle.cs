using System;
using System.Collections.Generic;
using UnityEngine;

namespace RadiatorToggle
{
    /// <summary>
    /// Adding this module to a radiator will enable toggling it to active/inactive
    /// via right-click menu and action groups.
    /// 
    /// A "radiator" is defined as anything that has ModuleActiveRadiator on it.
    /// Note that this would cause confusing behavior for any radiator that *also*
    /// has ModuleDeployableRadiator, since that module also tries to manage the
    /// active/inactive state of radiators. Therefore, do not add this module to
    /// deployable radiators.
    /// </summary>
    public class ModuleRadiatorToggle: PartModule
    {
        private static readonly String ACTIVE_STATUS = "Cooling";
        private static readonly String INACTIVE_STATUS = "Inactive";
        private static readonly String ACTIVATE_EVENT_TEXT = "Activate";
        private static readonly String DEACTIVATE_EVENT_TEXT = "Deactivate";

        /// <summary>
        /// Tracks the status of the radiator. The string is what's shown in the right-click menu.
        /// </summary>
        [KSPField(guiName = "Radiator Status", isPersistant = true, guiActive = true, guiActiveEditor = true)]
        public string status = ACTIVE_STATUS;

        /// <summary>
        /// Action-group method for toggling radiator.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Toggle Radiator")]
        public void toggleRadiatorAction(KSPActionParam actionParam)
        {
            isActive = !isActive;
        }

        /// <summary>
        /// Action-group method for enabling radiator.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Activate Radiator")]
        public void activateRadiatorAction(KSPActionParam actionParam)
        {
            isActive = true;
        }

        /// <summary>
        /// Action-group method for disabling radiator.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Deactivate Radiator")]
        public void deactivateRadiatorAction(KSPActionParam actionParam)
        {
            isActive = false;
        }

        /// <summary>
        /// Right-click context menu item for toggling radiator in flight scene.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiName = "Toggle Radiator")]
        public void toggleRadiatorFlightEvent()
        {
            isActive = !isActive;
        }

        /// <summary>
        /// Right-click context menu item for toggling radiator in edit.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPEvent(active = true, guiActive = false, guiActiveEditor = true, guiName = "Toggle Radiator")]
        public void toggleRadiatorEditorEvent()
        {
            // If this is done in the editor, we want to toggle it not just for this part,
            // but for all its symmetry counterparts as well.
            isActive = !isActive;
            Debug.Log("RadiatorToggle: Updating " + part.symmetryCounterparts.Count + " symmetry counterparts");
            foreach (Part counterpart in part.symmetryCounterparts)
            {
                foreach (ModuleRadiatorToggle module in counterpart.Modules.GetModules<ModuleRadiatorToggle>())
                {
                    module.isActive = isActive;
                }
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            // When the part is loaded, initialize the radiator state based on active/inactive
            // state in this module.
            base.OnLoad(node);
            setRadiatorState(part, Events, isActive);
        }

        /// <summary>
        /// Gets/sets whether the radiator is active.
        /// </summary>
        private bool isActive
        {
            get
            {
                return status == ACTIVE_STATUS;
            }
            set
            {
                if (isActive != value)
                {
                    if (setRadiatorState(part, Events, value))
                    {
                        status = value ? ACTIVE_STATUS : INACTIVE_STATUS;
                        Debug.Log("RadiatorToggle: Set status of" + part.name + " to " + status);
                    }
                    else
                    {
                        Debug.LogWarning("RadiatorToggle: Unable to adjust radiator status of " + this.part.name);
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to set radiator state. Returns true for success, false for failure.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="events"></param>
        /// <param name="isEnabled"></param>
        private static bool setRadiatorState(Part part, BaseEventList events, bool isEnabled)
        {
            List<ModuleActiveRadiator> radiatorModules = part.Modules.GetModules<ModuleActiveRadiator>();
            if (radiatorModules.Count == 0)
            {
                Debug.LogWarning("RadiatorToggle: No ModuleActiveRadiator found for " + part.name);
                events["toggleRadiatorFlightEvent"].active = false;
                events["toggleRadiatorEditorEvent"].active = false;
                return false;
            }
            foreach (ModuleActiveRadiator radiatorModule in radiatorModules)
            {
                radiatorModule.enabled = isEnabled;
            }
            String eventText = isEnabled ? DEACTIVATE_EVENT_TEXT : ACTIVATE_EVENT_TEXT;
            events["toggleRadiatorFlightEvent"].guiName = eventText;
            events["toggleRadiatorEditorEvent"].guiName = eventText;
            return true;
        }
    }
}

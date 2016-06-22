using System;
using UnityEngine;
using KSP.IO;
using System.Linq;
using System.Globalization;

namespace ABCORS
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    internal class ABookCaseOrbitalReferenceSystem : MonoBehaviour
    {
        private bool _mouseOver = false;
        private PatchedConics.PatchCastHit _mousedOverPatch;

        private Rect _popup = new Rect(0f, 0f, 160f, 160f);

        private bool _showTime = true;
        private bool _showAltitude = true;
        private bool _showSpeed = false;
        private bool _showEjectionAngle = false;

        protected void Start()
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<ABookCaseOrbitalReferenceSystem>();
            config.load();
            float rectWidth = config.GetValue<int>("displayWidth", 160);
            float rectHeight = config.GetValue<int>("displayHeight", 160);
            _popup.Set(0, 0, rectWidth, rectHeight);
            _showTime = config.GetValue<bool>("showTime", _showTime);
            _showAltitude = config.GetValue<bool>("showAltitude", _showAltitude);
            _showSpeed = config.GetValue<bool>("showSpeed", _showSpeed);
            _showEjectionAngle = config.GetValue<bool>("showEjectionAngle", _showEjectionAngle);
            config.save();
        }

        private void Awake()
        {
            _popup.center = new Vector2(Screen.width * 0.5f - _popup.width * 0.5f,
                Screen.height * 0.5f - _popup.height * 0.5f);
        }

        private void Update()
        {
            _mouseOver = MouseOverOrbit(out _mousedOverPatch);

            if (!_mouseOver)
                return;

            var updatedLocation = _mousedOverPatch.GetScreenSpacePoint();

            _popup.center = new Vector2(updatedLocation.x, Screen.height - updatedLocation.y);
        }

        private void OnGUI()
        {
            if (!_mouseOver)
                return;

            GUI.skin = HighLogic.Skin;

            Orbit orbit = _mousedOverPatch.pr.patch;
            Vector3d deltaPos = orbit.getPositionAtUT(_mousedOverPatch.UTatTA) - orbit.referenceBody.position;
            double altitude = deltaPos.magnitude - orbit.referenceBody.Radius;
            double speed = orbit.getOrbitalSpeedAt(orbit.getObtAtUT(_mousedOverPatch.UTatTA));

            string labelText = "";
            if (_showTime)
            {
                labelText += "T: " + KSPUtil.PrintTime((int)(Planetarium.GetUniversalTime() - _mousedOverPatch.UTatTA), 5, true) + "\n";
            }
            if (_showAltitude)
            {
                labelText += "Alt: " + altitude.ToString("N0", CultureInfo.CurrentCulture) + "m\n";
            }
            if (_showSpeed)
            {
                labelText += "Vel: " + speed.ToString("N0", CultureInfo.CurrentCulture) + "m/s\n";
            }
            if (_showEjectionAngle && orbit.referenceBody.orbit != null)
            {
                Vector3d bodyFwd = orbit.referenceBody.orbit.getOrbitalVelocityAtUT(_mousedOverPatch.UTatTA);
                Vector3d shipNormal = deltaPos;
                double angle = Vector3d.Angle(shipNormal, bodyFwd);
                labelText += "E\u03B1: " + angle.ToString("N2", CultureInfo.CurrentCulture) + "\u00B0\n";
                labelText += bodyFwd.ToString() + "\n" + shipNormal.ToString();
            }

            GUILayout.BeginArea(GUIUtility.ScreenToGUIRect(_popup));
            GUILayout.Label(labelText);
            GUILayout.EndArea();
        }

        private bool MouseOverOrbit(out PatchedConics.PatchCastHit hit)
        {
            hit = default(PatchedConics.PatchCastHit);

            if (FlightGlobals.ActiveVessel == null)
                return false;

            var patchRenderer = FlightGlobals.ActiveVessel.patchedConicRenderer;

            if (patchRenderer == null)
                return false;

            var patches = patchRenderer.solver.maneuverNodes.Any()
                ? patchRenderer.flightPlanRenders
                : patchRenderer.patchRenders;

            return PatchedConics.ScreenCast(Input.mousePosition, patches, out hit);
        }
    }
}

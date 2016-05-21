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

        private Rect _popup = new Rect(0f, 0f, 160f, 132f);

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
			Vector3d pos = orbit.getPositionAtUT(_mousedOverPatch.UTatTA);
			double altitude = (pos - orbit.referenceBody.position).magnitude - orbit.referenceBody.Radius;
			double speed = orbit.getOrbitalSpeedAtPos(pos);

			GUILayout.BeginArea(GUIUtility.ScreenToGUIRect(_popup));
			GUILayout.Label("T: " + KSPUtil.PrintTime((int)(Planetarium.GetUniversalTime() - _mousedOverPatch.UTatTA), 5, true) + "\nAlt: " + altitude.ToString("N0", CultureInfo.CurrentCulture) + "m\nVel: " + speed.ToString("N0", CultureInfo.CurrentCulture) + "m/s");

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

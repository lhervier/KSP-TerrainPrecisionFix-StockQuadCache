using System;
using HarmonyLib;
using UnityEngine;

namespace com.github.lhervier.ksp.stockquadcache
{
    /// <summary>
    /// Places terrain vertices with stock's own arithmetic, reading the two Transforms it needs once per
    /// quad instead of once per vertex. The terrain it builds is the terrain stock builds, vertex for
    /// vertex and bit for bit: only what it costs changes.
    ///
    /// It is a measuring aid, not a fix. Stock computes every terrain vertex in
    /// PQS.BuildVertexSurfaceRelative, a method called once per vertex which reads base.transform and
    /// buildQuad.transform every time. Terrain Precision Fix replaces that method with arithmetic of its
    /// own, and that replacement also happens to read those two Transforms once per quad. The two
    /// changes are therefore measured together, and nothing says how the time splits between them.
    /// Installed on its own, this mod carries the second change without the first, so a run against it
    /// says what each is worth.
    ///
    /// Only the quads of the highest subdivision level are touched, which is the scope Terrain Precision
    /// Fix acts on: a comparison between the two is only worth something over the same quads.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class StockQuadCacheMod : MonoBehaviour
    {
        private const string HarmonyId = "com.github.lhervier.ksp.stockquadcache";

        // Set once the patches are in. Until then, and after a failed install, the patches stand aside and
        // the terrain is built by stock.
        private static bool _active;

        // Private fields of PQS, read the same way stock reads them. Bound once: the vertex patch runs for
        // every vertex of every terrain quad.
        private static AccessTools.FieldRef<PQS, PQ> _buildQuad;
        private static AccessTools.FieldRef<PQS, int> _vertexIndex;

        private void Start()
        {
            Log.LoadLevel();
            try
            {
                _buildQuad = AccessTools.FieldRefAccess<PQS, PQ>("buildQuad");
                _vertexIndex = AccessTools.FieldRefAccess<PQS, int>("vertexIndex");
                new Harmony(HarmonyId).PatchAll(typeof(StockQuadCacheMod).Assembly);
                _active = true;
                Log.Info($"Version {typeof(StockQuadCacheMod).Assembly.GetName().Version} installed,"
                    + $" log level {Log.Level}");
            }
            catch (Exception e)
            {
                // With _active left false, whatever patch did get applied does nothing: the terrain is
                // exactly stock.
                Log.Error($"Could not install the patches, the terrain is left to stock: {e}");
            }
        }

        /// <summary>Whether this mod acts on this quad.</summary>
        private static bool AppliesTo(PQ quad)
        {
            PQS sphere = quad.sphereRoot;

            // Stock has two ways of placing vertices, and only the surface relative one goes through the
            // two Transforms this reads once per quad.
            if (!_active || sphere == null || !sphere.surfaceRelativeQuads || sphere.LocalSpacePQStorage == null)
            {
                return false;
            }

            // The quads of the highest subdivision level, the ones the game detaches into
            // LocalSpacePQStorage. Same test, on the same stock fields, as the one Terrain Precision Fix
            // uses to decide where it acts.
            return quad.transform.parent == sphere.LocalSpacePQStorage.transform;
        }

        // ==========================================================================
        // Where each vertex goes inside its quad
        // ==========================================================================

        /// <summary>
        /// Replaces the stock placement of a terrain vertex, on the quads this applies to, by the same
        /// computation reading its two Transforms once per quad.
        /// </summary>
        [HarmonyPatch(typeof(PQS), "BuildVertexSurfaceRelative")]
        private static class BuildVertexSurfaceRelativePatch
        {
            private static bool Prefix(PQS __instance, PQS.VertexBuildData data)
            {
                if (!_active)
                {
                    return true;
                }
                PQ quad = _buildQuad(__instance);
                if (quad == null)
                {
                    return true;
                }

                // The vertex relative to the centre of the body, exactly as stock works it out.
                Vector3d vertex = data.directionFromCenter * data.vertHeight;
                return !PlaceVertex(__instance, quad, _vertexIndex(__instance), vertex);
            }
        }

        /// <summary>
        /// Puts one terrain vertex where stock puts it, inside its quad. Returns whether it did: a vertex
        /// this does not apply to is left to stock, untouched.
        /// </summary>
        private static bool PlaceVertex(PQS sphere, PQ quad, int index, Vector3d vertex)
        {
            // The only difference with stock: the two Transforms are asked for once per quad and kept for
            // its couple of hundred vertices, where stock asks Unity for each of them on every vertex.
            if (!ReferenceEquals(quad, _contextQuad))
            {
                BuildQuadContext(sphere, quad);
            }
            if (!_contextApplies)
            {
                return false;
            }
            if (quad.verts == null || PQS.verts == null
                || index < 0 || index >= quad.verts.Length || index >= PQS.verts.Length)
            {
                return false;
            }

            // Stock's arithmetic, untouched and in the same order, on the same Transform objects: the
            // result is the same float, bit for bit. Stock keeps the vertex relative to the centre of the
            // body alongside the quad-local one, because the normals are computed from it.
            Vector3 planetRelative = _contextSphereTransform.TransformPoint((Vector3)vertex);
            PQS.verts[index] = vertex;
            quad.verts[index] = _contextQuadTransform.InverseTransformPoint(planetRelative);
            return true;
        }

        // What placing a vertex keeps from one vertex to the next, and the quad it holds for. Only ever
        // read after BuildQuadContext has run for that same quad.
        private static PQ _contextQuad;
        private static bool _contextApplies;
        private static Transform _contextSphereTransform;
        private static Transform _contextQuadTransform;

        /// <summary>
        /// Works out whether this applies to the quad, and the two Transforms its vertices are placed
        /// through.
        /// </summary>
        private static void BuildQuadContext(PQS sphere, PQ quad)
        {
            _contextQuad = quad;
            _contextApplies = AppliesTo(quad);
            if (!_contextApplies)
            {
                return;
            }
            _contextSphereTransform = sphere.transform;
            _contextQuadTransform = quad.transform;
        }

        /// <summary>Forgets what was kept for a quad, so that the next vertex asks for it again.</summary>
        private static void ForgetQuadContext()
        {
            _contextQuad = null;
        }

        /// <summary>
        /// A quad build starting is the signal that anything kept per quad is stale.
        ///
        /// Nothing here would actually go wrong without it — what is kept is two Transform references,
        /// which stay valid across a rebuild and are read at their current value every time. It is here
        /// because it is the signal a per-quad cache is expected to listen to, and because PQS Bench uses
        /// it to hand a formula a cold quad for each round it times.
        /// </summary>
        [HarmonyPatch(typeof(PQS), "BuildQuad")]
        private static class BuildQuadPatch
        {
            private static void Prefix()
            {
                ForgetQuadContext();
            }
        }
    }
}

# Stock Quad Cache

Places KSP terrain vertices with **stock's own arithmetic**, reading the two `Transform`s it needs
**once per quad** instead of once per vertex. The terrain it builds is the terrain stock builds, vertex
for vertex and bit for bit. Only what it costs changes.

**This is a measuring aid, not a mod to play with.** It fixes nothing and is not meant to be published.

## Why it exists

Stock computes every terrain vertex in `PQS.BuildVertexSurfaceRelative`, a method called once for each
vertex of each quad, which reads `base.transform` and `buildQuad.transform` every time:

```csharp
vertRel = vbData.directionFromCenter * vbData.vertHeight;
planetRel = base.transform.TransformPoint(vertRel);
verts[vertexIndex] = vertRel;
buildQuad.verts[vertexIndex] = buildQuad.transform.InverseTransformPoint(planetRel);
```

[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix) replaces that method with
arithmetic of its own — and its replacement also works its two frames out once per quad. So a run of
that mod against stock measures **two changes at once**, and says nothing about how the time splits
between them.

This mod carries the second change without the first: the same four lines, the same `Transform` objects,
the same floats out — with the two `Transform`s asked for once per quad. A run against it is therefore
the middle term of a three-way comparison:

| installed | what a terrain vertex costs |
|---|---|
| nothing | stock |
| this | stock's arithmetic, `Transform`s read once per quad |
| Terrain Precision Fix | its arithmetic, frames worked out once per quad |

Measured with [PQS Bench](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench), whose
`calibrate` mode times whatever is installed against a copy of the stock formula, in the same frame and
on the same quad.

## Scope

Only the quads of the **highest subdivision level** — the ones the game detaches into
`LocalSpacePQStorage`, which carry the collider craft stand on. That is exactly the scope Terrain
Precision Fix acts on, and a comparison between the two is only worth something over the same quads.
Every other quad is left to stock.

## Install

Requires KSP 1.12 and [HarmonyKSP](https://github.com/KSPModdingLibs/HarmonyKSP) (the usual
`GameData/000_Harmony`). Copy `GameData/StockQuadCacheMod` into the `GameData` of KSP.

Do not install it alongside Terrain Precision Fix: both patch the same method, and which one wins is
whichever Harmony runs first. Each run of a campaign has exactly one of them in `GameData`, or neither.

## Settings

`GameData/StockQuadCacheMod/PluginData/settings.cfg`, read once when KSP starts. `logLevel` takes
`Error`, `Warning`, `Info` (default), `Debug` or `Trace`. **Measure at `Info`.**

## Build

Set `KSPDIR` to your KSP install folder, which must contain `GameData/000_Harmony`, and run `build.bat`.
It needs the .NET SDK, and produces `GameData/StockQuadCacheMod/StockQuadCacheMod.dll`.

## Licence

MIT, see [LICENSE](LICENSE).

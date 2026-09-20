# Stock Quad Cache

**⚠️ Work in progress.** This is an active investigation, not a finished mod. The figures, the code and the conclusions on this page can still change, and several questions are still open.

**How this was made.** Written with Claude, Anthropic's AI assistant, and reviewed line by line by a
human — me. I am saying so before anything else, because contributions made with an AI deserve a closer
look than others, and because some people would rather stop reading here. What there is to look at is
small: the patch is one source file, and every figure on this page comes from a run whose log is in this
repository.

> **Do not install this to play.** This is a measuring instrument, not a fix. It corrects nothing, and
> what it saves is **0.16 ms per second of flight** 5 km over the Mun — 0.016 % of real time, which is
> less than two sessions of KSP differ from each other. It is published so that a figure quoted in two
> other repositories can be checked, not because it is worth having.

## What it does

Stock computes every terrain vertex in `PQS.BuildVertexSurfaceRelative`, called once for each vertex of
each quad — 225 times for a quad of the highest subdivision level:

```csharp
vertRel = vbData.directionFromCenter * vbData.vertHeight;
planetRel = base.transform.TransformPoint(vertRel);
verts[vertexIndex] = vertRel;
buildQuad.verts[vertexIndex] = buildQuad.transform.InverseTransformPoint(planetRel);
```

Two of those reads do not depend on the vertex. `base.transform` is the terrain sphere's, and
`buildQuad.transform` is the quad's: both are the same for all 225 vertices, and stock asks Unity for
them on every one. This mod asks once per quad and keeps them for the rest of it.

Everything else is stock's, untouched and in the same order, on the same `Transform` objects. The same
floats come out, bit for bit: the terrain it builds is the terrain stock builds. Only what it costs
changes.

It acts on the quads of the **highest subdivision level** only — the ones the game detaches into
`LocalSpacePQStorage`, which carry the collider craft stand on. That is exactly the scope
[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix) acts on, and a comparison
between the two is only worth something over the same quads. Every other quad is left to stock.

## What it costs

Measured with [PQS Bench](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench), whose
`calibrate` mode times whatever is installed against the stock formula itself, on the same quads and in
the same run. **Its page carries the procedure**; the run read here, with its logs, is in
[`perfs/`](perfs/). A command pod on rails 5 km over the Mun, 40 quads calibrated, 72 000 vertices per
formula:

| | ns per vertex |
|---|---|
| stock, this run's own yardstick | 232.3 |
| with this mod | 149.5 |
| difference | **−82.8**, or 41 ns per `Transform` read |

**Reading the two `Transform`s once per quad instead of once per vertex takes 36 % off the cost of
placing a vertex**, and the Harmony wrapper the mod pays for its own patch is inside that figure.

It is also undetectable in flight, and the two statements are not in conflict. At that altitude the game
builds 8.5 quads of the highest level per second, so 1 917 vertices: 82.8 ns each comes to 0.16 ms per
second of flight, 0.016 % of real time. Two sessions of KSP replaying the same stretch of the same orbit
differ by more than a percent either way, so a saving that small cannot be seen in a frame rate.
[`perfs/`](perfs/) has both readings, and shows why only a difference taken inside one run means
anything.

## Why it exists

[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix) replaces that same method,
because the stock version loses precision: the ground it builds comes back at a slightly different
height at every load. Its replacement is arithmetic of its own, in double precision, in a frame worked
out for each quad — and it is not merely free, it is faster than stock: 3.56× per vertex, on
[its page](https://github.com/lhervier/KSP-TerrainPrecisionFix/blob/main/perfs/README.md).

That figure invites an obvious objection, and the objection is fair. The replacement changes **two**
things at once: the arithmetic, and the fact that what does not depend on the vertex is worked out once
per quad. The second one fixes nothing — stock could have it too, and a mod can give it to stock without
touching a single float. So how much of that 3.56× is the better arithmetic, and how much is a cache
that stock simply does not have?

A run of the fix against stock cannot answer, because it only ever shows the two changes together. This
mod carries the second without the first, which makes the comparison a three-term one:

| installed | `differenceNsPerVertex` | measured in |
|---|---|---|
| nothing | −1.1, the floor of the method | [PQS Bench](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench/blob/main/perfs/README.md) |
| **this mod** | **−82.8** | [`perfs/`](perfs/), here |
| Terrain Precision Fix | −164.3 | [its own page](https://github.com/lhervier/KSP-TerrainPrecisionFix/blob/main/perfs/README.md) |

The answer is half and half: 82.8 ns for the two `Transform` reads, and almost exactly as much again,
81.5 ns, for the arithmetic — the split is [on the fix's page](https://github.com/lhervier/KSP-TerrainPrecisionFix/blob/main/perfs/README.md).
Give stock this cache and a vertex still costs 64 % of what stock costs in the same run, against 28 %
for the fix: a little over twice as much. The fix is not faster merely for being better organised, and
that one sentence is what this mod exists to support.

It is published for the same reason. Both of the other two figures are on public pages that quote this
one, and a term of a comparison that nobody can run is a number that nobody can check.

The step this page cannot put a number on is the one before: the fix works its frame out once per quad
because doing it per vertex would cost several times the placement itself. That is reasoning about the
code, not a measurement, and it belongs to [the fix's own
page](https://github.com/lhervier/KSP-TerrainPrecisionFix/blob/main/perfs/README.md).

## Install

Requires KSP 1.12 and [HarmonyKSP](https://github.com/KSPModdingLibs/HarmonyKSP) (the usual
`GameData/000_Harmony`). Copy `GameData/StockQuadCacheMod` into the `GameData` of KSP.

**Never alongside Terrain Precision Fix.** Both patch the same method, and which one wins is whichever
Harmony runs first. A run of a measuring campaign has one of them, the other, or neither.

## Settings

`GameData/StockQuadCacheMod/PluginData/settings.cfg`, read once when KSP starts. `logLevel` takes
`Error`, `Warning`, `Info` (default), `Debug` or `Trace`. **Measure at `Info`.**

## Build

Set `KSPDIR` to your KSP install folder, which must contain `GameData/000_Harmony`, and run `build.bat`.
It needs the .NET SDK, and produces `GameData/StockQuadCacheMod/StockQuadCacheMod.dll`.

## License

MIT, see [LICENSE](LICENSE).

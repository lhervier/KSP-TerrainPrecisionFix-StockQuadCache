# What reading the two `Transform`s costs

This mod exists to put a number on one thing: what stock pays for asking Unity for
`base.transform` and `buildQuad.transform` on **every** terrain vertex, instead of once per quad. It
changes nothing else, so the difference between a run with it and a run without it is that, and only
that.

Measured with [PQS Bench](https://github.com/lhervier/KSP-PQSBench), **whose page carries the
procedure** and whose own `perfs/` carries the stock runs these figures are read against.

## The runs

KSP 1.12.5. `GameData` holding Harmony, ModuleManager, KSP Community Fixes 1.41.1, the measuring mod and
this one. A command pod on rails in a circular orbit 5 km over the Mun, 70 seconds of game time from 30 s
of mission time. The save, the machine and the order of the runs are the stock runs', all described on
[their page](https://github.com/lhervier/KSP-PQSBench/blob/master/perfs/README.md) — figures from another
machine are not comparable to these.

| log | starts at | game time | real time | quads of the highest level built |
|---|---|---|---|---|
| [`mun-05km-stockquadcache-calibrate-1.log`](runs/mun-05km-stockquadcache-calibrate-1.log) | UT 54.64 | 70.02 s | 70.16 s | 704 |
| [`mun-05km-stockquadcache-calibrate-2.log`](runs/mun-05km-stockquadcache-calibrate-2.log) | UT 54.68 | 70.12 s | 70.26 s | 704 |

The figures come from their `BENCH run` lines, and match the stock runs': same save, same craft, same
stretch of the same orbit, the same quads built.

## What a vertex costs

22 quads calibrated per run, 39 600 vertices per formula:

| | run 1 | run 2 |
|---|---|---|
| `stockNsPerVertex` (the run's own yardstick) | 282.7 | 290.6 |
| `installedNsPerVertex` | 200.9 | 203.3 |
| `differenceNsPerVertex` | **−81.7** | **−87.4** |

**Hoisting the two `Transform` reads out of the per-vertex loop is worth about 85 ns, or 42 ns per
read.** Everything else is identical: same arithmetic, same order, same `Transform` objects, and the
Harmony wrapper this mod pays for its own patch is inside that figure.

Each figure is the difference within its own run, not an `installed` column set against another run's:
from one session of KSP to the next the whole replay runs a little faster or slower, and only a
difference taken inside one session cancels that out. The two runs land 5.7 ns apart.
[PQS Bench](https://github.com/lhervier/KSP-PQSBench/blob/master/perfs/README.md) shows how far apart
sessions land, and what the instrument reports with nothing installed: about 3 ns of difference, one way
or the other, between two ways of running the same code.

**The terrain this mod builds is stock's, bit for bit, by construction** — which is what makes it a
witness rather than a second fix. The instrument does not check it: where a vertex lands is not a
question about performance. The day this mod's arithmetic is touched, it stops standing for what it is
supposed to, and its figure is worth nothing.

## In a frame

This mod was also timed frame by frame, with the same save, against stock and against Terrain Precision
Fix. Those runs are read together, so they are kept together:
[with Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix/blob/master/perfs/README.md#what-a-frame-pays).

## Where this fits

Three configurations of the same flight, each in its own repository with its own logs, each read against
its own yardstick:

| installed | `differenceNsPerVertex`, runs 1 and 2 | |
|---|---|---|
| nothing | −2.5 and +3.4, the floor of the method | [PQS Bench](https://github.com/lhervier/KSP-PQSBench/blob/master/perfs/README.md) |
| **this mod** | **−81.7 and −87.4** | here |
| Terrain Precision Fix | −187.0 and −178.9 | [its own page](https://github.com/lhervier/KSP-TerrainPrecisionFix/blob/master/perfs/README.md) |

On average, the first step, 84.6 ns, is the `Transform` reads. The second, 98.4 ns, is the arithmetic.
That split is the whole reason this mod was written.

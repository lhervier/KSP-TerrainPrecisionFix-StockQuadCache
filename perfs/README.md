# What reading the two `Transform`s costs

This mod exists to put a number on one thing: what stock pays for asking Unity for
`base.transform` and `buildQuad.transform` on **every** terrain vertex, instead of once per quad. It
changes nothing else, so the difference between a run with it and a run without it is that, and only
that.

Measured with [PQS Bench](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench), **whose page
carries the procedure** and whose own `perfs/` carries the stock run these figures are read against.

## The runs

2026-09-15, KSP 1.12.5. `GameData` holding Harmony, ModuleManager, KSP Community Fixes, the measuring
mod and this one. A command pod on rails in a circular orbit 5 km over the Mun, 150 seconds of game
time. The save and the machine are the stock run's, both described on
[its page](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench/blob/main/perfs/README.md) —
figures from another machine are not comparable to these.

| log | mode |
|---|---|
| [`mun-05km-stockquadcache-calibrate.log`](runs/mun-05km-stockquadcache-calibrate.log) | `calibrate` |
| [`mun-05km-stockquadcache-counters.log`](runs/mun-05km-stockquadcache-counters.log) | `counters` |

Their `BENCH run` lines match the stock runs': same save, same craft, over the Mun from UT 54.84 and
54.70 at 5 000.0 m, for 150 seconds of game time against as much real time. The `counters` run recorded
148 samples and built **3 213 quads, of which 1 272 of the highest subdivision level**. The craft is on
rails; the same save covers the same ground.

## What a vertex costs

40 quads calibrated, 72 000 vertices per formula:

| | |
|---|---|
| `stockNsPerVertex` (this run's own yardstick) | 232.3 |
| `installedNsPerVertex` | 149.5 |
| `differenceNsPerVertex` | **−82.8** |

**Hoisting the two `Transform` reads out of the per-vertex loop is worth 82.8 ns, or 41 ns per read.**
Everything else is identical: same arithmetic, same order, same `Transform` objects, and the Harmony
wrapper this mod pays for its own patch is inside that figure.

The figure is the difference within this run, not 149.5 set against the stock run's `installed`
column: from one session of KSP to the next the whole replay runs a little faster or slower, and only a
difference taken inside one session cancels that out. [PQS Bench](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench/blob/main/perfs/README.md)
shows how far apart sessions land, and what the instrument reports with nothing installed: 1.1 ns of
difference between two ways of running the same code.

**The terrain this mod builds is stock's, bit for bit, by construction** — which is what makes it a
witness rather than a second fix. The instrument does not check it: where a vertex lands is not a
question about performance. The day this mod's arithmetic is touched, it stops standing for what it is
supposed to, and its figure is worth nothing.

## In flight

| | this mod | [stock](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench/blob/main/perfs/README.md) |
|---|---|---|
| frames per second | 75.82 | 77.04 |
| ms per quad of the highest level | 1.682 | 1.716 |
| terrain per frame | 1.170 ms | 1.195 ms |
| terrain share of real time | 8.87 % | 9.21 % |

**The measures of the same flight do not agree with the calibration, and that is the useful result.**
By the calibration this mod should be ahead by 82.8 ns × 225 vertices, 18.6 µs per quad: 1.1 % of
1.716 ms. Timed per quad, it is 2.0 % ahead — nearly twice what the calibration allows. Timed per frame,
it is 2.1 % ahead, which it cannot be: it only acts on the 8.5 quads of the highest level built each
second, so the most it can take off is 0.16 ms per second of flight, out of 92 ms of terrain — 0.17 %.
And its frame rate is 1.6 % lower.

Nothing is wrong with either measurement. They are measuring different things, and the second one is
mostly measuring the noise between two sessions of KSP. A saving of a percent of a quad is below what a
run of the game reproduces from one session to the next, and no amount of care with the flight path
changes that — the two runs flew the same stretch of the same orbit.

Put in the units a player would feel: at this altitude the game builds 8.5 of these quads a second, so
1 917 vertices, and 82.8 ns each is **0.16 ms per second of flight — 0.016 % of real time**.

So: **82.8 ns per vertex is real; it is also impossible to feel while playing.** That is worth stating
plainly, because the same reasoning applies to the mod this one was written to help measure —
[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix), which saves twice as much
per vertex and is just as invisible in a frame. What a `counters` run can establish is a bound: nothing
degrades. What a gain is worth belongs to the calibration.

## Where this fits

Three configurations of the same flight, each in its own repository with its own logs, each read against
its own yardstick:

| installed | `differenceNsPerVertex` | |
|---|---|---|
| nothing | −1.1, the floor of the method | [PQS Bench](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench/blob/main/perfs/README.md) |
| **this mod** | **−82.8** | here |
| Terrain Precision Fix | −164.3 | [its own page](https://github.com/lhervier/KSP-TerrainPrecisionFix/blob/main/perfs/README.md) |

The first step, 82.8 ns, is the `Transform` reads. The second, 81.5 ns, is the arithmetic. That split
is the whole reason this mod was written.

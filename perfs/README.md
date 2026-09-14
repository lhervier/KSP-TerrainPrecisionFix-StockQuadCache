# What reading the two `Transform`s costs

This mod exists to put a number on one thing: what stock pays for asking Unity for
`base.transform` and `buildQuad.transform` on **every** terrain vertex, instead of once per quad. It
changes nothing else, so the difference between a run with it and a run without it is that, and only
that.

Measured with [PQS Bench](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench), **whose page
carries the procedure** and whose own `perfs/` carries the stock run these figures are read against.

## The runs

2026-09-14, KSP 1.12.5. `GameData` holding Harmony, ModuleManager, KSP Community Fixes, the measuring
mod and this one. Same save as the stock run: a command pod on rails in a circular orbit 5 km over the
Mun, 150 seconds of game time.

| log | mode |
|---|---|
| [`mun-05km-stockquadcache-calibrate.log`](runs/mun-05km-stockquadcache-calibrate.log) | `calibrate` |
| [`mun-05km-stockquadcache-counters.log`](runs/mun-05km-stockquadcache-counters.log) | `counters` |

Both built the same **2 484 quads, of which exactly 960 of the highest subdivision level**, as the stock
runs did, over 147 samples. The craft is on rails; the same save covers the same ground.

## What a vertex costs

| | this mod | [stock](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench/blob/main/perfs/README.md) |
|---|---|---|
| `installedNsPerVertex` | **206.6** | 285.2 |
| `stockNsPerVertex` (the run's own yardstick) | 283.6 | 283.0 |
| `differingQuads` | 0 / 30 | 0 / 30 |

**Hoisting the two `Transform` reads out of the per-vertex loop is worth 78.6 ns, or 39.3 ns per read.**
Everything else is identical: same arithmetic, same order, same `Transform` objects, and the Harmony
wrapper this mod pays for its own patch is inside that figure.

**`differingQuads = 0` is what makes this a witness rather than a second fix.** On every calibrated
quad, every vertex landed exactly where stock puts it, compared component by component. The terrain is
stock's terrain; only the cost changed. The day that number is not zero, this mod has drifted from what
it is supposed to stand for and is worth nothing.

The two runs' own `stock` readings agree within 0.2 %, so the comparison above is between two sessions
that were running at the same speed.

## In flight

| | this mod | [stock](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench/blob/main/perfs/README.md) |
|---|---|---|
| frames per second | 114.09 | 114.72 |
| ms per quad of the highest level | 2.795 | 2.771 |
| terrain per frame | 0.984 ms | 0.947 ms |
| terrain share of real time | 11.22 % | 10.86 % |

**This mod comes out slower in flight, and that is the useful result.** By the calibration it should be
ahead: 78.6 ns × 225 vertices is 17.7 µs per quad, 0.6 % of 2.771 ms. It is 0.9 % behind instead.

Nothing is wrong with either measurement. They are measuring different things, and the second one is
mostly measuring the noise between two sessions of KSP. A saving of a fraction of a percent of a quad is
below what a run of the game reproduces from one session to the next, and no amount of care with the
flight path changes that — the two runs really did build the same 2 484 quads, and still landed a
percent apart.

Put in the units a player would feel: at this altitude the game builds 6.4 of these quads a second, so
1 440 vertices, and 78.6 ns each is **0.11 ms per second of flight — 0.011 % of real time**.

So: **78.6 ns per vertex is real and repeatable; it is also impossible to feel while playing.** That is
worth stating plainly, because the same reasoning applies to the mod this one was written to help
measure — [Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix), which saves
more than twice as much per vertex and is just as invisible in a frame. What a `counters` run can
establish is a bound: nothing degrades. What a gain is worth belongs to the calibration.

## Where this fits

Three configurations of the same flight, each in its own repository with its own logs:

| installed | ns per vertex | |
|---|---|---|
| nothing | 285.2 | [PQS Bench](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench/blob/main/perfs/README.md) |
| **this mod** | **206.6** | here |
| Terrain Precision Fix | 103.7 | [its own page](https://github.com/lhervier/KSP-TerrainPrecisionFix/blob/main/perfs/README.md) |

The first step, 78.6 ns, is the `Transform` reads. The second, 102.9 ns, is the arithmetic. That split
is the whole reason this mod was written.

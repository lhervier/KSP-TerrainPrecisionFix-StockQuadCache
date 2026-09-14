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

Their `BENCH run` lines match the stock runs': same save, same craft, over the Mun from UT 335.5 at
4 999.8 m, for 150 seconds of game time against as much real time. The `counters` run recorded 147
samples and built **2 484 quads, of which 960 of the highest subdivision level**. The craft is on rails;
the same save covers the same ground.

## What a vertex costs

30 quads calibrated, 54 000 vertices per formula:

| | |
|---|---|
| `stockNsPerVertex` (this run's own yardstick) | 290.3 |
| `installedNsPerVertex` | 214.1 |
| `differenceNsPerVertex` | **−76.1** |
| `differingQuads` | 0 / 30 |

**Hoisting the two `Transform` reads out of the per-vertex loop is worth 76.1 ns, or 38 ns per read.**
Everything else is identical: same arithmetic, same order, same `Transform` objects, and the Harmony
wrapper this mod pays for its own patch is inside that figure.

The figure is the difference within this run, not 214.1 set against the stock run's `installed`
column: from one session of KSP to the next the whole replay runs a little faster or slower, and only a
difference taken inside one session cancels that out. [PQS Bench](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench/blob/main/perfs/README.md)
shows how far apart sessions land, and what the instrument reports with nothing installed: 1.2 ns of
difference between two ways of running the same code, which makes this saving slightly conservative.

**`differingQuads = 0` is what makes this a witness rather than a second fix.** On every calibrated
quad, every vertex landed exactly where stock puts it, compared component by component. The terrain is
stock's terrain; only the cost changed. The day that number is not zero, this mod has drifted from what
it is supposed to stand for and is worth nothing.

## In flight

| | this mod | [stock](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench/blob/main/perfs/README.md) |
|---|---|---|
| frames per second | 114.49 | 114.56 |
| ms per quad of the highest level | 2.748 | 2.775 |
| terrain per frame | 0.940 ms | 0.937 ms |
| terrain share of real time | 10.76 % | 10.74 % |

**The two measures of the same flight disagree, and that is the useful result.** By the calibration
this mod should be ahead: 76.1 ns × 225 vertices is 17.1 µs per quad, 0.6 % of 2.775 ms. Timed per
quad, it is 1.0 % ahead — more than the calibration allows. Timed per frame, it is 0.3 % behind.

Nothing is wrong with either measurement. They are measuring different things, and the second one is
mostly measuring the noise between two sessions of KSP. A saving of a fraction of a percent of a quad is
below what a run of the game reproduces from one session to the next, and no amount of care with the
flight path changes that — the two runs flew the same stretch of the same orbit, and still disagree on
which of them was faster.

Put in the units a player would feel: at this altitude the game builds 6.4 of these quads a second, so
1 440 vertices, and 76.1 ns each is **0.11 ms per second of flight — 0.011 % of real time**.

So: **76.1 ns per vertex is real and repeatable; it is also impossible to feel while playing.** That is
worth stating plainly, because the same reasoning applies to the mod this one was written to help
measure — [Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix), which saves
more than twice as much per vertex and is just as invisible in a frame. What a `counters` run can
establish is a bound: nothing degrades. What a gain is worth belongs to the calibration.

## Where this fits

Three configurations of the same flight, each in its own repository with its own logs, each read against
its own yardstick:

| installed | `differenceNsPerVertex` | |
|---|---|---|
| nothing | +1.2, the floor of the method | [PQS Bench](https://github.com/lhervier/KSP-TerrainPrecisionFix-PQSBench/blob/main/perfs/README.md) |
| **this mod** | **−76.1** | here |
| Terrain Precision Fix | −180.0 | [its own page](https://github.com/lhervier/KSP-TerrainPrecisionFix/blob/main/perfs/README.md) |

The first step, 76.1 ns, is the `Transform` reads. The second, 103.9 ns, is the arithmetic. That split
is the whole reason this mod was written.

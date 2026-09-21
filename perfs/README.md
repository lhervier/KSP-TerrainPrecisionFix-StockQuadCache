# What reading the two `Transform`s costs: the runs

The logs this mod's figures are read from. The figures themselves, and what they say, are in
[What it costs](../README.md#what-it-costs) and [Why it exists](../README.md#why-it-exists).

Measured with [PQS Bench](https://github.com/lhervier/KSP-PQSBench), **whose page carries the
procedure** and whose own `perfs/` carries the stock runs these are read against.

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
stretch of the same orbit, the same quads built. Their result lines, as logged:

```
BENCH calibration;quads=22;roundsPerQuad=8;verticesPerFormula=39600;stockNsPerVertex=282.7;installedNsPerVertex=200.9;differenceNsPerVertex=-81.7;harnessNsPerVertex=5.9;stockRawNsPerVertex=288.6;installedRawNsPerVertex=206.9
BENCH calibration;quads=22;roundsPerQuad=8;verticesPerFormula=39600;stockNsPerVertex=290.6;installedNsPerVertex=203.3;differenceNsPerVertex=-87.4;harnessNsPerVertex=5.1;stockRawNsPerVertex=295.7;installedRawNsPerVertex=208.3
```

## In a frame

This mod was also timed frame by frame, with the same save, against stock and against Terrain Precision
Fix. Those runs are read together, so they are kept together:
[with Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix/blob/master/perfs/README.md#what-a-frame-pays).

# Cost Optimised (<200 GBP) Active Belt Tensioner
A cost-optimised and simplified active belt tensioner for sim racing. This has been designed for people who do not have a background in software or electronics; it requires no soldering or programming.

![20260222_164041](https://github.com/user-attachments/assets/2179f3e5-fdd0-48b4-a8db-6886b8e6a477)

## What Is It?

An active belt tensioner is a device that attaches to your sim rig and racing harness. It actively tensions the harness in response to in-simulation telemetry, giving a sense of the forces you'd be experencing in the vehicle when changing speed, braking, cornering and jumping/landing.

This projects consists of two parts:
- [Hardware](#) made up of off-the-shelf components and some easily printed parts
- A [SimHub plugin](#) that controls the hardware and allows customisation of the effects

The goals of this particular design are:
- Low Cost
- Simplicity
- Minimal Parts
- No Soldering
- No Progamming
- No 'Reclaimed' Motors
- Direct-Drive (FOC/BLDC) Performance

## What Does It Cost?

If you have your own 3D printer, _well under_ **200 GBP**. If not, _roughly_ **250 GBP**:

| Part | Price (GBP) | Description |
| - | - | - |
| 2 x Motors | 120 | Waveshare DDSM115 BLDC servo motors |
| Controller | 20 | Waveshare DDSM Hub Motor Driver Board |
| 2 x Bearings | 8 | 6809ZZ bearings (for the pulleys) |
| M2.5 Screw & Nut Set | 7 | A low-cost set that contains every needed screw and nut (+ spares) |
| \~2M UHMWPE/Dyneema Cord | 10 | The low-friction high-strength cord for the pulleys (1.5~2.0MM Diameter) |
| 12V 3A DC Power Supply | 20 | The power supply for the board and motors (5.5x2.5MM power jack or XT60 connector) |
| **Total** | **185** | |

You'll need to also account for the cost of the printed parts. That will depend largely on your choice of method:

| Method | Price (GBP) | Description |
| - | - | - |
| Self-Print | 5 | Roughly 200g or 75M of filament |
| FDM Service | 30-60 | Assuming an _eBay-tier_ printing provider, not a professional company |
| MJF/SLS Service | 70 | JLC3DP and 3DPrintUK used for reference pricing (nylon dyed black with peening) |

Therefore the _total installed cost_ of this comes to **190 to 255 GBP**

Note that you will need [SimHub Licensed Edition](https://www.simhubdash.com/get-a-license/) (currently 8 EUR or more) to use this device properly.

> **Note:** This entire project is open-source, using the MIT license. In simple terms, this means anyone can use it in any way they like, including for commercial purposes. Rights to trademarks such as the 'GW' logo are reserved.

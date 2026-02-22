# Cost Optimised (\~200 GBP) Active Belt Tensioner
A cost-optimised and simplified active belt tensioner for sim racing. This has been designed for people who do not have a background in software or electronics; it requires no soldering or programming.

![20260222_164041](https://github.com/user-attachments/assets/2179f3e5-fdd0-48b4-a8db-6886b8e6a477)

> **Note:** This entire project is open-source, using the MIT license. In simple terms, this means anyone can use it in any way they like, including for commercial purposes. Attribution is required, and rights to trademarks such as the 'GW' logo are reserved. Both branded and unbranded printable `.step` files are available to download.

## What Is It?

An active belt tensioner is a device that attaches to your sim rig and racing harness. It actively tensions the harness in response to in-simulation telemetry, giving a sense of the forces you'd be experencing in the vehicle when changing speed, braking, cornering and jumping/landing.

This projects consists of two parts:
- [Hardware](#) made up of off-the-shelf components and some easily printed parts
- A [SimHub plugin](#) that controls the hardware and allows customisation of the effects

The goals of this particular design are:
- Low Cost
- Simplicity
- Minimal Parts & Tools
- No Soldering
- No Progamming
- No 'Reclaimed' Motors
- Direct-Drive (FOC/BLDC) Performance

## Who Is It For?

Anyone with a sim rig that wants a more immersive experience. Since it requires no soldering or progamming, virtually anyone can build it.

Note that you'll need either an _aluminium profile_ (e.g. [GT Omega Prime](https://www.gtomega.co.uk/products/prime-cockpit)) or _2" tubular steel_ (e.g. [GT Omega Titan](https://www.gtomega.co.uk/products/titan-cockpit)) rig frame to mount this using the brackets available. Mounting to other types of rig is possible, but you'll need to fabricate your own brackets. _Folding-seat_ rigs (e.g. [Playseat Challenge](https://www.playseat.com/)) are _not suitable_.

## What Does It Cost?

If you have your own 3D printer, _well under_ **200 GBP**. If not, _roughly_ **250 GBP**:

| Price (GBP) | Part | Description |
| - | - | - |
| 120 | 2 x Motors | Waveshare DDSM115 BLDC servo motors |
| 20 | Controller | Waveshare DDSM Hub Motor Driver Board |
| 8 | 2 x Bearings | 6809ZZ bearings (for the pulleys) |
| 7 | M2.5 Screw & Nut Set | A low-cost set that contains every needed fastener (and _many_ spares) |
| 10 | 2 x 1M UHMWPE/Dyneema Cord | The low-friction high-strength cord for the pulleys (1.5~2.0MM Diameter) |
| 20 | 12V 3A DC Power Supply | The power supply for the board and motors (5.5x2.5MM power jack or XT60 connector) |
| **185** | **Total** (excluding printables) |

You'll need to also account for the cost of the printed parts. That will depend largely on your choice of method:

| Price (GBP) | Method | Description |
| - | - | - |
| 5 | Self-Print | Roughly 200g or 75M of filament |
| 30~60 | FDM Service | Assuming an _eBay-tier_ printing provider, not a professional company |
| 60~90 | MJF/SLS Service | [JLC3DP](http://jlc3dp.com/) and [3DPrintUK](http://3dprint-uk.co.uk/) used for reference pricing (nylon dyed black with peening) |

Note that you will need [SimHub Licensed Edition](https://www.simhubdash.com/get-a-license/) (currently **8 EUR** or more) to use this device properly.

## How Does It Perform?

Since we're using high-quality BLDC/FOC integrated servo motors, the effects are applied directly as _force_ (torque) rather than emulated by moving the belts a fixed distance, as with some DIY tensioners based on RC servos.

The benefits of this are:
- Smooth and completely silent operation
- Spring and auto-retraction functionality
- Compact & clean design

The maximum force appliable with these motors and pulley design is estimated to be about **8.5Kgf per belt**; imagine having **8.5Kg of weight** attached to each belt hanging down from the back of your seat.

This is plenty to give extra immersion and feedback, but significantly less than you'd feel in a real vehicle at motorsport velocities.

For comparison, the [QS-BT1](https://qubicsystem.com/product/qs-bt1) claims a _"41 kg sustained pulling force"_, but is a considerably larger and costs six times more.

## Where Can I See It In Action?

(Videos Here)

## Anything Else To Note?

Since seat and harness designs vary massively, I cannot provide _belt clamp_ and _belt roller_ designs for every combination. The included belt clamp is intended for 2" wide belts of up to 2MM thickness.

Rollers aren't strictly needed on most seats, but recommended for the smoothest experience. If not using them, you can place UHMW low-friction adhesive tape over the seat holes to reduce wear and increase smoothness.

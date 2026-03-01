# Simple Active Belt Tensioner
A haptic device for sim racing, designed specifically for people who do not have a background in software or electronics.

It requires **no soldering or programming** and can be built for **as little as ~210 GBP**, using easy-to-obtain components and some printed parts.

![20260222_164041](https://github.com/user-attachments/assets/2179f3e5-fdd0-48b4-a8db-6886b8e6a477)

## What Is It?

An _active belt tensioner_ is a device that attaches to your sim rig and racing harness. It actively tensions the harness in response to game telemetry; giving a sense of the forces you'd be experencing in a vehicle when changing speed, braking, cornering and jumping/landing.

It works with **any game fully supported by [SimHub](https://www.simhubdash.com/)**, but the current software is designed primarily for racing games and simluators.

This project consists of three parts:
- A shopping list of [components](#), readily available from online sellers
- Printable [parts](#) and instructions on how to assemble them
- A _SimHub_ [plugin](#) that controls the hardware and allows customisation of the effects

The printable files and software are **completely free** (except the required [SimHub License](https://www.simhubdash.com/get-a-license/)). The printed part designs are `CERN-OHL-P` licensed (open-source), which essentially means you can do what you like with them, including selling printed/machined parts kits.

### Design Goals
- Low Cost
- Minimal Parts & Tools
- No Soldering
- No Progamming
- No 'Reclaimed' Motors
- Direct-Drive (FOC/BLDC) Performance

## Who Is It For?

Anyone with a sim rig that desires a more immersive experience. It's a plug-and-play design that requires no soldering or progamming, so virtually anyone can build it.

Note that you'll need either an _aluminium profile_ (e.g. [GT Omega Prime](https://www.gtomega.co.uk/products/prime-cockpit)) or _2" tubular steel_ (e.g. [GT Omega Titan](https://www.gtomega.co.uk/products/titan-cockpit)) rig frame to mount this using the [available brackets](#). Mounting to other types of rig is possible, but you'll need to design and fabricate your own brackets. _Folding-seat_ rigs (e.g. [Playseat Challenge](https://www.playseat.com/)) are _not suitable_.

## What Does It Cost?

If you have your own 3D printer, as little as **210 GBP**. If not, **250~300 GBP** depending on your choices:

| Price (GBP) | Part | Description |
| - | - | - |
| 120 | 2 x Motors | Waveshare DDSM115 BLDC servo motors |
| 20 | Controller | Waveshare DDSM Hub Motor Driver Board |
| 8 | 2 x Bearings | 6809ZZ bearings (for the pulleys) |
| 7 | M2.5 Screw & Nut Set | A low-cost set that contains every needed fastener (and _many_ spares) |
| 10 | 2 x 1M UHMWPE/Dyneema Cord | The low-friction high-strength cord for the pulleys (1.5~2.0MM Diameter) |
| 20 | 12V 3A DC Power Supply | The power supply for the board and motors (5.5x2.5MM power jack or XT60 connector) |
| 25 | 5-Point 2" Harness | A low-cost Aliexpress model or used/expired FIA harness |
| **210** | **Total** | Excluding printables |

The cost of the printed parts will depend largely on your choice of priting process and material:

| Price (GBP) | Method | Description |
| - | - | - |
| 5 | Self-Print | Roughly 200g or 75M of filament |
| 30~60 | FDM Service | Assuming an _eBay-tier_ printing provider, not a professional company |
| 60~90 | MJF/SLS Service | [JLC3DP](http://jlc3dp.com/) and [3DPrintUK](http://3dprint-uk.co.uk/) used for reference pricing (nylon dyed black with peening) |

Note that you will need [SimHub Licensed Edition](https://www.simhubdash.com/get-a-license/) (currently **8 EUR** or more) to use this device.

## How Does It Perform?

Since we're using high-quality BLDC/FOC integrated servo motors, the effects are applied directly as _force_ (torque) rather than emulated by moving the belts a fixed distance, as with some DIY tensioners based on RC servos.

The benefits of this are:
- Smooth and silent operation
- Spring and auto-retraction functionality
- Compact & clean design

The maximum force appliable by these motors with the current pulley design is estimated to be about **8.5Kgf per belt**. Imagine having **8.5Kg of weight** attached to each belt hanging down from the back of your seat, and you'll get the idea.

This is plenty to give extra immersion and feedback, but significantly less than you'd feel in a real vehicle at motorsport velocities.

For comparison, the [QS-BT1](https://qubicsystem.com/product/qs-bt1) claims a _"41Kg sustained pulling force"_, but is a considerably larger and costs six times more.

## Where Can I See It In Action?

(Videos Here)

## Anything Else To Note?

Since seat and harness designs vary massively, I cannot provide _belt clamp_ and _belt roller_ designs for every combination. The included belt clamp is intended for 2" wide belts of up to 2MM thickness.

Rollers aren't strictly needed on most seats, but recommended for the smoothest experience. As a simple low-cost solution, you can place UHMW low-friction adhesive tape over the contact points in the seat holes to reduce wear and increase smoothness.

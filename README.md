> 📢 **Important:** This is a _pre-release_ of the project and considered to be under active development and testing. If you're not prepared to do some tinkering and/or deal with initial usability issues, it is recommended that you do not attempt to build it right now and instead wait for the `v1.0` release

![Diagram](https://github.com/user-attachments/assets/943fe759-e230-4e11-926b-9c8e9f3385c2)

# Simple Active Belt Tensioner

A haptic device for sim racing, designed specifically for people who do not have a background in software or electronics.

It requires **no soldering or programming** and can be built for **as little as ~215 GBP** _including the harness_.

<img alt="Belt Clamps" src="https://github.com/user-attachments/assets/431fbec1-53f8-4b44-bab7-e9f08b6276c0" />

## What Is It?

An _active belt tensioner_ is a device that attaches between your sim rig and the anchor points of your racing harness. It dynamically tensions the harness in response to game telemetry; giving a sense of the forces you'd be experencing in a vehicle when changing speed, braking, cornering and jumping/landing.

It works with **any game fully supported by [SimHub](https://www.simhubdash.com/)**, but the current software is designed primarily for racing games and simluators.

This project consists of three parts:
- A shopping list of [components](#what-does-it-cost), readily available from online sellers
- Printable [parts](/Printables/) and instructions on how to assemble them
- A _SimHub_ [plugin](/Plugin/) that controls the hardware and allows customisation of the effects

The printable files and software are **completely free** (except the required [SimHub License](https://www.simhubdash.com/get-a-license/)). The printed part designs are `CERN-OHL-P` licensed (open-source), and the software is `MIT` licensed, which essentially means you can do what you like with either; including selling printed/machined parts kits.

### Design Goals
- Lowest Practical Cost
- Minimal Parts & Tools
- No Soldering
- No Progamming
- No 'Reclaimed' Parts
- Direct-Drive (FOC/BLDC) Performance & Flexibility

## Who Is It For?

Anyone with a sim rig that desires a more immersive experience. It's a plug-and-play design that requires no soldering or progamming, so virtually anyone can build it.

Note that you'll need either an _aluminium profile_ (e.g. [GT Omega Prime](https://www.gtomega.co.uk/products/prime-cockpit)) or _2" tubular steel_ (e.g. [GT Omega Titan](https://www.gtomega.co.uk/products/titan-cockpit)) sim rig frame to mount this using the [available brackets](#). Mounting to other types of rig is possible, but you'll need to design and fabricate your own brackets (or [get in touch](mailto:sabt@georgewilkins.co.uk) with me). _Folding-seat_ rigs (e.g. [Playseat Challenge](https://www.playseat.com/)) are _not suitable_.

I would reccommend installing tactile tranducers (bass shakers) before embarking on belt tensioners and other more exotic haptic systems. Tranducers are by far the simplest and cheapest way of adding real immersion to your experience. They provide detail that belt tensioners cannot (road bumps, curbs, etc), while tensioners provide constant forces that tranduscers cannot (braking, cornering, etc).

## What Does It Cost?

If you have your own 3D printer, as little as **215 GBP** including taxes and delivery. If not, **250~300 GBP** depending on your choice of printed parts supplier.

### Purchased Parts
| Price (GBP) | Part | Notes |
| - | - | - |
| 120 | 2 x Motors | Provides the tensioning force |
| 20 | Controller | Provides USB control of the motors |
| 6 | 2 x Bearings | Allows the pulley covers to rotate smoothly |
| 7 | Screw & Nut Set | Provides every needed fastener in one set (with _many_ spares) |
| 5 | 1M Cord | A low-friction cord that winds around the pulleys (attached at the other end to your belts) |
| 20 | Power Supply | Provides DC power to the motors |
| 12 | Back Driving Protection Unit | Prevents power-supply resets if the motors are back-driven (**optional but recommended**) |
| 25 | 5-Point 2" Harness | A low-cost Aliexpress model or used/expired FIA harness |
| **215** | **Total** | ...excluding printables |

There is a [detailed parts list](/INSTRUCTIONS.md#motors-electronics--fixings) with sources in the build guide.

### Printing Options
| Price (GBP) | Method | Notes |
| - | - | - |
| 5~10 | Self-Print | Roughly 200g or 75M of filament |
| 30~60 | FDM Service | Assuming an _eBay-tier_ printing provider, not a professional company |
| 60~90 | MJF/SLS Service | [JLC3DP](http://jlc3dp.com/) and [3DPrintUK](http://3dprint-uk.co.uk/) used for reference pricing (nylon dyed black with peening) |

### Other Costs
Note that you will also need [SimHub Licensed Edition](https://www.simhubdash.com/get-a-license/) (currently **8 EUR** or more) to use this device. If you're reading this, you almost certainly already have it.

## How Does It Work?

The two motors are anchored to your rig with printed brackets. A self-orienting pulley is attached to the face of each motor.

The ends of each shoulder belt are attached to lengths UHMWPE/Dyneema cord, which are wrapped around each motor pulley.

When SimHub sends game telemetry to our plugin, it converts this into torque commands and send them to the motors over a serial connection.

## How Does It Perform?

Since we're using high-quality BLDC/FOC integrated servo motors, the effects are applied directly as _force_ (torque) rather than emulated by moving the belts a fixed distance, as with some DIY tensioners based on RC servos.

The benefits of this are:
- Smooth and silent operation
- Compact & clean design
- Auto-adjusting of the harness (adjustable idle tension)

The maximum force appliable by these motors with the current pulley design is estimated to be about **8Kgf to 12Kgf per belt**, putting us right around the rated mechanical loading for each motor. Imagine having **~10Kgf of weight** attached to each belt hanging down from the back of your seat, and you'll get the idea.

This is plenty to give extra immersion and feedback, but significantly less than you'd feel in a real vehicle at motorsport velocities.

For comparison, the [QS-BT1](https://qubicsystem.com/product/qs-bt1) claims _"20.5kg per channel"_, but is a considerably larger and costs six times more.

Note that this system conveys progressive constant forces (cornering, braking, etc) rather than fine details (road texture, track kerbs, etc). For the latter, you should install one or more tactile transducers. The two work together to imply the sensation of movement without a costly motion simulator.

## Where Can I See It In Action?

TBC

## Anything I Should Be Aware Of?

There is considerable variation in rig, seat and harness designs, so I cannot anticipate every possible configuration. You'll need to decide if this system is suitable for your rig by looking at the installations examples and instructions.

Of particular note:
- The belt clamp designs are intended for 2" wide belts of up to 2MM thickness. If your belts are wider or thicker than this, you will need to modify the design (or let me know and I'll create additional designs)
- Rollers aren't strictly needed on most seats, but recommended for the smoothest experience. Because of the huge variation in seats, I cannot provide a one-size-fits-all roller design. As a simple low-cost solution, you can place UHMW low-friction adhesive tape over the contact points in the seat holes to reduce wear and increase smoothness
- The system includes a design for a [Back Driving Protection Unit](INSTRUCTIONS.md#back-driving-protection), which prevents your power supply's protection circuitry from tripping when the motors are back-driven (e.g. by pulling fast on the belts). This is an optional component, but highly recommended. A _zero-soldering solution_ is available, but even the soldered version is very simple to assemble
- Although the motors and driver board can tolerate up to `24V` supply, I've found that `15V` is a good compromise. Operating at the maximum `24V` is not reccommended, because any voltage spikes risk damaging the motors. Common `19V` laptop power supplies have been tested and work; but the higher the voltage, the greater the risk of damage

## Is It Safe?

This is an open-source project involving powerful motors, pinch points and moving belts attached to your body.

⚠️ **No claims are made regarding the safety of this device. You are responsible for ensuring its safe use and no liability is accepted by the creator(s) for any damage or injury caused by this device.**

With that said, design measures have been taken with safety in mind. See [SAFETY.md](SAFETY.md) for details.

## How Do I Get Started?

Read through the [instructions](/INSTRUCTIONS.md) to get a better idea of what's involved and how to proceed.

> 📢 **Important:** This is a _pre-release_ of the project and considered to be under active development and testing. If you're not prepared to do some tinkering and/or deal with initial usability issues, it is recommended that you do not attempt to build it right now and instead wait for the `v1.0` release

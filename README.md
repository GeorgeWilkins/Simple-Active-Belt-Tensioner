> 📢 **Important:** This is a _pre-release_ of the project and considered to be under active development and testing. If you're not prepared to do some tinkering and/or deal with initial usability issues, it is recommended that you do not attempt to build it right now and instead wait for the `v1.0` release

![SABT Logo](https://github.com/user-attachments/assets/b6603db9-19e0-4a30-8c0e-a23675a9797a)

![Diagram](https://github.com/user-attachments/assets/405bf2b8-7b57-4307-a4cb-8e6dc5e54704)

# Simple Active Belt Tensioner

A haptic device for sim racing, designed specifically for people who do not have a background in software or electronics.

It requires **no soldering or programming** and can be built for **as little as ~215 GBP** _including the harness_.

It supports **up to four motors** for two or four-channel active belt tensioning.

## What Is It?

[![Introduction Video](https://github.com/user-attachments/assets/536edef6-e577-4989-a92d-927b88d4b97d)](https://youtu.be/b3R1tQvu-o4)

An _active belt tensioner_ is a device that attaches between your sim rig and the anchor points of your racing harness. It dynamically tensions the harness in response to game telemetry; giving a sense of the forces you'd be experencing in a vehicle when changing speed, braking, cornering and jumping/landing.

It works with **any game fully supported by [SimHub](https://www.simhubdash.com/)**, but the current software is designed primarily for racing games and simulators.

This project consists of three parts:
- A shopping list of [components](#what-does-it-cost), readily available from online sellers
- Printable [parts](/Printables/) and instructions on how to assemble them
- A _SimHub_ plugin that controls the hardware and allows customisation of the effects

The printable files and software are **completely free** (except the required [SimHub License](https://www.simhubdash.com/get-a-license/)). The printed part designs are `CERN-OHL-P` licensed (open-source), and the software is `MIT` licensed, which essentially means you can do what you like with either; including selling printed/machined parts kits.

### Design Goals
- Lowest Practical Cost
- Minimal Parts & Tools
- No Soldering
- No Programming
- No 'Reclaimed' Parts
- Direct-Drive (FOC/BLDC) Performance & Flexibility

## Who Is It For?

Anyone with a sim rig that desires a more immersive experience. It's a plug-and-play design that requires no soldering or programming, so virtually anyone can build it.

Thanks to a variety of printable motor bracket designs, it is compatible with most sim rigs, including **aluminium profile** and **tubular steel** designs; even unusual ones like the [Playseat Challenge](https://www.playseat.com/).

I would recommend installing tactile transducers (bass shakers) before embarking on belt tensioners and other more exotic haptic systems. Transducers are by far the simplest and cheapest way of adding real immersion to your experience. They provide detail that belt tensioners cannot (road bumps, curbs, etc), while tensioners provide constant forces that transducers cannot (braking, cornering, etc).

### Community Builds
As of November 2026, at least _fifty_ SABT builds are known to exist; probably many more.

Several members of the sim racing community have already provided photographs of their SABT builds, which I've added to a [shared album](https://photos.app.goo.gl/ju7rso1PFKGxnRhJ8); but we're moving over to a new [Discord Server](https://discord.gg/YwW3TvEJEv), where there is a dedicated channel allowing builders to post their builds directly.

## What Does It Cost?

If you have your own 3D printer, as little as **215 GBP** including taxes and delivery. If not, **250~300 GBP** depending on your choice of printed parts supplier.

Examples given below are for a standard **two-motor** configuration. The great thing about the modular design of **SABT** is you can start off with **two-motors** and later upgrade to **four-motors** without having to replace the parts you've already purchased.

### Purchased Parts
| Price (GBP) | Part | Notes |
| - | - | - |
| 120 | Motors | Provides the tensioning force |
| 20 | Controller | Provides USB control of the motors |
| 6 | Bearings | Allows the pulley covers to rotate smoothly |
| 7 | Screw & Nut Set | Provides every needed fastener in one set (with _many_ spares) |
| 5 | Cord | A low-friction cord that winds around the pulleys (attached at the other end to your belts) |
| 20 | Power Supply | Provides DC power to the motors |
| 12 | Back Driving Protection Unit | Prevents power-supply resets if the motors are back-driven (**optional but recommended**) |
| 25 | 4/5/6-Point 2" Harness | A low-cost Aliexpress model or used/expired FIA harness |
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

The motors are anchored to your rig with printed brackets. A self-orienting pulley is attached to the face of each motor.

The ends of each belt from your harness are attached to lengths UHMWPE/Dyneema cord, which wrap around each motor pulley.

When _SimHub_ sends game telemetry to our plugin, it converts this into torque commands and send them to the motor controller over USB.

## How Does It Perform?

Since we're using high-quality BLDC/FOC integrated servo motors, the effects are applied directly as _force_ (torque) rather than emulated by moving the belts a fixed distance, as with some DIY tensioners based on RC servos.

The benefits of this are:
- Smooth and silent operation
- Compact & clean design
- Auto-adjusting of the harness (adjustable idle tension)

The motors are each capable of exerting about **10Kgf**; roughly equivalent to having **10Kg of weights** attached to each belt, hanging down from the back of your seat.

Should this not be sufficient for your needs, **four motors** can be operated in pairs, such that each belt offers up to **20Kgf**, which is roughly equivalent to the [QS-BT1](https://qubicsystem.com/product/qs-bt1).

You may instead choose to run a true **four-channel** configuration, where your waist belts are actively tensioned in addition to the shoulders, providing an even more immersive experience.

Note that this system conveys progressive constant forces (cornering, braking, etc) better than fine details (road texture, track kerbs, etc). For the latter, you should install one or more tactile transducers. The two systems work together to imply the sensation of movement without a costly motion simulator.

[Force Testing.webm](https://github.com/user-attachments/assets/be7b9d7a-be3f-46f7-abf1-9659297854bb)

## Anything I Should Be Aware Of?

There is considerable variation in rig, seat and harness designs, so I cannot anticipate every possible configuration. You'll need to decide if this system is suitable for your rig by looking at the installations examples and instructions.

Of particular note:
- The belt clamp designs are intended for 2" wide belts of up to 2MM thickness. If your belts are wider or thicker than this, you will need to modify the design (or let me know and I'll create additional designs)
- The need for belt rollers depends on the seat design. In many cases applying low-friction tape is sufficient. A 'universal' [Belt Roller](INSTRUCTIONS.md#belt-rollers) design is included should you wish to use it
- The system includes a design for a [Back Driving Protection Unit](INSTRUCTIONS.md#back-driving-protection), which prevents your power supply's protection circuitry from tripping when the motors are back-driven (e.g. by pulling fast on the belts). This is an optional component, but highly recommended. A _zero-soldering solution_ is available, but even the soldered version is very simple to assemble
- Although the motors and driver board can tolerate up to `24V`, I've found that `15V` works just fine. Common `19V~20V` laptop power supplies are used by many SABT builders without issue; but using anything higher risks damage to the motors under back-driving conditions (pulling aggressively on the belts)

## Is It Safe?

This is an open-source project involving powerful motors, pinch points and moving belts attached to your body.

⚠️ **No claims are made regarding the safety of this device. You are responsible for ensuring its safe use and no liability is accepted by the creator(s) for any damage or injury caused by this device.**

With that said, design measures have been taken with safety in mind. See [SAFETY.md](SAFETY.md) for details.

## How Do I Get Started?

Read through the [instructions](/INSTRUCTIONS.md) to get a better idea of what's involved and how to proceed.

> 📢 **Important:** This is a _pre-release_ of the project and considered to be under active development and testing. If you're not prepared to do some tinkering and/or deal with initial usability issues, it is recommended that you do not attempt to build it right now and instead wait for the `v1.0` release

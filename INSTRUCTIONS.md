# Instructions

Everything you need to know about building a tensioner of your own is in this document. Please make sure you at least skim through each section before proceeding.

## Printed Parts

### Choosing Your Brackets

There are two main types of sim rig frame; _tubular_ and _profile_ (also known as _aluminium extrustion_):
- Tubular frames (e.g. GT Omega Titan) typically use `2"` steel tubing that is mandrel-bent and welded together
- Profile rigs (e.g. GT Omega Prime) typically use `40-Series` aluminium extrusion that is bolted together

Suitable bracket designs for both types are provided below. Designs marked as `Verified` have been printed and successfully used by one or more people.

**You only need to print _one_ of these options**. Remember that you need **two** brackets; one for each motor...

| File | For | Orientation | Holes | Verified |
| - | - | - | - | - |
| [Bracket (20XX Profile, Angle).step](#) | `20-Series` Aluminium Profile | 45° Fixed | `M5` | 🗹 |
| [Bracket (30XX Profile, Angle).step](#) | `30-Series` Aluminium Profile | 45° Fixed | `M8` | ☐ |
| [Bracket (40XX Profile, Angle).step](#) | `40-Series` Aluminium Profile | 45° Fixed | `M8` | ☐ |
| [Bracket (30XX Profile, Flat).step](#) | `30-Series` Aluminium Profile | Perpendicular | `M8` | ☐ |
| [Bracket (40XX Profile, Flat).step](#) | `40-Series` Aluminium Profile | Perpendicular | `M8` | ☐ |
| [Bracket (2" Tube, Truss Clamp).step](#) | `2"` Steel Tube With [Truss Clamp](#) | 360° Variable | | 🗹 |

If none of these are suitable for your rig, [get in touch](#) and we'll see if we can design something.

### Other Printed Parts

Irrespective of your bracket choice above, you'll also need to print **all of the following files**:

| File | Quantity | Descrption |
| - | - | - |
| [Belt Clamp (Front).step](#) | `2` | Front part of the harness/rope clamp |
| [Belt Clamp (Rear).step](#) | `2` | Rear part of the harness/rope clamp |
| [Pulley (Cover).step](#) | `2` | The largest outer part of the pulley |
| [Pulley (Hub).step](#) | `2` | The inner spindle of the pulley |
| [Pulley (Face).step](#) | `2` | The front-facing cover of the pulley |
| [Controller Case (Top).step](#) | `1` | The lid for the controller board box |
| [Controller Case (Base).step](#) | `1` | The base for the controller board box |

### Self Printing

Obviously if you have your own 3D printer, you can print these yourself. These parts have been designed with hobby FDM printers in mind.

I've used [DEEPLE PLA Plus](https://www.amazon.co.uk/dp/B0F66H47J8) on my Bambu H2S for all of my own parts; but virtually _any_ strong PLA/ABS/PETG filament should be fine. Fibre-reinforced filament is okay for the mounting brackets but _not reccommended_ for the pulley parts.

As for printing settings, I would suggest:
| Setting | Value |
| - | - |
| Layer Height | `0.16` |
| Walls | `7` |
| Top Shell Layers | `7` |
| Bottom Shell Layers | `7` |
| Infill Density | `30%` |
| Supports | `Manual` |

Manually add supports to the counterbored bolt & nut holes _only_. They aren't needed anywhere else.

The above takes about **8 hours** on my H2S if printing everything at once. It uses about 75M of filament, or ~230g of PLA. I'd suggest you do the parts in smaller batchces though, to minimise wastage if something goes wrong during the print.

### Third-Party Printing

There are plenty of hobbyists on eBay offering low-cost FDM printing. Quality will depend very much on the individual seller.

If you want a more professional product, there are companies running SLS/MJF machines which produce Nylon parts for reasonable money:
- [3DPrintUK](https://3dprint-uk.co.uk) are a provider of SLS/MJF Nylon parts in the UK. They have an instant quotation system that will give you a price right away (~**90 GBP** at the time of writing)
- [JLC3DP](https://jlc3dp.com) offer a similar service out of China with delivery to most countries

## Motors, Electronics & Fixings

You should be able to order the _Waveshare_ [motors](https://www.waveshare.com/wiki/DDSM115) and [control board](http://www.waveshare.com/wiki/DDSM_Driver_HAT_(A)) from the same supplier, as they are almost always stocked together. They are designed for robotics projects, so those kinds of retailers are your best bet.

The rest of the parts can be obtained from virtually anywhere, including [Amazon](https://www.amazon.co.uk) or [AliExpress](https://www.aliexpress.com). Where I've linked to particular products, these are not endorsements or items I've tested; simply representative examples. Shop around, as prices change constantly...

| Guide Price | Part | Description | Example |
| - | - | - | - |
| `120 GBP` | 2 x Motors | Waveshare DDSM115 BLDC servo motors | [PiHut UK](https://thepihut.com/products/ddsm115-direct-drive-servo-motor) |
| `20 GBP` | Controller | Waveshare DDSM Hub Motor Driver Board | [PiHut UK](https://thepihut.com/products/ddsm-hub-motor-driver-board) |
| `8 GBP` | 2 x Bearings | 6809ZZ bearings (for the pulleys) | [Amazon UK](https://www.amazon.co.uk/dp/B0D4DLV2ND) |
| `7 GBP` | M2.5 Screw & Nut Set | Four M2.5x16 (or x18) + Eight M2.5x10 + Six M2.5x12 + Six M2.5x20 | [Amazon UK](https://www.amazon.co.uk/dp/B0FSWHZPGD) |
| `10 GBP` | 2 x 1M UHMWPE/Dyneema Cord | The low-friction high-strength cord for the pulleys (1.5~2.0MM Diameter) | [Amazon UK](https://www.amazon.co.uk/dp/B0957PH16Q) |
| `20 GBP` | 15V 3A DC Power Supply | The power supply for the board and motors (5.5x2.5MM power jack or XT60 connector) | [Amazon UK](https://www.amazon.co.uk/dp/B09CPFVJVC) |
| `25 GBP` | 5-Point 2" Harness | A low-cost Aliexpress model or used/expired FIA harness | [AliExpress](https://www.aliexpress.com/item/1005008051519590.html) |

### Optional Items

You can reduce friction (and wear) on your seat's belt loops if you apply some low-friction tape over the contact points. I've had success with [2-3/8" PTFE Tape](https://www.amazon.co.uk/dp/B0F3XKJW2V). The best solution would be a roller, but given the variation in seat designs, you're going to have to implement that yourself.

If using the tubular brackets, you'll need to order two [2" Truss Clamps](https://www.amazon.co.uk/dp/B07DP1FK33), a pair of [`M10` Nuts](https://www.amazon.co.uk/dp/B0CGQVMP45) and [`M10x16MM` Low-Profile Bolts](https://www.amazon.co.uk/dp/B0DYHY2DHB).

## Assembly

### Motors

Since the motors are technically _wheels_, they come pre-fitted with rubber treads. We'll be removing these:
1. Unscrew the three M2.5 bolts holding on motor face plate (using the hex wrench included in the screws kit)
2. Apply pulling force towards the front (away from the axle) and gently wiggle off the rubber tread
3. Keep the rubber treads and packaging in case you need to return a motor

### Pulleys

1. Press the bearing into the largest part. This should be possible by hand. The accuracy/roughness of your print will dictate how easy this is:
  - If the fit is too tight, use a hammer to _gently_ tap it in (alternating sides with each tap) or consider shaving away some material with a knife or file
  - If the fit is too loose, cut up a drinks can and shim around the edges of the bearing to fill in the gap (ensuring the shims do no protrude)
2. Thread one end of the cord through one of the pulley cover holes:
  - For the left pulley, use the hole marked with 'L' inside the pulley cover
  - For the right pulley, use the hole marked with 'R' inside the pulley cover
3. Tie a tight knot in the end, then remove any excess cord after the knot
4. Push the knot into the base plate (the one with a large triangular hole), then fit the base plate onto the motor hub, with the knot facing the motor
5. Place the pulley face plate into the bearing hole
6. Install the pulley onto the motor hub
7. Insert and tighten three `M2.5x12MM` screws into the face plate

<img align="left" width="25%" height="auto" src="Assets/Assembly/Pulley%20Parts.png" alt="Pulley Parts" />
<img align="left" width="25%" height="auto" src="Assets/Assembly/Pulley%20Bearing%20Fitting.png" alt="Pulley Bearing Fitting" />
<img align="left" width="25%" height="auto" src="Assets/Assembly/Pulley%20Bearing%20Fitted.png" alt="Pulley Bearing Fitted" />
<img align="left" width="25%" height="auto" src="Assets/Assembly/Pulley%20Face%20Fitted.png" alt="Pulley Face Fitted" />
<img align="left" width="25%" height="auto" src="Assets/Assembly/Pulley%20Hub%20Fitted.png" alt="Pulley Hub Fitted" />
<img align="left" width="25%" height="auto" src="Assets/Assembly/Pulley%20Cord%20Fitting.png" alt="Pulley Cord Fitting" />
<img align="left" width="25%" height="auto" src="Assets/Assembly/Pulley%20Cord%20Fitted.png" alt="Pulley Cord Fitted" />
<br style="clear: both;" />

### Belt Clamps

1. Insert `M2.5` nuts into the hexagonal holes on the underside of the rear clamp part (as deeply as they will go)
2. Tie a tight knot in other end of the cord you previously attached to the _Pulley_ (`50~60CM` is usually enough length)
3. Push the knot into the circular detent in the rear clamp plate, so the cord comes out the bottom
4. Place the end of the harness belt into the rectangular detent of the rear clamp plate (it may overlap the knot; that's fine)
5. Push the front clamp plate down over both, and keep pressure on them while securing with four `M2.5x10` bolts:
  - Tighen each bolt partially in a circular pattern until they are all tight, rather than trying to fully tighten one at a time
  - Once fully tightened, the two clamp parts should be fully touching with ni visible gap between them

### Brackets

1. Feed the motor connectors and wires through the hole in the bracket:
  - Bend back the smaller connector so you're not trying to push both through at once
2. Secure the motor with three `M2.5x20MM` screws
3. Attach the bracket to your rig frame:
  - If using any of the profile bracket designs, secure the bracket to your rig frame with suitable t-nuts and bolts
  - If using the tubular mount (with truss clamp), insert the `M10` nut into the printed backet slot, then secure it to the truss clamp with the `M10x16` bolt

### Control Board

1. Flip the switch on the control board to the `USB` setting
2. Insert `M2.5` nuts into the hexangonal holes on the underside of the lower printed case (as deeply as they will go)
3. Insert the board into the lower half of the printed case (the _Raspberry Pi_ header on the rear may be a tight fit here)
4. Place the upper half of the case on to and secure with four `M2.5x16` bolts

## Adjustment

TBC

## Software

TBC

# Instructions

Everything you need to know about building a tensioner of your own is in this document. Please make sure you at least skim through each section before proceeding.

## Printed Parts

See the [/Printables/](/Printables/) directory to obtain the printable files, and detailed information on printing them.

## Motors, Electronics & Fixings

You should be able to order the _Waveshare_ [motors](https://www.waveshare.com/wiki/DDSM115) and [control board](http://www.waveshare.com/wiki/DDSM_Driver_HAT_(A)) from the same supplier, as they are almost always stocked together. They are designed for robotics projects, so those kinds of retailers are your best bet.

The rest of the parts can be obtained from virtually anywhere, including [Amazon](https://www.amazon.co.uk) or [AliExpress](https://www.aliexpress.com). Where I've linked to particular products, these are not endorsements or items I've tested; simply representative examples. Shop around, as prices change constantly...

| Guide Price | Part | Description | Example |
| - | - | - | - |
| `120 GBP` | 2 x Motors | Waveshare DDSM115 BLDC servo motors | [PiHut UK](https://thepihut.com/products/ddsm115-direct-drive-servo-motor) |
| `20 GBP` | Controller | Waveshare DDSM Hub Motor Driver Board | [PiHut UK](https://thepihut.com/products/ddsm-hub-motor-driver-board) |
| `6 GBP` | 2 x Bearings | `6706` bearings for the pulleys | [Amazon UK](https://www.amazon.co.uk/dp/B0D4DN3RW8) |
| `7 GBP` | M2.5 Screw & Nut Set | Four `M2.5x16` (or `M2.5x18`) + eight `M2.5x10` + six `M2.5x12` + six `M2.5x20` + twelve `M2.5` nuts | [Amazon UK](https://www.amazon.co.uk/dp/B0FSWHZPGD) |
| `5 GBP` | ~1M UHMWPE/Dyneema Cord | The low-friction high-strength cord for the pulleys (1.6~2.0MM Diameter) | (any by-the-metre seller; eBay, etc) |
| `20 GBP` | 15V 90W DC Power Supply | The power supply for the board and motors (standard 5.5x2.5MM centre-positive barrel plug) | [Amazon UK](https://www.amazon.co.uk/dp/B09RHC7QG9) |
| `25 GBP` | 5-Point 2" Harness | A low-cost Aliexpress model or used/expired FIA harness | [AliExpress](https://www.aliexpress.com/item/1005008051519590.html) |

### Optional Items

You can reduce friction (and wear) on your seat's belt loops if you apply some low-friction tape over the contact points. I've had success with [2-3/8" PTFE Tape](https://www.amazon.co.uk/dp/B0F3XKJW2V). The best solution would be a roller, but given the variation in seat designs, you're going to have to implement that yourself.

If using the tubular brackets, you'll need to order two [2" Truss Clamps](https://www.amazon.co.uk/dp/B07DP1FK33), a pair of [`M10` Nuts](https://www.amazon.co.uk/dp/B0CGQVMP45) and [`M10x16MM` Low-Profile Bolts](https://www.amazon.co.uk/dp/B0DYHY2DHB).

Details of optional [Back-Driving Protection](#back-driving-protection) options are listed later in this document.

## Assembly

### Motors

| Step | Instructions | Illustration |
| :-: | :- | :-: |
| 1 | Since the motors are technically _wheels_, they come pre-fitted with rubber treads, which we need to remove.<br /><br />Unscrew the three M2.5 bolts holding on motor face plate and remove it. You may need to remove a 'QA' sticker covering one of the screws.<br /><br />With the face plate removed, push the motor face (with the triangular hub) firmly with your thumbs, while holding the opposing face/rim of the tire with your fingers. Gradually working your way around the rim, moving small amounts at a time works best. | <img alt="Wheel Motor" src="https://github.com/user-attachments/assets/37161acb-f815-408a-b04b-bd79367bf17b" /> |
| 2 | You'll be left with the bare motor. Keep the rubber tire, face plate and bolts safe though; in case you need to return the motors or repurpose them later. | <img alt="Motor Without Tire" src="https://github.com/user-attachments/assets/522b9d83-0a32-4464-8a26-77f0ff1cfd14" /> |

### Pulleys

| Step | Instructions | Illustration |
| :-: | :- | :-: |
| 1 | Assemble the four pulley parts; the bearing, the outer cover, the face plate and hub. | <img alt="Pulley Parts" src="https://github.com/user-attachments/assets/a71e7c59-454f-469d-a038-f88115272059" /> |
| 2 | Press the bearing into the outer cover. This should be possible by hand. The accuracy/roughness of your print will dictate how easy this is.<br /><br />If the fit is too tight, use a hammer to _gently_ tap it in (alternating sides with each tap) or consider shaving away some material with a knife or file.<br /><br />If the fit is too loose, cut up a drinks can and shim around the edges of the bearing to fill in the gap (ensuring the shims do not protrude). | <img alt="Inserting The Bearing" src="https://github.com/user-attachments/assets/ad074009-27f4-4a20-aeaf-5cc2b9e5fe1e" /><img alt="Inserted Bearing" src="https://github.com/user-attachments/assets/36d34db8-b217-4201-b0ea-c0265f49e00e" /> |
| 3 | Insert the pulley face from the front so it sits inside the inner ring of the bearing. The notes above regarding fitting tolerances apply here too | <img alt="Face Inserted" src="https://github.com/user-attachments/assets/d301219f-ad1e-46d6-be3b-2027827be9d9" /> |
| 4 | Insert the pulley hub over the top, so that the triangular shape on the face part pushes into the triangular hole of the hub. They may pop together or be loose depending on the tolerances of your print (either is fine). The slot in the hub for the cord should be facing outward. | <img alt="Hub Fitted" src="https://github.com/user-attachments/assets/74074eec-8118-420c-ae5c-088a4f61ddf7" /> |
| 6 | Cut a `0.5M` length of the _UHMWPE/Dyneema cord_ and tie a tight knot in the end, then remove any excess cord after the knot | <img alt="Knotting The Cord" src="https://github.com/user-attachments/assets/e8b54846-080b-41f7-bd63-63bef5716200" /> |
| 5 | Thread one end of the cord through one of the pulley cover holes. When assembling the _Left_ pulley, use the hole marked with `L` inside the pulley cover (or `R` for the _Right_ pulley). Push the knot into the cut-out in the pulley hub. It should stay in place relatively well. | <img alt="Cord Inserted" src="https://github.com/user-attachments/assets/6f4bf1de-827f-4c2a-84de-9a5fa84f8851" /> |
| 8 | Install the pulley onto the motor hub and insert and tighten three `M2.5x12MM` screws into the face plate. Make sure that the cord stays in the cut-out when doing so. | <img alt="Installing The Pulley" src="https://github.com/user-attachments/assets/ebaff46f-a8b5-4f85-8226-95cd5d5e60f6" /> |

### Belt Clamps

| Step | Instructions | Illustration |
| :-: | ------- | :-: |
| 1 | Gather the belt clamp parts; the four `M2.5x10` bolts, four `M2.5` nuts, and the front and rear plates. | <img alt="Belt Clamp Parts" src="https://github.com/user-attachments/assets/b2617647-5f2d-4c36-b557-64a65785d3fc" /> |
| 1 | Insert `M2.5` nuts into the hexagonal holes on the underside of the rear clamp plate (as deeply as they will go). | <img alt="Nuits Fitted" src="https://github.com/user-attachments/assets/febc264f-4dce-4fbc-a7b0-05f986c8a296" /> |
| 2 | Tie a tight knot in other end of the cord you previously attached to the _Pulley_. Push the knot into the cut-out in the rear clamp plate, so the cord comes out the bottom. | <img alt="Cord Fitted" src="https://github.com/user-attachments/assets/d5fe03f3-3ded-4b5f-b801-8cff1d229c37" /> |
| 3 | Place the end of the harness belt into the rectangular detent of the rear clamp plate (it may overlap the knot; that's fine). Push the front clamp plate firmly down over both. | <img alt="Belt Positioned" src="https://github.com/user-attachments/assets/fc458388-391f-4c7b-82ae-c79fb7a6a81b" /><img alt="Clamp Applied" src="https://github.com/user-attachments/assets/75c67344-5ba3-4967-80b8-f37deb5d0f67" /> |
| 4 | Keep pressure on the assembly while securing with the four `M2.5x10` bolts. Tighen each bolt partially in a circular pattern until they are all tight, rather than trying to fully tighten one at a time. | <img alt="Tightening The Clamp" src="https://github.com/user-attachments/assets/f2bd5545-2124-48f2-95b8-cdd25818f3c8" /> |
| 5 | Once fully tightened, the two clamp parts should be fully touching with no visible gap between them. | <img alt="Clamp Tightened" src="https://github.com/user-attachments/assets/ede38304-9e67-4346-b12a-6ac284669664" /><img alt="Clamp Fully Assembled" src="https://github.com/user-attachments/assets/e4ce1ea0-6511-4f91-b2e1-c62682849e1b" /> |

### Brackets

| Step | Instructions | Illustration |
| :-: | ------- | :-: |
| 1 | Gather the bracket parts; three `M2.5x20` bolts, the bracket and the motor (with or without pulley fitted). Since there are a number of bracket designs, the hardware needed to actually attach them to your rig will vary (but should be obvious if you already have a rig). | <img alt="Bracket Parts" src="https://github.com/user-attachments/assets/ad8dff22-55bb-4067-9ee4-b4830107c591" /> |
| 1 | _Carefully_ feed the motor connectors and wires through the hole in the bracket. Bend back the smaller connector so you're not trying to push both through at once. | <img alt="Folding Back The Connector" src="https://github.com/user-attachments/assets/1b7b2707-8864-4f94-9705-66493df46735" /><img alt="Wires Fed Through" src="https://github.com/user-attachments/assets/e8dfd768-ad01-4a7f-9c6a-ea86f61c2314" /> |
| 3 | Insert the motor stem into the socket on the braket. The shaft and socket are three-sided and symmetrical, so rotate it until it aligns with the socket and push in. As long as it sits fully into the socket, the angle doesn't matter. | <img alt="Inserted The Motor" src="https://github.com/user-attachments/assets/b77ca3aa-80c6-4385-ae0c-92eddb5b90c2" /> |
| 2 | Secure the motor with the three `M2.5x20MM` bolts | <img alt="Motor Bolted In" src="https://github.com/user-attachments/assets/bee30fac-97a7-4d76-a025-d396fefac354" /> |
| 3 | Attach the bracket to your rig frame. If using the _aluminium profile_ bracket designs, secure the bracket to your rig frame with suitable t-nuts and bolts. If using the tubular mount (with truss clamp), insert the `M10` nut into the printed backet slot, then secure it to the truss clamp with the `M10x16` bolt. | <img alt="Assembled Bracket" src="https://github.com/user-attachments/assets/357d287c-e556-41b7-832f-6e56bf0db2f5" /> |

### Controller Board

| Step | Instructions | Illustration |
| :-: | ------- | :-: |
| 1 | Gather the control board parts; the four `M2.5x16` bolts, four `M2.5` nuts, and the upper and lower shells. | <img alt="Controller Board Parts" src="https://github.com/user-attachments/assets/b8023f4c-4e64-4e19-91b4-73b5ddaa23a3" /> |
| 2 | Flip the switch on the control board to the `USB` setting. | <img alt="Switch Set To 'USB'" src="https://github.com/user-attachments/assets/535d4f6b-7255-40b4-9976-2180dd30fd0d" /> |
| 3 | Insert `M2.5` nuts into the hexangonal holes on the underside of the lower printed case (as deeply as they will go). | <img alt="Nuts Inserted" src="https://github.com/user-attachments/assets/a611f28a-1c1b-444c-a974-b4a9d2b50296" /> |
| 4 | Insert the board into the lower half of the printed case. The _Raspberry Pi_ header on the rear may be a tight fit here; push down on the white circles either side of the header to press this in. | <img alt="Board Inserted" src="https://github.com/user-attachments/assets/e3a8817d-76d7-4f4c-a74f-d56d9005a955" /> |
| 5 | Place the upper half of the case on top and secure with four `M2.5x16` bolts. | <img alt="Case Assembled" src="https://github.com/user-attachments/assets/986af12a-4858-4d80-8412-7975f14e810f" /> |

### Back-Driving Protection

There are a few options if you wish to add back-driving protection to your setup. Which you choose will depend on your background:
- [Solderless + Ideal Diode](#solderless--ideal-diode): Using an ideal diode module with pre-soldered terminals
- [Soldered + Ideal Diode](#soldered--ideal-diode): Using an ideal diode module without terminals (requires soldering)
- [Discrete Schottky Diode](#discrete-schottky-diode): Using a discrete Schottky diode (requires _trickier_ soldering)

![Back Driving Protection Options](https://github.com/user-attachments/assets/bac8524d-1063-49b9-a9c6-e798fd1775da)

#### Solderless + Ideal Diode

If you're at all uncomfortable with the idea of soldering or don't have the equipment, you can choose this option. You will need to cut and strip some wires, but no soldering is required.

| Guide Price | Part | Description | Example |
| - | - | - | - |
| `5 GBP` | Ideal Diode Module | A pre-made diode board with soldered screw terminals | [AliExpress](https://www.aliexpress.com/item/1005009419896467.html) |
| `5 GBP` | Capacitor | A `2200uF` `35V` electrolytic capacitor (just one; you'll have spares) | [Amazon UK](https://www.amazon.co.uk/dp/B07K87YFP9) |
| `5 GBP` | DC Cable | A `5.5x2.5mm` DC extension cable (or `XT60` to same) | [Amazon UK](https://www.amazon.co.uk/dp/B004US2X8U) or [Amazon UK](https://www.amazon.co.uk/dp/B0BPKNG672) |

TBC (Awaiting Sample)

#### Soldered + Ideal Diode

The instructions are the same for the [#solderless](solderless option); just with the screw terminals replaced by soldered joints. If you don't need the pre-soldered screw terimnals, these are _many_ 'Ideal Diode' modules available. The one suggested for the solderless option can also be ordered without terminals, but you are not restricted to that model.

The main things you need to ensure of your chosen 'Ideal Diode' module are:
- The module actually blocks reverse current; not all such modules do (for 'OR-ing' purposes)
- The current handling exceeds `6A`
- The voltage handling exceeds `24V` (ideally `30V+`)
- The board has both positive and GND solder pads to make assembly easier

As long as these requirements are met, the module _should_ do the trick. These are some examples:
- [Pololu 5382](https://www.pololu.com/product/5382)
- [Pololu 5383](https://www.pololu.com/product/5383)
- [Pololu 5388](https://www.pololu.com/product/5388)
- [Pololu 5389](https://www.pololu.com/product/5389)
- [Generic LM74700 Module](https://www.amazon.co.uk/dp/B0FNY9MWLX)
- [Generic 'Solar Diode' Module](https://www.amazon.co.uk/dp/B07QGW5J1H)

This approach makes it easier to position the capactior within the footprint of the ideal diode board, rather than hanging out over the cable.

#### Discrete Schottky Diode

If you're a maker and familiar with electronics, a very simple circuit can be made using the same capacitor and any Schottky-like (e.g. SBR, SiC) diode with sufficient ratings. See above for the suggested values.

Strictly speaking this would be a downgrade from the 'Ideal Diode' style boards, but will still do the job just fine. Voltage drop is unlikely to be problematic with that class of diode, and the motors are wide-input.

See the diagrams above for a reference circuit design.

## Adjustment

If you already have a harness, you'll be accustomed to tightening it up with the buckles/straps it has. **You don't need to do that with this tensioner**, since it pre-tensions the harness for you (see `Idle Tension` in the `Tension` tab of the plugin).

However when first setting up the tensioner, you'll need to ensure the following:
- There is enough _plain belt_ (with no buckles or wider sections) to slide through the belt holes on your seat unhindered (allow for at least `20CM` of movement)
- Any excess belt is tucked/buckled away such that it does not catch or rub on anything

If you have excess shoulder belt length, you should run it through the seat holes and then buckle it back upon itself behind the seat (just above where you apply the tensioner's cord clamps).

![Belt Clamps](https://github.com/user-attachments/assets/320dab6c-9a38-4121-a394-edff68d2a8c5)

The method of adjustment may depend on your chosen belt clamp design:
- **End Clamps**: These require the shortening of the harness shoulder belts, which is _strongly discouraged_ until you're 100% happy with your setup. Try using the belt buckles that likely came with your harness to make non-destructive adjustments to length first
- **Through Clamps**: Loosen the clamp bolts and simply slide the clamp up or down the length of the harness shoulder belts, then retighten in the desired location
- **Loop Clamps**: You'll already be using belt buckles to secure the looped harness shoulder belts against themselves, so simply adjust those buckles until you achieve the desired lengths

![Adjustment](https://github.com/user-attachments/assets/36ae4e29-3134-4734-8135-1fb9f7847400)

The ideal total cord length is approximately `50CM`; of which we want about half to be wound around the pulley when the harness is fastened.

If you allow the cord to _fully unwind_, the motors will not be able to apply the correct force. Make sure _at least one turn_ of cord is wrapped around the pulleys. To do this, increase the length of your shoulder harness belt by roughly `10CM`.

If you allow _too much_ cord to wrap around a pulley, it may start binding. If so, reduce the length of your shoulder harness belt. If you cannot do this, reduce the length of the cord; ensuring that there is at least some cord _not_ wrapped around the pulley when the harness is fastened.

[INSERT VIDEO HERE]

## Software

TBC

## First Use

TBC

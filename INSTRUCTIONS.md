# Instructions

Everything you need to know about building a tensioner of your own is in this document. Please make sure you at least skim through each section before proceeding.

## Printed Parts

See the [/Printables/](/Printables/) directory to obtain the printable files, and detailed information on printing them. Make sure you read through this document and note which printable parts you _actually need_; some are optional.

## Ordering Components
### Motors, Electronics & Fixings

You should be able to order the [motors](https://www.waveshare.com/wiki/DDSM115) and [control board](http://www.waveshare.com/wiki/DDSM_Driver_HAT_(A)) from the same supplier, as they are almost always stocked together. They are designed for robotics projects, so those kinds of retailers are your best bet.

The rest of the parts can be obtained from virtually anywhere, including [Amazon](https://www.amazon.co.uk) or [AliExpress](https://www.aliexpress.com). Where I've linked to particular products, these are not endorsements or necessarilyitems I've tested; simply representative examples.

Since availability and prices change constantly, I can't keep buying and verifying each component personally. Take note of the listed specifications and shop around for the best deal, or check our [Discord](https://discord.gg/SVkZv7xQ8b) for advice and recommendations.

Some of these items are only sold in multiples, so our guide prices are calculated by dividing the total price _when bought together_ by the required quantity. They are not available _singularly_ at those prices. In some cases you'll have spares.

| Guide Price (Each) | Quantity | Part | Notes | Example |
| - | - | - | - | - |
| `60 GBP` | `2x`-`4x` | `DDSM115`/`M0601` Motor | These two servo motors are the same OEM model and functionally identical. A basic two-channel shoulder belt configuration requires `2x` motors. Four-channel or tandem setups require `4x` motors | [PiHut UK](https://thepihut.com/products/ddsm115-direct-drive-servo-motor) / [Waveshare](https://www.waveshare.com/ddsm115.htm) / [DFRobot](https://www.dfrobot.com/product-3077.html) |
| `20 GBP` | `1x` | `DDSM Driver HAT (A)` Motor Controller | A motor controller board designed specifically for these motors; up to four at a time. It **must** be the `(A)` model, as there are a few different ones for different motors that are incompatible | [PiHut UK](https://thepihut.com/products/ddsm-hub-motor-driver-board) |
| `2 GBP` | `2x`-`4x` | `6706` Bearing | `30x37x4mm` bearings for the self-orienting pulley covers (in the same quantity as the motors) | [Amazon UK](https://www.amazon.co.uk/dp/B0D4DN3RW8) |
| `7 GBP` | `1x` | `M2.5` Screw & Nut Set | A selection of `M2.5` screws and nuts that we use to fit the various parts together. The kit includes everything you need for any possible motor configuration, including a hex key | [Amazon UK](https://www.amazon.co.uk/dp/B0FSWHZPGD) |
| `5 GBP` | `~5M` | `~1.8MM` UHMWPE/Dyneema Cord | Low-friction high-strength cord for up to four motors and pulley anchors. Avoid _'Paracord'_ style products with outer sheaths; go for _100% braided UHMWPE/Dyneema_ in `1.6mm` to `2.0mm` diameters | eBay, Kite/Sail stores, etc |
| `40 GBP` | `1x` | `20V` `120W` DC Power Supply | The power supply for the board and motors (with a standard `5.5x2.5MM` centre-positive barrel plug). This can handle up to `4x` motor configurations | GST120A20-P1M |
| `25 GBP` | `1x` | 4/5/6-Point `2"` Harness | A low-cost Aliexpress model or used/expired FIA harness | [AliExpress](https://www.aliexpress.com/item/1005008051519590.html) |



> 📢 **Important:**
> - The motors are _sometimes_ sold in **pairs** rather than individually. Make sure you're ordering the correct quantity for your preferred setup
> - There are multiple motor models in the `DDSMXXX` series. Make sure you're buying the [DDSM115](https://www.waveshare.com/wiki/DDSM115)
> - There are multiple motor controllers in the `DDSM` series that are visually similar. Make sure you're buying the [DDSM Driver HAT (A)](https://www.waveshare.com/wiki/DDSM_Driver_HAT_(A))
> - You can use power supplies from `15V` to `20V` as long as they are capable of `6A` continuous output; but exceeding `20V` may damage your motors during back-driving conditions
> - If using the _tubular_ motor brackets, you'll also need [M5x25mm Clamping Knobs](https://www.amazon.co.uk/dp/B0CP2GQLQJ) and [M5 Nuts (8mm Hex)](https://www.amazon.co.uk/dp/B0FMRFPWJ3) to secure them to the tubing

### Belt Rollers

These are considered to be _optional_, but are usually required for seats with angled or irregularly shaped belt guides. They cost little to build, so are _recommended_ if you have any doubts about your seat's belt guide design or want maximum performance.

| Guide Price (Each) | Quantity | Part | Description | Example |
| - | - | - | - | - |
| `0.5 GBP` | `4x` | `605` Bearings | `5x14x5mm` bearings for the rollers | [Amazon UK](https://www.amazon.co.uk/dp/B0CZ8DJ6YM) / [Amazon UK](https://www.amazon.co.uk/dp/B0F6LNSC75) |
| `0.5 GBP` | `8x` | `M5` Countersunk Bolts | These **must** be _fully threaded_. You'll need to measure your belt hole depth (front to back) to determine the required length; add `10mm` to that and round up to the nearest standard bolt length (e.g. my `39mm` deep belt guides need `50mm` bolts) | [Amazon UK](https://www.amazon.co.uk/dp/B0C97LXKBP) |
| `0.5 GBP` | `8x` | `M5` Hex Nuts | Must be `<5mm` thick and `8mm` wide across the flats | [Amazon UK](https://www.amazon.co.uk/dp/B0FMRFPWJ3) |

> 📢 **Important:**
> - Countersunk bolts are typically measured by their _entire length_ including the head
> - The bolts _must_ be fully threaded; otherwise you won't be able to use them for the roller axles
> - If you use bolts that are longer than needed, they may interfere with the belts and rollers

If _not_ using rollers, you can reduce friction (and wear) on your seat's belt loops if you apply some low-friction tape over the contact points. I've had success with [2-3/8" PTFE Tape](https://www.amazon.co.uk/dp/B0F3XKJW2V).

### Back-Driving Protection

You are _strongly encouraged_ to assemble and use this, as without it any rapid pulling on the belts may trigger the OVP/OCP within your power supply (as the motors act as _generators_ in this scenario), disabling tensioner functionality until time passes or a manual power supply reset is performed. There are a number of protection options detailed below. The [printable case](Printables/Back-Driving%20Protection%20Case) is designed to accommodate all of these.

> ⚡ **Warning:** All of these options add a modest capacitance between the power supply and controller board. If you disconnect the protection unit, the capacitor may remain charged at your supply voltage for a period of time. Do not touch or short the terminals at either end of the unit, as doing so **may cause a shock or damage equipment**.

The parts required will depend on your chosen option, but none should exceed `15 GBP` in cost:
- [Solderless + Ideal Diode](#solderless--ideal-diode): Using an ideal diode module with pre-soldered terminals
- [Soldered + Ideal Diode](#soldered--ideal-diode): Using an ideal diode module without terminals (requires soldering)
- [Discrete Schottky Diode](#discrete-schottky-diode): Using a discrete Schottky diode (requires _trickier_ soldering)

![Back Driving Protection Options](https://github.com/user-attachments/assets/a692c416-b0e0-4845-8425-ca056b65f534)

#### Solderless + Ideal Diode

If you're at all uncomfortable with the idea of soldering or don't have the equipment, you can choose this option. You will need to cut and strip some wires, but no soldering is required.

We leverage the availability of premade 'Ideal Diode' modules with easy-to-use screw terminals and cheat a little by adding our capacitor across the output terminal, avoiding the need for any soldering at all.

| Guide Price | Part | Description | Example |
| - | - | - | - |
| `5 GBP` | Ideal Diode Module | A pre-made diode board with soldered screw terminals | [AliExpress](https://www.aliexpress.com/item/1005009419896467.html) |
| `5 GBP` | Capacitor | A `2200uF` `35V` electrolytic capacitor (just one; you'll have spares) | [Amazon UK](https://www.amazon.co.uk/dp/B07K87YFP9) |
| `5 GBP` | DC Cable | A `5.5x2.5mm` DC barrel plug to `XT60` adapter cable (or barrel-to-barrel extension)  | [Amazon UK](https://www.amazon.co.uk/dp/B0BPKNG672) or [Amazon UK](https://www.amazon.co.uk/dp/B0FJ8BKQCF) |

> 📢 **Important:** The printable case for the protection unit is intended for cables with a `5~5.5mm` external diameter, which is fairly typical. Any larger and you'll struggle to fit the cable into the ends of the case. Note also that the screw terminals on the diode board have a maximum gauge of roughly `10 AWG` (or `2.6mm` core diameter); though yours may vary.

The assembly instructions for this solderless option are included below under the [Assembly](#assembly) section.

#### Soldered + Ideal Diode

<img width="350" align="right" alt="Soldered Option" src="https://github.com/user-attachments/assets/8eefe655-81e7-454b-9835-d53c2832d935" />

This is essentially the same as the [Solderless + Ideal Diode](#solderless--ideal-diode) option; just using a premade 'Ideal Diode' module _without_ presoldered screw terimnals and soldering our wires and capacitor directly to the pads on the board.

As soldering jobs go this is very easy, and opens up many more options when it comes to 'Ideal Diode' modules. The one suggested for the solderless option can also be ordered without terminals and is a known good option; but you are not restricted to that specific module.

The main things you need to ensure of your chosen 'Ideal Diode' module are:
- The module actually blocks reverse current; not all such modules do (for 'OR-ing' purposes)
- The current handling exceeds `6A`
- The voltage handling exceeds `24V` (ideally `30V+`)
- The board has both `V+` and `GND` solder pads to make assembly easier (some omit the `GND` pads)

As long as these requirements are met, the module _should_ do the trick. These are some (unverified) examples:
- [Pololu 5382](https://www.pololu.com/product/5382)
- [Pololu 5383](https://www.pololu.com/product/5383)
- [Pololu 5388](https://www.pololu.com/product/5388)
- [Pololu 5389](https://www.pololu.com/product/5389)
- [Generic LM74700 Module](https://www.amazon.co.uk/dp/B0FNY9MWLX)
- [Generic 'Solar Diode' Module](https://www.amazon.co.uk/dp/B07QGW5J1H)

If you try any of these out or find others that work, please let me know and I'll add them to the list.

#### Discrete Schottky Diode

If you're a maker and familiar with electronics, a very simple circuit can be made using the same capacitor and any Schottky-like (e.g. SBR, SiC) diode with sufficient ratings. See above for the suggested values.

Strictly speaking this would be a downgrade from the 'Ideal Diode' style boards, but will still do the job just fine. Voltage drop is unlikely to be problematic with that class of diode, and the motors are wide-input.

See the diagrams above for a reference circuit design.

## Assembly

Depending on your choices above, some of these assembly instructions may not be relevant to you. Please read through them and only follow the steps that are applicable to your build.

The photographs provided are for illustrative purposes and may not show the _exact same_ part designs you are using (either due to updates or various options being available).

### Motors

The motors provide our belt tensioning forces. They receive power and torque commands from the motor controller.

Since the motors are technically _wheels_ intended for robotics projects, they come pre-fitted with rubber treads, which we'll need to remove. Note that the data/power cables on these motors are relatively fragile. Be careful not to damage them during assembly.

| Step | Instructions | Illustration |
| :-: | :- | :-: |
| 1 | Unscrew the three `M2.5` bolts holding on motor face plate and remove it. You may need to remove a 'QA' sticker covering one of the screws. | <img alt="Wheel Motor" src="https://github.com/user-attachments/assets/37161acb-f815-408a-b04b-bd79367bf17b" /> |
| 2 | Push the motor face (with the triangular hub) firmly with your thumbs, while holding the opposing face/rim of the tire with your fingers. Gradually working your way around the rim, moving small amounts at a time works best.<br /><br />If you have difficulty, warming the tires up with a hair dryer or heat gun will soften them and make this easier. | <img alt="Wheel Motor" src="https://github.com/user-attachments/assets/11f57779-0d66-4b99-9151-42d3f9739a8f" /> /> |
| 3 | You'll be left with the bare motor. Keep the rubber tire, face plate and bolts safe though; in case you need to return the motors or repurpose them later. | <img alt="Motor Without Tire" src="https://github.com/user-attachments/assets/522b9d83-0a32-4464-8a26-77f0ff1cfd14" /> |

### Belt Clamps
The belt clamps join our standard harness belts to the cord that will wrap around our pulleys. There are a few different designs you can choose from.

In this guide we'll be using the **End Clamps**; designed to attach to the bare end of 2" webbing. However the assembly steps are essentially the same across all clamp designs.

| Step | Instructions | Illustration |
| :-: | ------- | :-: |
| 1 | Gather the belt clamp parts; the four `M2.5x10` bolts, four `M2.5` nuts, and the front and rear plates.<br /><br />Note that the clamps have three cord positions. If you are using a **single motor** with this belt, use the centre position. If you are using a **pair of motors**, use the outer positions. | <img alt="Belt Clamp Parts" src="https://github.com/user-attachments/assets/35c971b6-75bb-495c-b9a5-f20d2064c719" /> |
| 1 | Insert `M2.5` nuts into the hexagonal holes on the underside of the rear clamp plate (as deeply as they will go). | <img alt="Nuits Fitted" src="https://github.com/user-attachments/assets/8b0e9e06-9d9d-4466-8b34-5d950757cdf7" /> |
| 2 | Tie a tight knot in other end of the cord you previously attached to the _Pulley_. Push the knot into the cut-out in the rear clamp plate, so the cord comes out the bottom.<br /><br />If using paired motors, repeat this for the second cord. | <img alt="Cord Fitted" src="https://github.com/user-attachments/assets/8b5d01b1-9f84-433b-b4ed-aa20c9cd9ab0" /> |
| 3 | Place the end of the harness belt into the rectangular detent of the rear clamp plate (it may overlap the knot; that's fine). Push the front clamp plate firmly down over both. | <img alt="Belt Positioned" src="https://github.com/user-attachments/assets/46525793-602e-4a68-b153-8ac3b75d40fc" /><img alt="Clamp Applied" src="https://github.com/user-attachments/assets/13527a52-6bf2-4621-86c8-55bebbf0f8cc" /> |
| 4 | Keep pressure on the assembly while securing with the four `M2.5x10` bolts. Tighen each bolt partially then move on to the next one, until they are all tight; rather than trying to fully tighten one at a time. | <img alt="Tightening The Clamp" src="https://github.com/user-attachments/assets/5b5760dd-792c-4324-875b-2024a9978939" /> |
| 5 | Once fully tightened, the two clamp parts should be fully touching with no visible gap between them. | <img alt="Clamp Fully Assembled" src="https://github.com/user-attachments/assets/efbe1e7a-ff0c-4ac8-bca8-95196e785525" /> |

### Pulleys
The pulleys turn the motor's torque into linear force by wrapping cords around small hubs. This design has self-orienting pulley covers, which orient themselves to the correct angle for your seat position. This allows for use with adjustable/sliding seats without needing to adjust the tensioning system.

They can be optionally _anchored_ to your frame if you have any issues with the covers spinning or cords tangling (most commonly with **waist belts**, which are not kept taut by the weight of your harness like **shoulder belts** are).

Note that attaching the pulleys to the motors is not recommended until _after_ you have fitted your motors to your rig. However we'll assemble them fully here for the purposes of this guide.

| Step | Instructions | Illustration |
| :-: | :- | :-: |
| 1 | Gather the pulley parts; three `M2.5x12` bolts, the bearing, the outer cover, the face plate and hub. | <img alt="Pulley Parts" src="https://github.com/user-attachments/assets/20ecef0e-e12a-44bf-87e7-396b65f2046a" /> |
| 2 | Press the bearing into the outer cover. This should be possible by hand. The accuracy/roughness of your print will dictate how easy this is.<br /><br />If the fit is too tight, use a hammer to _gently_ tap it in (alternating sides with each tap) or consider shaving away some material with a knife or file.<br /><br />If the fit is too loose, cut up a drinks can and shim around the edges of the bearing to fill in the gap (ensuring the shims do not protrude). | <img alt="Inserting The Bearing" src="https://github.com/user-attachments/assets/5488ba09-e4f6-406a-8622-1a27627ca863" /> |
| 3 | Once installed, the bearing will be flush with the inside face of the pulley. It must be fully inserted and flat, or the pulley won't function correctly. | <img alt="Inserted Bearing" src="https://github.com/user-attachments/assets/fea62189-76d1-4eaa-9b78-85f078385c4e" /> |
| 4 | **Optional:** Insert the _anchoring cord_ into the appropriate hole. This is just a `~0.3M` length of the same cord we use for our belts with a knot at one end.<br /><br />Once the motor and pulley are installed on your rig, you can attach the other end of this cord to the rig frame to anchor the pulley cover and prevent it from spinning freely (should that be an issue for your configuration).<br /><br />Many holes are provided so you can choose an appropriate anchoring angle relative to the belt cord exit angle (the large open slot in the side of the cover). | <img alt="Anchoring Cord" src="https://github.com/user-attachments/assets/1c1159e5-ec3a-496e-aec3-7f930c67eae3" /> |
| 5 | Next we'll attach the pulley and belt cord to the motor. Normally you'd wait until the motor is installed on your rig to do this. | <img alt="Belt Cord" src="https://github.com/user-attachments/assets/3b1b6f60-eb70-425b-9b3b-f2ec0b786ea4" /> |
| 6 | Place the knotted end of your belt cord into the recess inside the pulley hub. | <img alt="Cord Placement" src="https://github.com/user-attachments/assets/d54dfb8f-6569-42ab-8bba-ba0887265b58" /> |
| 7 | Push the hub over the motor's triangular face, keeping the cord end captive. It should press fit tightly and stay in place once fitted. Make sure the cord hasn't slipped out and is held securely by the knotted end. | <img alt="Hub Installed" src="https://github.com/user-attachments/assets/60e4d662-2f5f-40ed-953a-9adb78563de9" /> |
| 8 | Place the pulley cover over the motor face. Your belt cord must exit through the large open slot in the side of the pulley.<br /><br />If you have also installed an anchoring cord, pull it outwards while fitting the pulley cover, so that you don't trap it between the hub and cover. | <img alt="Installing The Pulley" src="https://github.com/user-attachments/assets/95318510-6dc2-45e1-b681-a0433a4c381c" /> |
| 9 | The pulley face plate has a matching triangular shape that pops into the centre of the cover and secures everything in place.<br /><br />Take note of the alignment of the triangular hub when installing it. | <img alt="Inserting The Face Plate" src="https://github.com/user-attachments/assets/36ee9ec7-18fc-4e16-83da-c9c4876d8834" /> |
| 10 | Insert and tighten the three `M2.5x12MM` screws into the face plate.<br /><br />You now have a completely assembled pulley. It should rotate freely without any wobble or rubbing against the motor. | <img alt="Inserting The Face Plate" src="https://github.com/user-attachments/assets/36ee9ec7-18fc-4e16-83da-c9c4876d8834" /> |

### Motor Brackets
Since there are so many different sim rig constructions, there are also various brackets designs for you to pick from. Here we're just going to show how to attach a motor to a bracket, which is the same process for every bracket design.

Where necessary, additional instructions may be included in the same directory as the printable bracket parts; but they are all extremely simple to assemble. Mounting hardware (bolts, t-nuts and washers) for your specific rig are not included in the **SABT** BOM, as it is assumed you'll already have these available.

| Step | Instructions | Illustration |
| :-: | ------- | :-: |
| 1 | Gather the bracket parts; three `M2.5x20` bolts, the bracket and the motor (with or without the pulley fitted). | <img alt="Bracket Parts" src="https://github.com/user-attachments/assets/f872c7e5-08b8-489a-ab4d-dcf05c2846e3" /> |
| 2 | _Carefully_ feed the motor connectors and wires through the hole in the bracket. Bend back the smaller connector so you're not trying to push both through at once. | <img alt="Folding Back The Connector" src="https://github.com/user-attachments/assets/d22e94b7-2e17-416f-8c49-0e0be0b90f23" /><img alt="Wires Fed Through" src="https://github.com/user-attachments/assets/342b0ea6-d163-4b98-be9c-85c0587ea623" /> |
| 3 | Insert the motor stem into the socket on the braket. The shaft and socket are three-sided and symmetrical, so rotate it until it aligns with the socket and push in. As long as it sits fully into the socket, the angle doesn't matter.<br /><br />Secure the motor with the three `M2.5x20MM` bolts; tightening them each bolt a little at a time.  | <img alt="Inserted The Motor" src="https://github.com/user-attachments/assets/17f964a0-787b-46e2-9c83-8de3ae23d5b1" /><img alt="Motor Bolted In" src="https://github.com/user-attachments/assets/1d3dd758-c869-469a-b425-27bfa8791cb9" /> |

### Controller Board
The controller board sends power and commands to the motors. To prevent shorting or damage, we enclose it in a printed case. There are two versions of the case; the only difference being the location of their mounting brackets. Pick whichever suits your mounting position best.

| Step | Instructions | Illustration |
| :-: | ------- | :-: |
| 1 | Gather the control board parts; the four `M2.5x16` bolts, four `M2.5` nuts, and the upper and lower shells. | <img alt="Controller Board Parts" src="https://github.com/user-attachments/assets/b8023f4c-4e64-4e19-91b4-73b5ddaa23a3" /> |
| 2 | Flip the switch on the control board to the `USB` setting. | <img alt="Switch Set To 'USB'" src="https://github.com/user-attachments/assets/535d4f6b-7255-40b4-9976-2180dd30fd0d" /> |
| 3 | Insert `M2.5` nuts into the hexangonal holes on the underside of the lower printed case (as deeply as they will go). | <img alt="Nuts Inserted" src="https://github.com/user-attachments/assets/a611f28a-1c1b-444c-a974-b4a9d2b50296" /> |
| 4 | Insert the board into the lower half of the printed case. The _Raspberry Pi_ header on the rear may be a tight fit here; push down on the white circles either side of the header to press this in. | <img alt="Board Inserted" src="https://github.com/user-attachments/assets/e3a8817d-76d7-4f4c-a74f-d56d9005a955" /> |
| 5 | Place the upper half of the case on top and secure with four `M2.5x16` bolts. | <img alt="Case Assembled" src="https://github.com/user-attachments/assets/986af12a-4858-4d80-8412-7975f14e810f" /> |

### Belt Rollers

| Step | Instructions | Illustration |
| :-: | ------- | :-: |
| 1 | Gather the parts for each belt roller; the five `M5` bolts (length according to your belt guide depth), five `M5` nuts, three spacers, two `605` bearings, bracket, plate and roller. | <img alt="Belt Roller Parts" src="https://github.com/user-attachments/assets/6d63f725-313d-4d9e-b69f-4a526df867df" /> |
| 2 | Press-fit the bearings into the ends of the roller. | <img alt="Fitting The Bearings" src="https://github.com/user-attachments/assets/8131e0b5-44e1-49d6-8490-dbcde63bac78" /> |
| 3 | Insert two `M5` nuts into the inside faces of the bracket ends and thread in the `M5` bolts from the outside; no more than a couple turns (so their ends are flush with the nuts). | <img alt="Inserting Nuts &amp; Bolts" src="https://github.com/user-attachments/assets/30ec19c5-ffd8-4b66-9886-5d0a59cf98fb" /> |
| 4 | Position the roller between the bracket ends and start screwing in the bolts all the way, securing the roller in place. | <img alt="Attaching The Roller" src="https://github.com/user-attachments/assets/338092c1-5afb-42e1-bf11-466ec6de9b88" /> |
| 5 | Take the partially assembled roller and remaining parts to your sim rig. | <img alt="Partially Assembled" src="https://github.com/user-attachments/assets/3ffca7dd-2bd9-46b1-ba57-1ad740ed98a1" /> |
| 6 | Establish where you'd like to mount the roller and which positioning holes you'll use. The roller must be as close to horizontal as possible and nideally cover the entire width of the belt guide hole. Horizontal guides only require two holes/bolts, while angled guides typically require three to prevent sideways movement. | <img alt="Positioning" src="https://github.com/user-attachments/assets/d3137559-1444-4194-bb93-3fb397a6fc0b" /> |
| 7 | On the face plate, find the corresponding holes to the ones you previously chose and insert the `M5` bolts. Place the printed spacers over those. | <img alt="Bolts &amp; Spacers" src="https://github.com/user-attachments/assets/bdc1d8c4-473c-4288-af75-3b34a11f1ddd" /> |
| 8 | Position the face plate inside the belt guide, with the bolts resting on the bottom face of the belt guide hole. At this point you may need to feed through your belt, as there may not be space to do so once the roller is fitted. | <img alt="Face Plate" src="https://github.com/user-attachments/assets/1c5f9365-3fa1-412e-a063-d15ad5c2c189" /> |
| 9 | Line up the bracket with the face plate and bolts, then insert the `M5` nuts and tighten from the other side. The face plate and bracket should _lightly_ clamp to the front and back faces of the seat's belt guide. | <img alt="Bracket Installation" src="https://github.com/user-attachments/assets/cbfdded9-015e-4a1d-b791-eb379cb9d14b" /> |
| 10 | Repeat for the other side and if you haven't already, feed the belts through and check that everything is moving freely. | <img alt="Installed" src="https://github.com/user-attachments/assets/7000174e-5d14-47a2-84d9-4a895e18c4bd" /> |

The choice of which holes to use is up to you and very much specific to your seat design. Below are some placement examples for common seat belt guide shapes. Note that horizontal belt guides can be fitted with only two `M5` positioning bolts, while angled guides usually need three to prevent sideways movement:

![Belt Guide Shapes](https://github.com/user-attachments/assets/94a605c8-a9ac-47c6-b105-aced3dc22706)

### Back-Driving Protection

| Step | Instructions | Illustration |
| :-: | :- | :-: |
| 1 | Assemble the protection unit parts; the four `M2.5x16` bolts, four `M2.5` nuts, the capacitor, the diode board, the adapter cable and printed case. Cut the adapter cable in half as shown. | <img alt="The Protection Unit Parts" src="https://github.com/user-attachments/assets/ddfda194-d2f4-4281-981d-05b19c949105" /> |
| 2 | Bend the capacitor legs as pictured, taking care not to damage the capacitor and noting the oritentation shown. If the legs of the screw terminals protrude significantly from the bottom of the board, take this opportunity to trim them down with some snips. | <img alt="Preparing The Capacitor" src="https://github.com/user-attachments/assets/6e90ebc7-1995-4ca5-8633-d8ee716afb25" /> |
| 3 | Fully loosen the screw terminals. Insert the capacitor legs into the **output** screw terminals as shown (`GND` and `VOUT` on the board). with the capacitor `-` leg going into the board's `GND` as pictured. Do not tighten the terminals yet. | <img alt="Fitting The Capacitor" src="https://github.com/user-attachments/assets/ac39b922-a48f-485c-b380-bf5970740076" /> |
| 4 | Trim off roughly `6mm` of sheathing from the adapter cable ends and twist as shown. | <img width="1554" height="1166" alt="image" src="https://github.com/user-attachments/assets/aae038d5-e829-486f-8191-7227fdbdf8bf" /> |
| 6 | Insert the bare wires into the screw terminals. The `XT60` cable should be on the **output** side and the round DC barrel cable should be on the **input** side. **Black** to `GND` and **red** to `VOUT` and `VIN`. Tighten up the screws as much as they will go. |  <img alt="Inserting The Wires" src="https://github.com/user-attachments/assets/0a9b5a0d-ac72-4451-a24e-41a58c598081" /> |
| 5 | Check for any stray or loose wires and adjust if needed. Install the unit into the printed case, taking care not to disturb the fitted wires. Make sure that the black outer sheath of the cable goes _completely inside_ the case as shown, so that the case lid can clamp down on it once closed. | <img alt="Fitting Into The Case" src="https://github.com/user-attachments/assets/99450677-ceb3-4b04-8392-2bc96aac6a44" /> |
| 8 | Install the `M2.5x16MM` screws and nuts into the case and tighten as much as they will reasonably go. Ideally if you happen to have a multimeter, before attempting to power the controller board, check that the `XT60` connector produces the expected `15V` in the correct polarity (the **flat** side should be `+` and the **beveled** side `-`) when the power supply is connected to the dc barrel plug. |  <img alt="The Completed Protection Unit" src="https://github.com/user-attachments/assets/f5234cca-f23d-4a27-9115-615e1edb59dc" /> |

## Wiring Up

Since the Waveshare board and motors are plug-and play, this is pretty straight forward.

If you're using the [Back-Driving Protection](#back-driving-protection), plug that into the control board (either the XT60 or DC Barrel sockets) and then into the power supply; otherwise just plug the power supply into the board directly.

If this is your first-time setup, your motors will need to be configured, so start with only a single motor plugged into the board. The plugin will tell you when to plug in the second motor.

It doesn't matter which of the exposed connectors on the top of the board you use for each motor. Make sure you've fully seated both the two-pin power plug and four-pin data plug.

Plug your USB type-c cable (one is supplied with the board) into the USB socket closest to the power connectors (marked `DDSM` on the board). If you use the other USB socket, it won't work.

If everything is connected correctly, the board will be detected by the SimHub plugin. No drivers are required.

> ⚠️ **Important:** Do not attempt to power or test the motors while not securely installed on the rig. If they start rotating while loose, they'll move around and potentially destroy themselves, the control board and the surface they are on

## Adjustment

[![Pulley & USB Port Guidance](https://github.com/user-attachments/assets/b133db7d-114e-4c2f-b4da-042420bd6363)](https://www.youtube.com/watch?v=9At6qqblOZY)

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

For reference, a _single turn_ of the pulley is about `~10CM` of cord. This is useful when working out how much cord you have wrapped around the enclosed pulley.

Since our harness is going to be self-adjusting (at least in the shoulders), we should make sure we have `~25CM` of cord between the belt clamps and the pulleys. Obviously we do not want our belt clamps colliding with the pulleys under normal operation.

We also want there to _always_ be at least some cord wrapped around the pulleys (`10~20CM`), as the motors will be unable to apply torque properly if the cord is fully unwound. Do not wrap more than `40CM` or about _four turns_ of cord, as this will start to bind the pulley.

![Cord Wrapping](https://github.com/user-attachments/assets/23a5a874-b905-484b-865d-39467735623d)

It is important that the cord is aligned such that it does not rub against the _sides_ of the pulley housing. The housing is designed to freely rotate with the cord at any angle along the axis of the motor; so if you move your chair backwards or forwards, it doesn't cause a problem. However the motor pulleys must line up perfectly underneath the seat's belt holes to avoid rubbing. In other words, the motor axles must be _perpendicular_ to the cords:

![Pulley Alignment](https://github.com/user-attachments/assets/458a506e-d968-46da-b6e9-95d0174b4486)

_(Instructional Video TBC)_

## Software

See the [/Software/](/Software/) directory for the _SimHub_ plugin download and installation instructions.

## Troubleshooting

See the [FAQ.md](FAQ.md) document for common issues and their solutions. If you have any questions or need further assistance, please reach out via [a new discussion](/discussions/new/choose) or [create a new issue](/issues/new) describing your problem or request.

# Frequently Asked Questions

Naturally potential **SABT** builders have questions about the kit before committing to a build.

Builders also have technical questions about the setup and operation of the kit, or may encounter issues during assembly or use.

- [General Pre-Build Queries](#general-pre-build-queries)
    - [How long does it take to build the kit?](#how-long-does-it-take-to-build-the-kit)
    - [How does it perform compared to other belt tensioners?](#how-does-it-perform-compared-to-other-belt-tensioners)
    - [What is the latency like?](#what-is-the-latency-like)
    - [How long will it last?](#how-long-will-it-last)
    - [Who are you and what is your background?](#who-are-you-and-what-is-your-background)
- [Component Selection](#component-selection)
    - [Can I use a different type of motor?](#can-i-use-a-different-type-of-motor)
    - [Can I use a different control board?](#can-i-use-a-different-control-board)
    - [Can I use higher voltages?](#can-i-use-higher-voltages)
    - [Which power supply should I get?](#which-power-supply-should-i-get)
    - [What type of harness do I need?](#what-type-of-harness-do-i-need)
- [Assembly & Adjustment](#assembly--adjustment)
    - [My pulley covers are not spinning freely](#my-pulley-covers-are-not-spinning-freely)
    - [The cords are tangling up when not under tension](#the-cords-are-tangling-up-when-not-under-tension)
    - [The cords or belts are rubbing against the back of my seat](#the-cords-or-belts-are-rubbing-against-the-back-of-my-seat)
- [Setup & Connectivity](#setup--connectivity)
    - [Why are there two USB ports on the control board?](#why-are-there-two-usb-ports-on-the-control-board)
    - [Which USB port should I use?](#which-usb-port-should-i-use)
    - [Which power connector should I use?](#which-power-connector-should-i-use)
    - [Which position should the switch on the control board be in?](#which-position-should-the-switch-on-the-control-board-be-in)
    - [Which serial/COM port should I select in the SimHub plugin?](#which-serial-or-com-port-should-i-select-in-the-simhub-plugin)
    - [Why are the motors not being detected by the plugin?](#why-are-the-motors-not-being-detected-by-the-plugin)
    - [Can I control the tensioner with buttons?](#can-i-control-the-tensioner-with-buttons)
- [Performance & Effects](#performance--effects)
    - [I'm not getting much force from the belts](#im-not-getting-much-force-from-the-belts)
    - [Force effects seem to be sluggish or muted](#force-effects-seem-to-be-sluggish-or-muted)
    - [Are flight sims supported?](#are-flight-sims-supported)
    - [Why can't I feel engine vibrations or kerb hits?](#why-cant-i-feel-engine-vibrations-or-kerb-hits)

These are common and anticipated questions and their answers. If you have a question that isn't answered here, please reach out via [a new discussion](/discussions/new/choose) or [create a new issue](/issues/new) describing your problem or request.

## General Pre-Build Queries

### How long does it take to build the kit?

Printing the parts can take several hours, depending on your printer and chosen options.

The assembly and setup **shouldn't take more than an afternoon**; though all sim rigs are different and you _may_ run into unique challenges that you'll need to solve.

SABT was designed with non-technical builders in mind, so the parts count was deliberately kept low and the assembly process is straightforward.

The SimHub plugin takes care of all of the software, and is simply copy+pasted into your SimHub installation folder.

### How does it perform compared to other belt tensioners?

**SABT** is a _force-based_ system which directly maps telemetry forces to output, so should provide a more realistic experience than _position/angle-based_ systems.

It is also _direct-drive_, so operates silently and smoothly. This permits it to act with a 'spring' effect and auto-tension the belts, meaning you don't have to pre-tighten your harness.

The maximum force output of **SABT** is 10Kg per motor. This is roughly half that of the [QS BT-1](https://qubicsystem.com/product/qs-bt1); but you can run two motors per belt to achieve equivalent force output if desired.

The vast majority of **SABT** builders have commented that 10Kg is plenty, and prefer to use additional motors to actively tension their waist belts, rather than increasing the force on the shoulder belts.

### What is the latency like?

Testing this accurately requires equipment I do not have; but I _have_ created a simple [demonstration video](https://youtube.com/shorts/pNn5PLNhad8) that uses the audible 'clack' of my shifter paddles to illustrate how quickly the motors respond to the change in acceleration caused by the gear-change.

The video shows this taking _roughly_ two frames at 60fps, which equates to `33ms`.

However that period is measured from the very start of the audible 'clack' waveform, which is some time _before_ the input is sent to the game and the resulting telemetry is made available by SimHub. Therefore the actual latency will be lower than that.

Total latency will vary depending on the game and your particular system.

The minimum achievable latency is constrained by _SimHub's_ telemetry update rate. For the (required) _licensed version_ this is `60Hz` (or `16.6ms`).

### How long will it last?

At the time of writing the project has only been public a few months, so long-term reliability is unknown. The **SABT** SimHub plugin _does not_ track usage, so I have no idea how heavily it is being used by the community.

There are at least **thirty** builds in the wild and I have yet to receive reports of any motor or controller failures. We are theoretically using them within their rated specifications, but probably not in the intended manner.

Motor or controller failures _may_ potentially be covered under your retailer's returns policy or manufacturer's warranty; but I would not rely on that being the case. Obviously as an open-source project, the authors provide absolutely no warranty, guarantees, compensation or responsibility for failed parts.

Assume that you may lose your investment; if that is not acceptable to you, I suggest paying a much higher price up-front for a commercial turnkey product with a full warranty and support.

### Who are you and what is your background?

My name is _George Wilkins_. I'm located in the United Kingdom and have been a professional software developer (web technology) for 20 years. One of my interests is hobby engineering and I'm always working on multiple projects across various disciplines. I recently got back into sim racing (after a couple decades away) and  noticed a few gaps in the peripherals market while building [my own rig](https://photos.app.goo.gl/Egvd6EPmVJsZK7Hn9).

I developed the [BDH Active Shifter](https://bdhracesim.com/uk/products/active/), which started off as a modification of the [BDH H1](https://bdhracesim.com/uk/products/h1/) and turned into a formal collaboration with BDH.

Towards the end of that project I started independently developing **SABT**, with the goal of making _active belt tensioning_ more accessible to the sim racing community.

## Component Selection

### Can I use a different type of motor?

The BOM states the [Waveshare DDSM115](https://www.waveshare.com/wiki/DDSM115) or [DFRobot M0601](https://www.dfrobot.com/product-3077.html) motors should be used. These are both rebranded versions of the same [Direct Drive Tech](https://shop.directdrive.com) motor, which appears to be designated the [M0601C-111](https://shop.directdrive.com/products/m0601c-111-direct-drive-motor).

I developed the **SABT** kit using the `DDSM115`. Other builders have since reported success with the `M0601`. There may be other rebrands of the same motor available, but they have not been tested. If you find one; let me know.

As for other types of motors; the printed parts, electronics and SimHub plugin are designed to work _only_ with the two motors noted above. You would have to modify or replace _all_ of these to make the tensioner work with other motor hardware.

If there is demand, a version of the kit for more powerful motors could be developed; but you'd lose the plug-and-play nature of the current design, requiring a custom PCB to control them.

### Can I use a different control board?

It would be possible to control the motors with a generic RS485-USB serial adapter board. In fact, adapters with the same serial bridge chipset may even be detected by the **SABT** plugin as a valid controller and would theoretically function correctly if you wired up the motors appropriately. However they would not offer the plug-and-play experience **SABT** was designed to provide.

If the project becomes particularly popular there's a good chance I'll look at designing and manufacturing a custom board that integrates everything we need (including the back-driving protection circuitry), but for now the Waveshare board is a good off-the-shelf solution.

### Can I use higher voltages?

Originally the recommended voltage for **SABT** was `15V`, out of concern for the motors being damaged by back-driving voltage spikes. The currently recommended power supply is `20V`, since we've now had enough time and builds to determine that it's a reasonably safe voltage for the motors.

The motors are _"wide-input"_, meaning they will accept a variety of input voltages between `12V` and `24V`. The _"rated voltage"_ is given as `18V` and `5S LiPo`, because as part of a robotics platform, they're intended to be powered by a battery pack. The control board simply passes the supply voltage through to the motors, so the motors will run at whatever voltage you supply to the board.

Since we're running from an SMPS, we have to pick from the commonly available voltages (e.g. `12V`, `15V`, `19V` or `24V`). Higher voltages for these _FOC_ servo motors result in a more energetic response, but not more torque. In practice, SABT builders have also reported higher operating temperatures at higher voltages.

An additional consideration is _back-driving_ of the motors. This is a situation where the motors are turned by the belts (for example, by you moving forward in your seat, or pulling the belts over your shoulder to fit your harness). The motors convert that mechanical movement into electrical energy, which must be dissipated somewhere (manifesting as a voltage spike).

In a battery-driven application, this is not a problem as the battery can absorb the energy (in effect, _regenerative braking_). In our SMPS-driven setup, without any extra protection circuitry this will typically trigger the OVP/RC features of the power supply. The [back-driving protection unit](INSTRUCTIONS.md#back-driving-protection) was devised to prevent this from happening. However it doesn't _stop_ the voltage spike; it simply blocks it from returning to the supply and instead dumps it into a capacitor. Once the voltage spike has been absorbed and the external mechanical input ceased, the motors draw this back down to the supply voltage and continue as normal.

Running your motors at a higher voltage means any relative spike that occurs will also be higher; potentially exceeding the voltage limits of the motors. This is why I would never run my motors at the maximum `24V`.

### Which power supply should I get?

The basic requirements are:
- `5.5x2.5mm` Barrel Plug (or `XT60`)
- `15V` to `20V` Fixed Output
- `1.5A` per motor (plus headroom)

Since the number of motors in a **SABT** configuration is up to you, the total current draw will vary. Regardless, I recommend buying a `≥7A` supply capable of supporting four motors; as you'll then not need to replace it if you decide to increase the motor count later.

However, should you plan on never going beyond two motors, you can opt for lower current supplies.

As a guide, each motor draws `~1.5A` at full stall, so a _quality_ `6A` supply capable of handling spikes is _theoretically_ capable of handling four motors (or `3A` for two motors). However I'd suggest looking for `~1A` of headroom to accommodate spikes and power supply underperformance (particularly with generic/unbranded ones).

The `7A` Meanwell `GST160A20-R7B` is a high-quality supply that has been tested and confirmed to work with four motors, but has an unusual connector that requires an adapter or modification. The smaller `6A` Meanwell `GST120A20-P1M` has the correct barrel plug for our motor controller and is currently being tested under the same load. It will become the 'standard' supply if testing is successful.

All other testing to date has been performed on supplies that far exceed (or at least claim to) the current draw of the motors.

### What type of harness do I need?

Conventional wisdom with _active belt tensioners_ is that a five or six-point harness is required to prevent the belts from pulling _upwards_ rather than _into_ your body, with the 'anti-submarine' belt(s) anchoring the harness into the lap (rather than serving as a safety feature in a real vehicle). This is certainly a more realistic setup for simulating race-specification vehicles, but obviously overkill road vehicles that would be fitted with inertia-reel seatbelts.

A number of builders have reported good performance with four-point configurations; specifically 'backpack' style setups where the shoulder straps are anchored directly to the sides of the seat, with nothing connecting each side and no anti-submarine belt. This has been reported to actually enhance the tensioning effects while reducing the hassle and discomfort of a five or six-point harness.

Regardless of your chosen harness configuration, a `2"` (`50mm`) belt width is recommended; as this should pass through all seat belt guide holes, minimises friction and can be accommodated by the project's included belt roller design.

## Assembly & Adjustment

### My pulley covers are not spinning freely

Most likely the bearings are not seated correctly in the pulley covers, or the end of the cord is not quite inside the slot/hole in the centre of the pulley and it's pushing the cover out of alignment. One way to check is to remove the cords and re-attach the covers to see if they spin freely without the cords in place. If they do, then the cords were being pinched somewhere inside the pulleys.

Reassemble the pulleys and make sure the cords are seated correctly in the pulley slots as pictured in the instructions. If the cord ends have frayed, consider trimming them back.

In rare cases, your printed pulley parts may be warped or undersized; in which case you will likely need to reprint them. I suggest [getting in touch](/discussions/new/choose) before doing so, as there may be a simple fix or adjustment that can be made to avoid reprinting.

### The cords are tangling up when not under tension

The self-orienting pulley covers are intended to prevent tangling from happening; but for them to work effectively the cords need to have at least _some_ tension applied. This usually happens naturally from the weight of your shoulder belts pulling on the cords, but in some configurations (especial four-channel builds with waist belts), this tension isn't present and the covers may simply spin with the motor, which causes tangling.

The solution is to make use of the [anchoring holes](INSTRUCTIONS.md#pulley-anchoring) in the pulley covers, loosely securing the free-spinning cover to your rig.

### The cords or belts are rubbing against the back of my seat

You'll need to print taller motor brackets or add a spacer between your existing brackets and your mounting surface.

Various designs are available in the [Printables](/Printables/Motor%20Brackets) directory.

## Setup & Connectivity

### Why are there two USB ports on the control board?

### Which USB port should I use?

The Waveshare control board has two USB ports, but only one of them is used for the **SABT** kit.

The port we use is essentially a direct connection to the motor drivers via a USB to RS485 adapter that's built into the Waveshare control board. It allows a host PC to send commands directly to the motor drivers.

The other port connects to an ESP32 microcontroller embedded on the control board, which also has access to the motor drivers and can be programmed to control the motors directly. That's what you'd use if working on a robotics project, which is what the control board is primarily designed for.

In our case the SimHub plugin does the telemetry and force calculations needed for our belt tensioning, so we don't need to use the ESP32 at all.

### Which power connector should I use?

There are two power connectors on the controller board; a `5.5x2.5mm` DC barrel jack and an `XT60` socket. They are electrically commoned together, so it makes no electrical difference which one you use.

It is most likely that you have a power supply with a DC barrel plug, so that is what you'll use. The `XT60` connector is a better choice otherwise, as it is less likely to come loose when exposed to vibration and movement. However if you make sure to zip-tie (or otherwise secure) the power cable close to the control board, neither connection should be a problem.

### Which position should the switch on the control board be in?

The small sliding switch near the centre of the control board is used to select whether the motors are controlled by the onboard ESP32 microcontroller or by the host computer (via the USB port). For our purposes, it should be set to the `USB` position; sliding it over to the position closest to the USB and power ports.

### Which serial or COM port should I select in the SimHub plugin?

The plugin will attempt to automatically identify and select your control board's serial port.

However you may find _multiple_ serial/COM ports listed in the SimHub plugin. If so, it means another device is using the same serial bridge chip as our control board. That's not a problem; just unplug the control board briefly to see which port disappears from the list, then plug it back in and select that port in the plugin when it reappears in the list.

### Why are the motors not being detected by the plugin?

Most connectivity problems happen while first setting up the motors. Once they're detected and configured, things should work reliably.

If the motors are not being detected or the guided setup process is failing, check the following:
- Check that you've used the correct USB port on the control board (the one closest to the power inputs)
- Check that the sliding switch on the control board is set to the `USB` position (the position closest to the USB and power ports)
- Check that the control board is powered with 15V~19V (and using the correct polarity)
- If using the back-driving protection unit, check that it is functioning correctly. Try without it to see if it's causing the problem
- Check that the motor connector(s) are properly seated on the controller board
- Try unplugging other USB devices (and disabling associated SimHub plugins) temporarily during the motor setup
- Restart your computer
- Check that _Device Manager_ can see a `USB-Enhanced-SERIAL CH343` device (under `Ports (COM & LPT)`) when the control board is connected

You can verify that the motors are powered by trying to turn them manually (by their shells, not the pulleys or cords). If they are powered, you should feel very noticable resistance. If they turn freely, the motors are not being powered.

### Can I control the tensioner with buttons?

Yes, SimHub has a `Controls & Events` feature that allows the mapping of any input device to any action exposed by a plugin; and the **SABT** plugin exposes various actions:
1. Select `Controls & Events` on the left-hand SimHub menu
2. Enter `sabt` into the `Target` filter to find the available actions
3. Select the desired action
4. Choose your input device and buttons under `Source`
5. Set the `Input Mode` to `ShortPress` or `LongPressNoAutoRepeat` (the latter is recommended if using the `...WithoutWarning` variations of actions)

> ⚠️ **Warning:**
> The `...WithoutWarning` variations of actions explicitly bypass any on-screen warnings related to powering-on the motors, so use with care.

## Performance & Effects

### I'm not getting much force from the belts

### Force effects seem to be sluggish or muted

Have a look at the [adjustment instructions](INSTRUCTIONS.md#adjustment) and then:
- Check that the cords are coming out of the pulleys at a perpendicular angle to the motor axles. They should not be touching the sides of the hole in the pulley (viewed from the side of the motor)
- When your harness is fitted and closed, there should be at least some cord wound around the pulleys. If not, the motors won't be able to apply torque to the belts as effectively as they could. Remember that this system is self-tightening, so you do not need to make your harness tight before closing it; the motors will do that for you
- Check if your cord or belts are snagging on anything; consider adding low-friction tape to your seat's belt holes to reduce belt friction or add the [Belt Rollers](INSTRUCTIONS.md#belt-rollers) for a smoother experience
- Re-tune the telemetry handling and effects settings for the game (and potentially specific vehicle class) in the SimHub plugin

### Are flight sims supported?

Strictly speaking _any_ games fully supported by SimHub are automatically supported by the SABT plugin, as it uses the normalised telemetry data from SimHub's API. However the plugin is primarily designed for sim racing (cars, trucks, etc). The UI and effects sliders are all designed with that in mind and flight-specific effects are not currently implemented.

This may change in a future release; but that will likely be a new major version with a sigificant rework of the plugin's UI and effects handling. If you are interested in this, please [add your voice](https://github.com/GeorgeWilkins/Simple-Active-Belt-Tensioner/issues/38) to the existing issue regarding this functionality. The more interest there is, the higher priority it will be given.

### Why can't I feel engine vibrations or kerb hits?

The tensioner is designed to simulate sustained forces such as braking, acceleration and cornering. It can also simulate large impactful _heave_ events (such as jumps or dips). However it is not intended to simulate high-frequency vibrations or small bumps. That is the job of tactile transducers (or "bass shakers") which are designed specifically to create those effects.

Adjusting the tensioner to express these small/brief impacts effects _is possible_, but it can make other output extremely aggressive and overly sensitive.

There are special effects that can be enabled in the plugin to help simulate or accentuate some of these things, but they will never be as realistic as a dedicated tactile transducer.

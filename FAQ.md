# Frequently Asked Questions

Naturally potential SABT builders have questions about the kit before committing to ordering the parts.

Builders also have technical questions about the setup and operation of the kit, or may encounter issues during assembly or use.

- [General Pre-Build Queries](#general-pre-build-queries)
    - [How long does it take to build the kit?](#how-long-does-it-take-to-build-the-kit)
- [Component Selection](#component-selection)
    - [Can I use a different type of motor?](#can-i-use-a-different-type-of-motor)
    - [Can I use a different control board?](#can-i-use-a-different-control-board)
- [Assembly & Adjustment](#assembly--adjustment)
    - [My pulley covers are not spinning freely](#my-pulley-covers-are-not-spinning-freely)
    - [The cords are tangling up when not under tension](#the-cords-are-tangling-up-when-not-under-tension)
    - [The cords or belts are rubbing against the back of my seat](#the-cords-or-belts-are-rubbing-against-the-back-of-my-seat)
- [Setup & Connectivity](#setup--connectivity)
    - [Why are there two USB ports on the control board?](#why-are-there-two-usb-ports-on-the-control-board)
    - [Which USB port should I use?](#which-usb-port-should-i-use)
    - [Which power connector should I use?](#which-power-connector-should-i-use)
    - [Which position should the switch on the control board be in?](#which-position-should-the-switch-on-the-control-board-be-in)
    - [Which serial/COM port should I select in the SimHub plugin?](#which-serial--com-port-should-i-select-in-the-simhub-plugin)
    - [Why are the motors not being detected by the plugin?](#why-are-the-motors-not-being-detected-by-the-plugin)
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

## Component Selection

### Can I use a different type of motor?

The BOM states the [Waveshare DDSM115](https://www.waveshare.com/wiki/DDSM115) or [DFRobot M0601](https://www.dfrobot.com/product-3077.html) motors should be used. These are both rebranded versions of the same [Direct Drive Tech](https://shop.directdrive.com) motor, which appears to be designated the [M0601C-111](https://shop.directdrive.com/products/m0601c-111-direct-drive-motor).

I developed the SABT kit using the `DDSM115`. Other builders have since reported success with the `M0601`. There may be other rebrands of the same motor available, but they have not been tested. If you find one; let me know.

As for other types of motors; the printed parts, electronics and SimHub plugin are designed to work _only_ with the two motors noted above. You would have to modify or replace _all_ of these to make the tensioner work with other motor hardware.

If there is demand, a version of the kit for more powerful motors could be developed; but you'd lose the plug-and-play nature of the current design, requiring a custom PCB to control them.

### Can I use a different control board?

It would be possible to control the DDSM115 motors with a generic RS485-USB adapter board. In fact, that's one of the approaches I considered when designing the kit. However you'd need two of those adapters to run the motors separately (or add custom electonics to support both motors on a single adapter), and the SimHub plugin would need to be modified to support the different control hardware.

Generally it would be more expensive and complicated than just using the Waveshare control board.

If the project becomes particularly popular there's a good chance I'll look at designing and manufacturing a custom board that integrates everything we need (including the back-driving protection circuitry), but for now the Waveshare board is a good off-the-shelf solution.

## Assembly & Adjustment

### My pulley covers are not spinning freely


Most likely the bearings are not seated correctly in the pulley covers, or the end of the cord is not quite inside the slot/hole in the centre of the pulley and it's pushing the cover out of alignment. One way to check is to remove the cords and re-attach the covers to see if they spin freely without the cords in place. If they do, then the cords were being pinched somewhere inside the pulleys.

Reassemble the pulleys and make sure the cords are seated correctly in the pulley slots as pictured in the instructions. If the cord ends have frayed, consider trimming them back.

In rare cases, your printed pulley parts may be warped or undersized; in which case you will likely need to reprint them. I suggest [getting in touch](/discussions/new/choose) before doing so, as there may be a simple fix or adjustment that can be made to avoid reprinting.

### The cords are tangling up when not under tension

With some UHMWPE/Dyneema cords, it is quite easy to twist them during installation. If done excessively, the cords will try to return to their untwisted state when the tensioner is not under load, which can cause them to tangle up. Remove the cords from the pulleys, straighten them out and re-install them, taking care not to twist them during installation.

### The cords or belts are rubbing against the back of my seat

You'll need to print taller motor brackets or add a spacer between your existing brackets and your mounting surface.

Various designs are available in the [Printables](/Printables/Motor%20Brackets) directory.

## Setup & Connectivity

### Why are there two USB ports on the control board?

### Which USB port should I use?

The Waveshare control board has two USB ports, but only one of them is used for the SABT kit.

The port we use is essentially a direct connection to the motor drivers via a USB to RS485 adapter that's built into the Waveshare control board. It allows a host PC to send commands directly to the motor drivers.

The other port connects to an ESP32 microcontroller embedded on the control board, which also has access to the motor drivers and can be programmed to control the motors directly. That's what you'd use if working on a robotics project, which is what the control board is primarily designed for.

In our case the SimHub plugin does the telemetry and force calculations needed for our belt tensioning, so we don't need to use the ESP32 at all.

### Which power connector should I use?

There are two power connectors on the controller board; a `5.5x2.5mm` DC barrel jack and an `XT60` socket. They are electrically commoned together, so it makes no electrical difference which one you use.

It is most likely that you have a power supply with a DC barrel plug, so that is what you'll use. The `XT60` connector is a better choice otherwise, as it is less likely to come loose when exposed to vibration and movement. However if you make sure to zip-tie (or otherwise secure) the power cable close to the control board, neither connection should be a problem.

### Which position should the switch on the control board be in?

The small sliding switch near the centre of the control board is used to select whether the motors are controlled by the onboard ESP32 microcontroller or by the host computer (via the USB port). For our purposes, it should be set to the `USB` position; sliding it over to the position closest to the USB and power ports.

### Which serial/COM port should I select in the SimHub plugin?

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

Adjusting the tensioner to express these small/brief impacts effects _is possible_, but it makes all other output extremely aggressive and overly sensitive. All braking, acceleration and cornering forces become oversaturated, making for an unpleasant experience.

There is [a feature request](https://github.com/GeorgeWilkins/Simple-Active-Belt-Tensioner/issues/74) being worked on that adds some additional effects (such as engine vibrations) to the plugin; however these are not strictly force-based (instead using RPM telemetry to achieve an _approximation_ of engine vibration) and will not be as realistic as a dedicated tactile transducer.

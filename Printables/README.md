 # Printables

The files in this directory and its sub-directories are what you need to print (or to have printed by a third party) to complete the kit.

Since there are a couple of options to choose from, you should read this before selecting and printing (or ordering) your parts.

### Choosing Your Brackets

These attach your motors to your sim rig, so you'll need one per motor. Follow the [selection and printing instructions](/Motor%20Brackets/README.md) for guidance.

### Choosing Your Belt Clamps
These connect your harness belts to the cord we wrap around our pulleys. Follow the [selection and printing instructions](/Belt%20Clamps/README.md) for guidance.

### Opting For Belt Rollers

There's a lot of variation in seat design. In cases where the belt guides have horizontal bottom surfaces (perpendicular to your pulley cords), you don't _need_ belt rollers; the belts will just slide through the guides without issue. In this case low-friction tape _is_ recommended for a smoother experience and to protect your belt guides.

However with irregular or angled belt guides, you may run into issues with the belts bunching up in the corners of the guides and not sliding smoothly. For such cases, you can print and assemble 'universal' [belt rollers](/Printables/Belt%20Roller/), which have been designed with an array of mounting holes to accommodate most belt guide shapes and angles. These will potentially work with side/waist belt guides as well, should you be building a four-channel configuration.

These will not work well with curved belt guides (where they match the curvature of the seat). In such cases you may need to design your own version of the roller. You can use the [FreeCAD source file](/Sources/Printables/Belt%20Roller.FCStd) as a reference and starting point if so.

## Printables List

Regardless of your choices above, you should end up with:
- 2~4x Belt Clamps _(2 or 3 Parts Each)_
- 1x Controller Case _(2 Parts)_
- 2~4x Motor Brackets _(1 or 2 Parts Each)_
- 2~4x Pulleys _(3 Parts Each)_

If you're making the [Back-Driving Protection Unit](/INSTRUCTIONS.md#back-driving-protection) you'll additionally need:
- 1x Back-Driving Protection Case

If you're making the [Belt Rollers](/Belt%20Rollers) you'll additionally need:
- 2~4x Belt Rollers > **Bracket**
- 2~4x Belt Rollers > **Plate**
- 2~4x Belt Rollers > **Roller**
- 4~8x Belt Rollers > **Spacer** \*

\* These are intended to be vertically scaled in your slicer software to be slightly shorter (by `~0.5mm`) than the depth of your belt guide (usually `12~40mm`). They help to prevent the bracket and plate from bending once clamped and also protect your belt guide plastics from the screw threads. It is recommended that you print the rollers and spacers together (in a vertical orientation) and the bracket and plate together (in a horizontal orientation).

### Self Printing

Obviously if you have your own 3D printer, you can print these yourself. These parts have been designed with hobby FDM printers in mind (not SLA/DLP/MSLA/LCD resin printers).

All parts have been designed to avoid overhangs where possible, making them printable without supports. The only exceptions are parts with counterbored bolt & nut holes, which may need supports depending on how well-tuned your printer is and the selected slicing settings.

The default orientation of the STEP files likely won't be appropriate when imported into your slicer; so use the auto-orient feature or manually rotate the parts to be flat on the print bed. The preferred orientation will be obvious from the geometry of the parts.

#### Filament Choices

For FDM printing, `PETG` or `ABS` are recommended over `PLA` due to their higher heat resistance.

Testing has shown a maximum temperature rise of about `35°C` over ambient for the motors. In other words; if your room temperature is `20°C`, the motors could reach `55°C` _internally_. If run for long enough, the exterior of the motors will eventually reach the same temperature. That would pose a problem for `PLA`, but not for `PETG` or `ABS`.

For many users `PLA` has proven to be adequate, but make sure to use the **SABT** plugin's thermal management features to monitor and limit the temperature of the motors, ensuring that your printed parts are not exposed to excessive heat.

Fibre-reinforced filament is okay for the mounting brackets but _not recommended_ for the pulley parts due to abrasion concerns.

#### Printing Settings

If you're self-printing you probably know what works best for your printer, but here are the settings I use for reference:
| Setting | Value |
| - | - |
| Layer Height | `0.16` |
| Walls | `7` |
| Top Shell Layers | `7` |
| Bottom Shell Layers | `7` |
| Infill Density | `30%` |
| Supports | `Manual` |

Supports are only needed for the counterbored bolt & nut holes. The designs are intentionally without overhangs.

I suggest printing in relatively small batches to minimise wastage if something goes wrong during the print.

#### Asking A Friend

You probably know someone that has a 3D printer and might be considering asking them to print the parts for you.

Please note that printing these parts does take several hours, may consume _up to half_ a standard 1Kg spool of filament (roughly `5~10 GBP` worth), and may not always succeed on the first attempt. It is not a trivial task; especially on low-cost printers, so be considerate of their time and effort before asking.

### Third-Party Printing

There are plenty of hobbyists on eBay offering low-cost FDM printing. Quality will depend very much on the individual seller.

If you want a more commercial-looking end result, there are companies running SLS/MJF machines which produce Nylon parts for reasonable money:
- [3DPrintUK](https://3dprint-uk.co.uk) are a provider of SLS/MJF Nylon parts in the UK. They have an instant quotation system that will give you a price right away (~**90 GBP** at the time of writing)
- [JLC3DP](https://jlc3dp.com) offer a similar service out of China with delivery to most countries

I've had test prints done in MJF Nylon PA12 (dyed black & shot peened) which came out great. A few other builders have similarly had SLS/MJF prints made for their builds with great results. So far the tolerances of the press-fit bearings and other critical features have been fine.

### CNC Machining

At some point I'd love to see machined aluminium parts become available; but the parts included in this repository are very much designed for FDM printing, not CNC subtractive manufacturing.

Some of the parts would need to be adapted considerably to work with the latter process.

If you have the means to do this and want to make them available, please get in touch.

## Downloading

The printable files are provided in the `STEP` format. This provides the highest quality of geometry while being compatible with all major 3D printer slicing sofware _and_ commercial printing and CNC services.

The simplest approach to getting these is to [download the entire repository](https://github.com/GeorgeWilkins/Simple-Active-Belt-Tensioner/archive/refs/heads/main.zip) as a ZIP file, then extract it and navigate to the `/Printables/` folder, where you'll find all the `STEP` files.

If you wish to download a specific file, you should note that _GitHub_ treats `STEP` files as _text_, so displays their contents rather than offering a download. To download a single printable `STEP` file, use the `Download Raw File` option, which looks like a _tray and arrow_ icon:
![Download STEP File](https://github.com/user-attachments/assets/0109cad7-64ec-42a7-b945-4ba9ba94bee7)

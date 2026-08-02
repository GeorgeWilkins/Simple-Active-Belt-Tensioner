 # Printables

The files in this directory and its sub-directories are what you need to print (or to have printed by a third party) to complete the kit.

Since there are a couple of options to choose from, you should read this before selecting and printing (or ordering) your parts.

### Choosing Your Brackets

<img align="right" width="30%" height="auto" style="margin: 0 0 0 5%" alt="Flat Motor Bracket (30~40mm Hole Spacing)" src="https://github.com/user-attachments/assets/121acdba-215d-4968-9570-e6a5bc9e8709" />

There are two main types of sim rig frame:
- **Profile / Extrusion** (e.g. [GT Omega Prime](https://www.gtomega.co.uk/products/prime-cockpit)) typically use `40-Series` aluminium extrusion that is bolted together in a modular fashion
- **Tubular** (e.g. [GT Omega Titan](https://www.gtomega.co.uk/products/titan-cockpit)) typically use `2"` steel tubing that is mandrel-bent into shape and welded together. Lighter foldable frames may use `1"` tubing instead

If you have a **profile frame**, choose from the `For Aluminium Profile...` bracket designs; selecting from `Edge` or `Face` mounting options with the appropriate hole spacing for your particular size of aluminium profile. If you're uncertain, load them up into your preferred slicer and use the measuring tool to check the dimensions.

Most bracket options have `Short` and `Tall` variants to choose from. Use the latter only if you need additional clearance.

If you have a **tubular frame**, choose from the `For {Size} Tube...` bracket designs.

Alternatively there may be a _brand-specific_ bracket design available for your frame, where non-standard fitment is required.

You only need to print _two_ of a single type of bracket; though some designs have multiple parts (the tube clamps in particular). Many bracket designs are symmetrical, _but not all_. Non-symmertical designs require one of the printed brackets to be mirrored in your slicer, so check this before printing.

Here are the various bracket options plated up in a slicer so you can see the intended printing orientations:

![Bracket Designs](https://github.com/user-attachments/assets/7a57f8bd-5f96-40a3-8217-5ff12b6b8618)


### Choosing Your Belt Clamps

Since there are a variety of belt types and terminations, a few options are available:
- [For Belt Ends](/Printables/Belt%20Clamps/For%20Belt%20Ends/): This is designed to clamp the plain end of an unterminated 2" belt (suitable for bare belt ends, or belts you are happy to shorten to the ideal length)
- [For Belt Loops](/Printables/Belt%20Clamps/For%20Belt%20Loops/): This is designed to loop through a 2" belt (up to 4mm thick) and secure back on itself using a standard belt buckle (likely to have come with your hanress)
- [For Belt Passthrough](/Printables/Belt%20Clamps/For%20Belt%20Passthrough/): This is designed to clamp anywhere along a 2" belt without needing to shorten or terminate the belt itself

The [Belt Passthrough](/Printables/Belt%20Clamps/For%20Belt%20Passthrough/) option is probably the easiest to integrate, while the [Belt Ends](/Printables/Belt%20Clamps/For%20Belt%20Ends/) option is the neatest; but needing the belts to be shortened specifically for the tensioner (or shortened non-destructively with buckles).

Here are the various clamp options plated up in a slicer so you can see the intended printing orientations:

![Clamp Designs](https://github.com/user-attachments/assets/9ea6e941-5708-461f-a7fb-e527a2807cee)


### Opting For Belt Rollers

There's a lot of variation in seat design. In cases where the belt guides have horizontal bottom surfaces (perpendicular to your pulley cords), you don't _need_ belt rollers; the belts will just slide through the guides without issue. In this case low-friction tape _is_ recommended for a smoother experience and to protect your belt guides.

However with irregular or angled belt guides, you may run into issues with the belts bunching up in the corners of the guides and not sliding smoothly. For such cases, you can print and assemble two 'universal' [belt rollers](/Printables/Belt%20Roller/), which have been designed with an array of mounting holes to accommodate most belt guide shapes and angles.

These will not work well with curved belt guides (where they match the curvature of the seat). In such cases you may need to design your own version of the roller. You can use the [FreeCAD source file](/Sources/Printables/Belt%20Roller.FCStd) as a reference and starting point if so.

## Printables List

Regardless of your choices above, you should end up with:
- 2x Belt Clamps > For Belt `Ends`|`Loops`|`Passthrough` > **Front**
- 2x Belt Clamps > For Belt `Ends`|`Loops`|`Passthrough` > **Rear**
- 1x Controller Case > **Base** With `Side`|`End` Tabs
- 1x Controller Case > **Top**
- 2x Motor Brackets > _Parts Vary_
- 2x Pulleys > **Cover**
- 2x Pulleys > **Face**
- 2x Pulleys > **Hub**

If you're making the [Back-Driving Protection Unit](/INSTRUCTIONS.md#back-driving-protection) you'll additionally need:
- 1x Back-Driving Protection Case > **Base**
- 1x Back-Driving Protection Case > **Top**

If you're making the [Belt Rollers](/Belt%20Rollers) you'll additionally need:
- 2x Belt Rollers > **Bracket**
- 2x Belt Rollers > **Plate**
- 2x Belt Rollers > **Roller**
- 4~8x Belt Rollers > **Spacer** \*

\* These are intended to be vertically scaled in your slicer software to be slightly shorter (by `~0.5mm`) than the depth of your belt guide (usually `12~40mm`). They help to prevent the bracket and plate from bending once clamped and also protect your belt guide plastics from the screw threads. It is recommended that you print the rollers and spacers together (in a vertical orientation) and the bracket and plate together (in a horizontal orientation).

### Self Printing

Obviously if you have your own 3D printer, you can print these yourself. These parts have been designed with hobby FDM printers in mind (not SLA/DLP/MSLA/LCD resin printers).

All parts have been designed to avoid overhangs where possible, making them printable without supports. The only exceptions are parts with counterbored bolt & nut holes, which may need supports depending on how well-tuned your printer is and the selected slicing settings.

The default orientation of the STEP files likely won't be appropriate when imported into your slicer; so use the auto-orient feature or manually rotate the parts to be flat on the print bed. The preferred orientation will be obvious from the geometry of the parts.

I've used [DEEPLE PLA Plus](https://www.amazon.co.uk/dp/B0F66H47J8) on my Bambu H2S for all of my own prints; but virtually _any_ strong PLA/ABS/PETG filament should be fine. Fibre-reinforced filament is okay for the mounting brackets but _not reccommended_ for the pulley parts due to abrasion concerns.

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

If you want a more commercial-looking end result, there are companies running SLS/MJF machines which produce Nylon parts for reasonable money:
- [3DPrintUK](https://3dprint-uk.co.uk) are a provider of SLS/MJF Nylon parts in the UK. They have an instant quotation system that will give you a price right away (~**90 GBP** at the time of writing)
- [JLC3DP](https://jlc3dp.com) offer a similar service out of China with delivery to most countries

I've had test prints done in MJF Nylon PA12 (dyed black & shot peened) which came out great, but as with FDM printing, getting the tolerances correct for the press-fit bearings requires some trial and error.

![FDM PLA (Left) &amp; MJF Nylon (Right)](https://github.com/user-attachments/assets/bfac6e96-2e1a-40e1-896b-42d3a9c5b132)
![FDM PLA (Left) &amp; MJF Nylon (Right)](https://github.com/user-attachments/assets/4709e92b-cbfa-41a9-ae36-7251c432661b)

### CNC Machining

At some point I'd love to see machined aluminium parts become available; but the parts included in this repository are very much designed for FDM printing, not CNC subtractive manufacturing.

Some of the parts would need to be adapted considerably to work with the latter process.

If you have the means to do this and want to make them available, please get in touch.

## Downloading

The printable files are provided in the `STEP` format. This provides the highest quality of geometry while being compatible with all major 3D printer slicing sofware _and_ commercial printing and CNC services.

GitHub treats these as _text_ files, so displays their contents rather than offering a download. To download a printable `STEP` file, use the `Download Raw File` option, which looks like a _tray and arrow_ icon:
![Download STEP File](https://github.com/user-attachments/assets/0109cad7-64ec-42a7-b945-4ba9ba94bee7)


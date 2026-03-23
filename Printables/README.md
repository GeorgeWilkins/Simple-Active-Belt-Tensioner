# Printables

The files in this directory are what you need to print (or to have printed by a third party) to complete the kit.

Since there are a couple of options to choose from, you should read this before selecting and printing (or ordering) your parts.

### Branded VS Unbranded

The first choice you'll need to make is whether to use the [Branded](/Printables/Branded) or [Unbranded](/Printables/Unbranded) parts. The former have the [GW](https://georgewilkins.co.uk/) logo in various places. If you don't want that on your parts (or are **intending to sell pre-printed parts**), use the latter. You only need to print one set.

### Choosing Your Brackets

<img align="right" width="30%" height="auto" style="margin: 0 0 5% 5%" alt="Printed Bracket Example" src="https://github.com/user-attachments/assets/02c48ec1-51da-48e7-ac1e-b915f9ce38fb" />

There are two main types of sim rig frame; _tubular_ and _profile_ (also known as _aluminium extrustion_):
- **Profile rigs** (e.g. [GT Omega Prime](https://www.gtomega.co.uk/products/prime-cockpit)) typically use `40-Series` aluminium extrusion that is bolted together in a modular fashion
- **Tubular frames** (e.g. [GT Omega Titan](https://www.gtomega.co.uk/products/titan-cockpit)) typically use `2"` steel tubing that is mandrel-bent into shape and welded together

If you have a _profile rig_, choose the bracket file labelled as _'Profile'_ in the appropriate size.

You only need to print _two_ of a single type of bracket. The designs are all symetrical.

There may be multiple bracket styles for your rig type. Consider which one will fit your rig best before printing.

### Choosing Your Pulley Size

At the moment there are two pulley options; one for `6706` breaings (`30x37x4MM`) and another for `6809` (`45x58x7MM`) bearings. They have the same basic design, but have different hub sizes. This affects the torque (force) and speed (reactivness) of the tensioner effects.

I'm currently trialling these with a couple early-adpoters to see which one performs the best.

Each pulley has three parts; the outer cover, the face and the hub. You'll need to print two of each part in your chosen size.

## Printables List

Regardless of your choices above, you should end up with:
- 2x Belt Clamp Front
- 2x Belt Clamp Rear
- 1x Controller Case Top
- 1x Controller Case Base
- 2x Motor Bracket (whichever is suitable for your rig)
- 2x Pulley Cover (`6706` for **torque** or `6809` for **speed**)
- 2x Pulley Face (`6706` for **torque** or `6809` for **speed**)
- 2x Pulley Hub (`6706` for **torque** or `6809` for **speed**)

<img alt="Printable Parts" src="https://github.com/user-attachments/assets/0f1a7170-193e-4ecd-94d9-9bf6c1afe0a8" />

### Self Printing

Obviously if you have your own 3D printer, you can print these yourself. These parts have been designed with hobby FDM printers in mind (not SLA/DLP/MSLA/LCD resin printers).

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

### CNC Machining

At some point I'd love to see machined aluminium parts become available; but the parts included in this repository are very much designed for FDM printing, not CNC subtractive manufacturing.

Some of the parts would need to be adapted considerably to work with the latter process.

If you have the means to do this and want to make them available, please [get in touch](sabt@georgewilkins.co.uk).

# Printables

The files in this directory and its sub-directories are what you need to print (or to have printed by a third party) to complete the kit.

Since there are a couple of options to choose from, you should read this before selecting and printing (or ordering) your parts.

### Choosing Your Brackets

<img align="right" width="30%" height="auto" style="margin: 0 0 5% 5%" alt="Printed Bracket Example" src="https://github.com/user-attachments/assets/02c48ec1-51da-48e7-ac1e-b915f9ce38fb" />

There are two main types of sim rig frame; _tubular_ and _profile_ (also known as _aluminium extrustion_):
- **Profile rigs** (e.g. [GT Omega Prime](https://www.gtomega.co.uk/products/prime-cockpit)) typically use `40-Series` aluminium extrusion that is bolted together in a modular fashion
- **Tubular frames** (e.g. [GT Omega Titan](https://www.gtomega.co.uk/products/titan-cockpit)) typically use `2"` steel tubing that is mandrel-bent into shape and welded together

If you have a _profile rig_, choose from the `Corner` or `Flat` profile bracket designs. They support both `30-series` and `40-series` profile.

If you have a _tubular frame_, choose the `Tube` clamp bracket design (which integrates with a commercial [truss clamp](https://www.amazon.co.uk/dp/B07DP1FK33) to attach to the tube).

You only need to print _two_ of a single type of bracket. The designs are all symetrical.

### Choosing Your Belt Clamps

Since there are a variety of belt types and terminations, a few options are available:
- **End Clamp**: This is designed to clamp the plain end of an unterminated 2" belt (suitable for bare belt ends, or belts you are happy to shorten to the ideal length)
- **Loop Clamp**: This is designed to loop through a 2" belt (up to 4mm thick) and secure back on itself using a standard belt buckle (likely to have come with your hanress)
- **Through Clamp**: This is designed to clamp anywhere along a 2" belt without needing to shorten or terminate the belt itself

The _through clamp_ is probably the easiest to integrate, with the _end clamp_ being the neatest; but needing the belts to be shortened specifically for the tensioner (or shortened non-destructively with buckles).

## Printables List

Regardless of your choices above, you should end up with:
- 2x Belt `End`|`Loop`|`Through` Clamp (Front)
- 2x Belt `End`|`Loop`|`Through` Clamp (Rear)
- 1x Controller Case (Top)
- 1x Controller Case (Base)
- 2x `Corner`|`Flat`|`Tube` Motor Bracket
- 2x Bearing Pulley (Cover)
- 2x Bearing Pulley (Face)
- 2x Bearing Pulley (Hub)

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

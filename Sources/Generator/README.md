# Generator

This pair of _Python_ scripts automate the process of generating printable `.STEP` files from the original [FreeCAD](https://www.freecad.org/) design files.

These can be run on a Linux server to act as a back-end for a web-based generator, which is exactly what I've made accessible under [https://georgewilkins.github.io/Simple-Active-Belt-Tensioner/](https://georgewilkins.github.io/Simple-Active-Belt-Tensioner/)).

The included `Dockerfile` allows this to be run in a containerised environment, including many Docker-based cloud hosting providers.

- `main.py` is the primary script for handling requests, input/output and responses
- `freecad.py` is passed to `freecadcmd` to instruct it to apply the given variables and generate the `.STEP` file
- `www/...` contains a simple web form for submitted requests to the generator

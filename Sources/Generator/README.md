# Printables Generator
While the repository includes a number of pre-designed printable files for various configurations, they won't be appropriate for every scenario. The [Printables Generator](https://georgewilkins.github.io/Simple-Active-Belt-Tensioner/) is a web-based tool that allows builders to configure some of the printable parts without needing to learn FreeCAD.

The files within this directory are only relevant if you'd like to customise the tool itself. They are not needed to use the generator; unless you wish to host it yourself.

If you'd prefer to customise the parametric `.FCStd` source files manually, these are available [here](/Sources/Printables/), but require FreeCAD to be installed on your machine and knowledge of how to use it.

## Summary
Surprisingly there are only a few components to this tool:
- `main.py`: Defines the configuration variables, handles web requests and performs basic IO and validation
- `freecad.py`: A script passed to FreeCAD to actually generate the printable `.STEP` files from the parametric `.FCStd` source files
- `www/`: Contains the web form and imagery it displays

If you wish to host the generator yourself, you will need to install FreeCAD and Python on your server. When running the generator in a containerised environment (e.g. Docker, Google Cloud Run, Jelastic, etc), you can use the provided `Dockerfile` to build an image that contains all the necessary software.

The public [Printables Generator](https://georgewilkins.github.io/Simple-Active-Belt-Tensioner/) is hosted on _GitHub Pages_; but the Python scripts and FreeCAD are run on a _Google Cloud Run_ instance, which the web form submits requests to.

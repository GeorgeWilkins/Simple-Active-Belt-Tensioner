# Contributing

Thank you for wanting to contribute to this project.

In the interest of keeping things tidy and minimising unecessary or wasted work for all involved, there are a few guidelines we should follow when making contributions:

## Creating Issues
- Use concise yet descriptive issue names (in _Title Case_)
    - For example: `"Improve Motor Initialization Failure Handling"` rather than `"Bug Fixes"`
- Keep the scope as tight as possible; one _Issue_ per feature, enhancement or bug fix
- Include a description of the issue to be resolved:
    - For _Bugs_, describe the problem and include a method of replicating it (if known)
    - For _Features_ and _Enhancements_, describe the intended behaviour (and appearance where applicable)
- Apply appropriate _Labels_ from the right-hand sidebar

### Issue Assignment
If you intend to address the _Issue_ yourself with a follow-up _Pull Request_, please **assign yourself** to the issue from the right-hand sidebar. This informs others that you working on it and avoids duplicated work.

Check the existing assignments before working on an issue; as there may already be people actioning it.

## Creating Pull Requests
- Please make sure your _Pull Request_ is explicitly related to a single _Issue_
    - Name it with the numerical issue number: `Issue 123`
    - Add a link to the issue at the top of the description: `#123`
- Make sure your code builds and functions properly _before_ submitting a _Pull Request_
- If you'd like to share in-progress work, create it as _Draft Pull Request_
- Do not add superfluous files unrelated to the _Issue_ being addressed (including OS-specific metadata and temporary files)

## Versioning & Builds
I'm following [SEMVER](https://semver.org/) rationale for versioning of the _SimHub Plugin_. The version number is only incremented (and [released](https://github.com/GeorgeWilkins/Simple-Active-Belt-Tensioner/releases)) where the plugin has been changed. Other contributions, such as changes to the FreeCAD/STEP printable files, do not require this and are silently merged into `main`.

While you're encouraged to test your code contributions by compiling the plugin and physically testing your changes on a sim rig, this is not strictly necessary. I always test and rebuild such contributions myself before merging anything.

When doing so I will also set appropriate version numbers, so you don't need to worry about this.

## LLM/AI/Agent Generated Content
Using the assistance of coding support agents (e.g. CoPilot) is absoutely fine. However submissions that are _entirely_ or _mostly_ agent-generated will not be accepted.

These usually create far more work for me than simply writing the code myself.

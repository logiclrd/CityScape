# CityScape

Generates a random cityscape. The parameters are configured to make it look similar to the static image in the KDE `city-pixels-loading` splash screen.

Output:

* `output.png`: The entire cityscape, which is twice the width of a standard FHD screen (3840 pixels) and which wraps seamlessly across the horizontal edges.
* `Frames/Frame%04d.png`: A seamlessly loopable animation of the cityscape scrolling to the left. Each frame is a 1920 pixel crop of the cityscape.
* `cityscape.gif`: (Produced by the `./encode_gif` script using FFMPEG) An animated GIF of the frames in `Frames`.
* `cityscape.mng`: (Produced by the `./encode_mng` script using ImageMagick) An animated MNG of the frames in `Frames`. An MNG file is an animation whose frames are compressed with PNG. It is not widely-supported, but Qt and thus KDE which is based on Qt support it, so it can be used in a KDE Plasma Splashscreen, and is a bit smaller than the GIF version. :-)

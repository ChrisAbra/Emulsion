---
title: "Darkroom emulation: towards a more complete a mathematical model."
date: \today
author: Christopher Abraham
bibliography: "bibliography.bib"
colorlinks: true
mainfont: DejaVuSerif.ttf
sansfont: DejaVuSans.ttf
monofont: DejaVuSansMono.ttf 
mathfont: texgyredejavu-math.otf 
geometry: margin=3cm
fontsize: 12pt
---

Representing the outputs of analogue photography in a digital medium requires careful management of light and colour. A number of software exist to produce useable images with scans from consumer colour digital cameras^[At time of writing, *Negative Lab Pro* an Adobe Lightroom plugin and Darktable's *negadoctor* module are the most common and accessible, but almost all rely on a single image from a Colour-Filter Array camera.], but they tend to struggle with variances between image illuminant and camera specific sensitivities. Their approach is to assume the illuminant is the same as the one the camera is calibrated for and mapped via its camera profile. The process then is to try and invert the image along with specifically tuned transfer functions to recreate the look of RA-4 paper. Some of these software attempt image specific analysis to try and produce automatic colour balance similar to a lab technician's adjustments with older, specialty film-scanners. 

Similarly, many digital effects and image processing systems evolved from emulating darkroom processes^[cf. Dodging, burning, cropping, unsharp masks, etc.], but more often than not these occur *after* the inversion emulation and thus don't provide the same results as they would in a traditional darkroom. We can however more properly (and consistently) emulate the results of negative film printing by starting at first-principles and attempting to remove as many assumptions and variability as we can.

While determining an "accurate" digital representation of an image stored on developed print film has myriad factors and is subject to interpretation and artistry, there are a number of real and modellable parts of the process. Ultimately the question consists of three steps:

1. How is light affected by passing through developed negative print film and how can we measure this?
2. How does photographic paper respond to that light to produce an image after development?
3. How does the eye perceive that image, and how can we represent that digitally?

After this we can combine these to form a model of how an image can be represented digitally, which parts of the process are variables we can control for artistic input, and how we can compute this efficiently.

How can we measure the effects of film on light?
=

Colour in digital formats is often expressed in ratios of Red, Green and Blue. These however are emergent phenomena of the way our eyes and brains perceive colour and are a lossy, problematic way of modelling light transport ^[@ComputerColorIsBroken] ^[@CIE]. For calculations that better model the physics, it is better for us to consider a spectral power distribution (SPD) of light and how this is attenuated by transmissions and reflections ^[We will consider these mathematically identical. Transmission being the absorption of light through a medium and reflectance being the ratio of different wavelengths reflected, both of which "impart" an imbalance we perceive as colour.]

A spectral power distribution is the distribution of energy across a spectrum of wavelengths/frequencies. The area under this graph/its integral is the total energy but colour filters selectively attenuate different frequencies of light more than others and so it's the most useful mathematical object for transmission and reflectance calculations. 

![SPD Curve examples ^[@SPD_Example]](images/SPD_CurveExamples.png "SPD Examples")

Developed colour print film is a stack of 3 colour filter dyes suspended in a gelatine matrix. As light passes through the Cyan, Magenta and Yellow colour filters (and the dyes of the film base), it is attenuated by their characteristic filtering curve proportionally to the densities of each dye.

![Cross-section of film ^[@X400DataSheet]](images/filmdye%20stack.png "Film Stack")

The intensity of light though a single point/"pixel", of a specific wavelength on RA-4 paper is then found through:

$$ S_λ = I_λ⋅A^c_λ ⋅A^m_λ⋅A^y_λ⋅A^b_λ $$

where 
$S$ is a sample of transmittance energy, 
$I$ is the illuminant energy, 
$A$ is the attenuation of the light provided by each sequential dye filter layer at this wavelength from $0$ to $1$ where $0$ is total attenuation and $1$ is total transmission.

The attenuation ($A_λ$) of a dye layer is proportional to the height of the layer, the density of the dye, and the filtering characteristics of the dye at that^[This is more accurately expanded by the Beer–Lambert law]. As the incident light which formed our image only affects one of those, the density, the variation between samples is also proportional to density. 

To fully measure the exact SPD of light which would emerge after transmitting through print film, we would have to measure this attenuation across the range of wavelengths we're interested in ^[usually 400-800nm for human vision]. Unfortunately this is not feasible for hobby or even professional solutions at high-resolutions.

If instead however, we could seek to measure the relative densities of each dye layer, given a known set of characteristic filtering curves for each dye^[a trick/efficiency will also allow us to not necessarily have to find or model these particularly precisely which will be dealt with later on.], we could reconstruct this SPD mathematically, and even change the illuminant and produce different, equally valid results. 



How does photographic paper respond to light?
=

How do we perceive an image?
=

When we perceive colours in a printed ^[In this article I'll be referring largely to RA-4 paper but some elements may apply to other types of photographic paper.] image, we are perceiving:

1. the light illuminating the scene -  $I$
2. the amount of that light absorbed by the paper and dyes $R$
3. the relative excitement of the cones in our eyes and the rest of human colour perception. 

Thankfully, the CIE colour matching experiments ^[@CIE] provide a model for the relative excitement of our cones in the $\bar{x},\bar{y},\bar{z}$ colour matching functions that allow us to go from spectral data to the XYZ colour space. This is the colour space which underpins most other digital colour spaces so if we can define our image in coordinates here, we can transform it reliably to our screens.

Similarly, there are various standard illuminants such as D65 or A which have known SPDs or calculable outputs^[Illuminant D65 being the usual illuminant digital cameras are calibrated against of afternoon daylight and Illuminant A being a black-body radiator at a certain temperature, most normally encountered as incandescent light].


\newpage
References
===



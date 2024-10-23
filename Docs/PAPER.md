---
title: "Darkroom emulation: towards a more complete a mathematical model."
date: \today
author: Christopher Abraham
bibliography: "bibliography.bib"
colorlinks: true
mathfont: texgyredejavu-math.otf 
geometry: margin=3cm
fontsize: 12pt
---

Representing the outputs of analogue photography in a digital medium requires careful management of light and colour. A number of software exist to produce useable images with scans from consumer colour digital cameras^[At time of writing, *Negative Lab Pro* an Adobe Lightroom plugin and Darktable's *negadoctor* module are the most common and accessible, but almost all rely on a single image from a Colour-Filter Array camera.], but they tend to struggle with variances between image illuminant and camera specific sensitivities. Their approach is to assume the illuminant is the same as the one the camera is calibrated for and mapped via its camera profile. The process then is to try and invert the image along with specifically tuned transfer functions to recreate the look of photographic paper. Some of these software attempt image specific analysis to try and produce automatic colour balance similar to a lab technician's adjustments with older, specialty film-scanners. 

Similarly, many digital effects and image processing systems evolved from emulating darkroom processes^[cf. Dodging, burning, cropping, unsharp masks, etc.], but more often than not these occur *after* the inversion emulation and thus don't provide the same results as they would in a traditional darkroom. We can however more properly (and consistently) emulate the results of negative film printing by starting at first-principles and attempting to remove as many assumptions and variability as we can.

While determining an "accurate" digital representation of an image stored on developed print film has myriad factors and is subject to interpretation and artistry, there are a number of real and modellable parts of the process. Ultimately the question consists of three steps:

1. How is light affected by passing through developed negative print film and how can we measure this?
2. How does photographic paper respond to that light to produce an image after development?
3. How does the eye perceive that image, and how can we represent that digitally?

After this we can combine these to form a model of how an image can be represented digitally, which parts of the process are variables we can control for artistic input, and how we can compute this efficiently.


****
How can we measure the effects of film on light?
===

### Brief reflection on Colour

Colour in digital formats is often expressed in ratios of Red, Green and Blue. These however are emergent phenomena of the way our eyes and brains perceive colour and are a lossy, problematic way of modelling light transport ^[@ComputerColorIsBroken] ^[@CIE]. For calculations that better model the physics, it is better for us to consider a spectral power distribution (SPD) of light and how this is attenuated by transmissions and reflections ^[We will consider these mathematically identical. Transmission being the absorption of light through a medium and reflectance being the ratio of different wavelengths reflected, both of which "impart" an imbalance we perceive as colour.].

A spectral power distribution (see Figure \ref{SPD_Examples}) is the distribution of energy across a spectrum of wavelengths/frequencies. The area under this graph, its integral, is the total energy of light, but colour filters selectively attenuate different frequencies more than others and, given that the relative energy distribution across the spectrum is what we detect as colour, it's the most useful mathematical object for transmission and reflectance calculations. 

![SPD Curve examples ^[@SPD_Example]\label{SPD_Examples}](images/SPD_CurveExamples.png "SPD_Examples")


Metamerism, the perceived matching of colours from different spectral power distributions, means we shouldn't work in a pure RGB or XYZ space to properly model how light and dyes interact as what is sensed by our eye or a camera as a combination of RGB stimulus values might transform differently on reflection/transmission and we would have no way of determining the difference.

### Film as a series of colour filters

Developed colour print film is a stack of 3 colour filter dyes suspended in a gelatine matrix (see Figure \ref{Film_Stack}). As light passes through the Cyan, Magenta and Yellow colour filters (and the dyes of the film base), it is attenuated by their characteristic filtering curve proportionally to the densities of each dye.

![Cross-section of film ^[@X400DataSheet]\label{Film_Stack}](images/filmdye%20stack.png "Film_Stack")

The intensity of light though a single point/"pixel", of a specific wavelength on photographic paper is then found through:

$$ S_λ = I_λ⋅A^c_λ ⋅A^m_λ⋅A^y_λ⋅A^b_λ $$

where 
$S$ is a sample of transmittance energy, 
$I$ is the illuminant energy, 
$A$ is the attenuation of the light provided by each sequential dye filter (and film base) layer at this wavelength from $0$ to $1$ where $0$ is total attenuation and $1$ is total transmission.

The attenuation ($A_λ$) of a dye layer is proportional to the height of the layer, the density of the dye, and the filtering characteristics of the dye at that wavelength^[This is more accurately expanded by the Beer–Lambert law]. As the incident light which formed our image only affects one of those, the density, the variation between samples is also proportional to density. 

To fully measure the exact SPD of light which emerges after transmitting through print film, we would have to measure this attenuation across the range of wavelengths we're interested in ^[usually 380-750nm for human vision]. Unfortunately this is not feasible for hobby or even most professional measuring equipment at the high-resolutions needed for photography.

### Why we can't just take a picture of the film?

Digital colour-filter array cameras are very good at capturing real-life images of every-day scenes. They're designed similarly to our eyes in having short, medium and long wavelength sensitive sensors and can be calibrated so that given certain reference illuminants, their RGB values can be reliably mapped to the XYZ colour space and reproduced by digital monitors in a way which looks similar to how we would have seen that image. As a result digital cameras, like us, suffer from metamerism, but unfortunately has *different* metamers than our eyes or photographic paper. The response curves of each are all different and so all need to be taken into account for accurate modelling of the process.

The reason camera-scanning largely works at present is enforced consistency and published camera profiles. The algorithms are designed to expect a certain kind of light, cameras are designed to translate readings of a certain kind of light to perceptible colour spaces and then we have designed algorithms to work with that output to emulate the look of photographic paper by eye, feel and trial and error. The issue is that "certain kind of light" is often poorly specified. Most LED lights, even ones purporting "High CRI" produce a very different spectral power distributions than traditional light sources and each other^[@OscarsSolidStateLighting]. Given the spectrally-selective nature of the filters in film, this ends up producing different sensor readings^[@LightSourcesForFilmScanning].

There is also the problem of white-balance and colour depth. Different channels of a digital camera are more sensitive than others due to the way the photo-diodes produce different voltages at different wavelengths. A properly exposed, unclipped image on one channel thus means that some channels never reach their maximum values and after white-balancing lose colour depth in 2 out of 3 of the channels.

### Removing variability

The two variances that cause inconsistency are in the lights and a cameras colour transformation matrix/calibration of its channels. These issues are compounded by the crossovers in most camera's filter curves^[@RGBLightSource]. The problem is that a particular sensor reading on a camera's R,G,B values in a single image captures a number of factors conflated together:

$$ S = T\intop_{λ}{I(λ)⋅A^F(λ)⋅A^C(λ) },dλ  $$

where $S$ is the amount of energy received by the sensor, $T$ is Time, $I$ is the illuminant energy, $A^F$ the attenuation of the film, $A^C$ is the attenuation of the relevant colour filter in the bayer filter. Computationally however, it is better to consider a Riemann sum.

$$ S = T\sum_{λ}{I_λ⋅A^F_λ⋅A^C_λ },Δλ  $$

We can see then that if any of $I_λ,A^F_λ,A^C_λ$ are 0, the whole product will be 0 and can be ignored. If we could ensure that $I_λ$ was always 0 and non-zero only once, our sum would only ever have one non-zero term and the sample would be:

$$ S = T⋅ I ⋅A^F_λ⋅A^C_λ  $$

This simplification can be achieved physically with narrow-band light. 

Similarly, we're actually only interested in the relative energy between each sample as we can linearly scale the total energy afterwards as needed. This means if we take a sample of our narrow-band light at a point unimpeded by the film such as a sprocket hole or prior to loading, we get a sample of:

$$ S_{max} = T⋅ I ⋅ A^C_λ  $$

Our relative sample then can be calculated:

$$ {S_{pixel}\over{S_{max}}} = {{T⋅ I ⋅A^F_λ⋅A^C_λ}\over{T⋅ I ⋅A^C_λ}} = A^F_λ $$

We have thus measured the attenuation of the film at our chosen wavelength by dividing a sample by the max the sample could be, unobstructed by film at all. 



****


How does photographic paper respond to light?
===


The SPD of light which hits photographic paper in a traditional darkroom is usually a black-body illuminator such as incandescent bulbs, attenuated through spectrally-selective colour balancing filters to account for scene-lighting and artistic choices, and then through the spectrally-selective coloured filters of the film stock.


****

How do we perceive an image?
===

When we perceive colours in a printed ^[In this article I'll be referring largely to RA-4 paper but some elements may apply to other types of photographic paper.] image, we are perceiving:

1. the light illuminating the scene -  $I$
2. the amount of that light absorbed by the paper and dyes $R$
3. the relative excitement of the cones in our eyes and the rest of human colour perception. 

Thankfully, the CIE colour matching experiments ^[@CIE] provide a model for the relative excitement of our cones in the $\bar{x},\bar{y},\bar{z}$ colour matching functions that allow us to go from spectral data to the XYZ colour space. This is the colour space which underpins most other digital colour spaces so if we can define our image in coordinates here, we can transform it reliably to our screens.

Similarly, there are various standard illuminants such as D65 or A which have known SPDs or calculable outputs^[Illuminant D65 being the usual illuminant digital cameras are calibrated against of afternoon daylight and Illuminant A being a black-body radiator at a certain temperature, most normally encountered as incandescent light].


\newpage
References
===
# Visual Magnitude for ArcGIS Pro

This is an Add-In for ArcGIS 2.x which aims to add support for _visual magnitude viewsheds_.

## Features
* Visual magnitude calculation
* Viewshed generation
* Support for visibility of wind turbines
* Vector viewpoints
* Vector roads, trails,...
* Weighted viewpoints
* Custom elevation offset for each viewpoint
* Parallel processing

## Example
Input - Digital Elevation Model (DEM) and vector viewpoints

![alt text](http://share.kubacech.cz/input1.jpg "DEM and viewpoints")

Output - Visual magnitude viewshed

![alt text](http://share.kubacech.cz/result1.jpg "Visual magnitude viewshed")

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites
Software you need to have installed to use or develop the add-in.
#### Use
```
ArcGis Pro 2.x
```

#### Development
```
ArcGis Pro 2.x
Visual Studio 2017
.NET Framework 4.6
```

### Installing

This part will guide you through the add-in installation process.

To install the Add-In:
1. Install the ArcGIS Pro 2.x and all its dependencies.
2. Download the latest release of Visual Magnitude Add-In from [Release tab](https://github.com/czechflek/VisualMagnitude/releases).
3. Open the downloaded _VisualMagnitude.esriAddinX_ file.
4. Click _Install Add-In_.

Now, the Visual Magnitude Add-In is installed. To test this:
1. Open ArcGis Pro.
2. Open a project.
3. In a ribbon, check the _Visual Magnitude Tab_.
![alt text](http://share.kubacech.cz/ribbon.PNG "Visual Magnitude tab")

## Usage
See a video tutorial on YouTube: [https://youtu.be/UA4JMfoY38I](https://youtu.be/UA4JMfoY38I)

### Basic Usage
1. Import Digital Elevation Model (DEM) and viewpoints to a project.
2. Go to _Visual Magnitude_ tab.
3. Go to _Settings_ and update them if necesssary.
4. Save the new settings.
5. Select DEM layer and viewpoints layer from dropdowns in _Visual Magnitude_ tab.
6. Click _Start Analysis_.
7. When the analysis is finished, the result will be displayed and saved to _VisualMagnitudeOutput_ folder inside the project folder.

### Per viewpoint elevation offset
1. Open _Attribute Table_ of the viewpoint layer
2. Click _Add Field_.
3. Add new field _OFFSET_ with Data Type _Double_.
4. Save the changes.
5. Add offsets for each viewpoint in the table.
6. Run the analysis.

### Weighted viewpoints
1. Open _Attribute Table_ of the viewpoint layer
2. Click _Add Field_.
3. Add new field _WEIGHT_ with Data Type _Double_.
4. Save the changes.
5. Add the weight for each viewpoint in the table.
6. Run the analysis.

## Documentation
Download the [Documentation.chm](Documentation.chm) which contains HTML documentation.

## Contributing

If you wish to contribute to this project:
- E-mail me at visualmagnitude ![alt text](http://share.kubacech.cz/at_sign.png "at") kubacech.cz
- Create a new [issue](https://github.com/czechflek/VisualMagnitude/issues)


## Authors

* **Jakub Čech** - [CzechFlek](https://github.com/czechflek)
* **Brent Chamberlain** - [http://brentchamberlain.org/](http://brentchamberlain.org/)


## License

This project is licensed under the GPL-3 License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Brent Chamberlain for developing the methodology
* Kansas State University

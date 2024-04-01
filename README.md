# Installation from Git
This is how you should download the project as an end user. If you just want to use our package, and are not interested in developing, do this.

1. In Unity, go to the Package Manager.
![image](https://github.com/harrisonheld/UnityWorldGen/assets/24709296/fac65276-98cd-4a63-a0b0-73eb419cbc94)

2. In the package manager, select Add package from git URL.
![image](https://github.com/harrisonheld/UnityWorldGen/assets/24709296/a1628d57-c9ab-4f94-9595-d30794d5787e)

3. Enter 'https://github.com/harrisonheld/UnityWorldGen.git' and add it.
![image](https://github.com/harrisonheld/UnityWorldGen/assets/24709296/2878fc65-79ae-46e7-9ce5-ad00382f311e)

## Installation from Disk
If you are a developer and want to edit the package, use this method of installation.

Download a release or this repo, and in the Package Manager, select Add package from Disk. Select the project's package.json.
![image](https://github.com/harrisonheld/UnityWorldGen/assets/24709296/9910b1c1-5747-4e61-9ad5-e1706582336b)

# Usage
This tutorial assumes you are familiar with Unity.

1. Add a new Empty to your scene.
2. Click 'Add Component' in the inspector and search for CustomTerrain (World Generator).
3. You will need to add a biome before you generate, so use the Add Biome section to add one.
4. Click 'Generate Terrain'.



# Unit Testing
To run unit tests, you must list the package as a testable in your project's manifest.

Open your Unity project (this is your own project on your own local machine) in the file explorer.
![image](https://github.com/harrisonheld/UnityWorldGen/assets/24709296/897c9053-0922-4b7e-85d1-2069fbdcae3f)

Open manifest.json and add the following line at the bottom.
![image](https://github.com/harrisonheld/UnityWorldGen/assets/24709296/f6f899c2-b5ca-4397-a4f9-fc8c1848b903)
"testables": [
  "com.csce482.worldgenerator"
]

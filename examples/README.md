# C# examples

Here you will find some examples to test out the RGB LED Matrix with the C# library.

## Building

To build the C# wrapper for the RGB Matrix C library you need to first have mono installed.

### Install Mono

```shell
$ sudo apt-get update
$ sudo apt-get install mono-complete
```

Then, in the root directory for the matrix library type

```shell
make build-csharp
```

To run the example applications in the `./examples` folder

### font-example.exe

Displays a simple Hello World! message using the selected font

```shell
sudo mono minimal-example.exe ../fonts/6x10.bdf "Hello World!"
```

### matrix-rain.exe

Displays a matrix waterfall like the movie

```shell
sudo mono matrix-rain.exe
```

### minimal-example.exe

Draws a shape in the center of a coloured background

```shell
sudo mono minimal-example.exe
```

### pulsing-brightness.exe

Pulses random colours on the RGB LED Matrix

```shell
sudo mono pulsing-brightness.exe
```

## Notes

C# applications look for libraries in the working directory of the application. To use this library for your own projects you will need to ensure you have RGBLedMatrix.dll and librgbmatrix.so in the same folder as your exe.

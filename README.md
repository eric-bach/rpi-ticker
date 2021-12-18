# Raspberry Pi Ticker (rpi-ticker)

## Building core libraries

Build the libraries needed for the rgb matrix based on hzeller's repo

```shell
$ make
```

## Building C# bindings

To build the C# wrapper for the RGB Matrix C library you need to first have mono installed.

### Install Mono

```shell
$ sudo apt-get update
$ sudo apt-get install mono-complete
```

//Then, in the `bindings` directory for the matrix library type
Then, in the root directory

```shell
make build-csharp
```

To run the example applications in the `src` folder

```shell
sudo mono rpi-ticker.exe
```

## Notes

C# applications look for libraries in the working directory of the application. To use this library for your own projects you will need to ensure you have RGBLedMatrix.dll and librgbmatrix.so in the same folder as your exe.

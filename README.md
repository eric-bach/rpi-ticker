﻿# Raspberry Pi Ticker (rpi-ticker)

This project contains the source code for deploying a news headline and stock ticker powered by RGB LED matrix panels and the [Raspberry Pi control library](https://github.com/hzeller/rpi-rgb-led-matrix) from [hzeller](https://github.com/hzeller). The specific RGB LED matrix panels and bonnet/HAT I used for this project were sourced from [Adafruit](https://www.adafruit.com) but others may work as well.

## Build and run the project

1. In the root directory build all the necessary libraries based on [hzeller/rpi=rgb-led-matrix](https://github.com/hzeller/rpi-rgb-led-matrix)

   ```shell
   $ make
   ```

2. To build the C# wrapper for the RGB Matrix C library you need to first have mono installed

   ```shell
   $ sudo apt-get update
   $ sudo apt-get install mono-complete
   ```

3. Then, in the root directory build the C# libraries and project

   ```shell
   make build-csharp
   ```

4. To run the example applications in the `src` folder

   ```shell
   sudo mono rpi-ticker.exe
   ```

### Notes

C# applications look for libraries in the working directory of the application. To use this library for your own projects you will need to ensure you have RGBLedMatrix.dll and librgbmatrix.so in the same folder as your exe.

## Adafruit demos

Adafruit provides a few example applications to test the RGB LED Matrix as well as detailed instructions on how to setup the panels to work with the Raspberry Pi.

1. Follow Adafruit documentation to setup bonnet/HAT with the Adafruit RGB LED matrix panel

   https://cdn-learn.adafruit.com/downloads/pdf/adafruit-rgb-matrix-bonnet-for-raspberry-pi.pdf

2. Note that if you are using the Makefile in the rpi-rgb-led-matrix repo, add the --led-gpio-mapping=adafruit-hat input argument

   sudo ./demo -D0 --led-rows=32 --led-cols=16
   sudo ./scrolling-text-example --led-rows=32 --led-cols=64 --led-chain=2 --led-gpio-mapping=adafruit-hat -f ../fonts/6x10.bdf "Hello World"

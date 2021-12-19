const matrix = require('rpi-led-matrix');

const matrix = new LedMatrix(
  {
    ...LedMatrix.defaultMatrixOptions(),
    rows: 32,
    cols: 64,
    chainLength: 2,
    hardwareMapping: GpioMapping.AdafruitHat,
    pixelMapperConfig: LedMatrixUtils.encodeMappers({
      type: PixelMapperType.U,
    }),
  },
  {
    ...LedMatrix.defaultRuntimeOptions(),
    gpioSlowdown: 1,
  }
);

matrix.clear();

matrix.drawCircle(
  matrix.width() / 2,
  matrix.height() / 2,
  matrix.width() / 2 - 1
);

matrix.sync();

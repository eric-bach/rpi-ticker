CSHARP_LIB=RGBLedMatrix.dll
SOURCES=RGBLedCanvas.cs RGBLedMatrix.cs RGBLedFont.cs
CSHARP_COMPILER=mcs

RGB_LIBDIR=../lib
RGB_LIBRARY_NAME=rgbmatrix
RGB_LIBRARY=$(RGB_LIBDIR)/lib$(RGB_LIBRARY_NAME).so.1

SRC_DIR=../src

$(CSHARP_LIB) : $(SOURCES) $(RGB_LIBRARY)
	$(CSHARP_COMPILER) -target:library -out:$@ $(SOURCES)

$(RGB_LIBRARY):
	$(MAKE) -C $(RGB_LIBDIR)

build: $(CSHARP_LIB)
	$(MAKE) -C $(SRC_DIR) all

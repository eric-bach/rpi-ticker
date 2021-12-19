CSHARP_LIB=RGBLedMatrix.dll
CSHARP_LIB2=System.Net.Http.dll
CSHARP_COMPILER=mcs
CSHARP_LIBDIR=../bindings
CSHARP_LIBRARY=$(CSHARP_LIBDIR)/$(CSHARP_LIB)

RGB_LIBDIR=../lib
RGB_LIBRARY_NAME=librgbmatrix
RGB_LIBRARY=$(RGB_LIBDIR)/$(RGB_LIBRARY_NAME).so.1

all: $(CSHARP_LIB)
	cp $(RGB_LIBRARY) $(RGB_LIBRARY_NAME).so
	cp $(CSHARP_LIBRARY) $(CSHARP_LIB)
	$(CSHARP_COMPILER) -r:$(CSHARP_LIB) -r:$(CSHARP_LIB2) -out:rpi-ticker.exe ./ScrollingText/Program.cs

rpi-ticker.exe: $(CSHARP_LIB)
	cp $(RGB_LIBRARY) $(RGB_LIBRARY_NAME).so
	cp $(CSHARP_LIBRARY) $(CSHARP_LIB)
	$(CSHARP_COMPILER) -r:$(CSHARP_LIB) -r:$(CSHARP_LIB2) -out:rpi-ticker.exe ./ScrollingText/Program.cs

$(CSHARP_LIB) :
	$(MAKE) -C $(CSHARP_LIBDIR)

.PHONY : all

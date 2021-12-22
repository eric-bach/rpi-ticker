CSHARP_LIB=RGBLedMatrix.dll
CSHARP_HTTP_LIB=System.Net.Http.dll
CSHARP_JSON_LIB=Newtonsoft.Json.dll
CSHARP_COMPILER=mcs
CSHARP_LIBDIR=../bindings
CSHARP_LIBRARY=$(CSHARP_LIBDIR)/$(CSHARP_LIB)
CSHARP_PROJECT_LIBDIR=lib
RGB_LIBDIR=../lib
RGB_LIBRARY_NAME=librgbmatrix
RGB_LIBRARY=$(RGB_LIBDIR)/$(RGB_LIBRARY_NAME).so.1

all: $(CSHARP_LIB)
	cp $(RGB_LIBRARY) $(RGB_LIBRARY_NAME).so
	cp $(CSHARP_LIBRARY) $(CSHARP_LIB)
	cp $(CSHARP_PROJECT_LIBDIR)/$(CSHARP_HTTP_LIB) $(CSHARP_HTTP_LIB)
	cp $(CSHARP_PROJECT_LIBDIR)/$(CSHARP_JSON_LIB) $(CSHARP_JSON_LIB)
	$(CSHARP_COMPILER) -r:$(CSHARP_LIB) -r:$(CSHARP_HTTP_LIB) -r:$(CSHARP_JSON_LIB) -out:rpi-ticker.exe ./Program.cs ./Utils/DotEnv.cs ./Controllers/MatrixController.cs ./Controllers/NewsData.cs ./Controllers/YahooFinance.cs ./ViewModels/NewsdataHeadlines.cs ./ViewModels/QuotesViewModel.cs ./ViewModels/YahooFinanceQuote.cs

rpi-ticker.exe: $(CSHARP_LIB)
	cp $(RGB_LIBRARY) $(RGB_LIBRARY_NAME).so
	cp $(CSHARP_LIBRARY) $(CSHARP_LIB)
	cp $(CSHARP_PROJECT_LIBDIR)/$(CSHARP_HTTP_LIB) $(CSHARP_HTTP_LIB)
	cp $(CSHARP_PROJECT_LIBDIR)/$(CSHARP_JSON_LIB) $(CSHARP_JSON_LIB)
	$(CSHARP_COMPILER) -r:$(CSHARP_LIB) -r:$(CSHARP_HTTP_LIB) -r:$(CSHARP_JSON_LIB) -out:rpi-ticker.exe ./Program.cs ./Utils/DotEnv.cs ./Controllers/MatrixController.cs ./Controllers/NewsData.cs ./Controllers/YahooFinance.cs ./ViewModels/NewsdataHeadlines.cs ./ViewModels/QuotesViewModel.cs ./ViewModels/YahooFinanceQuote.cs	

$(CSHARP_LIB) :
	$(MAKE) -C $(CSHARP_LIBDIR)

.PHONY : all

# unix 
build:
	rm -rf bin
	rm -rf obj
	rm -rf content
	mkdir content
	cp -R ../RxTemplate content/RxTemplate
	dotnet pack

uninstall:
	dotnet new uninstall RazorX.Template

install:
	dotnet new install bin/Release/RazorX.Template.1.0.0-beta.nupkg

reinstall: build uninstall install

# windows
win-build:
	rmdir /s/q content
	mkdir content
	xcopy ..\RxTemplate content\RxTemplate /E
	dotnet pack

win-uninstall:
	dotnet new uninstall RazorX.Template

win-install:
	dotnet new install bin\Release\RazorX.Template.1.0.0-beta.nupkg

win-reinstall: win-build win-uninstall win-install
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
	dotnet new install bin/Release/RazorX.Template.1.0.0-alpha.nupkg

reinstall: build uninstall install
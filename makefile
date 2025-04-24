SHELL = cmd

LOCAL = Local
UNITY_PATH := $(file < $(LOCAL)/UnityPath.txt)
REPO_PATH := $(file < $(LOCAL)/REPOPath.txt)
PUBLISH_TOKEN := $(file < $(LOCAL)/PublishToken.txt)
NAME = MapUpgrade
DLL = bin\Debug\netstandard2.1\$(NAME).dll

All: Compile Push Debug

Compile: $(DLL) 

$(DLL): Source/*.cs $(NAME).csproj
	dotnet build
	copy $(DLL) "$(REPO_PATH)\BepInEx\plugins\$(NAME).dll" /B
ifneq ("$(UNITY_PATH)","")
	copy $(DLL) "$(UNITY_PATH)\Content\$(NAME).dll" /B
endif

Debug: 
	$(REPO_PATH)\REPO.exe $(ARGS)

Push:
	cd /D "$(REPO_PATH)\BepInEx\plugins" &&\
		git add . && \
		git commit -m "Recompiled plugins" &&\
		git push origin main

Copy:
	copy Build\package\Map_Upgrade.repobundle $(REPO_PATH)\BepInEx\plugins\Map_Upgrade.repobundle

Publish:
	tcli build
	tcli publish --token $(PUBLISH_TOKEN)

Clean:
	del $(DLL)
	
.IGNORE: Push

.PHONY: Compile Debug Push Copy
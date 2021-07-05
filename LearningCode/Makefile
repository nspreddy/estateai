# Usage:
# make buid : makes docker build 
# make local: make local build, executables are @ $GOPATH/bin
.DEFAULT_GOAL := build
HOST=$(shell hostname -s)

#Dependencies

install_deps:
	go get -u github.com/Masterminds/glide
	glide update -v
	git submodule init
	git submodule update

########################################################################################################################
# Build the code on local machine without docker.
########################################################################################################################

format:
	go fmt ./src/... 2>&1 | read; if [ $$? -eq 0 ]; then echo "go fmt detected and repaired style." ; false ; fi

local: format 
	go install ./src/...

localtest: format
	go test -race -cover ./src/...

########################################################################################################################
# Build the code on local machine by emulating TnRP docker build.
########################################################################################################################

docker-prep:
	rm -rf docker/
	mkdir -p docker/Samples/bin
	cp ${GOPATH}/bin/* docker/samples/bin/

build: INTERACTIVE_ARG = -it
build: HOST_SRC_ROOT = $(PWD)
build: dockerbuild
	@echo ">$(HOST)"

test: INTERACTIVE_ARG = -it
test: TASK_ARG = -test
test: HOST_SRC_ROOT = $(PWD)
test: dockerbuild
	@echo ">$(HOST)"

cover: INTERACTIVE_ARG = -it
cover: TASK_ARG = -cover
cover: HOST_SRC_ROOT = $(PWD)
cover: dockerbuild
	@echo ">$(HOST)"

BASE_IMAGE = golang:latest
dockerbuild:
	docker run $(INTERACTIVE_ARG)  --rm -v $(HOST_SRC_ROOT):/tmp/EstateAI/ -w /tmp/EstateAI $(BASE_IMAGE) /tmp/EstateAI/build.sh
	    
#Publish Images in Google gcr.io
IMAGE_NAME = estateai

image: build
		docker build --tag $(IMAGE_NAME) --file=Dockerfile . || exit 1; \

IMAGE_TAG = $(USER)-D-$(shell date +'%Y%m%d%H%M')
REGISTRY = docker.io/nagareddy/estateai
push: image
		docker tag  $(IMAGE_NAME) $(REGISTRY):$(IMAGE_TAG); \
		docker push $(REGISTRY):$(IMAGE_TAG)

PROJECT_ID=nrrealestateai
GCPREG= gcr.io/${PROJECT_ID}/$(IMAGE_NAME)
pushgcp: image
		docker tag  $(IMAGE_NAME) $(GCPREG$):$(IMAGE_TAG); \
		gcloud docker -- push $(GCPREG):$(IMAGE_TAG)

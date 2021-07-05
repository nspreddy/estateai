FROM golang:latest

LABEL name=sample1
# Copy the local binary files to the container’s workspace.
COPY .  /go/src/HW
ENV GOBIN=/go/bin
RUN cd /go/src/HW;go install helloworld.go;go install hellocmd.go

WORKDIR /go/bin
